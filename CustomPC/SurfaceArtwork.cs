namespace CustomPC;

internal static class SurfaceArtwork
{
    private static readonly Dictionary<string, Image> images = new();
    private sealed record Artwork(Image Image, Color Tint);
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Control, Artwork> surfaces = new();
    public static void Attach(Control control, string file, Color? tint = null)
    {
        if (!images.TryGetValue(file, out Image? image))
        {
            using Stream stream = typeof(SurfaceArtwork).Assembly.GetManifestResourceStream("CustomPC.Assets." + file)!;
            using Image source = Image.FromStream(stream);
            image = new Bitmap(source); images.Add(file, image);
        }
        surfaces.Add(control, new Artwork(image, tint ?? Color.FromArgb(100, StoreTheme.Ink)));
        control.Paint += (_, e) => Paint(e.Graphics, control);
        control.Resize += (_, _) => control.Invalidate(true);
    }
    public static void Paint(Graphics graphics, Control target)
    {
        Control? root = target;
        Point offset = Point.Empty;
        while (root != null && !surfaces.TryGetValue(root, out _))
        {
            offset.Offset(root.Left, root.Top); root = root.Parent;
        }
        using SolidBrush fill = new(root?.BackColor ?? target.Parent?.BackColor ?? target.BackColor);
        graphics.FillRectangle(fill, target.ClientRectangle);
        if (root == null || !surfaces.TryGetValue(root, out Artwork? artwork)) return;
        Image image = artwork.Image;
        float scale = Math.Max(root.Width / (float)image.Width, root.Height / (float)image.Height);
        float width = image.Width * scale, height = image.Height * scale;
        var state = graphics.Save(); graphics.SetClip(target.ClientRectangle);
        graphics.DrawImage(image, (root.Width - width) / 2 - offset.X, (root.Height - height) / 2 - offset.Y, width, height);
        // Keep text readable while retaining the supplied texture.
        using SolidBrush shade = new(artwork.Tint);
        graphics.FillRectangle(shade, target.ClientRectangle); graphics.Restore(state);
    }
}
