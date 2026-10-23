namespace CustomPC;

internal sealed class ProductTablePhotos : IDisposable
{
    private readonly Dictionary<string, (string Key, Image Image)> thumbnails = new();
    public Image Read(Part part, string databasePath)
    {
        string key = part.PhotoFileName;
        try { if (key.Length > 0) key += "|" + File.GetLastWriteTimeUtc(ProductPhotos.Resolve(databasePath, part.PhotoFileName)).Ticks; }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException) { key += "|unavailable"; }
        if (thumbnails.TryGetValue(part.Id, out var cached) && cached.Key == key) return cached.Image;
        using Image? source = ProductPhotos.ReadForDisplay(databasePath, part.PhotoFileName);
        Bitmap thumbnail = new(96, 80);
        using (Graphics graphics = Graphics.FromImage(thumbnail))
        {
            graphics.Clear(Color.White);
            if (source != null)
            {
                float scale = Math.Min(88F / source.Width, 72F / source.Height);
                float width = source.Width * scale, height = source.Height * scale;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(source, (96 - width) / 2, (80 - height) / 2, width, height);
            }
            else
            {
                using Font font = new("Segoe UI", 9);
                TextRenderer.DrawText(graphics, part.Category + "\nNo photo", font, new Rectangle(2, 2, 92, 76), StoreTheme.Muted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            }
        }
        if (thumbnails.TryGetValue(part.Id, out cached)) cached.Image.Dispose();
        thumbnails[part.Id] = (key, thumbnail);
        return thumbnail;
    }

    public void Dispose() { foreach (var item in thumbnails.Values) item.Image.Dispose(); thumbnails.Clear(); }
}
