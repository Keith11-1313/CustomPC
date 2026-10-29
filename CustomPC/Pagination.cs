using System.Data;
using System.Runtime.CompilerServices;

namespace CustomPC;

internal sealed class PaginationBar : UserControl
{
    private readonly Button previous = new() { Name = "previousPage", Text = "Previous", BackColor = Color.White };
    private readonly Button next = new() { Name = "nextPage", Text = "Next", BackColor = Color.White };
    private readonly Label summary = new() { TextAlign = ContentAlignment.MiddleLeft, ForeColor = StoreTheme.Muted };
    private readonly List<Button> numbers = new();
    private int total;
    private bool fitting;
    internal int PageIndex { get; private set; }
    internal int PageSize { get; }
    internal event Action? PageChanged;
    internal PaginationBar(int pageSize)
    {
        PageSize = pageSize; Dock = DockStyle.Fill;
        Controls.AddRange(new Control[] { summary, previous, next });
        for (int i = 0; i < 5; i++)
        {
            var number = new Button { Name = "pageNumber", BackColor = Color.White };
            number.Click += (_, _) => { if (number.Tag is int target) GoTo(target); };
            numbers.Add(number); Controls.Add(number);
        }
        previous.Click += (_, _) => GoTo(PageIndex - 1);
        next.Click += (_, _) => GoTo(PageIndex + 1);
        foreach (Control control in Controls) control.FontChanged += (_, _) => Fit();
        SizeChanged += (_, _) => Fit(); FontChanged += (_, _) => Fit();
        DpiChangedAfterParent += (_, _) => Fit(); Load += (_, _) => Fit();
        SetTotal(0);
    }
    internal void SetTotal(int count, bool reset = false)
    {
        total = count;
        PageIndex = reset ? 0 : Math.Clamp(PageIndex, 0, Math.Max(0, (count - 1) / PageSize));
        previous.Enabled = PageIndex > 0;
        next.Enabled = (PageIndex + 1) * PageSize < count;
        Fit();
    }
    private void GoTo(int target)
    {
        target = Math.Clamp(target, 0, Math.Max(0, (total - 1) / PageSize));
        if (target == PageIndex) return;
        PageIndex = target; SetTotal(total); PageChanged?.Invoke();
    }
    private void Fit()
    {
        if (fitting) return;
        fitting = true;
        try
        {
            int Px(int value) => (int)(value * DeviceDpi / 96F);
            int gap = Px(8), height = Px(36);
            // Measure the actual button fonts after theme application, including icon space.
            int ButtonWidth(Button button) => Math.Max(Px(104), TextRenderer.MeasureText(button.Text, button.Font).Width + Px(76));
            int previousWidth = ButtonWidth(previous), nextWidth = ButtonWidth(next);
            int pages = Math.Max(1, (total + PageSize - 1) / PageSize);
            int numberWidth = Math.Max(Px(36), TextRenderer.MeasureText(pages.ToString(), numbers[0].Font).Width + Px(20));
            int slots = Math.Min(pages, Math.Clamp((Width - previousWidth - nextWidth - Px(160) - gap * 2) / (numberWidth + gap), 1, 5));
            int start = Math.Clamp(PageIndex - slots / 2, 0, Math.Max(0, pages - slots));
            int x = Math.Max(0, Width - nextWidth - previousWidth - slots * (numberWidth + gap) - gap);
            int summaryWidth = Math.Max(0, x - gap);
            string range = total == 0 ? "No results" : $"{PageIndex * PageSize + 1:N0}–{Math.Min(total, (PageIndex + 1) * PageSize):N0} of {total:N0}";
            summary.Text = range;
            summary.SetBounds(0, 0, summaryWidth, height);
            previous.SetBounds(x, 0, previousWidth, height); x += previousWidth + gap;
            for (int i = 0; i < numbers.Count; i++)
            {
                var number = numbers[i]; number.Visible = i < slots;
                if (i >= slots) continue;
                int index = start + i;
                number.Text = (index + 1).ToString(); number.Tag = index;
                number.AccessibleName = $"Page {index + 1} of {pages}";
                number.AccessibleDescription = index == PageIndex ? "Current page" : "Go to page";
                number.BackColor = index == PageIndex ? StoreTheme.Action : Color.White;
                number.SetBounds(x, 0, numberWidth, height); number.Invalidate(); x += numberWidth + gap;
            }
            next.SetBounds(x, 0, nextWidth, height);
        }
        finally { fitting = false; }
    }
}
internal sealed class TablePagination
{
    private static readonly ConditionalWeakTable<DataGridView, TablePagination> instances = new();
    internal static TablePagination? For(DataGridView grid) => instances.TryGetValue(grid, out var pager) ? pager : null;
    private readonly DataGridView grid;
    private readonly PaginationBar bar;
    internal DataTable? Source { get; private set; }
    private string identity = "";
    internal static PaginationBar Attach(DataGridView grid)
    {
        if (For(grid) is TablePagination existing) return existing.bar;
        var pager = new TablePagination(grid); instances.Add(grid, pager);
        if (grid.DataSource is DataTable table) pager.Bind(table);
        return pager.bar;
    }
    private TablePagination(DataGridView grid)
    {
        this.grid = grid; bar = new PaginationBar(20); bar.PageChanged += Render;
        grid.ColumnHeaderMouseClick += (_, e) =>
        {
            if (Source == null || e.ColumnIndex < 0) return;
            var column = grid.Columns[e.ColumnIndex];
            if (column is DataGridViewImageColumn || !Source.Columns.Contains(column.DataPropertyName)) return;
            string field = "[" + column.DataPropertyName.Replace("]", "\\]") + "]";
            Source.DefaultView.Sort = field + (Source.DefaultView.Sort == field + " ASC" ? " DESC" : " ASC");
            bar.SetTotal(Source.Rows.Count, true); Render();
        };
    }
    internal void Bind(DataTable table)
    {
        string key = string.Join("|", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName)) + ":" +
            string.Join("|", table.Rows.Cast<DataRow>().Select(r => Convert.ToString(r[0])));
        string sort = Source?.DefaultView.Sort ?? "";
        Source = table;
        if (key == identity && sort.Length > 0) Source.DefaultView.Sort = sort;
        bar.SetTotal(table.Rows.Count, key != identity); identity = key; Render();
    }
    private void Render()
    {
        if (Source == null) return;
        var page = Source.Clone();
        foreach (DataRowView row in Source.DefaultView.Cast<DataRowView>().Skip(bar.PageIndex * bar.PageSize).Take(bar.PageSize)) page.ImportRow(row.Row);
        Ui.Bind(grid, page, false);
        foreach (DataGridViewColumn column in grid.Columns)
        {
            column.SortMode = DataGridViewColumnSortMode.Programmatic;
            if (column is DataGridViewImageColumn photo)
            {
                photo.ImageLayout = DataGridViewImageCellLayout.Zoom;
                photo.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                photo.MinimumWidth = photo.Width = (int)(86 * grid.DeviceDpi / 96F);
                photo.DefaultCellStyle.Padding = new Padding(6); photo.DefaultCellStyle.NullValue = null;
            }
        }
        if (page.Columns.Contains("Photo")) foreach (DataGridViewRow row in grid.Rows) row.Height = (int)(72 * grid.DeviceDpi / 96F);
    }
}
