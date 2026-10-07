#nullable enable
namespace CustomPC;

partial class StockAdjustmentForm
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
        heading.Size = new Size(516, 40);
        heading.TabIndex = 0;
        heading.Text = "Stock adjustment";
        heading.UseMnemonic = false;
        heading.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        Controls.Add(heading);
        subheading = new Label();
        subheading.Name = "subheading";
        subheading.Location = new Point(32, 72);
        subheading.Size = new Size(516, 40);
        subheading.TabIndex = 1;
        subheading.Text = "Use a positive quantity for restock and a negative quantity for corrections.";
        subheading.UseMnemonic = false;
        subheading.ForeColor = Color.FromArgb(71, 85, 105);
        Controls.Add(subheading);
        partLabel = new Label();
        partLabel.Name = "partLabel";
        partLabel.Location = new Point(32, 140);
        partLabel.Size = new Size(500, 52);
        partLabel.TabIndex = 2;
        partLabel.Text = "Selected component";
        partLabel.UseMnemonic = false;
        Controls.Add(partLabel);
        quantityLabel = new Label();
        quantityLabel.Name = "quantityLabel";
        quantityLabel.Location = new Point(32, 216);
        quantityLabel.Size = new Size(480, 28);
        quantityLabel.TabIndex = 3;
        quantityLabel.Text = "Quantity change";
        quantityLabel.UseMnemonic = false;
        Controls.Add(quantityLabel);
        quantity = new NumericUpDown();
        quantity.Name = "quantity";
        quantity.Location = new Point(32, 246);
        quantity.Size = new Size(480, 32);
        quantity.TabIndex = 4;
        quantity.Maximum = 10000000;
        quantity.ThousandsSeparator = true;
        quantity.Minimum = -10000000;
        Controls.Add(quantity);
        reasonLabel = new Label();
        reasonLabel.Name = "reasonLabel";
        reasonLabel.Location = new Point(32, 300);
        reasonLabel.Size = new Size(480, 28);
        reasonLabel.TabIndex = 5;
        reasonLabel.Text = "Reason / supply reference";
        reasonLabel.UseMnemonic = false;
        Controls.Add(reasonLabel);
        reason = new TextBox();
        reason.Name = "reason";
        reason.Location = new Point(32, 330);
        reason.Size = new Size(480, 32);
        reason.TabIndex = 6;
        Controls.Add(reason);
        save = new Button();
        save.Name = "save";
        save.Location = new Point(32, 410);
        save.Size = new Size(480, 40);
        save.TabIndex = 7;
        save.Text = "Record adjustment";
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
        ClientSize = new Size(580, 490);
        MinimumSize = new Size(596, 529);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CustomPC | Stock adjustment";
        Name = "StockAdjustmentForm";
        ResumeLayout(false);
        PerformLayout();
    }
    private Label heading = null!;
    private Label subheading = null!;
    private Label partLabel = null!;
    private Label quantityLabel = null!;
    private NumericUpDown quantity = null!;
    private Label reasonLabel = null!;
    private TextBox reason = null!;
    private Button save = null!;
}
