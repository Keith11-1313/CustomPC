using System.Drawing.Printing;

namespace CustomPC;

public partial class ReceiptForm : StoreWindow
{
    private string? orderId;
    private string[] printLines = Array.Empty<string>();
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
        workspace.Bind(this, heading, back, "Your order summary and store payment details.", receiptText,
            null, new[] { print, save });

        Shown += ReceiptForm_Shown;
        print.Click += PrintButton_Click;
        save.Click += SaveButton_Click;
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
            receiptText.Clear();
            save.Enabled = false;
            print.Enabled = false;
            throw new InvalidOperationException("Receipt access denied.");
        }

        receiptText.Text = Ui.Receipt(selectedOrder, store);
    }

    private void BackButton_Click(object? sender, EventArgs e)
    {
        // Return to the previous screen without saving changes.
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void ReceiptForm_Shown(object? sender, EventArgs e)
    {
        receiptText.Select(0, 0);
        receiptText.ScrollToCaret();
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

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            RefreshReceipt();
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Text receipt (*.txt)|*.txt";
                saveDialog.FileName = "CustomPC-receipt.txt";

                if (saveDialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, receiptText.Text);
                }
            }
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
        printLines = receiptText.Lines;
    }

    private void PrintDocument_PrintPage(object? sender, PrintPageEventArgs e)
    {
        Graphics graphics = e.Graphics ?? throw new InvalidOperationException("The printer is unavailable.");

        using (Font receiptFont = new Font("Consolas", 10F))
        {
            float lineHeight = receiptFont.GetHeight(graphics);
            float verticalPosition = e.MarginBounds.Top;

            // Continue on another page when the receipt reaches the bottom margin.
            while (printPosition < printLines.Length &&
                verticalPosition + lineHeight <= e.MarginBounds.Bottom)
            {
                graphics.DrawString(printLines[printPosition], receiptFont, Brushes.Black,
                    e.MarginBounds.Left, verticalPosition);

                printPosition++;
                verticalPosition += lineHeight;
            }
        }

        e.HasMorePages = printPosition < printLines.Length;
    }
}
