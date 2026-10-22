namespace CustomPC;

// Let Windows paint a complete frame before displaying a resized or scrolled table.
public class StoreGrid : DataGridView
{
    public StoreGrid()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }
}
