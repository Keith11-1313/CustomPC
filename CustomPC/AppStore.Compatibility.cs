using Microsoft.Data.Sqlite;

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

            if (selectedCount != 1)
            {
                errors.Add("Select one " + category + ".");
            }
        }

        Part? cpu = null;
        Part? motherboard = null;
        Part? ram = null;
        Part? powerSupply = null;
        int componentWatts = 0;

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
            }

            if (part.Category != "Power Supply")
            {
                componentWatts += part.Watts;
            }
        }

        if (RuleIsEnabled("Socket") && cpu != null && motherboard != null)
        {
            bool matchingSockets = cpu.Socket.Equals(motherboard.Socket, StringComparison.OrdinalIgnoreCase);
            if (cpu.Socket.Length == 0 || !matchingSockets)
            {
                errors.Add("CPU and motherboard sockets do not match or are unspecified.");
            }
        }

        if (RuleIsEnabled("Memory") && ram != null && motherboard != null)
        {
            bool matchingMemory = ram.MemoryType.Equals(motherboard.MemoryType, StringComparison.OrdinalIgnoreCase);
            if (ram.MemoryType.Length == 0 || !matchingMemory)
            {
                errors.Add("RAM and motherboard memory types do not match or are unspecified.");
            }
        }

        if (RuleIsEnabled("Power") && powerSupply != null)
        {
            int requiredWatts = componentWatts + 100;
            if (powerSupply.Watts <= 0 || powerSupply.Watts < requiredWatts)
            {
                errors.Add("Power supply needs at least estimated component wattage plus 100 W headroom.");
            }
        }

        return errors;
    }

    private bool RuleIsEnabled(string kind)
    {
        foreach (CompatibilityRule rule in Rules)
        {
            if (rule.Kind == kind && rule.Enabled)
            {
                return true;
            }
        }

        return false;
    }

    public void SaveRule(CompatibilityRule rule)
    {
        using (SqliteConnection connection = database.OpenConnection())
        using (SqliteTransaction transaction = connection.BeginTransaction())
        {
            try
            {
                LoadData(connection);
                RequireAdmin();

                bool supportedKind = rule.Kind == "Socket" || rule.Kind == "Memory" || rule.Kind == "Power";
                if (string.IsNullOrWhiteSpace(rule.Name) || !supportedKind)
                {
                    throw new InvalidOperationException("Enter a rule name and supported rule type.");
                }

                CompatibilityRule? existingRule = null;
                foreach (CompatibilityRule savedRule in Rules)
                {
                    if (savedRule.Id == rule.Id)
                    {
                        existingRule = savedRule;
                    }
                    else if (savedRule.Kind == rule.Kind)
                    {
                        throw new InvalidOperationException("Edit the existing rule of this type.");
                    }
                }

                if (existingRule != null)
                {
                    Rules.Remove(existingRule);
                }

                Rules.Add(rule);
                SaveData(connection, transaction);
            }
            catch
            {
                UndoFailedChange(connection, transaction);
                throw;
            }
        }
    }
}
