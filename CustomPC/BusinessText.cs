using System.Globalization;
using System.Text;

namespace CustomPC;

internal static class BusinessText
{
    // Preserve letters, accents, model punctuation and technical symbols; reject pictographs and hidden controls.
    internal static void Validate(string value, string field)
    {
        foreach (Rune rune in value.EnumerateRunes())
        {
            int c = rune.Value;
            var category = Rune.GetUnicodeCategory(rune);
            bool emoji = c is >= 0x1F000 and <= 0x1FAFF or >= 0x2600 and <= 0x27BF or
                0x203C or 0x2049 or 0x2139 or
                >= 0x2194 and <= 0x2199 or 0x21A9 or 0x21AA or 0x231A or 0x231B or
                0x2328 or 0x23CF or >= 0x23E9 and <= 0x23F3 or >= 0x23F8 and <= 0x23FA or
                0x24C2 or 0x25AA or 0x25AB or 0x25B6 or 0x25C0 or 0x25FB or 0x25FC or 0x25FD or 0x25FE or
                0x2934 or 0x2935 or >= 0x2B05 and <= 0x2B07 or 0x2B1B or 0x2B1C or 0x2B50 or 0x2B55 or
                0x3030 or 0x303D or 0x3297 or 0x3299 or 0x20E3 or 0xFE0F;
            if (emoji || category is UnicodeCategory.Format or UnicodeCategory.Surrogate ||
                category == UnicodeCategory.Control && c is not (9 or 10 or 13))
                throw new InvalidOperationException($"{field} cannot contain emojis or hidden control characters. Use text instead.");
        }
    }
}
