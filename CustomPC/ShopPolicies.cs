namespace CustomPC;

// Customer-facing shop policies are kept in one place so every entry point agrees.
internal static class ShopPolicies
{
    internal sealed record Section(string Heading, string Body);
    internal static (string Title, Section[] Sections) Content(string policy) => policy switch
    {
        "terms" => ("Terms & Conditions", new Section[] {
            new("Ordering and prices", "Review your components, quantities and order total before confirming. Prices are recorded when your order is placed. Changes to a confirmed order must be arranged with our service counter."),
            new("Payment and reservations", "Pay in-store within 24 hours of placing your order. Your payment deadline appears on your order and receipt. Components are reserved until that deadline. Unpaid orders automatically expire and reserved stock is released."),
            new("Compatibility and assembly", "Our PC builder checks supported socket, memory and power requirements. Before assembly, our staff also reviews BIOS support, component dimensions and physical clearance. We will discuss any necessary component changes with you before proceeding."),
            new("Cancellations and changes", "You can cancel an unpaid order through My orders. For paid orders or builds already being prepared, visit our service counter so we can review your request and explain the available options."),
            new("Collection", "Orders are for in-store pickup. Wait until your order is marked Ready for pickup, then present your receipt or order number at the service counter. Please check your components and included accessories when collecting your purchase."),
            new("After-sales support", "Warranty claims and defective-item concerns are handled under our Warranty & Returns Policy and applicable Philippine consumer law.") }),
        "warranty" => ("Warranty & Returns", new Section[] {
            new("Product warranty", "Warranty coverage and duration depend on the component and manufacturer. Refer to the manufacturer's warranty documentation supplied with your product for its coverage and duration. Keep your receipt and warranty documents for after-sales support."),
            new("Reporting a fault", "Bring the affected item and your receipt or other proof of purchase to the CustomPC service counter. Describe the fault and include any supplied accessories needed for testing. Back up personal files before bringing in a computer or storage device for service."),
            new("Assessment and resolution", "We assess reported faults and explain the available repair, replacement or refund process under the applicable warranty and Philippine consumer law. Manufacturer assessment may be required. Our staff will explain the next steps and any expected processing time."),
            new("Damage and exclusions", "Accidental damage, misuse, unauthorized modifications and use outside manufacturer specifications may affect warranty coverage. Any exclusion is assessed against the reported fault, the applicable warranty and consumer law."),
            new("Change-of-mind requests", "Contact our service counter before assembly begins if you wish to change your selection. Change-of-mind requests are reviewed separately from claims involving defective or incorrectly supplied items."),
            new("Your consumer rights", "This policy does not remove or limit your rights to remedies for defective goods under Philippine law.") }),
        "privacy" => ("Privacy Notice", new Section[] {
            new("Information we collect", "CustomPC records your name, email address, protected login credentials, selected components, order history and in-store payment confirmations. Account and transaction activity is also recorded for security and accountability."),
            new("How we use your information", "We use this information to manage your account, reserve components, process orders, record payments, arrange collection and provide warranty or after-sales assistance. Security records help us protect accounts and investigate unauthorized activity."),
            new("Who can access it", "Authorized shop personnel can access information needed for their responsibilities. Customer accounts can view their own orders. Relevant purchase details may be shared when needed to handle a warranty claim or meet a legal obligation."),
            new("Storage and protection", "Records are stored in the shop's local system and authorized backups. Passwords are stored as protected hashes rather than readable passwords. Access is controlled by account permissions."),
            new("Retention", "We retain information for as long as necessary to fulfil transactions, support warranty claims, maintain security and meet applicable record-keeping obligations. Information no longer required is securely removed in accordance with those obligations, including relevant backup records."),
            new("Your information and your rights", "Visit the CustomPC service counter to request access to or correction of your information, raise a privacy concern or request deletion where applicable. We may verify your identity before handling a request. Some records must be retained to satisfy legal obligations or resolve outstanding transactions.") }),
        _ => throw new ArgumentException("Unknown shop policy.", nameof(policy))
    };
    internal static LinkLabel Link(string text, string policy, IWin32Window owner)
    {
        var link = new LinkLabel { Name = policy + "PolicyLink", Text = text, AutoSize = true,
            LinkColor = StoreTheme.Action, ActiveLinkColor = StoreTheme.Ink, VisitedLinkColor = StoreTheme.Action,
            Font = AppFonts.Body(10), Padding = new Padding(0, 8, 0, 8), UseMnemonic = false,
            Margin = new Padding(0, 0, 20, 0), AccessibleName = text };
        link.LinkClicked += (_, _) => { using var dialog = new PolicyForm(policy); dialog.ShowDialog(owner); };
        return link;
    }
    internal static bool ConfirmOrder(Form owner, decimal total)
    {
        using var dialog = new StoreDialog("Confirm your order", "Order total: " + Ui.Money(total) +
            "\n\nYour parts will be reserved for 24 hours. Pay at the store before the deadline shown on your order. Unpaid reservations automatically expire.", true);
        dialog.AddPolicyLink(Link("Terms & Conditions", "terms", dialog));
        return dialog.ShowDialog(owner) == DialogResult.Yes;
    }
}
