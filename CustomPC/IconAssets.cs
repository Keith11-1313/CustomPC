using System.Reflection;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace CustomPC;

// Load each transparent PNG once. Windows drawing supplies scaling and colour tinting.
internal static class IconAssets
{
    private static readonly Dictionary<string, Bitmap> icons = new();

    public static void Paint(Graphics graphics, string icon, Rectangle bounds, Color color)
    {
        if (icon == "") return;
        string name = icon switch { "email" => "mail", "lock" => "lock-keyhole", "back" => "arrow-left",
            "add" => "plus", "refresh" => "refresh-cw", "grid" => "layout-grid", "list" => "clipboard-list",
            "chart" => "chart-no-axes-column", "user" => "user-round", "build" => "cpu", _ => icon };
        if (!icons.TryGetValue(name, out Bitmap? bitmap))
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomPC.Assets.Icons." + name + ".png")
                ?? throw new InvalidOperationException("Missing icon asset: " + name);
            using Image source = Image.FromStream(stream);
            bitmap = new Bitmap(source); // Detach from the resource stream before it closes.
            icons.Add(name, bitmap);
        }
        // The PNG alpha defines the shape; tint it for normal, disabled and dark surfaces.
        using ImageAttributes attributes = new();
        attributes.SetColorMatrix(new ColorMatrix(new float[][] {
            new float[] { 0, 0, 0, 0, 0 },
            new float[] { 0, 0, 0, 0, 0 },
            new float[] { 0, 0, 0, 0, 0 },
            new float[] { 0, 0, 0, color.A / 255F, 0 },
            new float[] { color.R / 255F, color.G / 255F, color.B / 255F, 0, 1 }
        }));
        var state = graphics.Save();
        try
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.DrawImage(bitmap, bounds, 0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, attributes);
        }
        finally { graphics.Restore(state); }
    }
}
