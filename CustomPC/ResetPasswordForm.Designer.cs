namespace CustomPC;

partial class ResetPasswordForm
{
    private Label heading = null!, passwordLabel = null!, confirmLabel = null!, accountSummary = null!;
    private StoreTextField password = null!, confirmPassword = null!;
    private StoreCheckBox identityVerified = null!;
    private Button save = null!, back = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing) { password?.Clear(); confirmPassword?.Clear(); }
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        SuspendLayout();
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3,
            Padding = new Padding(24), BackColor = Color.White };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        heading = new Label { Text = "Reset password", Font = new Font("Segoe UI", 22, FontStyle.Bold),
            AutoSize = true, Margin = new Padding(0, 0, 0, 20) };
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Margin = Padding.Empty };
        var fields = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 7,
            Padding = new Padding(0, 0, 8, 0), Margin = Padding.Empty };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 7; i++) fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        accountSummary = new Label { Name = "accountSummary", Text = "Selected account", UseMnemonic = false,
            AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(14), BackColor = StoreTheme.Selection,
            ForeColor = StoreTheme.Ink, Margin = new Padding(0, 0, 0, 20) };
        passwordLabel = new Label { Text = "New password", Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            AutoSize = true, Margin = new Padding(0, 0, 0, 8) };
        password = new StoreTextField { Name = "newPassword", Dock = DockStyle.Top, Height = 44,
            UseSystemPasswordChar = true, ShowPasswordToggle = true, MaxLength = 128,
            AccessibleName = "New password", Margin = Padding.Empty };
        var requirements = new Label { Text = PasswordGuidance.Requirements,
            AutoSize = true, ForeColor = StoreTheme.Muted, Margin = new Padding(0, 8, 0, 18) };
        confirmLabel = new Label { Text = "Confirm new password", Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            AutoSize = true, Margin = new Padding(0, 0, 0, 8) };
        confirmPassword = new StoreTextField { Name = "confirmPassword", Dock = DockStyle.Top, Height = 44,
            UseSystemPasswordChar = true, ShowPasswordToggle = true, MaxLength = 128,
            AccessibleName = "Confirm new password", Margin = new Padding(0, 0, 0, 14) };
        identityVerified = new StoreCheckBox { Name = "identityVerified", Text = "Account holder's identity verified", Dock = DockStyle.Top,
            Height = 42, Margin = Padding.Empty };
        fields.Controls.Add(accountSummary, 0, 0); fields.Controls.Add(passwordLabel, 0, 1);
        fields.Controls.Add(password, 0, 2); fields.Controls.Add(requirements, 0, 3);
        fields.Controls.Add(confirmLabel, 0, 4); fields.Controls.Add(confirmPassword, 0, 5);
        fields.Controls.Add(identityVerified, 0, 6); scroll.Controls.Add(fields);
        void FitText()
        {
            int width = Math.Max(100, scroll.ClientSize.Width - fields.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth);
            fields.Width = Math.Max(100, scroll.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
            accountSummary.MaximumSize = requirements.MaximumSize = new Size(width, 0);
            heading.MaximumSize = new Size(Math.Max(100, layout.ClientSize.Width - layout.Padding.Horizontal), 0);
        }
        scroll.SizeChanged += (_, _) => FitText();
        accountSummary.TextChanged += (_, _) => FitText();
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false, Padding = new Padding(0, 16, 0, 0), Margin = Padding.Empty };
        save = new Button { Name = "save", Text = "Reset password", BackColor = StoreTheme.Action,
            ForeColor = Color.White, Enabled = false, Size = new Size(176, 44), Margin = Padding.Empty };
        back = new Button { Text = "Cancel", BackColor = Color.White, Size = new Size(116, 44), Margin = new Padding(0, 0, 12, 0) };
        actions.Controls.Add(save); actions.Controls.Add(back);
        layout.Controls.Add(heading, 0, 0); layout.Controls.Add(scroll, 0, 1); layout.Controls.Add(actions, 0, 2);
        Controls.Add(layout);
        Text = "CustomPC | Reset password"; ClientSize = new Size(540, 570); MinimumSize = new Size(440, 480);
        AutoScaleMode = AutoScaleMode.Dpi; Font = new Font("Segoe UI", 10.5F);
        StartPosition = FormStartPosition.CenterParent; MinimizeBox = MaximizeBox = false; ShowInTaskbar = false;
        ResumeLayout(true);
    }
}
