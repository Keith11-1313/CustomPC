using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CustomPC;

// Load our bundled fonts only into this process. Nothing is installed in Windows.
internal static class AppFonts
{
    private static readonly PrivateFontCollection fonts = new PrivateFontCollection();
    // Windows drawing APIs need these buffers for the lifetime of the application.
    private static readonly List<IntPtr> fontMemory = new List<IntPtr>();

    static AppFonts()
    {
        foreach (string file in new[] { "Righteous-Regular.ttf", "Roboto-Regular.ttf", "Roboto-Bold.ttf" })
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomPC.Assets.Fonts." + file)!;
            using MemoryStream copy = new MemoryStream();
            stream.CopyTo(copy);
            byte[] bytes = copy.ToArray();
            IntPtr memory = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, memory, bytes.Length);
            fonts.AddMemoryFont(memory, bytes.Length);
            // Native text boxes and dropdowns use GDI; labels can also use GDI+.
            IntPtr handle = AddFontMemResourceEx(memory, (uint)bytes.Length, IntPtr.Zero, out uint count);
            if (handle == IntPtr.Zero || count == 0) throw new InvalidOperationException("A bundled application font could not be loaded.");
            fontMemory.Add(memory);
        }
    }

    public static Font Heading(float size) => new Font(fonts.Families.First(f => f.Name == "Righteous"), size, FontStyle.Regular);

    public static void Apply(Control control)
    {
        control.Font = Resolve(control.Font);
        if (control is DataGridView grid)
        {
            if (grid.ColumnHeadersDefaultCellStyle.Font != null)
                grid.ColumnHeadersDefaultCellStyle.Font = Resolve(grid.ColumnHeadersDefaultCellStyle.Font);
            if (grid.DefaultCellStyle.Font != null)
                grid.DefaultCellStyle.Font = Resolve(grid.DefaultCellStyle.Font);
        }
        foreach (Control child in control.Controls) Apply(child);
    }

    private static Font Resolve(Font original)
    {
        // Designer keeps ordinary font-name assignments. Windows may silently substitute
        // an installed font, so recover the requested name and use our private font family.
        string name = original.OriginalFontName ?? original.Name;
        if (name != "Righteous" && name != "Roboto") return original;
        if (original.Name == name) return original;
        FontFamily family = fonts.Families.First(f => f.Name == name);
        return new Font(family, original.Size, name == "Righteous" ? FontStyle.Regular : original.Style, original.Unit);
    }

    [DllImport("gdi32.dll")]
    private static extern IntPtr AddFontMemResourceEx(IntPtr font, uint length, IntPtr reserved, out uint count);
}
