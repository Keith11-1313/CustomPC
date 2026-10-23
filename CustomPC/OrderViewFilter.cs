namespace CustomPC;

// Drill-downs filter the already permission-checked order list; they never grant access.
public sealed record OrderViewFilter(DateTime? FirstDate = null, DateTime? LastDate = null,
    bool PaidDate = false, string? Status = null, string? PartId = null, bool WorkQueue = false)
{
    public string Description => WorkQueue ? "Open orders" + (Status == null ? "" : ": " + Status)
        : (PaidDate ? "Payment date" : "Created date") + $": {FirstDate:MMM d, yyyy} – {LastDate:MMM d, yyyy}";
    public bool Matches(CustomerOrder order, AppStore store)
    {
        if (Status != null && order.Status != Status) return false;
        if (WorkQueue && !AppStore.Statuses.Take(5).Contains(order.Status)) return false;
        if (PartId != null && !order.Lines.Any(line => line.PartId == PartId)) return false;
        IEnumerable<DateTime> dates = PaidDate
            ? store.Payments.Where(payment => payment.OrderId == order.Id).Select(payment => payment.PaidUtc.ToLocalTime().Date)
            : new[] { order.CreatedUtc.ToLocalTime().Date };
        return dates.Any(date => (!FirstDate.HasValue || date >= FirstDate.Value.Date)
            && (!LastDate.HasValue || date <= LastDate.Value.Date));
    }
}
