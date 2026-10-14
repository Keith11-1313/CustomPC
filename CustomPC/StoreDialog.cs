namespace CustomPC;

// One consistent modal for confirmations, messages and product specifications.
// File selection and printing remain Windows dialogs.
public partial class StoreDialog : StoreWindow
{
    public StoreDialog()
    {
        InitializeComponent();
        bodyPanel.SizeChanged += (_, _) => body.MaximumSize = new Size(Math.Max(1, bodyPanel.Width - 32), 0);
    }
    public StoreDialog(string title, string message, bool confirmation = false) : this()
    {
        Text = "CustomPC | " + title;
        heading.Text = title;
        body.Text = message;
        int lines = message.Split('\n').Sum(line => Math.Max(1, (int)Math.Ceiling(line.Length / 65.0)));
        ClientSize = new Size(600, Math.Clamp(220 + lines * 24, 270, 570));
        cancel.Visible = confirmation;
        accept.Text = confirmation ? "Confirm" : "OK";
        accept.DialogResult = confirmation ? DialogResult.Yes : DialogResult.OK;
        cancel.DialogResult = DialogResult.No;
        AcceptButton = accept;
        CancelButton = confirmation ? cancel : accept;
    }
}
