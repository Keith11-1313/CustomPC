namespace CustomPC;

partial class DashboardChart
{
    private Panel selectionFooter = null!;
    private ComboBox selection = null!;
    private Button openSelection = null!;
    private void InitializeComponent()
    {
        selectionFooter = new Panel();
        selection = new ComboBox();
        openSelection = new Button();
        selectionFooter.Dock = DockStyle.Bottom;
        selectionFooter.Height = 38;
        selectionFooter.Padding = new Padding(6);
        selectionFooter.Visible = false;
        selection.Dock = DockStyle.Fill;
        selection.DropDownStyle = ComboBoxStyle.DropDownList;
        selection.DropDownWidth = 520;
        selection.AccessibleName = "Chart selection";
        openSelection.Dock = DockStyle.Right;
        openSelection.Width = 110;
        openSelection.Text = "Open selected";
        openSelection.FlatStyle = FlatStyle.Flat;
        openSelection.BackColor = Color.White;
        selectionFooter.Controls.Add(selection);
        selectionFooter.Controls.Add(openSelection);
        Controls.Add(selectionFooter);
        Size = new Size(420, 280);
    }
}
