using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

namespace CustomPC;

public sealed record ChartSelection(string Label, DateTime? Date = null, string? PartId = null,
    string? Status = null, string? Category = null, string? StockState = null);

// The Designer sees a regular panel. LiveCharts controls and saved data are runtime-only.
public sealed partial class DashboardChart : Panel
{
    private Control? chart;
    private readonly Label chartHeading = new() { Dock = DockStyle.Top, Height = 48,
        TextAlign = ContentAlignment.MiddleCenter, Font = AppFonts.Heading(11) };
    private readonly EmptyStateControl empty = new() { Dock = DockStyle.Fill };
    private static readonly SKColor Green = SKColor.Parse("147750");
    private static readonly SKColor[] StockColors = { Green, SKColor.Parse("D49B25"), SKColor.Parse("C65050") };
    private List<ChartSelection> targets = new();
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
        Controls.Add(chartHeading);
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
        targets.Clear();
        selection.DataSource = null;
        selectionFooter.Visible = false;
        if (chart != null) { Controls.Remove(chart); chart.Dispose(); chart = null; }
        empty.Visible = true;
    }

    private bool Prepare(string title, bool hasData, string noData)
    {
        if (Ui.IsDesign) return false;
        ClearRuntimeData();
        chartHeading.Text = title;
        empty.Message = noData;
        empty.Detail = "";
        chartHeading.SendToBack();
        empty.BringToFront();
        return hasData;
    }

    private void ShowChart(Control control, List<ChartSelection> options)
    {
        chart = control;
        control.Dock = DockStyle.Fill;
        control.BackColor = Color.White;
        control.MinimumSize = Size.Empty;
        control.DoubleClick += (_, _) => ChartActivated?.Invoke(this, EventArgs.Empty);
        Controls.Add(control);
        empty.Visible = false;
        targets = options;
        selection.DisplayMember = nameof(ChartSelection.Label);
        selection.DataSource = targets;
        selectionFooter.Visible = targets.Count > 0;
        chartHeading.SendToBack();
        control.BringToFront();
        PerformLayout();
    }

    private void OpenPoint(int index)
    {
        if (index >= 0 && index < targets.Count) SelectionOpened?.Invoke(targets[index]);
    }

    private static Axis NumberAxis(string unit) => new()
    {
        MinLimit = 0, MinStep = 1, TextSize = 11,
        Labeler = value => unit == "PHP" ? (value >= 1000 ? $"{value / 1000:0.#}k" : value.ToString("0")) : value.ToString("0"),
        Name = unit, NameTextSize = 11,
        LabelsPaint = new SolidColorPaint(SKColor.Parse("65736B")),
        SeparatorsPaint = new SolidColorPaint(SKColor.Parse("EDF1EE")) { StrokeThickness = 1 }
    };
    private static Axis CategoryAxis(string[] labels) => new()
    {
        Labels = labels, MinStep = 1, ForceStepToMin = true, TextSize = 11,
        LabelsPaint = new SolidColorPaint(SKColor.Parse("65736B")), SeparatorsPaint = null
    };
    private static CartesianChart Cartesian(ISeries[] series, Axis x, Axis y) => new()
    {
        Series = series, XAxes = new[] { x }, YAxes = new[] { y },
        LegendPosition = LegendPosition.Hidden, AnimationsSpeed = TimeSpan.FromMilliseconds(250),
        TooltipTextSize = 12, ZoomMode = ZoomAndPanMode.None
    };
    private static string ShortLabel(string label) => label.Length > 19 ? label[..16] + "..." : label;

    public void Line(string title, List<DailyValue> values, string unit, string noData)
    {
        if (!Prepare(title, values.Any(v => v.Value > 0), noData)) return;
        var series = new LineSeries<double>
        {
            Name = unit, Values = values.Select(v => v.Value).ToArray(),
            Stroke = new SolidColorPaint(Green) { StrokeThickness = 3 },
            Fill = new SolidColorPaint(Green.WithAlpha(22)), GeometrySize = 5,
            GeometryFill = new SolidColorPaint(SKColors.White), GeometryStroke = new SolidColorPaint(Green) { StrokeThickness = 2 },
            XToolTipLabelFormatter = point => values[point.Index].Date.ToString("MMM d"),
            LineSmoothness = 0.15
        };
        series.DataPointerDown += (_, points) => { var point = points.FirstOrDefault(); if (point != null) OpenPoint(point.Index); };
        int stride = Math.Max(1, (int)Math.Ceiling(values.Count / 3.0));
        var dates = CategoryAxis(values.Select((v, i) => i % stride == 0 ? v.Date.ToString("MMM d") : "").ToArray());
        ShowChart(Cartesian(new ISeries[] { series }, dates, NumberAxis(unit)), values.Select(v =>
            new ChartSelection($"{v.Date:MMM d}: {v.Value:N2} {unit}", Date: v.Date)).ToList());
    }

    public void Bars(string title, List<NamedValue> values, bool horizontal, string noData)
    {
        if (!Prepare(title, values.Any(v => v.Value > 0), noData)) return;
        ISeries series;
        if (horizontal)
        {
            var rows = new RowSeries<double> { Values = values.Select(v => v.Value).ToArray(), Name = "Units",
                Fill = new SolidColorPaint(Green), Stroke = null, Rx = 5, Ry = 5, MaxBarWidth = 26 };
            rows.YToolTipLabelFormatter = point => values[point.Index].Name;
            rows.DataPointerDown += (_, points) => { var point = points.FirstOrDefault(); if (point != null) OpenPoint(point.Index); };
            series = rows;
        }
        else
        {
            var columns = new ColumnSeries<double> { Values = values.Select(v => v.Value).ToArray(), Name = "Orders",
                Fill = new SolidColorPaint(Green), Stroke = null, Rx = 5, Ry = 5, MaxBarWidth = 28 };
            columns.XToolTipLabelFormatter = point => values[point.Index].Name;
            columns.DataPointerDown += (_, points) => { var point = points.FirstOrDefault(); if (point != null) OpenPoint(point.Index); };
            series = columns;
        }
        Axis categories = CategoryAxis(values.Select(v => ShortLabel(v.Name == "Pending Payment" ? "Pending" : v.Name == "Ready for Pickup" ? "Ready" : v.Name)).ToArray());
        if (!horizontal) categories.TextSize = 10;
        ShowChart(Cartesian(new[] { series }, horizontal ? NumberAxis("Units") : categories,
            horizontal ? categories : NumberAxis("Orders")), values.Select(v => new ChartSelection($"{v.Name}: {v.Value:N0}",
                PartId: v.Id, Status: v.Id == null && AppStore.Statuses.Contains(v.Name) ? v.Name : null)).ToList());
    }

    public void Donut(List<NamedValue> values)
    {
        if (!Prepare("Inventory health - current", values.Any(v => v.Value > 0), "Add products to your catalog")) return;
        var slices = new List<ISeries>();
        var options = new List<ChartSelection>();
        for (int i = 0; i < values.Count; i++)
        {
            var value = values[i];
            if (value.Value <= 0) continue;
            int index = options.Count;
            var slice = new PieSeries<double> { Values = new[] { value.Value }, Name = $"{value.Name}: {value.Value:N0}",
                InnerRadius = 58, Fill = new SolidColorPaint(StockColors[i % StockColors.Length]), Stroke = null, HoverPushout = 4 };
            slice.DataPointerDown += (_, _) => OpenPoint(index);
            slices.Add(slice);
            options.Add(new($"{value.Name}: {value.Value:N0} products", StockState: value.Name));
        }
        ShowChart(new PieChart { Series = slices, LegendPosition = LegendPosition.Bottom, LegendTextSize = 11,
            AnimationsSpeed = TimeSpan.FromMilliseconds(250), TooltipTextSize = 12 }, options);
    }

    public void StackedStock(List<CategoryStock> values)
    {
        if (!Prepare("Stock readiness - current", values.Any(v => v.Available + v.LowStock + v.OutOfStock > 0), "No products in inventory")) return;
        string[] states = { "Available", "Low stock", "Out of stock" };
        var series = new List<ISeries>();
        for (int state = 0; state < 3; state++)
        {
            int stateIndex = state;
            var rows = new StackedRowSeries<double> { Name = states[state], Values = values.Select(v =>
                (double)(state == 0 ? v.Available : state == 1 ? v.LowStock : v.OutOfStock)).ToArray(),
                Fill = new SolidColorPaint(StockColors[state]), Stroke = null, MaxBarWidth = 22 };
            rows.DataPointerDown += (_, points) => { var point = points.FirstOrDefault(); if (point != null) OpenPoint(point.Index * 3 + stateIndex); };
            series.Add(rows);
        }
        var chart = Cartesian(series.ToArray(), NumberAxis("Products"), CategoryAxis(values.Select(v =>
            v.Category == "Motherboard" ? "Board" : v.Category == "Power Supply" ? "PSU" : v.Category).ToArray()));
        chart.LegendPosition = LegendPosition.Bottom; chart.LegendTextSize = 10;
        ShowChart(chart, values.SelectMany(v => states.Select((state, i) => new ChartSelection(
            $"{v.Category} - {state}: {(i == 0 ? v.Available : i == 1 ? v.LowStock : v.OutOfStock)}", Category: v.Category, StockState: state))).ToList());
    }
}
