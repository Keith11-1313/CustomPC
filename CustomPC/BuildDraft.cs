namespace CustomPC;

// A browsing draft is not an order. It holds IDs, so prices and stock are always read again.
public sealed class BuildDraft
{
    private readonly Dictionary<string, string> selections = new();
    public string SelectedId(string category) => selections.GetValueOrDefault(category, "");
    public void Select(string category, string? partId)
    {
        if (!AppStore.Categories.Contains(category)) throw new ArgumentException("Unknown category.");
        if (string.IsNullOrEmpty(partId)) selections.Remove(category);
        else selections[category] = partId;
    }
    public void Clear() => selections.Clear();
}
