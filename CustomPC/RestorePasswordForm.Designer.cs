namespace CustomPC;

partial class RestorePasswordForm
{
    private StoreTextField password = null!;
    private Label failure = null!;
    private Button confirm = null!;
    protected override void Dispose(bool disposing)
    {
        if (disposing) password?.Clear();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        SuspendLayout();
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(24) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.Controls.Add(new Label { Text = "Confirm your password", AutoSize = true, Font = AppFonts.Heading(21), Margin = new Padding(0, 0, 0, 20) }, 0, 0);
        layout.Controls.Add(new Label { Text = "Your password", AutoSize = true, Margin = new Padding(0, 0, 0, 8) }, 0, 1);
        password = new StoreTextField { Name = "password", Dock = DockStyle.Fill, UseSystemPasswordChar = true, ShowPasswordToggle = true, MaxLength = 128, AccessibleName = "Your password", Margin = Padding.Empty };
        layout.Controls.Add(password, 0, 2);
        failure = new Label { Dock = DockStyle.Fill, ForeColor = Color.Firebrick, Margin = new Padding(0, 8, 0, 8), UseMnemonic = false };
        layout.Controls.Add(failure, 0, 3);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Margin = Padding.Empty };
        confirm = new Button { Text = "Choose backup", BackColor = StoreTheme.Action, Width = 170, Height = 44, Enabled = false, Margin = Padding.Empty };
        var cancel = new Button { Text = "Cancel", BackColor = Color.White, Width = 120, Height = 44, DialogResult = DialogResult.Cancel, Margin = new Padding(0, 0, 12, 0) };
        actions.Controls.Add(confirm); actions.Controls.Add(cancel); layout.Controls.Add(actions, 0, 4);
        Controls.Add(layout);
        AcceptButton = confirm; CancelButton = cancel;
        ClientSize = new Size(480, 310); MinimumSize = new Size(496, 349);
        MinimizeBox = MaximizeBox = false; ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "CustomPC | Confirm your password";
        AutoScaleMode = AutoScaleMode.Dpi;
        ResumeLayout(true);
    }
}
