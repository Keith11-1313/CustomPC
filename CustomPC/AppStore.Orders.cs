using Microsoft.Data.Sqlite;

namespace CustomPC;

public sealed partial class AppStore
{
    public CustomerOrder CreateOrder(List<string> partIds, string customerId, decimal? confirmedTotal = null)
    {
        using (SqliteConnection connection = database.OpenConnection())
        using (SqliteTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                LoadData(connection);
                CancelExpiredOrders();

                if (Session == null)
                {
                    throw new InvalidOperationException("Sign in to place an order.");
                }

                if (!IsStaff && customerId != Session.Id)
                {
                    throw new InvalidOperationException("You can only order for your own account.");
                }

                UserAccount customer = FindActiveCustomer(customerId);
                List<Part> selectedParts = new List<Part>();

                foreach (string partId in partIds)
                {
                    selectedParts.Add(FindPart(partId));
                }

                List<string> errors = CheckBuild(selectedParts);
                if (errors.Count > 0)
                {
                    throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
                }

                // Check again inside the transaction: another instance could change a price
                // while the customer is reading the confirmation message.
                decimal currentTotal = 0;
                foreach (Part selectedPart in selectedParts)
                {
                    currentTotal += selectedPart.Price;
                }

                if (confirmedTotal.HasValue && confirmedTotal.Value != currentTotal)
                {
                    throw new InvalidOperationException("Component prices changed. Review the updated build total and confirm again.");
                }

                PcBuild build = new PcBuild();
                build.CustomerId = customer.Id;
                build.PartIds = new List<string>(partIds);
                Builds.Add(build);

                CustomerOrder order = new CustomerOrder();
                order.Number = CreateOrderNumber();
                order.CustomerId = customer.Id;
                order.CustomerName = customer.Name;
                order.BuildId = build.Id;
                order.CreatedUtc = DateTime.UtcNow;
                order.DeadlineUtc = order.CreatedUtc.AddHours(24);

                foreach (Part part in selectedParts)
                {
                    OrderLine line = new OrderLine();
                    line.PartId = part.Id;
                    line.Name = part.Name;
                    line.Price = part.Price;
                    line.Quantity = 1;
                    order.Lines.Add(line);
                }

                // Reserved() counts active order lines, so adding the order reserves its parts.
                Orders.Add(order);
                SaveData(connection, transaction);
                return order;
            }
            catch
            {
                UndoFailedChange(connection, transaction);
                throw;
            }
        }
    }

    private UserAccount FindActiveCustomer(string customerId)
    {
        foreach (UserAccount account in Users)
        {
            if (account.Id == customerId && !account.Archived && account.Role == "Customer")
            {
                return account;
            }
        }

        throw new InvalidOperationException("Choose an active customer account.");
    }

    private string CreateOrderNumber()
    {
        string date = DateTime.Now.ToString("yyyyMMdd");
        string uniqueCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        return "PC-" + date + "-" + uniqueCode;
    }

    public List<CustomerOrder> VisibleOrders()
    {
        List<CustomerOrder> visibleOrders = new List<CustomerOrder>();

        foreach (CustomerOrder order in Orders)
        {
            bool ownOrder = Session != null && order.CustomerId == Session.Id;
            if (IsStaff || ownOrder)
            {
                visibleOrders.Add(order);
            }
        }

        visibleOrders.Sort(CompareOrderDates);
        return visibleOrders;
    }

    private static int CompareOrderDates(CustomerOrder first, CustomerOrder second)
    {
        return second.CreatedUtc.CompareTo(first.CreatedUtc);
    }

    public void ExpireOrders()
    {
        bool expiredOrderFound = false;
        foreach (CustomerOrder order in Orders)
        {
            if (order.Status == "Pending Payment" && order.DeadlineUtc <= DateTime.UtcNow)
            {
                expiredOrderFound = true;
                break;
            }
        }

        if (!expiredOrderFound)
        {
            return;
        }

        using (SqliteConnection connection = database.OpenConnection())
        using (SqliteTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                LoadData(connection);
                CancelExpiredOrders();
                SaveData(connection, transaction);
            }
            catch
            {
                UndoFailedChange(connection, transaction);
                throw;
            }
        }
    }

    private void CancelExpiredOrders()
    {
        foreach (CustomerOrder order in Orders)
        {
            if (order.Status == "Pending Payment" && order.DeadlineUtc <= DateTime.UtcNow)
            {
                // Cancelled orders no longer count toward Reserved(), releasing their stock.
                order.Status = "Cancelled";
            }
        }
    }

    public void ConfirmPayment(string orderId)
    {
        ExpireOrders();

        using (SqliteConnection connection = database.OpenConnection())
        using (SqliteTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                LoadData(connection);
                RequireStaff();
                CustomerOrder order = FindOrder(orderId);

                if (order.Status != "Pending Payment" || order.DeadlineUtc <= DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Only an unexpired pending order can be paid.");
                }

                Payment payment = new Payment();
                payment.OrderId = order.Id;
                payment.StaffId = Session!.Id;
                payment.Amount = order.Total;
                Payments.Add(payment);

                order.Status = "Paid";
                SaveData(connection, transaction);
            }
            catch
            {
                UndoFailedChange(connection, transaction);
                throw;
            }
        }
    }

    public void AdvanceOrder(string orderId)
    {
        using (SqliteConnection connection = database.OpenConnection())
        using (SqliteTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                LoadData(connection);
                RequireStaff();
                CustomerOrder order = FindOrder(orderId);

                // Only staff can move a paid order forward, one stage at a time.
                switch (order.Status)
                {
                    case "Paid":
                        order.Status = "Processing";
                        break;
                    case "Processing":
                        order.Status = "Assembly";
                        break;
                    case "Assembly":
                        order.Status = "Ready for Pickup";
                        break;
                    case "Ready for Pickup":
                        order.Status = "Completed";
                        DeductCompletedOrderStock(order);
                        break;
                    default:
                        throw new InvalidOperationException("Confirm payment first, or this order already ended.");
                }

                SaveData(connection, transaction);
            }
            catch
            {
                UndoFailedChange(connection, transaction);
                throw;
            }
        }
    }

    private void DeductCompletedOrderStock(CustomerOrder order)
    {
        foreach (OrderLine line in order.Lines)
        {
            Part part = FindPart(line.PartId);
            part.Stock -= line.Quantity;
            RecordStockMovement(part.Id, -line.Quantity, "Completed " + order.Number);
        }
    }

    public void CancelOrder(string orderId)
    {
        using (SqliteConnection connection = database.OpenConnection())
        using (SqliteTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                LoadData(connection);
                CustomerOrder order = FindOrder(orderId);

                if (Session == null || (!IsStaff && order.CustomerId != Session.Id))
                {
                    throw new InvalidOperationException("You cannot cancel this order.");
                }

                if (order.Status != "Pending Payment")
                {
                    throw new InvalidOperationException("Only unpaid pending orders can be cancelled.");
                }

                order.Status = "Cancelled";
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
