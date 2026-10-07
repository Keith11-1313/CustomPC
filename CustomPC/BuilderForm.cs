namespace CustomPC;

public partial class BuilderForm : Form
{
    private ComboBox[] Selectors => [part0, part1, part2, part3, part4, part5, part6];
    public BuilderForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return;
        var s = AppStore.Current; s.Refresh();
        for (int i = 0; i < Selectors.Length; i++)
        {
            Selectors[i].DataSource = s.Parts.Where(p => !p.Archived && s.Available(p) > 0 && p.Category == AppStore.Categories[i]).ToList();
            Selectors[i].SelectedIndex = -1; Selectors[i].SelectedIndexChanged += (_, _) => Check();
        }
        customer.DisplayMember = "Name"; customer.ValueMember = "Id";
        customer.DataSource = s.Users.Where(x => !x.Archived && x.Role == "Customer" && (s.IsStaff || x.Id == s.Session?.Id)).ToList();
        customer.Enabled = s.IsStaff;
        submit.Click += (_, _) => Ui.Run(this, () => {
            if (!Ui.Confirm(this, "Confirm this build and reserve its parts for 24 hours?")) return;
            var account = customer.SelectedItem as UserAccount ?? throw new InvalidOperationException("Choose a customer account.");
            var order = s.CreateOrder(Selected().Select(x => x.Id).ToList(), account.Id);
            using var receipt = new ReceiptForm(order.Id); receipt.ShowDialog(this); DialogResult = DialogResult.OK;
        }); Check();
    }
    private List<Part> Selected() => Selectors.Select(x => x.SelectedItem).OfType<Part>().ToList();
    private void Check()
    {
        var s = AppStore.Current; var parts = Selected(); var errors = s.CheckBuild(parts);
        total.Text = "Build total: " + Ui.Money(parts.Sum(p => p.Price));
        validation.Text = errors.Count == 0 ? "Compatibility checks passed.\nReview your selection before confirming." : string.Join("\n", errors);
        validation.ForeColor = errors.Count == 0 ? Color.FromArgb(15, 98, 84) : Color.FromArgb(153, 27, 27);
        submit.Enabled = s.Session != null && errors.Count == 0 && customer.Items.Count > 0;
        submit.BackColor = submit.Enabled ? Color.FromArgb(15, 98, 84) : Color.FromArgb(226, 232, 240);
        if (s.Session == null) validation.Text += "\n\nSign in or register to place an order.";
    }
}
