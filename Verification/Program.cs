using CustomPC;
using System.Text.Json;
using Microsoft.Data.Sqlite;

internal static class Program
{
    private static int checks;
    private static void Check(bool condition, string label) { if (!condition) throw new Exception(label); checks++; Console.WriteLine("PASS " + label); }
    private static void Reject(Action action, string label)
    {
        try { action(); } catch (InvalidOperationException) { checks++; Console.WriteLine("PASS " + label); return; }
        throw new Exception("Expected rejection: " + label);
    }
    [STAThread]
    private static void Main()
    {
        var output = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.qa"));
        Directory.CreateDirectory(output);
        var database = Path.Combine(output, "test-" + Guid.NewGuid().ToString("N") + ".db");
        Environment.SetEnvironmentVariable("CUSTOMPC_DATABASE_PATH", database);
        ApplicationConfiguration.Initialize();
        var s = AppStore.Current;
        Check(s.Users.Count == 3 && s.Parts.Count == 12, "SQLite schema and demo seed");
        Reject(() => s.Login("customer@custompc.local", "wrong"), "incorrect password");
        Reject(() => s.SavePart(new()), "guest cannot manage inventory");
        var selected = AppStore.Categories.Select(c => s.Parts.First(p => p.Category == c)).ToList();
        Check(s.CheckBuild(selected).Count == 0, "valid socket, RAM and PSU build");
        var mismatch = selected.ToList(); mismatch[1] = s.Parts.Single(x => x.Name == "B660M motherboard");
        Check(s.CheckBuild(mismatch).Any(x => x.Contains("sockets")), "CPU socket mismatch");
        mismatch = selected.ToList(); mismatch[2] = s.Parts.Single(x => x.MemoryType == "DDR5");
        Check(s.CheckBuild(mismatch).Any(x => x.Contains("memory types")), "RAM type mismatch");
        var psu = selected.Single(x => x.Category == "Power Supply"); var originalWatts = psu.Watts; psu.Watts = 100;
        Check(s.CheckBuild(selected).Any(x => x.Contains("Power supply")), "insufficient PSU"); psu.Watts = originalWatts;
        Reject(() => s.CreateOrder(selected.Select(x => x.Id).ToList(), s.Users.Last().Id), "guest cannot order");
        s.Login("customer@custompc.local", "CustomPC123!");
        var customer = s.Session!.Id;
        Reject(() => s.SaveSupplier(new() { Name = "Denied", Contact = "Denied" }), "customer cannot manage suppliers");
        Reject(() => s.SaveUser(new() { Name = "Denied", Email = "denied@example.com" }, "Password123"), "customer cannot create elevated accounts");
        var ids = selected.Select(x => x.Id).ToList();
        var order = s.CreateOrder(ids, customer);
        Check(order.Lines.Count == 7 && Math.Abs((order.DeadlineUtc - order.CreatedUtc).TotalHours - 24) < 0.01, "order receipt snapshot and 24-hour deadline");
        Check(s.Reserved(ids[0]) == 1 && s.Available(s.Parts.Single(x => x.Id == ids[0])) == 9, "inventory reservation");
        Reject(() => s.ConfirmPayment(order.Id), "customer cannot confirm payment");
        s.SaveUser(new() { Name = "Other Customer", Email = "other@example.com" }, "Password123", true);
        s.Login("other@example.com", "Password123"); Check(s.VisibleOrders().Count == 0, "customer order isolation");
        Reject(() => s.CancelOrder(order.Id), "customer cannot cancel someone else's order");
        s.Login("customer@custompc.local", "CustomPC123!"); s.CancelOrder(order.Id);
        Check(s.Reserved(ids[0]) == 0, "cancellation releases stock");
        order = s.CreateOrder(ids, customer);
        // Move only the test order deadline into the past to exercise real expiry.
        using (var db = new SqliteConnection("Data Source=" + database))
        {
            db.Open(); using var command = db.CreateCommand(); var expired = s.Orders.Single(x => x.Id == order.Id); expired.DeadlineUtc = DateTime.UtcNow.AddSeconds(-1);
            command.CommandText = "UPDATE Orders SET Data=$data WHERE Id=$id";
            command.Parameters.AddWithValue("$data", JsonSerializer.Serialize(expired)); command.Parameters.AddWithValue("$id", order.Id); command.ExecuteNonQuery();
        }
        s.Refresh(); Check(s.Orders.Single(x => x.Id == order.Id).Status == "Cancelled" && s.Reserved(ids[0]) == 0, "automatic expiry releases stock");
        order = s.CreateOrder(ids, customer); s.Login("staff@custompc.local", "CustomPC123!");
        Reject(() => s.SaveRule(new()), "staff cannot change compatibility rules");
        Reject(() => s.AdvanceOrder(order.Id), "unpaid order cannot advance");
        s.ConfirmPayment(order.Id); Reject(() => s.ConfirmPayment(order.Id), "duplicate payment rejected");
        Check(s.Payments.Single().Amount == order.Total && s.Reserved(ids[0]) == 1, "confirmed payment retains reservation");
        for (int i = 0; i < 4; i++) s.AdvanceOrder(order.Id);
        Check(s.Orders.Single(x => x.Id == order.Id).Status == "Completed" && s.Parts.Single(x => x.Id == ids[0]).Stock == 9 && s.Reserved(ids[0]) == 0, "paid-to-pickup lifecycle deducts stock once");
        Reject(() => s.AdvanceOrder(order.Id), "completed order cannot deduct stock twice");
        s.AdjustStock(ids[0], 2, "Test restock");
        Check(s.Parts.Single(x => x.Id == ids[0]).Stock == 11 && s.Movements.Any(x => x.Reason == "Test restock"), "staff restock and movement audit");
        Reject(() => s.AdjustStock(ids[0], -99, "Invalid correction"), "negative stock rejected");
        s.Login("admin@custompc.local", "CustomPC123!");
        var clone = JsonSerializer.Deserialize<UserAccount>(JsonSerializer.Serialize(s.Session))!; clone.Archived = true;
        Reject(() => s.SaveUser(clone, ""), "admin cannot archive current account");
        var newPart = new Part { Name = "Test part", Category = "Case", Brand = "Test", Price = 123, Stock = 2 }; s.SavePart(newPart);
        newPart.Archived = true; s.SavePart(newPart); Check(s.Parts.Single(x => x.Id == newPart.Id).Archived, "part edit and archive");
        var supplier = new Supplier { Name = "Test supplier", Contact = "Test contact" }; s.SaveSupplier(supplier); supplier.Archived = true; s.SaveSupplier(supplier);
        Check(s.Suppliers.Single(x => x.Id == supplier.Id).Archived, "supplier edit and archive");
        var reloaded = new AppStore(database); Check(reloaded.Orders.Count == 3 && reloaded.Payments.Count == 1 && reloaded.Parts.Count == 13, "records persist after reopening database");
        var scarce = JsonSerializer.Deserialize<Part>(JsonSerializer.Serialize(s.Parts.Single(x => x.Id == ids[0])))!;
        scarce.Stock = 1; s.SavePart(scarce);
        s.Login("customer@custompc.local", "CustomPC123!");
        var lastUnit = s.CreateOrder(ids, customer);
        Reject(() => s.CreateOrder(ids, customer), "last available unit cannot be double-reserved");
        s.Login("admin@custompc.local", "CustomPC123!"); scarce.Stock = 0;
        Reject(() => s.SavePart(scarce), "stock edit cannot erase reserved stock");
        s.CancelOrder(lastUnit.Id);
        Check(s.Available(s.Parts.Single(x => x.Id == ids[0])) == 1, "last-unit cancellation restores availability");
        var rule = JsonSerializer.Deserialize<CompatibilityRule>(JsonSerializer.Serialize(s.Rules.Single(x => x.Kind == "Memory")))!;
        rule.Enabled = false; s.SaveRule(rule);
        selected = ids.Select(id => s.Parts.Single(x => x.Id == id)).ToList();
        selected[2] = s.Parts.Single(x => x.MemoryType == "DDR5");
        Check(!s.CheckBuild(selected).Any(x => x.Contains("memory types")), "admin rule changes affect build checks");
        rule.Enabled = true; s.SaveRule(rule);
        Render(new LoginForm(), "login"); Render(new DashboardForm(false), "dashboard"); Render(new CatalogForm(), "catalog"); Render(new BuilderForm(), "builder");
        Render(new OrdersForm(), "orders"); Render(new ReceiptForm(order.Id), "receipt"); Render(new InventoryForm(), "inventory"); Render(new PartEditForm(), "part-edit");
        Render(new UsersForm(), "users"); Render(new UserEditForm(), "user-edit"); Render(new SuppliersForm(), "suppliers"); Render(new SupplierEditForm(), "supplier-edit");
        Render(new CompatibilityForm(), "compatibility"); Render(new RuleEditForm(), "rule-edit"); Render(new ReportsForm(), "reports"); Render(new RegisterForm(), "register"); Render(new StockAdjustmentForm(ids[0]), "stock-adjustment");
        Console.WriteLine($"{checks} checks passed. Rendered forms: {output}");
        void Render(Form form, string name)
        {
            using (form)
            {
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(-20000, -20000);
                form.ShowInTaskbar = false;
                form.Show();
                Application.DoEvents();
                using var bitmap = new Bitmap(form.Width, form.Height); form.DrawToBitmap(bitmap, new Rectangle(0, 0, form.Width, form.Height));
                bitmap.Save(Path.Combine(output, name + ".png"));
                Check(form.Controls.Count > 2, name + " form creates and renders");
            }
        }
    }
}
