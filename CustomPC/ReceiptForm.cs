using System.Drawing.Printing;

namespace CustomPC;

public partial class ReceiptForm : StoreWindow
{
    private string? orderId;
    private ReceiptDocument? document;
    private readonly ReceiptPreview preview = new() { Name = "receiptPreview" };
    private int printPosition;

    public ReceiptForm()
    {
        InitializeComponent();
        back.Click += BackButton_Click;

        if (Ui.IsDesign)
        {
            return;
        }

        var workspace = new WorkspacePage { Name = "workspace" };
        workspace.Bind(this, heading, back, "", preview,
            null, new[] { print });

        workspace.AddNotice(ShopPolicies.Link("Warranty & Returns", "warranty", this));
        Shown += ReceiptForm_Shown;
        print.Click += PrintButton_Click;
    }

    public ReceiptForm(string id) : this()
    {
        orderId = id;
        RefreshReceipt();
    }

    private void RefreshReceipt()
    {
        AppStore store = AppStore.Current;
        store.Refresh();

        CustomerOrder? selectedOrder = null;
        foreach (CustomerOrder order in store.VisibleOrders())
        {
            if (order.Id == orderId)
            {
                selectedOrder = order;
                break;
            }
        }

        if (selectedOrder == null)
        {
            document = null;
            preview.SetDocument(null);
            print.Enabled = false;
            throw new InvalidOperationException("Receipt access denied.");
        }

        document = new ReceiptDocument(selectedOrder, store.Payments.FirstOrDefault(p => p.OrderId == selectedOrder.Id));
        preview.SetDocument(document);
        preview.AccessibleDescription = Ui.Receipt(selectedOrder, store);
    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        // Return to the previous screen without saving changes.
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void ReceiptForm_Shown(object? sender, EventArgs e)
    {
        preview.AutoScrollPosition = Point.Empty;
    }

    private void PrintButton_Click(object? sender, EventArgs e)
    {
        try
        {
            PrintReceipt();
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void PrintReceipt()
    {
        if (orderId == null)
        {
            throw new InvalidOperationException("Open a receipt from an order first.");
        }

        RefreshReceipt();

        using (PrintDocument printDocument = new PrintDocument())
        {
            // Quarter-inch outer margins; the document adds its own inner padding.
            printDocument.DefaultPageSettings.Margins = new Margins(25, 25, 25, 25);
            printDocument.DocumentName = "CustomPC receipt";
            printDocument.BeginPrint += PrintDocument_BeginPrint;
            printDocument.PrintPage += PrintDocument_PrintPage;

            using (PrintDialog printDialog = new PrintDialog())
            {
                printDialog.Document = printDocument;
                printDialog.UseEXDialog = true;

                if (printDialog.ShowDialog(this) == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
        }
    }

    private void PrintDocument_BeginPrint(object? sender, PrintEventArgs e)
    {
        printPosition = 0;
    }

    private void PrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
    {
        Graphics graphics = e.Graphics ?? throw new InvalidOperationException("The printer is unavailable.");

        if (document == null) throw new InvalidOperationException("Open a receipt first.");
        // Keep the same paper proportions in portrait and landscape printer bounds.
        float width = Math.Min(e.MarginBounds.Width, e.MarginBounds.Height * 760F / 1000);
        var bounds = new RectangleF(e.MarginBounds.Left + (e.MarginBounds.Width - width) / 2,
            e.MarginBounds.Top, width, width * 1000F / 760);
        // PrintPage bounds use hundredths of an inch; the renderer uses device pixels.
        bounds = new RectangleF(bounds.X * graphics.DpiX / 100, bounds.Y * graphics.DpiY / 100,
            bounds.Width * graphics.DpiX / 100, bounds.Height * graphics.DpiY / 100);
        e.HasMorePages = document.DrawPage(graphics, bounds, printPosition++);
    }
}
