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
        storeName.Text = AppStore.Current.StoreName;
        contact.Text = AppStore.Current.StoreContact;
        databaseLocation.Text = AppStore.Current.DatabasePath;
        save.Click += SaveButton_Click;
        backup.Click += BackupButton_Click;
        restore.Click += RestoreButton_Click;
        refresh.Click += RefreshButton_Click;
        LoadHistory();
    }

    private void LoadHistory()
    {
        // Recheck access, including an account changed while this screen was open.
        try
        {
            Ui.Bind(grid, AppStore.Current.ActivityHistory());
            int defaults = AppStore.Current.DefaultPasswordAccounts();
            health.Text = defaults > 0
                ? $"Action required: {defaults} legacy demo accounts still use the public password. Change their passwords or archive them in User accounts. Review demonstration stock before store use."
                : "Account check passed. Keep a backup on an external drive, and verify prices, stock and specifications before taking orders.";
        }
        catch
        {
            grid.DataSource = null;
            save.Enabled = backup.Enabled = restore.Enabled = false;
            health.Text = "Administrator access is required. Close this screen and sign in again.";
            throw;
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.SaveStoreDetails(storeName.Text, contact.Text);
            LoadHistory();
            MessageBox.Show(this, "Store details saved. New receipt views use this header.", "CustomPC");
        }
        catch (Exception exception) { Ui.ShowError(this, exception); }
    }

    private void BackupButton_Click(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            if (!AppStore.Current.IsAdmin) throw new InvalidOperationException("Administrator access is required.");
            using SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "CustomPC backup (*.db)|*.db", FileName = "CustomPC-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".db",
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
            using OpenFileDialog dialog = new OpenFileDialog { Filter = "CustomPC backup (*.db)|*.db", InitialDirectory = AppStore.Current.BackupFolder };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            if (!Ui.Confirm(this, "Restore " + dialog.FileName + "?\n\nThis replaces current records with the backup. A safety backup of current records is saved first. The app will close; reopen it and use an account from the restored backup.")) return;
            AppStore.Current.RestoreBackup(dialog.FileName, adminPassword.Text);
            MessageBox.Show(this, "Backup restored. CustomPC will close. Reopen it to sign in.", "CustomPC");
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
