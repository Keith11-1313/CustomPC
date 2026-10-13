using System.Drawing.Imaging;

namespace CustomPC;

// Photos are local store data. Never retain an uploaded file's original external path.
public static class ProductPhotos
{
    public static string Folder(string databasePath) => Path.Combine(Path.GetDirectoryName(databasePath)!, "ProductImages");

    public static string Resolve(string databasePath, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName is "." or ".." || Path.IsPathRooted(fileName) || Path.GetFileName(fileName) != fileName
            || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new InvalidOperationException("The product photo filename is invalid.");
        return Path.Combine(Folder(databasePath), fileName);
    }

    public static Image? Read(string databasePath, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return null;
        try
        {
            using Image original = Image.FromFile(Resolve(databasePath, fileName));
            return new Bitmap(original); // Detach from the file: editors and backups can use it safely.
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException or OutOfMemoryException or InvalidOperationException)
        { return null; }
    }

    public static string Import(string databasePath, string source)
    {
        FileInfo info = new(source);
        if (!info.Exists || info.Length > 15 * 1024 * 1024)
            throw new InvalidOperationException("Choose a JPG or PNG photograph up to 15 MB.");
        using Image original = Image.FromFile(source);
        if (original.RawFormat.Guid != ImageFormat.Jpeg.Guid && original.RawFormat.Guid != ImageFormat.Png.Guid)
            throw new InvalidOperationException("Only JPG and PNG photographs are supported.");
        if (original.Width > 10000 || original.Height > 10000)
            throw new InvalidOperationException("The photograph dimensions are too large.");
        Directory.CreateDirectory(Folder(databasePath));
        string fileName = Guid.NewGuid().ToString("N") + ".png";
        string target = Resolve(databasePath, fileName);
        bool created = false;
        try
        {
            double scale = Math.Min(1, 1600.0 / Math.Max(original.Width, original.Height));
            using Bitmap copy = new(Math.Max(1, (int)(original.Width * scale)), Math.Max(1, (int)(original.Height * scale)));
            using (Graphics graphics = Graphics.FromImage(copy))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(original, new Rectangle(Point.Empty, copy.Size));
            }
            using FileStream file = new(target, FileMode.CreateNew);
            created = true;
            copy.Save(file, ImageFormat.Png);
            return fileName;
        }
        catch { if (created && File.Exists(target)) File.Delete(target); throw; }
    }

    public static Image? ReadForDisplay(string databasePath, string fileName)
    {
        using Image? image = Read(databasePath, fileName);
        if (image == null) return null;
        using Bitmap bitmap = new(image);
        int left = bitmap.Width, top = bitmap.Height, right = 0, bottom = 0;
        // Crop only the UI copy's empty margins. The saved source photograph stays intact.
        int step = Math.Max(1, Math.Max(bitmap.Width, bitmap.Height) / 160);
        for (int y = 0; y < bitmap.Height; y += step)
            for (int x = 0; x < bitmap.Width; x += step)
            {
                Color pixel = bitmap.GetPixel(x, y);
                if (pixel.A < 20 || pixel.R > 245 && pixel.G > 245 && pixel.B > 245) continue;
                left = Math.Min(left, x); top = Math.Min(top, y);
                right = Math.Max(right, x); bottom = Math.Max(bottom, y);
            }
        Rectangle bounds = left > right ? new Rectangle(Point.Empty, bitmap.Size)
            : Rectangle.FromLTRB(Math.Max(0, left - step * 2), Math.Max(0, top - step * 2),
                Math.Min(bitmap.Width, right + step * 3), Math.Min(bitmap.Height, bottom + step * 3));
        double scale = Math.Min(1, 512.0 / Math.Max(bounds.Width, bounds.Height));
        Bitmap display = new(Math.Max(1, (int)(bounds.Width * scale)), Math.Max(1, (int)(bounds.Height * scale)));
        using Graphics graphics = Graphics.FromImage(display);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.DrawImage(bitmap, new Rectangle(Point.Empty, display.Size), bounds, GraphicsUnit.Pixel);
        return display;
    }
}
