namespace CustomPC;

public partial class BuilderForm : StoreWindow
{
    private ComboBox[] partSelectors = Array.Empty<ComboBox>();
    private BuildDraft draft = new();
    private bool restoringDraft;

    public BuilderForm(BuildDraft sharedDraft) : this()
    {
        draft = sharedDraft;
        RestoreDraft();
    }

    private void RestoreDraft()
    {
        restoringDraft = true;
        for (int i = 0; i < partSelectors.Length; i++)
            foreach (object item in partSelectors[i].Items)
                if (item is Part part && part.Id == draft.SelectedId(AppStore.Categories[i]))
                    partSelectors[i].SelectedItem = part;
        restoringDraft = false;
        UpdateBuildSummary();
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

    private void LoadPartChoices()
    {
        AppStore store = AppStore.Current;
        store.Refresh();

        // The selectors follow the same order as AppStore.Categories.
        for (int index = 0; index < partSelectors.Length; index++)
        {
            string partCategory = AppStore.Categories[index];
            List<object> availableParts = new List<object>();
            if (partCategory == "GPU") availableParts.Add("No dedicated GPU (integrated graphics)");

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
            selector.SelectedIndex = partCategory == "GPU" ? 0 : -1;
            selector.SelectedIndexChanged += PartSelectionChanged;
        }
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
        if (!restoringDraft) for (int i = 0; i < partSelectors.Length; i++)
            draft.Select(AppStore.Categories[i], (partSelectors[i].SelectedItem as Part)?.Id);
        UpdateBuildSummary();
    }

    private void UpdateBuildSummary()
    {
        AppStore store = AppStore.Current;
        List<Part> selectedParts = GetSelectedParts();
        List<string> errors = store.CheckBuild(selectedParts);
        ProductThumbnail[] photos = { photo0, photo1, photo2, photo3, photo4, photo5, photo6 };
        for (int i = 0; i < partSelectors.Length; i++)
            photos[i].Display(partSelectors[i].SelectedItem as Part, store.DatabasePath);
        if (part3.SelectedItem is string) photo3.PlaceholderText = "Integrated graphics";
        for (int i = 0; i < partSelectors.Length; i++)
            if (AppStore.Categories[i] != "GPU" && partSelectors[i].Items.Count == 0)
                errors.Add("No available " + AppStore.Categories[i] + " products. The store must receive stock before checkout.");
        decimal buildTotal = 0;

        foreach (Part part in selectedParts)
        {
            buildTotal += part.Price;
        }

        total.Text = "Build total: " + Ui.Money(buildTotal);

        if (errors.Count == 0)
        {
            validation.Text = "Socket, memory and power checks passed.\nReview your selection before confirming. Staff will review physical fit and BIOS support.";
            validation.ForeColor = Color.FromArgb(15, 98, 84);
        }
        else
        {
            validation.Text = string.Join("\n", errors);
            validation.ForeColor = Color.FromArgb(153, 27, 27);
        }

        // A valid Guest build continues to Customer login; ordering still requires that account.
        submit.Enabled = (store.Session == null || store.IsCustomer) && errors.Count == 0;
        submit.BackColor = submit.Enabled ? Color.FromArgb(84, 177, 55) : Color.FromArgb(226, 232, 224);

        if (store.Session == null)
        {
            validation.Text += "\n\nSign in with a Customer account to place your own order.";
        }
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

            if (!Ui.Confirm(this, "Confirm this build for " + Ui.Money(confirmedTotal) + " and reserve its parts for 24 hours?"))
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

        LoadPartChoices();
        for (int index = 0; index < partSelectors.Length; index++)
        {
            foreach (object item in partSelectors[index].Items)
            {
                if (item is Part part && part.Id == chosenIds[index])
                {
                    partSelectors[index].SelectedItem = part;
                    break;
                }
            }
        }

        UpdateBuildSummary();
    }
}
