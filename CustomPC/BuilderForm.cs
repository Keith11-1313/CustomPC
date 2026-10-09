namespace CustomPC;

public partial class BuilderForm : Form
{
    private ComboBox[] partSelectors = Array.Empty<ComboBox>();

    public BuilderForm()
    {
        InitializeComponent();

        if (Ui.IsDesign)
        {
            return;
        }

        partSelectors = new ComboBox[] { part0, part1, part2, part3, part4, part5, part6 };
        submit.Click += SubmitButton_Click;

        LoadPartChoices();
        LoadCustomerChoices();
        UpdateBuildSummary();
    }

    private void LoadPartChoices()
    {
        AppStore store = AppStore.Current;
        store.Refresh();

        // The selectors follow the same order as AppStore.Categories.
        for (int index = 0; index < partSelectors.Length; index++)
        {
            string partCategory = AppStore.Categories[index];
            List<Part> availableParts = new List<Part>();

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
            selector.SelectedIndex = -1;
            selector.SelectedIndexChanged += PartSelectionChanged;
        }
    }

    private void LoadCustomerChoices()
    {
        AppStore store = AppStore.Current;
        List<UserAccount> customers = new List<UserAccount>();

        foreach (UserAccount account in store.Users)
        {
            bool canOrderForCustomer = store.IsStaff || account.Id == store.Session?.Id;
            if (!account.Archived && account.Role == "Customer" && canOrderForCustomer)
            {
                customers.Add(account);
            }
        }

        customer.DisplayMember = "Name";
        customer.ValueMember = "Id";
        customer.DataSource = customers;
        customer.Enabled = store.IsStaff;
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
        UpdateBuildSummary();
    }

    private void UpdateBuildSummary()
    {
        AppStore store = AppStore.Current;
        List<Part> selectedParts = GetSelectedParts();
        List<string> errors = store.CheckBuild(selectedParts);
        decimal buildTotal = 0;

        foreach (Part part in selectedParts)
        {
            buildTotal += part.Price;
        }

        total.Text = "Build total: " + Ui.Money(buildTotal);

        if (errors.Count == 0)
        {
            validation.Text = "Compatibility checks passed.\nReview your selection before confirming.";
            validation.ForeColor = Color.FromArgb(15, 98, 84);
        }
        else
        {
            validation.Text = string.Join("\n", errors);
            validation.ForeColor = Color.FromArgb(153, 27, 27);
        }

        submit.Enabled = store.Session != null && errors.Count == 0 && customer.Items.Count > 0;
        submit.BackColor = submit.Enabled ? Color.FromArgb(15, 98, 84) : Color.FromArgb(226, 232, 240);

        if (store.Session == null)
        {
            validation.Text += "\n\nSign in or register to place an order.";
        }
    }

    private void SubmitButton_Click(object? sender, EventArgs e)
    {
        try
        {
            decimal confirmedTotal = 0;
            foreach (Part selectedPart in GetSelectedParts())
            {
                confirmedTotal += selectedPart.Price;
            }

            if (!Ui.Confirm(this, "Confirm this build for " + Ui.Money(confirmedTotal) + " and reserve its parts for 24 hours?"))
            {
                return;
            }

            UserAccount? selectedCustomer = customer.SelectedItem as UserAccount;
            if (selectedCustomer == null)
            {
                throw new InvalidOperationException("Choose a customer account.");
            }

            List<string> selectedPartIds = new List<string>();
            foreach (Part part in GetSelectedParts())
            {
                selectedPartIds.Add(part.Id);
            }

            CustomerOrder newOrder = AppStore.Current.CreateOrder(selectedPartIds, selectedCustomer.Id, confirmedTotal);
            using (ReceiptForm receiptForm = new ReceiptForm(newOrder.Id))
            {
                receiptForm.ShowDialog(this);
            }

            DialogResult = DialogResult.OK;
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

    private void RefreshSelectedParts()
    {
        // Keep the chosen IDs while replacing old ComboBox objects with current records.
        List<string> chosenIds = new List<string>();
        string chosenCustomerId = "";
        if (customer.SelectedItem is UserAccount selectedCustomer)
        {
            chosenCustomerId = selectedCustomer.Id;
        }
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
            foreach (Part part in partSelectors[index].Items)
            {
                if (part.Id == chosenIds[index])
                {
                    partSelectors[index].SelectedItem = part;
                    break;
                }
            }
        }

        LoadCustomerChoices();
        foreach (UserAccount account in customer.Items)
        {
            if (account.Id == chosenCustomerId)
            {
                customer.SelectedItem = account;
                break;
            }
        }
        UpdateBuildSummary();
    }
}
