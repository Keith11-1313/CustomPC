using ScottPlot.WinForms;

namespace CustomPC;

// Designer sees a normal panel; ScottPlot is constructed only when a runtime chart is drawn.
public sealed record ChartSelection(string Label, DateTime? Date = null, string? PartId = null,
    string? Status = null, string? Category = null, string? StockState = null);

public sealed partial class DashboardChart : Panel
{
    private FormsPlot? chart;
    private readonly EmptyStateControl empty = new() { Dock = DockStyle.Fill };
    private List<ChartSelection> targets = new();
    private Func<ScottPlot.Coordinates, int>? clickIndex;
    public event EventHandler? ChartActivated;
    public event Action<ChartSelection>? SelectionOpened;
    public event EventHandler? EmptyActionRequested;
    [System.ComponentModel.DefaultValue("")]
    public string EmptyActionText { get => empty.ActionText; set => empty.ActionText = value; }
    public DashboardChart()
    {
        InitializeComponent();
        BackColor = Color.White;
        Controls.Add(empty);
        empty.Message = "Chart preview";
        empty.Detail = "Saved data is loaded when the application runs.";
        empty.ActionClicked += (_, _) => EmptyActionRequested?.Invoke(this, EventArgs.Empty);
        openSelection.Click += (_, _) =>
        {
            if (selection.SelectedItem is ChartSelection target) SelectionOpened?.Invoke(target);
        };
        AccessibleName = "Dashboard chart";
    }

    public void ClearRuntimeData()
    {
        targets.Clear(); clickIndex = null;
        selection.DataSource = null; selectionFooter.Visible = false;
        chart?.Dispose(); chart = null;
        empty.Visible = true;
    }

    private void SetTargets(List<ChartSelection> items, Func<ScottPlot.Coordinates, int>? resolver)
    {
        targets = items; clickIndex = resolver;
        selection.DisplayMember = nameof(ChartSelection.Label);
        selection.DataSource = items;
        selectionFooter.Visible = items.Count > 0;
        // Dock the footer first, then give the chart the remaining space.
        chart?.BringToFront();
        PerformLayout();
    }

    private void Chart_DoubleClick(object? sender, MouseEventArgs e)
    {
        if (chart != null && clickIndex != null)
        {
            int index = clickIndex(chart.Plot.GetCoordinates(e.X, e.Y));
            if (index >= 0 && index < targets.Count) { SelectionOpened?.Invoke(targets[index]); return; }
        }
        ChartActivated?.Invoke(this, EventArgs.Empty);
    }

    public void Draw(string title, Action<ScottPlot.Plot> draw, bool hasData, string noData)
    {
        if (Ui.IsDesign) return;
        if (!hasData)
        {
            targets.Clear(); selection.DataSource = null; selectionFooter.Visible = false;
            chart?.Plot.Clear();
            if (chart != null) chart.Visible = false;
            empty.Message = noData;
            empty.Detail = "";
            empty.Visible = true;
            empty.BringToFront();
            return;
        }
        if (chart == null)
        {
            chart = new FormsPlot { MinimumSize = Size.Empty, Dock = DockStyle.Fill };
            chart.MouseDoubleClick += Chart_DoubleClick;
            Controls.Add(chart);
        }
        empty.Visible = false;
        chart.Visible = true;
        chart.Plot.Clear();
        draw(chart.Plot);
        chart.Plot.Title(title);
        chart.Plot.Axes.AutoScale();
        chart.Plot.Axes.Margins(.1, .15);
        chart.Refresh();
    }

    public void Line(string title, List<DailyValue> values, string unit, string noData)
    {
        Draw(title, plot =>
        {
            var line = plot.Add.Scatter(values.Select(v => v.Date.ToOADate()).ToArray(), values.Select(v => v.Value).ToArray());
            line.Color = ScottPlot.Color.FromHex("54B137");
            line.LineWidth = 2;
            line.LegendText = unit;
            plot.Axes.DateTimeTicksBottom();
            plot.YLabel(unit);
            plot.ShowLegend();
        }, values.Any(v => v.Value > 0), noData);
        if (values.Any(v => v.Value > 0))
            SetTargets(values.Select(value => new ChartSelection($"{value.Date:MMM d}: {value.Value.ToString(unit == "PHP" ? "N2" : "N0")} {unit}", Date: value.Date)).ToList(),
                point => (int)Math.Round(point.X - values[0].Date.ToOADate()));
    }

    public void Bars(string title, List<NamedValue> values, bool horizontal, string noData)
    {
        Draw(title, plot =>
        {
            var bars = plot.Add.Bars(values.Select(v => v.Value).ToArray());
            bars.Horizontal = horizontal;
            for (int i = 0; i < bars.Bars.Count; i++)
            {
                bars.Bars[i].FillColor = ScottPlot.Color.FromHex("54B137");
                bars.Bars[i].Label = values[i].Value.ToString("N0");
            }
            var ticks = values.Select((v, i) => new ScottPlot.Tick(i, horizontal && v.Name.Length > 24 ? v.Name[..21] + "…"
                : v.Name == "Pending Payment" ? "Pending\nPayment" : v.Name == "Ready for Pickup" ? "Ready for\nPickup" : v.Name)).ToArray();
            if (horizontal) { plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks); plot.XLabel("Units"); }
            else { plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks); plot.YLabel("Orders"); }
        }, values.Any(v => v.Value > 0), noData);
        if (values.Any(v => v.Value > 0))
            SetTargets(values.Select(value => new ChartSelection($"{value.Name}: {value.Value:N0}", PartId: value.Id,
                Status: value.Id == null && AppStore.Statuses.Contains(value.Name) ? value.Name : null)).ToList(),
                point => (int)Math.Round(horizontal ? point.Y : point.X));
    }

    public void Donut(List<NamedValue> values)
    {
        ScottPlot.Plot? piePlot = null;
        Draw("Inventory health · current", plot =>
        {
            piePlot = plot;
            string[] colors = { "54B137", "D49B25", "C65050" };
            var slices = values.Select((v, i) => new ScottPlot.PieSlice
            {
                Value = v.Value, FillColor = ScottPlot.Color.FromHex(colors[i]),
                LegendText = $"{v.Name}: {v.Value:N0}", Label = v.Value > 0 ? v.Value.ToString("N0") : ""
            }).Where(s => s.Value > 0).ToList();
            var pie = plot.Add.Pie(slices);
            pie.DonutFraction = .65;
            pie.Rotation = ScottPlot.Angle.FromDegrees(0);
            plot.Axes.Frameless(); plot.HideGrid(); plot.ShowLegend();
        }, values.Any(v => v.Value > 0), "Add products to your catalog");
        if (values.Any(v => v.Value > 0))
        {
            List<NamedValue> slices = values.Where(v => v.Value > 0).ToList();
            SetTargets(slices.Select(value => new ChartSelection($"{value.Name}: {value.Value:N0} products", StockState: value.Name)).ToList(), point =>
            {
                // ScottPlot draws slices clockwise in pixel space. Resolve the actual ring hit.
                var origin = piePlot!.GetPixel(ScottPlot.Coordinates.Origin);
                var pixel = piePlot.GetPixel(point);
                double dx = pixel.X - origin.X, dy = pixel.Y - origin.Y;
                double radius = Math.Min(Math.Abs(piePlot.GetPixel(new ScottPlot.Coordinates(1, 0)).X - origin.X),
                    Math.Abs(piePlot.GetPixel(new ScottPlot.Coordinates(0, 1)).Y - origin.Y));
                double distance = Math.Sqrt(dx * dx + dy * dy);
                if (distance < radius * .65 || distance > radius) return -1;
                double fraction = (Math.Atan2(dy, dx) / (2 * Math.PI) + 1) % 1;
                double cumulative = 0, total = slices.Sum(slice => slice.Value);
                for (int i = 0; i < slices.Count; i++)
                {
                    cumulative += slices[i].Value / total;
                    if (fraction <= cumulative) return i;
                }
                return -1;
            });
        }
    }

    public void StackedStock(List<CategoryStock> values)
    {
        Draw("Stock readiness · current", plot =>
        {
            string[] colors = { "54B137", "D49B25", "C65050" };
            string[] labels = { "Available", "Low stock", "Out of stock" };
            for (int state = 0; state < 3; state++)
            {
                var bars = values.Select((v, i) => new ScottPlot.Bar
                {
                    Position = i,
                    ValueBase = state == 0 ? 0 : state == 1 ? v.Available : v.Available + v.LowStock,
                    Value = state == 0 ? v.Available : state == 1 ? v.Available + v.LowStock : v.Available + v.LowStock + v.OutOfStock,
                    FillColor = ScottPlot.Color.FromHex(colors[state])
                }).ToArray();
                plot.Add.Bars(bars).LegendText = labels[state];
            }
            plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(values.Select((v, i) => new ScottPlot.Tick(i, v.Category == "Motherboard" ? "Board" : v.Category == "Power Supply" ? "PSU" : v.Category)).ToArray());
            plot.YLabel("Products"); plot.ShowLegend();
        }, values.Any(v => v.Available + v.LowStock + v.OutOfStock > 0), "No products in inventory");
        if (values.Any(v => v.Available + v.LowStock + v.OutOfStock > 0))
        {
            List<ChartSelection> options = new();
            foreach (CategoryStock category in values)
            {
                options.Add(new($"{category.Category} · Available: {category.Available}", Category: category.Category, StockState: "Available"));
                options.Add(new($"{category.Category} · Low stock: {category.LowStock}", Category: category.Category, StockState: "Low stock"));
                options.Add(new($"{category.Category} · Out of stock: {category.OutOfStock}", Category: category.Category, StockState: "Out of stock"));
            }
            SetTargets(options, point =>
            {
                int category = (int)Math.Round(point.X);
                if (category < 0 || category >= values.Count || point.Y < 0) return -1;
                CategoryStock value = values[category];
                int state = point.Y <= value.Available ? 0 : point.Y <= value.Available + value.LowStock ? 1 : 2;
                return category * 3 + state;
            });
        }
    }
}
