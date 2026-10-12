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
