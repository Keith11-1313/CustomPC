using Microsoft.Data.Sqlite;

namespace CustomPC;

public sealed partial class AppStore
{
    public const int MaximumStock = 10000000;
    public const int MaximumComponentWatts = 10000;
    public int Reserved(string partId)
    {
        int reservedQuantity = 0;

        foreach (CustomerOrder order in Orders)
        {
            bool reservesStock = order.Status == "Pending Payment" ||
                order.Status == "Paid" || order.Status == "Processing" ||
                order.Status == "Assembly" || order.Status == "Ready for Pickup";

            if (!reservesStock)
            {
                continue;
            }

            foreach (OrderLine line in order.Lines)
            {
                if (line.PartId == partId)
                {
                    reservedQuantity += line.Quantity;
                }
            }
        }

        return reservedQuantity;
    }

    public int Available(Part part)
    {
        return part.Stock - Reserved(part.Id);
    }

    public string StockState(Part part)
    {
        int availableQuantity = Available(part);

        if (availableQuantity <= 0)
        {
            return "Out of stock";
        }

        if (availableQuantity <= part.LowStock)
        {
            return "Low stock";
        }

        return "Available";
    }

    public void SavePart(Part part)
    {
        using (SqliteConnection connection = database.OpenConnection())
        using (SqliteTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                LoadData(connection);
                RequireAdmin();
                ValidatePart(part);

                Part? existingPart = null;
                foreach (Part savedPart in Parts)
                {
                    if (savedPart.Id == part.Id)
                    {
                        existingPart = savedPart;
                        break;
                    }
                }

                int previousStock = 0;
                if (existingPart != null)
                {
                    previousStock = existingPart.Stock;
                }

                // Catalog maintenance cannot bypass the Staff-only stock adjustment action.
                if (part.Stock != previousStock)
                {
                    throw new InvalidOperationException("Only Staff can change stock. New parts start at zero; use Adjust stock with a Staff account.");
                }

                if (existingPart != null) Parts.Remove(existingPart);
                Parts.Add(part);
                SaveData(connection, transaction);
            }
            catch
            {
                UndoFailedChange(connection, transaction);
                throw;
            }
        }
    }

    private void ValidatePart(Part part)
    {
        BusinessText.Validate(part.Name, "Product name");
        BusinessText.Validate(part.Brand, "Brand");
        BusinessText.Validate(part.Specifications, "Specifications");
        BusinessText.Validate(part.Socket, "Socket");
        BusinessText.Validate(part.MemoryType, "Memory type");
        BusinessText.Validate(part.CatalogCode, "Catalog code");
        if (part.PhotoFileName.Length > 0) ProductPhotos.Resolve(DatabasePath, part.PhotoFileName);
        if (string.IsNullOrWhiteSpace(part.Name) || string.IsNullOrWhiteSpace(part.Brand))
        {
            throw new InvalidOperationException("Enter a component name and brand.");
        }

        if (Array.IndexOf(Categories, part.Category) == -1 || part.Price <= 0)
        {
            throw new InvalidOperationException("Choose a category and enter a positive price.");
        }

        // Compare product identity, not prices or stock; edits exclude their own saved record.
        static string Normalize(string value) => string.Join(" ", value.Split((char[]?)null,
            StringSplitOptions.RemoveEmptyEntries));
        string name = Normalize(part.Name);
        string brand = Normalize(part.Brand);
        Part? duplicate = Parts.FirstOrDefault(saved => saved.Id != part.Id &&
            string.Equals(saved.Category, part.Category, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(Normalize(saved.Brand), brand, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(Normalize(saved.Name), name, StringComparison.OrdinalIgnoreCase));
        if (duplicate != null)
        {
            throw new InvalidOperationException(duplicate.Archived
                ? "This product already exists in archived parts. Select Include archived and edit the existing part to restore it."
                : "This product already exists. Edit the existing part instead.");
        }

        if (part.Stock < Reserved(part.Id))
        {
            throw new InvalidOperationException("Stock cannot be below the reserved quantity.");
        }

        if (part.Stock > MaximumStock)
        {
            throw new InvalidOperationException("Stock cannot exceed 10,000,000 units.");
        }

        if (part.LowStock < 0 || part.Watts < 0 || part.RecommendedPsuWatts < 0)
        {
            throw new InvalidOperationException("Low-stock threshold and wattage cannot be negative.");
        }
        if (part.Watts > MaximumComponentWatts || part.RecommendedPsuWatts > MaximumComponentWatts || part.LowStock > MaximumStock)
            throw new InvalidOperationException("Wattage must be at most 10,000 W, and the low-stock threshold at most 10,000,000 units.");

    }

    public void AdjustStock(string partId, int quantity, string reason)
    {
        using (SqliteConnection connection = database.OpenConnection())
        using (SqliteTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                LoadData(connection);
                RequireStaff();
                BusinessText.Validate(reason, "Adjustment reason");
                Part part = FindPart(partId);

                if (quantity == 0 || string.IsNullOrWhiteSpace(reason))
                {
                    throw new InvalidOperationException("Enter a nonzero adjustment and a reason.");
                }

                // Calculate as long first so a large adjustment cannot overflow an int.
                long updatedStock = (long)part.Stock + quantity;
                if (updatedStock < Reserved(partId))
                {
                    throw new InvalidOperationException("Stock cannot be below the reserved quantity.");
                }

                if (updatedStock > MaximumStock)
                {
                    throw new InvalidOperationException("Stock cannot exceed 10,000,000 units.");
                }

                part.Stock = (int)updatedStock;
                RecordStockMovement(partId, quantity, reason.Trim());
                SaveData(connection, transaction);
            }
            catch
            {
                UndoFailedChange(connection, transaction);
                throw;
            }
        }
    }

    private void RecordStockMovement(string partId, int quantity, string reason)
    {
        InventoryMovement movement = new InventoryMovement();
        movement.PartId = partId;
        movement.Quantity = quantity;
        movement.Reason = reason;
        movement.UserId = Session!.Id;
        Movements.Add(movement);
    }

}
