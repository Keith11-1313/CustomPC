using System.ComponentModel;
using System.Runtime.InteropServices;

namespace CustomPC;

// A normal PictureBox with the supplied logo. Its image is visible in the Designer too.
public sealed class BrandLogo : PictureBox
{
    private static readonly Bitmap logo = ReadLogo();
    public static readonly Icon WindowIcon = CreateIcon();

    public BrandLogo()
    {
        base.Image = logo;
        SizeMode = PictureBoxSizeMode.Zoom;
        TabStop = false;
        AccessibleName = "CustomPC logo";
    }

    // The asset is managed in Assets/logo.png, rather than copied into every form's resource file.
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public new Image? Image { get => base.Image; set => base.Image = value; }

    // Reuse the same embedded artwork on printed documents, preserving its proportions.
    internal static void Draw(Graphics graphics, RectangleF bounds)
    {
        float scale = Math.Min(bounds.Width / logo.Width, bounds.Height / logo.Height);
        float width = logo.Width * scale, height = logo.Height * scale;
        graphics.DrawImage(logo, bounds.X + (bounds.Width - width) / 2,
            bounds.Y + (bounds.Height - height) / 2, width, height);
    }

    private static Bitmap ReadLogo()
    {
        using Stream stream = typeof(BrandLogo).Assembly.GetManifestResourceStream("CustomPC.Assets.logo.png")!;
        using Bitmap source = new Bitmap(stream);
        return new Bitmap(source);
    }

    private static Icon CreateIcon()
    {
        using Bitmap square = new Bitmap(64, 64);
        using (Graphics drawing = Graphics.FromImage(square)) drawing.DrawImage(logo, new Rectangle(0, 10, 64, 43));
        IntPtr handle = square.GetHicon();
        try { return (Icon)Icon.FromHandle(handle).Clone(); }
        finally { DestroyIcon(handle); }
    }

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr handle);
}
