namespace CustomPC;

// Refresh visible read-only views when saved data changes, without interrupting editors.
internal sealed class AutomaticViewRefresh : IDisposable
{
    private readonly Form owner;
    private readonly Action reload;
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 2000 };
    private string stamp = "";
    public AutomaticViewRefresh(Form owner, Action reload)
    {
        this.owner = owner; this.reload = reload;
        timer.Tick += Check;
        owner.VisibleChanged += Check;
        owner.Activated += Check;
        owner.Disposed += (_, _) => Dispose();
        timer.Start();
    }
    private void Check(object? sender, EventArgs e)
    {
        if (!owner.Visible || owner.IsDisposed || Form.ActiveForm is { Modal: true }) return;
        string path = AppStore.Current.DatabasePath;
        string current = string.Join("|", new[] { path, path + "-wal" }.Select(file =>
            File.Exists(file) ? File.GetLastWriteTimeUtc(file).Ticks + ":" + new FileInfo(file).Length : "missing"));
        if (current == stamp) return;
        stamp = current;
        try
        {
            var selections = Grids(owner).Where(grid => grid.CurrentRow != null && grid.Columns.Contains("Id"))
                .Select(grid => (Grid: grid, Id: grid.CurrentRow!.Cells["Id"].Value?.ToString(),
                    Scroll: grid.FirstDisplayedScrollingRowIndex)).ToArray();
            reload();
            foreach (var selection in selections)
            {
                var grid = selection.Grid;
                if (grid.IsDisposed) continue;
                foreach (DataGridViewRow row in grid.Rows)
                    if (row.Cells["Id"].Value?.ToString() == selection.Id)
                    {
                        grid.ClearSelection(); row.Selected = true;
                        var cell = row.Cells.Cast<DataGridViewCell>().FirstOrDefault(cell => cell.Visible);
                        if (cell != null) grid.CurrentCell = cell;
                        break;
                    }
                if (selection.Scroll >= 0 && selection.Scroll < grid.Rows.Count)
                    grid.FirstDisplayedScrollingRowIndex = selection.Scroll;
            }
        }
        catch (Exception error) { AppLog.Write(error); stamp = ""; }
    }
    private static IEnumerable<DataGridView> Grids(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            if (child is DataGridView grid) yield return grid;
            foreach (var nested in Grids(child)) yield return nested;
        }
    }
    public void Dispose() { timer.Dispose(); owner.VisibleChanged -= Check; owner.Activated -= Check; }
}
