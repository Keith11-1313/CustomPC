namespace CustomPC;

public partial class StoreToolsForm : StoreWindow
{
    private Panel? backupViewport;
    public StoreToolsForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;
        if (Ui.IsDesign) return;
        backupViewport = new Panel { AutoScroll = true, Dock = DockStyle.Fill };
        contentLayout.Parent = backupViewport;
        contentLayout.Dock = DockStyle.Top;
        var workspace = new WorkspacePage { Name = "workspace" };
        workspace.Bind(this, heading, back, "Store activity, backups and recovery.", backupViewport,
            null, Array.Empty<Button>());
        contentLayout.Padding = new Padding(0);
        contentLayout.RowStyles[0].Height = 0;
        BuildBackupActions();
        SizeChanged += (_, _) => FitBackupCards();
        backupViewport.SizeChanged += (_, _) => FitBackupCards();
        FitBackupCards();
        AppStore.Current.Refresh();
        if (!AppStore.Current.IsAdmin) throw new InvalidOperationException("Administrator access is required.");




        backup.Click += BackupButton_Click;
        restore.Click += RestoreButton_Click;
        refresh.Visible = false;
        _ = new AutomaticViewRefresh(this, () => RefreshButton_Click(this, EventArgs.Empty));
        grid.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            string details = grid.Rows[e.RowIndex].Cells["Details"].Value?.ToString() ?? "";
            Ui.Message(this, details, "Activity details");
        };
        LoadHistory();
    }

    private void BuildBackupActions()
    {
        saveCard.Controls.Clear(); restoreCard.Controls.Clear();
        saveDescription.Visible = restoreDescription.Visible = false;
        saveTitle.Text = "Create a backup";
        restoreTitle.Text = "Restore from backup";
        foreach (var card in new[] { saveCard, restoreCard })
        {
            card.BackColor = StoreTheme.Background;
            card.Padding = new Padding(16);
            card.RowStyles.Clear(); card.RowCount = 3;
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        }
        saveCard.ColumnCount = 1;
        saveCard.Controls.Add(saveTitle, 0, 0);
        saveCard.Controls.Add(backup, 0, 2);
        backup.Dock = DockStyle.None; backup.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        backup.BackColor = StoreTheme.Action;
        restoreCard.ColumnCount = 1;
        restoreCard.ColumnStyles.Clear(); restoreCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        restoreCard.Controls.Add(restoreTitle, 0, 0);
        restoreCard.Controls.Add(restore, 0, 2);
        restore.Dock = DockStyle.None; restore.Anchor = AnchorStyles.Left | AnchorStyles.Top;
        adminPasswordLabel.Visible = adminPassword.Visible = false;
    }

    private void FitBackupCards()
    {
        float scale = DeviceDpi / 96F;
        int Px(int value) => (int)(value * scale);
        bool stacked = ClientSize.Width < Px(1000);
        backupLayout.SuspendLayout();
        backupLayout.ColumnCount = stacked ? 1 : 2;
        backupLayout.RowCount = stacked ? 2 : 1;
        backupLayout.ColumnStyles.Clear(); backupLayout.RowStyles.Clear();
        for (int i = 0; i < backupLayout.ColumnCount; i++) backupLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / backupLayout.ColumnCount));
        for (int i = 0; i < backupLayout.RowCount; i++) backupLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / backupLayout.RowCount));
        backupLayout.SetCellPosition(restoreCard, new TableLayoutPanelCellPosition(stacked ? 0 : 1, stacked ? 1 : 0));
        foreach (var card in new[] { saveCard, restoreCard })
        {
            card.Padding = new Padding(Px(16));
            card.RowStyles[0].Height = Px(36); card.RowStyles[1].Height = Px(28); card.RowStyles[2].Height = Px(46);
        }
        saveCard.Margin = new Padding(0, 0, stacked ? 0 : Px(8), stacked ? Px(12) : 0);
        restoreCard.Margin = new Padding(stacked ? 0 : Px(8), 0, 0, 0);
        backup.Size = new Size(TextRenderer.MeasureText(backup.Text, backup.Font).Width + Px(64), Px(44));
        restore.Size = new Size(TextRenderer.MeasureText(restore.Text, restore.Font).Width + Px(64), Px(44));
        contentLayout.RowStyles[1].Height = Px(stacked ? 320 : 156);
        contentLayout.AutoScroll = false;
        contentLayout.AutoScrollMinSize = Size.Empty;
        contentLayout.Height = Math.Max(backupViewport?.ClientSize.Height ?? 0, Px(stacked ? 680 : 440));
        contentLayout.Width = Math.Max(1, (backupViewport?.ClientSize.Width ?? ClientSize.Width) - SystemInformation.VerticalScrollBarWidth);
        saveCard.ColumnStyles.Clear(); saveCard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        backupLayout.ResumeLayout(true);
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
            health.Visible = defaults > 0;
            contentLayout.RowStyles[2].Height = defaults > 0 ? 64 * DeviceDpi / 96F : 0;
            health.Text = defaults > 0
                ? $"Action required: {defaults} legacy demo accounts still use the public password. Change their passwords or archive them in User accounts. Review demonstration stock before store use."
                : "Account check passed. Keep a backup on an external drive.";
        }
        catch
        {
            health.Visible = true;
            contentLayout.RowStyles[2].Height = 64 * DeviceDpi / 96F;
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
                Title = "Save a backup",
                Filter = "Complete CustomPC backup (*.zip)|*.zip", FileName = "CustomPC-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".zip",
                InitialDirectory = AppStore.Current.BackupFolder
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            AppStore.Current.CreateBackup(dialog.FileName);
            health.Text = "Verified backup saved: " + dialog.FileName;
            Ui.Message(this, "Backup saved successfully.\n\nYour records and product photos are included.\n\nSaved to:\n" + dialog.FileName, "Backup saved");
        }
        catch (Exception exception) { Ui.ShowError(this, exception); }
    }

    private void RestoreButton_Click(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Refresh();
            if (!AppStore.Current.IsAdmin) throw new InvalidOperationException("Administrator access is required.");
            using RestorePasswordForm passwordDialog = new();
            if (passwordDialog.ShowDialog(this) != DialogResult.OK) return;
            using OpenFileDialog dialog = new OpenFileDialog { Title = "Choose a backup to restore", Filter = "CustomPC backup (*.zip;*.db)|*.zip;*.db", InitialDirectory = AppStore.Current.BackupFolder };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            string replacedData = Path.GetExtension(dialog.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase)
                ? "current records and product photos" : "current database records";
            if (!Ui.Confirm(this, "Are you sure you want to restore this backup?\n\n" + dialog.FileName + "\n\nThis replaces your " + replacedData + ". A safety backup is saved first.\n\nAfter restoration, CustomPC will close. Open it again manually and sign in with an account from the restored backup.", "Restore this backup?", "Restore backup")) return;
            AppStore.Current.RestoreBackup(dialog.FileName, passwordDialog.Password);
            Ui.Message(this, "Backup restored successfully.\n\nCustomPC will now close. Open CustomPC again manually, then sign in with an account from the restored backup.", "Restore complete");
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
