using System.Text.RegularExpressions;

namespace CustomPC;

// Translate the saved semicolon-separated descriptions into category-aware labels.
// Unknown descriptions remain visible as Additional detail; nothing is guessed or discarded.
public static class ProductSpecifications
{
    public static List<KeyValuePair<string, string>> Rows(Part part)
    {
        var rows = new List<KeyValuePair<string, string>>();
        void Add(string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value) && !rows.Any(r => r.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
                rows.Add(new(key, value));
        }
        Add("Brand", part.Brand);
        Add("Category", part.Category);
        foreach (string value in part.Specifications.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string text = value.ToLowerInvariant();
            if (part.Category == "CPU")
            {
                Match cores = Regex.Match(value, @"^(.*?) cores / (\d+) threads$", RegexOptions.IgnoreCase);
                if (cores.Success) { Add("Cores", cores.Groups[1].Value); Add("Threads", cores.Groups[2].Value); continue; }
            }
            string label = text switch
            {
                _ when text.Contains("cores") && text.Contains("threads") => "Cores / threads",
                _ when text.Contains("ghz") && text.Contains("base") => "Base frequency",
                _ when text.Contains("ghz") => "Maximum boost frequency",
                _ when text.Contains("maximum turbo power") => "Maximum turbo power",
                _ when text.Contains("tdp") => "Thermal design power",
                _ when text.Contains("graphics") => "Integrated graphics",
                _ when text.Contains("recommended") && text.Contains("psu") => "Recommended power supply",
                _ when text.Contains("board power") || text.Contains("consumption") => "Graphics power",
                _ when text.Contains("gddr") => "Video memory",
                _ when text.EndsWith("length") => "Card length",
                _ when text.Contains("gpu clearance") => "GPU clearance",
                _ when text.Contains("fans") => "Included cooling",
                _ when text is "black" or "white" or "silver" => "Colour",
                _ when text.Contains("modular") => "Cabling",
                _ when text.Contains("80 plus") => "Efficiency certification",
                _ when text.Contains("connector") => "Power connector",
                _ when text.Contains(" w") && part.Category == "Power Supply" => "Rated output",
                _ when text.Contains("wi-fi") => "Wireless connectivity",
                _ when text.Contains("hdmi") || text.Contains("displayport") => "Display outputs",
                _ when text.Contains("b550") || text.Contains("b650") || text.Contains("b660") || text.Contains("b760") => "Chipset",
                _ when text is "am4" or "am5" or "lga1700" => "Socket",
                _ when text.Contains("ddr") && part.Category == "Motherboard" => "Memory support",
                _ when text.Contains("ddr") && part.Category == "RAM" => "Memory type / speed",
                _ when text.Contains("gb") || text.Contains("tb") => "Capacity / configuration",
                _ when text.StartsWith("cl") => "CAS latency",
                _ when text.Contains("xmp") || text.Contains("expo") => "Memory profile",
                _ when text.Contains("nvme") => "Interface",
                _ when text.StartsWith("m.2") => "Form factor",
                _ when text.Contains("heatsink") => "Heatsink",
                _ when text.Contains("micro-atx") || text.Contains("mid tower") => "Form factor / board support",
                _ when text.Contains("mesh") => "Front panel",
                _ when text.Contains("tempered glass") => "Side panel",
                _ when text.Contains("variant") || text.Contains("model") || text.Contains("generation") => "Variant",
                _ => "Additional detail"
            };
            // Admin-entered labelled values are supported without requiring new code.
            int colon = value.IndexOf(':');
            if (colon > 0) Add(value[..colon].Trim(), value[(colon + 1)..].Trim());
            else Add(label, value);
        }
        Add("Socket", part.Socket);
        if (!rows.Any(r => r.Key.Contains("Memory", StringComparison.OrdinalIgnoreCase))) Add("Memory type", part.MemoryType);
        if (part.RecommendedPsuWatts > 0 && !rows.Any(r => r.Key == "Recommended power supply")) Add("Recommended power supply", part.RecommendedPsuWatts + " W");
        return rows;
    }
}
