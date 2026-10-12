namespace CustomPC;

// Unexpected errors go to a local diagnostic log. Input validation is shown directly.
internal static class AppLog
{
    public static string Folder => Path.Combine(Path.GetDirectoryName(
        Environment.GetEnvironmentVariable("CUSTOMPC_DATABASE_PATH") ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CustomPC", "custompc.db"))!, "Logs");

    public static string Write(Exception error)
    {
        string reference = Guid.NewGuid().ToString("N")[..8];
        try
        {
            Directory.CreateDirectory(Folder);
            // Do not log UI input, passwords, account hashes or database contents.
            string line = $"{DateTime.UtcNow:O} [{reference}] {error.GetType().FullName}\n{error.StackTrace}\n";
            File.AppendAllText(Path.Combine(Folder, DateTime.Today.ToString("yyyy-MM-dd") + ".log"), line);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return reference;
    }
}
