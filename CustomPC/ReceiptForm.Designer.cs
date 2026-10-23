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
        print = new Button();
        print.Name = "print";
        print.Location = new Point(32, 578);
        print.Size = new Size(220, 40);
        print.TabIndex = 3;
        print.Text = "Print / save PDF";
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
    private Button print = null!;
}
