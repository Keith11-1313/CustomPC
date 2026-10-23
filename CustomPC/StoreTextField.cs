using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace CustomPC;

// A padded surface around an ordinary Windows text box. Windows still handles
// typing, password masking, clipboard shortcuts and the text caret.
public class StoreTextField : UserControl
{
    private readonly TextBox input = new() { BorderStyle = BorderStyle.None };
    private readonly PasswordEyeButton passwordEye = new() { Visible = false };
    private readonly ToolTip eyeTip = new();
    private readonly SearchClearButton clearSearch = new() { Visible = false };
    private readonly Label filterChip = new() { Visible = false, AutoEllipsis = true, BackColor = StoreTheme.Selection, ForeColor = StoreTheme.Action, Padding = new Padding(6, 0, 6, 0), TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false };
    private string filterText = "";
    [DefaultValue("")]
    public string FilterText
    {
        get => filterText;
        set
        {
            if (filterText == value) return;
            filterText = value ?? ""; filterChip.Text = filterText;
            eyeTip.SetToolTip(filterChip, filterText);
            Height = (int)(40 * DeviceDpi / 96F);
            PerformLayout(); Invalidate();
        }
    }
    public event EventHandler? SearchCleared;
    public void ClearSearch()
    {
        FilterText = ""; input.Clear(); SearchCleared?.Invoke(this, EventArgs.Empty); input.Focus();
    }
    private bool showPasswordToggle;
    private bool eyeClickedWithMouse;
    public StoreTextField()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        BackColor = Color.White;
        Size = new Size(220, 40);
        Controls.Add(input);
        Controls.Add(passwordEye);
        Controls.Add(filterChip); Controls.Add(clearSearch);
        filterChip.FontChanged += (_, _) => { PerformLayout(); Invalidate(); };
        clearSearch.Click += (_, _) => ClearSearch();
        Load += (_, _) => PerformLayout();
        passwordEye.MouseDown += (_, _) => eyeClickedWithMouse = true;
        passwordEye.Click += (_, _) =>
        {
            int start = input.SelectionStart, length = input.SelectionLength;
            UseSystemPasswordChar = !UseSystemPasswordChar;
            input.Select(start, length);
            if (eyeClickedWithMouse) input.Focus();
            eyeClickedWithMouse = false;
        };
        UpdateEye();
        input.TextChanged += (_, _) => { PerformLayout(); OnTextChanged(EventArgs.Empty); };
        input.GotFocus += (_, _) => Invalidate();
        input.LostFocus += (_, _) => Invalidate();
        input.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape && IsSearch && (Text.Length > 0 || filterText.Length > 0))
            { ClearSearch(); e.SuppressKeyPress = true; return; }
            OnKeyDown(e);
        };
        input.KeyPress += (_, e) => OnKeyPress(e);
        Click += (_, _) => input.Focus();
    }

    [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public override string Text { get => input?.Text ?? ""; set { if (input != null) input.Text = value ?? ""; } }
    [DefaultValue("")]
    public string PlaceholderText { get => input.PlaceholderText; set => input.PlaceholderText = value; }
    [DefaultValue(false)]
    public bool UseSystemPasswordChar { get => input.UseSystemPasswordChar; set { input.UseSystemPasswordChar = value; UpdateEye(); } }
    [DefaultValue(false)]
    public bool ShowPasswordToggle
    {
        get => showPasswordToggle;
        set { showPasswordToggle = value; passwordEye.Visible = value; PerformLayout(); UpdateEye(); }
    }
    private void UpdateEye()
    {
        passwordEye.PasswordVisible = !input.UseSystemPasswordChar;
        passwordEye.AccessibleName = input.UseSystemPasswordChar ? "Show password" : "Hide password";
        eyeTip.SetToolTip(passwordEye, passwordEye.AccessibleName);
        passwordEye.Invalidate();
    }
    [DefaultValue(false)]
    public bool Multiline { get => input.Multiline; set { input.Multiline = value; PerformLayout(); } }
    [DefaultValue(false)]
    public bool ReadOnly { get => input.ReadOnly; set => input.ReadOnly = value; }
    [DefaultValue(32767)]
    public int MaxLength { get => input.MaxLength; set => input.MaxLength = value; }
    [DefaultValue(ScrollBars.None)]
    public ScrollBars ScrollBars { get => input.ScrollBars; set => input.ScrollBars = value; }
    public void Clear() => input.Clear();
    public void Select(int start, int length) => input.Select(start, length);
    public void ScrollToCaret() => input.ScrollToCaret();
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string[] Lines => input.Lines;
    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
        if (input != null && !input.Multiline && height > 0) height = Math.Max((int)(38 * DeviceDpi / 96F), height);
        base.SetBoundsCore(x, y, width, height, specified);
    }

    protected override void OnEnter(EventArgs e) { base.OnEnter(e); input.Focus(); }
    protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); if (input != null) { input.Font = Font; PerformLayout(); } }
    protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (input == null) return;
        int left = FieldIcon == "" ? 12 : 38;
        bool filtered = IsSearch && filterText.Length > 0;
        bool canClear = IsSearch && (Text.Length > 0 || filtered);
        int chipHeight = Math.Min(Height - 8, (int)(28 * DeviceDpi / 96F));
        filterChip.Visible = filtered;
        int chipWidth = filtered ? Math.Min(TextRenderer.MeasureText(filterText, filterChip.Font).Width + filterChip.Padding.Horizontal + 12,
            Math.Max(1, Width - left - 44 - (int)(80 * DeviceDpi / 96F))) : 0;
        filterChip.SetBounds(left, (Height - chipHeight) / 2, Math.Max(1, chipWidth), chipHeight);
        if (filtered) left += chipWidth + 8;
        clearSearch.Visible = canClear;
        clearSearch.SetBounds(Math.Max(0, Width - 36), Math.Max(2, (Height - 30) / 2), 30, 30);
        input.BackColor = Enabled ? BackColor : Color.FromArgb(241, 243, 242);
        input.ForeColor = StoreTheme.Ink;
        int inputTop = Multiline ? 10 : Math.Max(4, (Height - input.PreferredHeight) / 2);
        input.SetBounds(left, inputTop,
            Math.Max(1, Width - left - (showPasswordToggle || canClear ? 44 : 12)), Multiline ? Math.Max(1, Height - 20) : input.PreferredHeight);
        passwordEye.SetBounds(Math.Max(0, Width - 40), 3, 34, Math.Max(20, Height - 6));
        input.AccessibleName = AccessibleName ?? Name;
    }
    private bool IsSearch => Name.Contains("search", StringComparison.OrdinalIgnoreCase);
    private string FieldIcon => Name.Contains("search", StringComparison.OrdinalIgnoreCase) ? "search" :
        Name.Contains("email", StringComparison.OrdinalIgnoreCase) ? "email" :
        Name.Contains("password", StringComparison.OrdinalIgnoreCase) ? "lock" : "";
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? StoreTheme.Background);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using GraphicsPath path = StoreTheme.RoundedRectangle(new Rectangle(1, 1, Width - 3, Height - 3), 7);
        using SolidBrush fill = new(Enabled ? BackColor : Color.FromArgb(241, 243, 242));
        using Pen outline = new(ContainsFocus ? StoreTheme.Action : StoreTheme.Border, ContainsFocus ? 2 : 1);
        e.Graphics.FillPath(fill, path); e.Graphics.DrawPath(outline, path);
        IconAssets.Paint(e.Graphics, FieldIcon, new Rectangle(12, (Height - 18) / 2, 18, 18), StoreTheme.Muted);
    }
    protected override void Dispose(bool disposing) { if (disposing) eyeTip.Dispose(); base.Dispose(disposing); }
}
