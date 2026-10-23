namespace CustomPC;

internal sealed class SearchClearButton : Button
{
    private bool hovering;
    public SearchClearButton()
    {
        Name = "clearSearch"; Text = "×"; AccessibleName = "Clear search and active search filter";
        FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseOverBackColor = StoreTheme.Selection;
        BackColor = Color.White; ForeColor = StoreTheme.Muted;
        Font = new Font("Segoe UI", 11); Cursor = Cursors.Hand;
        TextAlign = ContentAlignment.MiddleCenter;
        Padding = Padding.Empty;
        UseCompatibleTextRendering = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        MouseEnter += (_, _) => { hovering = true; Invalidate(); };
        MouseLeave += (_, _) => { hovering = false; Invalidate(); };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using var background = new SolidBrush(hovering ? StoreTheme.Selection : Color.White);
        e.Graphics.FillRectangle(background, ClientRectangle);
        // Use a pixel-sized glyph independent of inherited body fonts and padding.
        using var glyph = new Font("Segoe UI", Math.Min(20, Height - 8), FontStyle.Regular, GraphicsUnit.Pixel);
        TextRenderer.DrawText(e.Graphics, "×", glyph, ClientRectangle, StoreTheme.Muted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        if (Focused) ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -3, -3));
    }
}
