namespace CustomPC;

// In Visual Studio, keep editable store data beside the solution, outside source assets.
public static class StoreDataPath
{
    public static string DefaultDatabase()
    {
        foreach (string start in new[] { AppContext.BaseDirectory, Environment.CurrentDirectory })
            for (DirectoryInfo? folder = new(start); folder != null; folder = folder.Parent)
                if (File.Exists(Path.Combine(folder.FullName, "CustomPC.slnx")))
                    return Path.Combine(folder.FullName, "Database", "custompc.db");

        // If the app is copied elsewhere, its Database folder travels with it.
        return Path.Combine(AppContext.BaseDirectory, "Database", "custompc.db");
    }
}
