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
        heading.Size = new Size(836, 40);
        heading.TabIndex = 0;
        heading.Text = "Order receipt";
        heading.UseMnemonic = false;
        heading.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        Controls.Add(heading);
        subheading = new Label();
        subheading.Name = "subheading";
        subheading.Location = new Point(32, 72);
        subheading.Size = new Size(836, 40);
        subheading.TabIndex = 1;
        subheading.Text = "Keep your order number. Payment and pickup take place at the physical store.";
        subheading.UseMnemonic = false;
        subheading.ForeColor = Color.FromArgb(71, 85, 105);
        Controls.Add(subheading);
        receiptText = new TextBox();
        receiptText.Name = "receiptText";
        receiptText.Location = new Point(32, 130);
        receiptText.Size = new Size(836, 450);
        receiptText.TabIndex = 2;
        receiptText.Text = "CUSTOMPC\r\nORDER RECEIPT\r\n\r\nOrder details appear here after confirmation.";
        receiptText.Multiline = true; receiptText.ReadOnly = true; receiptText.ScrollBars = ScrollBars.Vertical; receiptText.Font = new Font("Consolas", 11F); receiptText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        Controls.Add(receiptText);
        print = new Button();
        print.Name = "print";
        print.Location = new Point(32, 610);
        print.Size = new Size(220, 40);
        print.TabIndex = 3;
        print.Text = "Print receipt";
        print.BackColor = Color.FromArgb(15, 98, 84);
        print.ForeColor = Color.White;
        print.FlatStyle = FlatStyle.Flat;
        print.FlatAppearance.BorderSize = 0;
        print.Cursor = Cursors.Hand;
        print.UseMnemonic = false;
        Controls.Add(print);
        save = new Button();
        save.Name = "save";
        save.Location = new Point(268, 610);
        save.Size = new Size(220, 40);
        save.TabIndex = 4;
        save.Text = "Save text receipt";
        save.BackColor = Color.FromArgb(15, 98, 84);
        save.ForeColor = Color.White;
        save.FlatStyle = FlatStyle.Flat;
        save.FlatAppearance.BorderSize = 0;
        save.Cursor = Cursors.Hand;
        save.UseMnemonic = false;
        Controls.Add(save);
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 10F);
        ForeColor = Color.FromArgb(15, 23, 42);
        BackColor = Color.FromArgb(248, 250, 252);
        ClientSize = new Size(900, 680);
        MinimumSize = new Size(916, 719);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CustomPC | Order receipt";
        Name = "ReceiptForm";
        ResumeLayout(false);
        PerformLayout();
    }
    private Label heading = null!;
    private Label subheading = null!;
    private TextBox receiptText = null!;
    private Button print = null!;
    private Button save = null!;
}
