namespace CustomPC;

public partial class BuilderForm : StoreWindow
{
    private ComboBox[] partSelectors = Array.Empty<ComboBox>();
    private BuildDraft draft = new();
    private bool restoringDraft;
    private bool fittingBuilder;

    public BuilderForm(BuildDraft sharedDraft) : this()
    {
        draft = sharedDraft;
        RestoreDraft();
    }

    private void RestoreDraft()
    {
        LoadPartChoices(AppStore.Categories.Select(category => draft.SelectedId(category)).ToArray());
    }

    public BuilderForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;

        if (Ui.IsDesign)
        {
            return;
        }

        AppStore.Current.Refresh();
        if (AppStore.Current.Session != null && !AppStore.Current.IsCustomer)
            throw new InvalidOperationException("Customer access is required to build and order a PC. Guests may preview only.");

        partSelectors = new ComboBox[] { part0, part1, part2, part3, part4, part5, part6 };
        part0.FormattingEnabled = true;
        part0.Format += (_, e) =>
        {
            if (e.ListItem is Part cpu)
                e.Value = $"{cpu.Name} · {(cpu.HasIntegratedGraphics ? "Integrated graphics" : "Dedicated GPU required")} | {Ui.Money(cpu.Price)}";
        };
        foreach (StoreComboBox selector in partSelectors.OfType<StoreComboBox>())
            selector.WarningText = item => item is Part part ? CandidateConflict(part) : null;
        BuildComponentWorkspace();
        SizeChanged += (_, _) => FitBuilder();
        selectionScroll.SizeChanged += (_, _) => FitBuilder();
        Shown += (_, _) => FitBuilder();
        submit.Click += SubmitButton_Click;

        LoadPartChoices();
        UpdateBuildSummary();
    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        // Closing this screen returns to the window that opened it.
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void LoadPartChoices(string[]? chosenIds = null)
    {
        AppStore store = AppStore.Current;
        store.Refresh();
        chosenIds ??= partSelectors.Select(selector => (selector.SelectedItem as Part)?.Id ?? "").ToArray();
        restoringDraft = true;
        // The selectors follow the same order as AppStore.Categories.
        for (int index = 0; index < partSelectors.Length; index++)
        {
            string partCategory = AppStore.Categories[index];
            List<object> availableParts = new List<object>();
            Part? cpu = part0.SelectedItem as Part;
            Part? motherboard = part1.SelectedItem as Part;
            bool integrated = cpu?.HasIntegratedGraphics == true && motherboard?.HasVideoOutput == true;
            if (partCategory == "GPU" && integrated) availableParts.Add("No dedicated GPU (integrated graphics)");

            foreach (Part part in store.Parts)
            {
                if (!part.Archived && store.Available(part) > 0 && part.Category == partCategory)
                {
                    availableParts.Add(part);
                }
            }

            ComboBox selector = partSelectors[index];
            selector.SelectedIndexChanged -= PartSelectionChanged;
            selector.DataSource = availableParts;
            selector.SelectedIndex = partCategory == "GPU" && integrated ? 0 : -1;
            foreach (object item in availableParts)
                if (item is Part part && part.Id == chosenIds[index]) selector.SelectedItem = part;
            selector.SelectedIndexChanged += PartSelectionChanged;
        }
        restoringDraft = false;
        for (int i = 0; i < partSelectors.Length; i++) draft.Select(AppStore.Categories[i], (partSelectors[i].SelectedItem as Part)?.Id);
        UpdateBuildSummary();
    }

    private List<Part> GetSelectedParts()
    {
        List<Part> selectedParts = new List<Part>();

        foreach (ComboBox selector in partSelectors)
        {
            if (selector.SelectedItem is Part selectedPart)
            {
                selectedParts.Add(selectedPart);
            }
        }

        return selectedParts;
    }

    private void PartSelectionChanged(object? sender, EventArgs e)
    {
        if (!restoringDraft) LoadPartChoices();
    }

    private void UpdateBuildSummary()
    {
        AppStore store = AppStore.Current;
        List<Part> selectedParts = GetSelectedParts();
        List<string> errors = store.CheckBuild(selectedParts);
        foreach (StoreComboBox selector in partSelectors.OfType<StoreComboBox>())
        {
            selector.AccessibleDescription = selector.SelectedItem is Part part ? CandidateConflict(part) ?? "No conflict with current selections" : "Choose a component";
            selector.Invalidate();
        }
        ProductThumbnail[] photos = { photo0, photo1, photo2, photo3, photo4, photo5, photo6 };
        for (int i = 0; i < partSelectors.Length; i++)
            photos[i].Display(partSelectors[i].SelectedItem as Part, store.DatabasePath);
        if (part3.SelectedItem is string) photo3.PlaceholderText = "Integrated graphics";
        for (int i = 0; i < partSelectors.Length; i++)
            if (AppStore.Categories[i] != "GPU" && !partSelectors[i].Items.OfType<Part>().Any())
                errors.Add("No in-stock " + AppStore.Categories[i] + " choices. Check stock.");
        decimal buildTotal = 0;

        foreach (Part part in selectedParts)
        {
            buildTotal += part.Price;
        }

        total.Text = "Build total\n" + Ui.Money(buildTotal);

        if (errors.Count == 0)
        {
            validation.Text = "Supported compatibility checks passed.\nStaff will verify storage connections, BIOS support and physical fit before assembly.";
            validation.ForeColor = Color.FromArgb(15, 98, 84);
        }
        else
        {
            // Detailed socket, memory and PSU conflicts already appear in the red review section.
            List<string> issues = errors.Where(error => !error.StartsWith("Select one ") &&
                error != "CPU and motherboard sockets do not match or are unspecified." &&
                error != "RAM and motherboard memory types do not match or are unspecified." &&
                !error.StartsWith("Power supply needs at least")).ToList();
            validation.Text = string.Join("\n", issues);
            validation.ForeColor = StoreTheme.Muted;
        }

        // A valid Guest build continues to Customer login; ordering still requires that account.
        submit.Enabled = (store.Session == null || store.IsCustomer) && errors.Count == 0;
        submit.BackColor = submit.Enabled ? Color.FromArgb(84, 177, 55) : Color.FromArgb(226, 232, 224);

        submit.Text = store.Session == null ? "Log in and order" : "Confirm build & order";
        UpdateBuildReview(selectedParts);
    }

    private void SubmitButton_Click(object? sender, EventArgs e)
    {
        try
        {
            AppStore store = AppStore.Current;
            store.Refresh();
            if (store.Session == null)
            {
                SignInFromBuilder();
                if (IsDisposed || !store.IsCustomer) return;
                if (store.CheckBuild(GetSelectedParts()).Count > 0) return;
                // Continue this checkout with the retained parts and the newly signed-in owner.
            }
            if (!store.IsCustomer)
                throw new InvalidOperationException("Sign in with a Customer account to place an order.");

            decimal confirmedTotal = 0;
            foreach (Part selectedPart in GetSelectedParts())
            {
                confirmedTotal += selectedPart.Price;
            }

            if (!ShopPolicies.ConfirmOrder(this, confirmedTotal))
            {
                return;
            }

            store.Refresh();
            if (!store.IsCustomer)
            {
                throw new InvalidOperationException("Sign in with a Customer account to place an order.");
            }

            List<string> selectedPartIds = new List<string>();
            foreach (Part part in GetSelectedParts())
            {
                selectedPartIds.Add(part.Id);
            }

            // Ownership comes from the signed-in account, never a dropdown or typed customer ID.
            CustomerOrder newOrder = store.CreateOrder(selectedPartIds, store.Session!.Id, confirmedTotal);
            draft.Clear();
            using (ReceiptForm receiptForm = new ReceiptForm(newOrder.Id))
            {
                receiptForm.ShowDialog(this);
            }

            DialogResult = DialogResult.OK;
            if (!TopLevel) Close();
        }
        catch (Exception exception)
        {
            try
            {
                RefreshSelectedParts();
            }
            catch (Exception refreshError)
            {
                exception = new InvalidOperationException(exception.Message + "\nThe build could not be refreshed: " + refreshError.Message);
            }
            Ui.ShowError(this, exception);
        }
    }

    private void SignInFromBuilder()
    {
        Form? dashboard = TopLevel ? Owner : Parent?.FindForm();
        bool dashboardWasVisible = dashboard != null && dashboard.Visible;
        Hide();
        if (dashboardWasVisible)
        {
            if (dashboard is DashboardForm main) main.PauseForCheckoutLogin();
            else dashboard!.Hide();
        }
        try
        {
            using LoginForm loginForm = new LoginForm();
            loginForm.ShowDialog();
        }
        finally
        {
            if (dashboardWasVisible && !dashboard!.IsDisposed)
            {
                if (dashboard is DashboardForm main) main.ResumeAfterCheckoutLogin();
                else dashboard.Show();
            }
            if (!IsDisposed) Show();
        }

        if (IsDisposed) return;
        AppStore.Current.Refresh();
        if (AppStore.Current.Session != null && !AppStore.Current.IsCustomer)
        {
            // Staff/Admin continue to their own dashboard instead of a Customer-only builder.
            DialogResult = DialogResult.Cancel;
            Close();
            return;
        }
        RefreshSelectedParts();
    }

    private void RefreshSelectedParts()
    {
        // Keep the chosen IDs while replacing old ComboBox objects with current records.
        List<string> chosenIds = new List<string>();
        foreach (ComboBox selector in partSelectors)
        {
            string chosenId = "";
            if (selector.SelectedItem is Part part)
            {
                chosenId = part.Id;
            }
            chosenIds.Add(chosenId);
        }

        LoadPartChoices(chosenIds.ToArray());
    }

    private void FitBuilder()
    {
        if (builderLayout == null || fittingBuilder) return;
        fittingBuilder = true;
        try
        {
            bool stacked = ClientSize.Width < 1050 * DeviceDpi / 96F;
            builderLayout.SuspendLayout();
            if (!stacked)
            {
                builderLayout.SetColumnSpan(checkoutLayout, 1);
                builderLayout.SetCellPosition(checkoutLayout, new TableLayoutPanelCellPosition(1, 1));
            }
            builderLayout.RowCount = stacked ? 3 : 2;
            builderLayout.RowStyles.Clear();
            builderLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92 * DeviceDpi / 96F));
            builderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            if (stacked) builderLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 300 * DeviceDpi / 96F));
            builderLayout.SetColumnSpan(selectionScroll, stacked ? 2 : 1);
            builderLayout.SetCellPosition(checkoutLayout, new TableLayoutPanelCellPosition(stacked ? 0 : 1, stacked ? 2 : 1));
            builderLayout.SetColumnSpan(checkoutLayout, stacked ? 2 : 1);
            builderLayout.ResumeLayout(true);
            selectionLayout.Width = Math.Max(1, selectionScroll.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
            selectionLayout.Height = (int)(952 * DeviceDpi / 96F);
            float scale = DeviceDpi / 96F;
            int Px(int value) => (int)(value * scale);
            back.Size = new Size(Math.Max(Px(116), TextRenderer.MeasureText(back.Text, back.Font).Width + Px(60)), Px(42));
            back.Location = new Point(Math.Max(0, headerSurface.ClientSize.Width - back.Width - Px(24)), Px(20));
            heading.SetBounds(Px(32), Px(20), Math.Max(1, back.Left - Px(48)), Px(54));
            int totalWidth = Math.Max(1, checkoutLayout.ClientSize.Width - checkoutLayout.Padding.Horizontal - total.Margin.Horizontal);
            checkoutLayout.RowStyles[0].Height = TextRenderer.MeasureText(total.Text, total.Font,
                new Size(totalWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix).Height + Px(16);
            headerSurface.BackColor = StoreTheme.Background;
            heading.BackColor = StoreTheme.Background;
            if (validationViewport != null) validation.MaximumSize = new Size(Math.Max(1, validationViewport.ClientSize.Width - SystemInformation.VerticalScrollBarWidth), 0);
        }
        finally { fittingBuilder = false; }
    }
}
