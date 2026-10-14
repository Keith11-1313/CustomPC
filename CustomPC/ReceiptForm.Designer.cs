#nullable enable
namespace CustomPC;

partial class ReceiptForm
{
    private System.ComponentModel.IContainer? components = null;
    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        SuspendLayout();
        heading = new Label();
        heading.Name = "heading";
        heading.Location = new Point(32, 24);
        heading.Size = new Size(716, 40);
        heading.TabIndex = 0;
        heading.Text = "Order receipt";
        heading.UseMnemonic = false;
        heading.Font = new Font("Righteous", 22F, FontStyle.Regular);
        Controls.Add(heading);
        back = new Button();
        back.Name = "back";
        back.Location = new Point(768, 24);
        back.Size = new Size(100, 40);
        back.TabIndex = 5;
        back.Text = "Back";
        back.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        back.BackColor = Color.White;
        back.ForeColor = Color.FromArgb(32, 41, 35);
        back.FlatStyle = FlatStyle.Flat;
        back.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
        back.FlatAppearance.BorderColor = Color.FromArgb(112, 133, 107);
        back.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 243, 231);
        back.Font = new Font("Roboto", 10F, FontStyle.Bold);
        back.Cursor = Cursors.Hand;
        back.UseMnemonic = false;
        Controls.Add(back);
        receiptText = new StoreTextField();
        receiptText.Name = "receiptText";
        receiptText.Location = new Point(32, 98);
        receiptText.Size = new Size(836, 450);
        receiptText.TabIndex = 2;
        receiptText.Text = "CUSTOMPC\r\nORDER RECEIPT\r\n\r\nOrder details appear here after confirmation.";
        receiptText.Multiline = true; receiptText.ReadOnly = true; receiptText.ScrollBars = ScrollBars.Vertical; receiptText.Font = new Font("Consolas", 11F); receiptText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        Controls.Add(receiptText);
        print = new Button();
        print.Name = "print";
        print.Location = new Point(32, 578);
        print.Size = new Size(220, 40);
        print.TabIndex = 3;
        print.Text = "Print receipt";
        print.BackColor = Color.White;
        print.ForeColor = Color.FromArgb(32, 41, 35);
        print.FlatStyle = FlatStyle.Flat;
        print.FlatAppearance.BorderSize = 1;
        print.FlatAppearance.BorderColor = Color.FromArgb(112, 133, 107);
        print.FlatAppearance.MouseOverBackColor = Color.FromArgb(235, 243, 231);
        print.Font = new Font("Roboto", 10F, FontStyle.Bold);
        print.Cursor = Cursors.Hand;
        print.UseMnemonic = false;
        Controls.Add(print);
        save = new Button();
        save.Name = "save";
        save.Location = new Point(268, 578);
        save.Size = new Size(220, 40);
        save.TabIndex = 4;
        save.Text = "Save text receipt";
        save.BackColor = Color.FromArgb(84, 177, 55);
        save.ForeColor = Color.FromArgb(32, 41, 35);
        save.FlatStyle = FlatStyle.Flat;
        save.FlatAppearance.BorderSize = 0;
        save.FlatAppearance.BorderColor = Color.FromArgb(112, 133, 107);
        save.FlatAppearance.MouseOverBackColor = Color.FromArgb(123, 195, 92);
        save.Font = new Font("Roboto", 10F, FontStyle.Bold);
        save.Cursor = Cursors.Hand;
        save.UseMnemonic = false;
        Controls.Add(save);
        headerSurface = new Panel();
        headerSurface.Name = "headerSurface";
        headerSurface.Location = new Point(0, 0);
        headerSurface.Size = new Size(900, 84);
        headerSurface.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        headerSurface.BackColor = Color.White;
        headerSurface.TabStop = false;
        Controls.Add(headerSurface);
        headerSurface.SendToBack();
        heading.BackColor = Color.White;
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Roboto", 10F);
        ForeColor = Color.FromArgb(32, 41, 35);
        BackColor = Color.FromArgb(244, 248, 240);
        ClientSize = new Size(900, 648);
        MinimumSize = new Size(916, 687);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CustomPC | Order receipt";
        Name = "ReceiptForm";
        headerSurface.Controls.Add(back);
        back.Location = new Point(768, 24);
        headerSurface.Controls.Add(heading);
        heading.Location = new Point(32, 24);
        CancelButton = back;
        ResumeLayout(false);
        PerformLayout();
    }
    private Panel headerSurface = null!;
    private Label heading = null!;
    private Button back = null!;
    private StoreTextField receiptText = null!;
    private Button print = null!;
    private Button save = null!;
}
