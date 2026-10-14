using System.Reflection;
using Svg;

namespace CustomPC;

// Load the original SVG assets once; SVG.NET renders their paths at the requested size.
// There are no PNG conversions, bitmap caches or hand-written icon shapes.
internal static class IconAssets
{
    private static readonly Dictionary<(string Name, int Color), SvgDocument> icons = new();
    static IconAssets()
    {
        SvgDocument.ResolveExternalElements = ExternalType.None;
        SvgDocument.ResolveExternalImages = ExternalType.None;
    }

    public static void Paint(Graphics graphics, string icon, Rectangle bounds, Color color)
    {
        if (icon == "") return;
        string name = icon switch { "email" => "mail", "lock" => "lock-keyhole", "back" => "arrow-left",
            "add" => "plus", "refresh" => "refresh-cw", "grid" => "layout-grid", "list" => "clipboard-list",
            "chart" => "chart-no-axes-column", "user" => "user-round", "build" => "cpu", _ => icon };
        var key = (name, color.ToArgb());
        if (!icons.TryGetValue(key, out SvgDocument? document))
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CustomPC.Assets.Icons." + name + ".svg")
                ?? throw new InvalidOperationException("Missing icon asset: " + name);
            using StreamReader reader = new(stream);
            string source = reader.ReadToEnd().Replace("currentColor", ColorTranslator.ToHtml(color));
            document = SvgDocument.FromSvg<SvgDocument>(source);
            icons.Add(key, document);
        }
        var state = graphics.Save();
        try
        {
            graphics.TranslateTransform(bounds.X, bounds.Y);
            graphics.ScaleTransform(bounds.Width / document.ViewBox.Width, bounds.Height / document.ViewBox.Height);
            document.Draw(graphics);
        }
        finally { graphics.Restore(state); }
    }
}
