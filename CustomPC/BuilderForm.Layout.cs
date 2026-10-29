namespace CustomPC;

public partial class BuilderForm
{
    private readonly List<Panel> componentCards = new();
    private Panel? validationViewport;
    private TableLayoutPanel? buildReview;
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
            labels[i].Font = AppFonts.Heading(12);
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
        heading.Font = AppFonts.Heading(25); heading.Text = "Configure your PC";
        total.Font = AppFonts.Body(23, FontStyle.Bold); validation.Font = AppFonts.Body(11);
        total.TextChanged += (_, _) => FitBuilder();
        checkoutLayout.Padding = new Padding(24); checkoutLayout.RowStyles[0].Height = 90;
        validationViewport = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Margin = new Padding(0, 8, 0, 8) };
        buildReview = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Margin = Padding.Empty };
        buildReview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        buildReview.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        validationViewport.Controls.Add(buildReview);
        validationViewport.SizeChanged += (_, _) => FitReviewText();
        validation.Dock = DockStyle.Top; validation.AutoSize = true;
        checkoutLayout.Controls.Add(validationViewport, 0, 1);
    }

    private void UpdateBuildReview(List<Part> parts)
    {
        if (buildReview == null) return;
        buildReview.SuspendLayout();
        buildReview.Controls.Remove(validation);
        while (buildReview.Controls.Count > 0) buildReview.Controls[0].Dispose();
        buildReview.RowStyles.Clear(); buildReview.RowCount = 0;
        void Full(Control control)
        {
            if (control is Label label) label.UseMnemonic = false;
            int row = buildReview.RowCount++;
            buildReview.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            buildReview.Controls.Add(control, 0, row); buildReview.SetColumnSpan(control, 2);
        }
        bool integrated = part3.SelectedItem is string;
        Full(new Label { Text = $"Your build · {parts.Count + (integrated ? 1 : 0)} of 7 selections", AutoSize = true,
            Font = AppFonts.Heading(11), Margin = new Padding(0, 0, 0, 16) });
        string[] conflicts = parts.Select(CandidateConflict).Where(reason => !string.IsNullOrEmpty(reason)).SelectMany(reason => reason!.Split('\n')).Distinct().ToArray();
        if (conflicts.Length > 0)
            Full(new Label { Name = "compatibilityConflicts", Text = "Incompatible selections\n" + string.Join("\n\n", conflicts),
                AutoSize = true, Font = AppFonts.Body(10), ForeColor = Color.Firebrick, Margin = new Padding(0, 0, 0, 16) });
        foreach (string category in AppStore.Categories)
        {
            Part? part = parts.FirstOrDefault(part => part.Category == category);
            int row = buildReview.RowCount++;
            buildReview.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var item = new Label { Text = category + "\n" + (part?.Name ?? (category == "GPU" && integrated ? "Integrated graphics" : "Not selected")),
                AutoSize = true, UseMnemonic = false, Dock = DockStyle.Top, Font = AppFonts.Body(10),
                ForeColor = part != null || category == "GPU" && integrated ? StoreTheme.Ink : StoreTheme.Muted,
                Margin = new Padding(0, 0, 12, 16) };
            var price = new Label { Text = part != null ? Ui.Money(part.Price) : "—", AutoSize = true,
                Font = AppFonts.Body(10, FontStyle.Bold), TextAlign = ContentAlignment.TopRight, Margin = new Padding(0, 0, 0, 16) };
            buildReview.Controls.Add(item, 0, row); buildReview.Controls.Add(price, 1, row);
        }
        Part? cpu = parts.FirstOrDefault(part => part.Category == "CPU");
        Part? board = parts.FirstOrDefault(part => part.Category == "Motherboard");
        Part? ram = parts.FirstOrDefault(part => part.Category == "RAM");
        Part? psu = parts.FirstOrDefault(part => part.Category == "Power Supply");
        string socket = cpu == null || board == null ? "Choose CPU and motherboard" :
            cpu.Socket.Length > 0 && cpu.Socket.Equals(board.Socket, StringComparison.OrdinalIgnoreCase) ? cpu.Socket + " matches" : "Check socket compatibility";
        string memory = ram == null || board == null ? "Choose RAM and motherboard" :
            ram.MemoryType.Length > 0 && ram.MemoryType.Equals(board.MemoryType, StringComparison.OrdinalIgnoreCase) ? ram.MemoryType + " matches" : "Check memory compatibility";
        long required = AppStore.RequiredPowerSupplyWatts(parts);
        string power = parts.Count == 0 ? "Choose components first" : $"Minimum PSU: {required:N0} W for selected parts\n" +
            (psu == null ? "Power supply not selected" : $"Selected PSU: {psu.Watts:N0} W" + (psu.Watts >= required ? " · Sufficient" : " · More power required"));
        Full(new Label { Text = "Compatibility & power", AutoSize = true, Font = AppFonts.Heading(11), Margin = new Padding(0, 8, 0, 10) });
        var checks = new List<string>();
        // A failed check is explained once, with the selected specifications, above the parts.
        if (cpu == null || board == null ||
            cpu.Socket.Length > 0 && cpu.Socket.Equals(board.Socket, StringComparison.OrdinalIgnoreCase))
            checks.Add("Socket: " + socket);
        if (ram == null || board == null ||
            ram.MemoryType.Length > 0 && ram.MemoryType.Equals(board.MemoryType, StringComparison.OrdinalIgnoreCase))
            checks.Add("Memory: " + memory);
        checks.Add(power);
        Full(new Label { Text = string.Join("\n", checks),
            AutoSize = true, Font = AppFonts.Body(10), ForeColor = StoreTheme.Ink, Margin = new Padding(0, 0, 0, 16) });
        validation.Margin = new Padding(0, 0, 0, 12);
        Full(validation);
        buildReview.ResumeLayout(true);
        FitReviewText();
    }

    private void FitReviewText()
    {
        if (buildReview == null || validationViewport == null) return;
        int width = Math.Max(1, validationViewport.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
        buildReview.Width = width;
        int priceWidth = buildReview.Controls.OfType<Label>().Where(label => buildReview.GetColumn(label) == 1)
            .Select(label => label.PreferredWidth).DefaultIfEmpty(0).Max();
        foreach (Label label in buildReview.Controls.OfType<Label>())
            if (buildReview.GetColumn(label) == 0)
                label.MaximumSize = new Size(Math.Max(1, width - (buildReview.GetColumnSpan(label) == 2 ? 0 : priceWidth + 16)), 0);
    }
}
