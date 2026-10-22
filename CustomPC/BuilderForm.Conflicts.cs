namespace CustomPC;

public partial class BuilderForm
{
    // Compare a candidate against the other current choices without changing the draft.
    private string? CandidateConflict(Part candidate)
    {
        var parts = GetSelectedParts().Where(part => part.Category != candidate.Category).Append(candidate).ToList();
        Part? cpu = parts.FirstOrDefault(part => part.Category == "CPU");
        Part? board = parts.FirstOrDefault(part => part.Category == "Motherboard");
        Part? ram = parts.FirstOrDefault(part => part.Category == "RAM");
        Part? psu = parts.FirstOrDefault(part => part.Category == "Power Supply");
        var reasons = new List<string>();
        if (candidate.Category is "CPU" or "Motherboard" && cpu != null && board != null &&
            (cpu.Socket.Length == 0 || !cpu.Socket.Equals(board.Socket, StringComparison.OrdinalIgnoreCase)))
            reasons.Add($"{cpu.Name} uses {Known(cpu.Socket)}; {board.Name} uses {Known(board.Socket)}. Choose matching sockets.");
        if (candidate.Category is "Motherboard" or "RAM" && ram != null && board != null &&
            (ram.MemoryType.Length == 0 || !ram.MemoryType.Equals(board.MemoryType, StringComparison.OrdinalIgnoreCase)))
            reasons.Add($"{ram.Name} uses {Known(ram.MemoryType)} memory; {board.Name} requires {Known(board.MemoryType)}.");
        long required = AppStore.RequiredPowerSupplyWatts(parts);
        if (psu != null && (candidate.Category == "Power Supply" || candidate.Watts > 0 || candidate.RecommendedPsuWatts > 0) &&
            (psu.Watts <= 0 || psu.Watts < required))
            reasons.Add($"{psu.Name} supplies {psu.Watts:N0} W; these selections require at least {required:N0} W. Choose a higher-capacity PSU.");
        return reasons.Count == 0 ? null : string.Join("\n", reasons);
    }
    private static string Known(string value) => value.Length == 0 ? "an unspecified type" : value;
}
