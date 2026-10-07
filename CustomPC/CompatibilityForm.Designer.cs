#nullable enable
namespace CustomPC;

partial class CompatibilityForm
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
        heading.Size = new Size(1036, 40);
        heading.TabIndex = 0;
        heading.Text = "Compatibility rules";
        heading.UseMnemonic = false;
        heading.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        Controls.Add(heading);
        subheading = new Label();
        subheading.Name = "subheading";
        subheading.Location = new Point(32, 72);
        subheading.Size = new Size(1036, 40);
        subheading.TabIndex = 1;
        subheading.Text = "Manage the stored checks used by the PC builder. Physical clearance requires manual review.";
        subheading.UseMnemonic = false;
        subheading.ForeColor = Color.FromArgb(71, 85, 105);
        Controls.Add(subheading);
        grid = new DataGridView();
        grid.Name = "grid";
        grid.Location = new Point(32, 138);
        grid.Size = new Size(1036, 458);
        grid.TabIndex = 2;
        grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.None;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersHeight = 42;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.RowTemplate.Height = 38;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(204, 235, 225);
        grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        Controls.Add(grid);
        note = new Label();
        note.Name = "note";
        note.Location = new Point(32, 608);
        note.Size = new Size(1000, 28);
        note.TabIndex = 3;
        note.Text = "Supported rules: CPU socket, motherboard RAM type, and PSU capacity with 100 W headroom.";
        note.UseMnemonic = false;
        Controls.Add(note);
        edit = new Button();
        edit.Name = "edit";
        edit.Location = new Point(32, 652);
        edit.Size = new Size(250, 40);
        edit.TabIndex = 4;
        edit.Text = "Edit selected rule";
        edit.BackColor = Color.FromArgb(15, 98, 84);
        edit.ForeColor = Color.White;
        edit.FlatStyle = FlatStyle.Flat;
        edit.FlatAppearance.BorderSize = 0;
        edit.Cursor = Cursors.Hand;
        edit.UseMnemonic = false;
        Controls.Add(edit);
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Segoe UI", 10F);
        ForeColor = Color.FromArgb(15, 23, 42);
        BackColor = Color.FromArgb(248, 250, 252);
        ClientSize = new Size(1100, 720);
        MinimumSize = new Size(1116, 759);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CustomPC | Compatibility rules";
        Name = "CompatibilityForm";
        ResumeLayout(false);
        PerformLayout();
    }
    private Label heading = null!;
    private Label subheading = null!;
    private DataGridView grid = null!;
    private Label note = null!;
    private Button edit = null!;
}
