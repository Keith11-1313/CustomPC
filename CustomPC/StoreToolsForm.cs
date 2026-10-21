namespace CustomPC;

public partial class StoreToolsForm : StoreWindow
{
    public StoreToolsForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;
        if (Ui.IsDesign) return;
        AppStore.Current.Refresh();
        if (!AppStore.Current.IsAdmin) throw new InvalidOperationException("Administrator access is required.");




        backup.Click += BackupButton_Click;
        restore.Click += RestoreButton_Click;
        refresh.Click += RefreshButton_Click;
        grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            string details = grid.Rows[e.RowIndex].Cells["Details"].Value?.ToString() ?? "";
            Ui.Message(this, details, "Activity details");
        };
        LoadHistory();
    }

    // Render the PNG icons; keep the headings as normal editable labels.
    private void BackupHeading_Paint(object? sender, PaintEventArgs e)
    {
        if (sender is not Label label) return;
        int size = (int)(20 * label.DeviceDpi / 96F);
        IconAssets.Paint(e.Graphics, label == saveTitle ? "download" : "rotate-ccw",
            new Rectangle(0, Math.Max(0, (label.Height - size) / 2), size, size), label.ForeColor);
    }

    private void LoadHistory()
    {
        // Recheck access, including an account changed while this screen was open.
        try
        {
            Ui.Bind(grid, AppStore.Current.ActivityHistory());
            if (grid.Columns["Details"] is DataGridViewColumn detailsColumn)
            {
                detailsColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                detailsColumn.FillWeight = 400;
            }
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            int defaults = AppStore.Current.DefaultPasswordAccounts();
            health.Text = defaults > 0
                ? $"Action required: {defaults} legacy demo accounts still use the public password. Change their passwords or archive them in User accounts. Review demonstration stock before store use."
                : "Account check passed. Keep a backup on an external drive.";
        }
        catch
        {
            grid.DataSource = null;
            backup.Enabled = restore.Enabled = false;
            health.Text = "Administrator access is required. Close this screen and sign in again.";
            throw;
        }
    }

    private void BackupButton_Click(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            if (!AppStore.Current.IsAdmin) throw new InvalidOperationException("Administrator access is required.");
            using SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Complete CustomPC backup (*.zip)|*.zip", FileName = "CustomPC-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".zip",
                InitialDirectory = AppStore.Current.BackupFolder
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            AppStore.Current.CreateBackup(dialog.FileName);
            health.Text = "Verified backup saved: " + dialog.FileName;
        }
        catch (Exception exception) { Ui.ShowError(this, exception); }
    }

    private void RestoreButton_Click(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            if (!AppStore.Current.IsAdmin) throw new InvalidOperationException("Administrator access is required.");
            using OpenFileDialog dialog = new OpenFileDialog { Filter = "CustomPC backup (*.zip;*.db)|*.zip;*.db", InitialDirectory = AppStore.Current.BackupFolder };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (!Ui.Confirm(this, "Restore " + dialog.FileName + "?\n\nThis replaces current records with the backup. A safety backup of current records is saved first. The app will close; reopen it and use an account from the restored backup.")) return;
            AppStore.Current.RestoreBackup(dialog.FileName, adminPassword.Text);
            Ui.Message(this, "Backup restored. CustomPC will close. Reopen it to sign in.", "CustomPC");
            Application.Exit();
        }
        catch (Exception exception) { Ui.ShowError(this, exception); }
        finally { adminPassword.Clear(); }
    }

    private void RefreshButton_Click(object? sender, EventArgs e)
    {
        try { LoadHistory(); }
        catch (Exception exception) { Ui.ShowError(this, exception); }
    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
