using Microsoft.Data.Sqlite;

namespace CustomPC;

public sealed partial class AppStore
{
    public const int MaximumStock = 10000000;
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

        if (availableQuantity == 0)
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
                    Parts.Remove(existingPart);
                }

                if (existingPart == null || previousStock != part.Stock)
                {
                    RecordStockMovement(part.Id, part.Stock - previousStock, "Admin stock entry");
                }

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
        if (string.IsNullOrWhiteSpace(part.Name) || string.IsNullOrWhiteSpace(part.Brand))
        {
            throw new InvalidOperationException("Enter a component name and brand.");
        }

        if (Array.IndexOf(Categories, part.Category) == -1 || part.Price <= 0)
        {
            throw new InvalidOperationException("Choose a category and enter a positive price.");
        }

        if (part.Stock < Reserved(part.Id))
        {
            throw new InvalidOperationException("Stock cannot be below the reserved quantity.");
        }

        if (part.Stock > MaximumStock)
        {
            throw new InvalidOperationException("Stock cannot exceed 10,000,000 units.");
        }

        if (part.LowStock < 0 || part.Watts < 0)
        {
            throw new InvalidOperationException("Low-stock threshold and wattage cannot be negative.");
        }

        if (part.SupplierId.Length > 0)
        {
            bool allowedSupplierFound = false;
            foreach (Supplier supplier in Suppliers)
            {
                if (supplier.Id != part.SupplierId)
                {
                    continue;
                }

                if (!supplier.Archived)
                {
                    allowedSupplierFound = true;
                    break;
                }

                // Archiving a supplier must not erase historical component associations.
                foreach (Part existingPart in Parts)
                {
                    if (existingPart.Id == part.Id && existingPart.SupplierId == supplier.Id)
                    {
                        allowedSupplierFound = true;
                        break;
                    }
                }
            }

            if (!allowedSupplierFound)
            {
                throw new InvalidOperationException("Choose an active supplier.");
            }
        }
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

    public void SaveSupplier(Supplier supplier)
    {
        using (SqliteConnection connection = database.OpenConnection())
        using (SqliteTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                LoadData(connection);
                RequireAdmin();

                if (string.IsNullOrWhiteSpace(supplier.Name) || string.IsNullOrWhiteSpace(supplier.Contact))
                {
                    throw new InvalidOperationException("Supplier name and contact are required.");
                }

                for (int index = 0; index < Suppliers.Count; index++)
                {
                    if (Suppliers[index].Id == supplier.Id)
                    {
                        Suppliers.RemoveAt(index);
                        break;
                    }
                }

                Suppliers.Add(supplier);
                SaveData(connection, transaction);
            }
            catch
            {
                UndoFailedChange(connection, transaction);
                throw;
            }
        }
    }
}
