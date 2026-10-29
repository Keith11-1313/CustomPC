namespace CustomPC;

// One consistent modal for confirmations, messages and product specifications.
// File selection and printing remain Windows dialogs.
public partial class StoreDialog : StoreWindow
{
    public StoreDialog()
    {
        InitializeComponent();
        body.UseMnemonic = false;
        bodyPanel.SizeChanged += (_, _) => body.MaximumSize = new Size(Math.Max(1, bodyPanel.Width - 32), 0);
    }
    internal void AddPolicyLink(LinkLabel link)
    {
        layout.SuspendLayout();
        layout.Controls.Remove(actions);
        layout.RowCount = 4;
        layout.RowStyles[2].Height = 44;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
        layout.Controls.Add(link, 0, 2);
        layout.Controls.Add(actions, 0, 3);
        ClientSize = new Size(ClientSize.Width, ClientSize.Height + 44);
        layout.ResumeLayout(true);
    }
    public StoreDialog(string title, string message, bool confirmation = false, string? confirmText = null) : this()
    {
        Text = "CustomPC | " + title;
        heading.Text = title;
        body.Text = message;
        int lines = message.Split('\n').Sum(line => Math.Max(1, (int)Math.Ceiling(line.Length / 65.0)));
        ClientSize = new Size(600, Math.Clamp(220 + lines * 24, 270, 570));
        cancel.Visible = confirmation;
        accept.Text = confirmation ? confirmText ?? "Confirm" : "OK";
        accept.DialogResult = confirmation ? DialogResult.Yes : DialogResult.OK;
        cancel.DialogResult = DialogResult.No;
        AcceptButton = confirmation ? cancel : accept;
        CancelButton = confirmation ? cancel : accept;
        if (confirmation) ActiveControl = cancel;
        Shown += (_, _) =>
        {
            foreach (Button button in new[] { accept, cancel })
                button.Width = Math.Max((int)(116 * DeviceDpi / 96F), TextRenderer.MeasureText(button.Text, button.Font).Width + (int)(64 * DeviceDpi / 96F));
            layout.RowStyles[0].Height = Math.Max(layout.RowStyles[0].Height,
                TextRenderer.MeasureText(heading.Text, heading.Font, new Size(Math.Max(1, heading.Width - heading.Padding.Horizontal), int.MaxValue),
                    TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + 16);
        };
    }
}
