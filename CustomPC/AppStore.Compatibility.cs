

namespace CustomPC;

public sealed partial class AppStore
{
    public List<string> CheckBuild(List<Part> selectedParts)
    {
        List<string> errors = new List<string>();

        foreach (string category in Categories)
        {
            int selectedCount = 0;
            foreach (Part part in selectedParts)
            {
                if (part.Category == category)
                {
                    selectedCount++;
                }
            }

            if (category == "GPU")
            {
                if (selectedCount > 1) errors.Add("Select at most one GPU.");
            }
            else if (selectedCount != 1)
            {
                errors.Add("Select one " + category + ".");
            }
        }

        Part? cpu = null;
        Part? motherboard = null;
        Part? ram = null;
        Part? powerSupply = null;
        bool hasDedicatedGpu = false;
        long componentWatts = 0;

        foreach (Part part in selectedParts)
        {
            if (part.Archived || Available(part) < 1)
            {
                errors.Add(part.Name + " is unavailable.");
            }

            switch (part.Category)
            {
                case "CPU":
                    cpu = part;
                    break;
                case "Motherboard":
                    motherboard = part;
                    break;
                case "RAM":
                    ram = part;
                    break;
                case "Power Supply":
                    powerSupply = part;
                    break;
                case "GPU":
                    hasDedicatedGpu = true;
                    break;
            }

            if (part.Category != "Power Supply")
            {
                componentWatts += part.Watts;
            }
        }

        // Graphics output is required even when the dedicated GPU is optional.
        if (!hasDedicatedGpu && cpu != null)
        {
            if (!cpu.HasIntegratedGraphics)
                errors.Add("This CPU requires a dedicated GPU.");
            else if (motherboard != null && !motherboard.HasVideoOutput)
                errors.Add("The motherboard needs a video output to use integrated graphics.");
        }

        if (cpu != null && motherboard != null)
        {
            bool matchingSockets = cpu.Socket.Equals(motherboard.Socket, StringComparison.OrdinalIgnoreCase);
            if (cpu.Socket.Length == 0 || !matchingSockets)
            {
                errors.Add("CPU and motherboard sockets do not match or are unspecified.");
            }
        }

        if (ram != null && motherboard != null)
        {
            bool matchingMemory = ram.MemoryType.Equals(motherboard.MemoryType, StringComparison.OrdinalIgnoreCase);
            if (ram.MemoryType.Length == 0 || !matchingMemory)
            {
                errors.Add("RAM and motherboard memory types do not match or are unspecified.");
            }
        }

        if (powerSupply != null)
        {
            long requiredWatts = Math.Max(componentWatts + 100, selectedParts.Select(part => part.RecommendedPsuWatts).DefaultIfEmpty(0).Max());
            if (powerSupply.Watts <= 0 || powerSupply.Watts < requiredWatts)
            {
                errors.Add("Power supply needs at least estimated component wattage plus 100 W headroom and the component's recommended PSU capacity.");
            }
        }

        return errors;
    }

}
