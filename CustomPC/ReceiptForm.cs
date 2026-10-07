namespace CustomPC;

public partial class ReceiptForm : Form
{
    private string? orderId;
    private string[] printLines = [];
    private int printPosition;
    public ReceiptForm()
    {
        InitializeComponent(); if (Ui.IsDesign) return;
        Shown += (_, _) => { receiptText.Select(0, 0); receiptText.ScrollToCaret(); };
        print.Click += (_, _) => Ui.Run(this, PrintReceipt);
        save.Click += (_, _) => Ui.Run(this, () => { using var dialog = new SaveFileDialog { Filter = "Text receipt (*.txt)|*.txt", FileName = "CustomPC-receipt.txt" }; if (dialog.ShowDialog(this) == DialogResult.OK) File.WriteAllText(dialog.FileName, receiptText.Text); });
    }
    public ReceiptForm(string id) : this()
    {
        orderId = id; var s = AppStore.Current; s.Refresh();
        var order = s.VisibleOrders().SingleOrDefault(x => x.Id == id) ?? throw new InvalidOperationException("Receipt access denied.");
        receiptText.Text = Ui.Receipt(order, s);
    }
    private void PrintReceipt()
    {
        if (orderId == null) throw new InvalidOperationException("Open a receipt from an order first.");
        using var document = new System.Drawing.Printing.PrintDocument();
        document.BeginPrint += (_, _) => { printPosition = 0; printLines = receiptText.Lines; };
        document.PrintPage += (_, e) => {
            using var font = new Font("Consolas", 10F); float y = e.MarginBounds.Top;
            while (printPosition < printLines.Length && y + font.GetHeight(e.Graphics!) <= e.MarginBounds.Bottom)
            { e.Graphics!.DrawString(printLines[printPosition++], font, Brushes.Black, e.MarginBounds.Left, y); y += font.GetHeight(e.Graphics!); }
            e.HasMorePages = printPosition < printLines.Length;
        };
        using var dialog = new PrintDialog { Document = document, UseEXDialog = true };
        if (dialog.ShowDialog(this) == DialogResult.OK) document.Print();
    }
}
