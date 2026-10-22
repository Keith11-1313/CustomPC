namespace CustomPC;

public partial class BuilderForm
{
    private readonly List<Panel> componentCards = new();
    private Label? builderDescription;
    private Panel? validationViewport;
    private void BuildComponentWorkspace()
    {
        MinimumSize = new Size(640, 560);
        Label[] labels = { part0Label, part1Label, part2Label, part3Label, part4Label, part5Label, part6Label };
        ProductThumbnail[] photos = { photo0, photo1, photo2, photo3, photo4, photo5, photo6 };
        selectionLayout.SuspendLayout(); selectionLayout.Controls.Clear(); selectionLayout.ColumnCount = 1; selectionLayout.RowCount = 7;
        selectionLayout.ColumnStyles.Clear(); selectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        selectionLayout.RowStyles.Clear(); selectionLayout.Padding = new Padding(0); selectionLayout.BackColor = StoreTheme.Background;
        for (int i = 0; i < 7; i++)
        {
            int index = i;
            selectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 136));
            Panel card = new() { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, 0, 12) };
            photos[i].Parent = card; photos[i].Dock = DockStyle.None; photos[i].SetBounds(16, 20, 72, 76);
            labels[i].Parent = card; labels[i].Dock = DockStyle.None; labels[i].SetBounds(108, 16, 300, 28);
            labels[i].Font = AppFonts.Body(12, FontStyle.Bold);
            partSelectors[i].Parent = card; partSelectors[i].Dock = DockStyle.None; partSelectors[i].SetBounds(108, 56, 300, 40);
            Button details = new() { Name = "details", Text = "Details", BackColor = Color.White, Width = 82, Height = 32, Top = 16 };
            details.Click += (_, _) => { if (partSelectors[index].SelectedItem is Part part) { using var dialog = new ProductDetailsForm(part); dialog.ShowDialog(this); } };
            card.Controls.Add(details);
            card.SizeChanged += (_, _) =>
            {
                labels[index].Width = Math.Max(1, card.Width - 220);
                partSelectors[index].Width = Math.Max(1, card.Width - 128);
                details.Left = card.Width - details.Width - 16;
            };
            componentCards.Add(card); selectionLayout.Controls.Add(card, 0, i);
        }
        selectionLayout.ResumeLayout(true);
        selectionScroll.BackColor = StoreTheme.Background;
        builderDescription = new Label { Text = "Choose compatible components. Your build stays with you as you browse.", ForeColor = StoreTheme.Muted, AutoEllipsis = true };
        headerSurface.Controls.Add(builderDescription);
        heading.Font = AppFonts.Heading(25); heading.Text = "Configure your PC";
        total.Font = AppFonts.Body(23, FontStyle.Bold); validation.Font = AppFonts.Body(11);
        checkoutLayout.Padding = new Padding(24); checkoutLayout.RowStyles[0].Height = 90;
        validationViewport = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Margin = new Padding(0, 8, 0, 8) };
        validation.Parent = validationViewport; validation.Dock = DockStyle.Top; validation.AutoSize = true;
        checkoutLayout.Controls.Add(validationViewport, 0, 1);
    }
}
