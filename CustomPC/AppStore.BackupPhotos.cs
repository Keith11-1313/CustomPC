using System.IO.Compression;
using Microsoft.Data.Sqlite;

namespace CustomPC;

public sealed partial class AppStore
{
    private void CreateCompleteBackup(string target)
    {
        target = Path.GetFullPath(target);
        if (File.Exists(target)) throw new InvalidOperationException("Choose a new backup filename.");
        string staging = Path.Combine(Path.GetTempPath(), "CustomPC-backup-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        bool created = false;
        try
        {
            CopyDatabase(DatabasePath, Path.Combine(staging, "custompc.db"));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            using FileStream file = new(target, FileMode.CreateNew);
            created = true;
            using ZipArchive archive = new(file, ZipArchiveMode.Create);
            archive.CreateEntryFromFile(Path.Combine(staging, "custompc.db"), "custompc.db");
            string photos = ProductPhotos.Folder(DatabasePath);
            if (Directory.Exists(photos))
                foreach (string photo in Directory.EnumerateFiles(photos))
                    archive.CreateEntryFromFile(photo, "ProductImages/" + Path.GetFileName(photo));
        }
        catch { if (created && File.Exists(target)) File.Delete(target); throw; }
        finally { Directory.Delete(staging, true); }
    }

    private void RestoreCompleteBackup(string source, string password)
    {
        string staging = Path.Combine(Path.GetTempPath(), "CustomPC-restore-" + Guid.NewGuid().ToString("N"));
        string stagedPhotos = Path.Combine(staging, "ProductImages");
        Directory.CreateDirectory(stagedPhotos);
        string livePhotos = ProductPhotos.Folder(DatabasePath);
        string savedPhotos = livePhotos + ".recovery-" + Guid.NewGuid().ToString("N");
        string safetyDatabase = Path.Combine(staging, "safety.db");
        bool databaseChanged = false;
        bool photosMoved = false;
        bool retainRecoveryFiles = false;
        try
        {
            using (ZipArchive archive = ZipFile.OpenRead(source))
            {
                long total = 0;
                HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string name = entry.FullName;
                    if (!names.Add(name)) throw new InvalidOperationException("The backup contains duplicate entries.");
                    if (name == "ProductImages/") continue;
                    bool databaseEntry = name == "custompc.db";
                    bool photoEntry = name.StartsWith("ProductImages/", StringComparison.Ordinal)
                        && name[14..].Length > 0 && Path.GetFileName(name) == name[14..]
                        && !name.Contains('\\');
                    if (!databaseEntry && !photoEntry) throw new InvalidOperationException("The backup contains an unexpected path.");
                    total += entry.Length;
                    if (entry.Length > (databaseEntry ? 512L * 1024 * 1024 : 15L * 1024 * 1024)
                        || total > 1024L * 1024 * 1024 || names.Count > 10000)
                        throw new InvalidOperationException("This backup exceeds the supported recovery size.");
                    string destination = databaseEntry ? Path.Combine(staging, "custompc.db") : ProductPhotos.Resolve(Path.Combine(staging, "custompc.db"), name[14..]);
                    entry.ExtractToFile(destination);
                }
            }
            string stagedDatabase = Path.Combine(staging, "custompc.db");
            if (!File.Exists(stagedDatabase)) throw new InvalidOperationException("This backup has no database.");
            // Validate image bytes before replacing any store data.
            foreach (string photo in Directory.EnumerateFiles(stagedPhotos))
            {
                using Image? image = ProductPhotos.Read(stagedDatabase, Path.GetFileName(photo));
                if (image == null) throw new InvalidOperationException("The backup contains an unreadable photograph.");
            }
            CopyDatabase(DatabasePath, safetyDatabase);
            // Existing database recovery validates accounts and all linked records first.
            RestoreBackup(stagedDatabase, password);
            databaseChanged = true;
            if (Directory.Exists(livePhotos)) { Directory.Move(livePhotos, savedPhotos); photosMoved = true; }
            Directory.Move(stagedPhotos, livePhotos);
            // Recovery is complete. A locked old photo must not undo a successful restore.
            if (photosMoved)
            {
                try { Directory.Delete(savedPhotos, true); }
                catch (IOException) { /* Keep the recovery folder for later cleanup. */ }
                catch (UnauthorizedAccessException) { /* Keep the recovery folder for later cleanup. */ }
            }
        }
        catch (Exception restoreError)
        {
            try
            {
                if (databaseChanged)
                {
                    using SqliteConnection safety = new(new SqliteConnectionStringBuilder { DataSource = safetyDatabase, Mode = SqliteOpenMode.ReadOnly, Pooling = false }.ToString());
                    using SqliteConnection live = database.OpenConnection();
                    safety.Open(); safety.BackupDatabase(live);
                    LoadData(live);
                }
                if (photosMoved && Directory.Exists(savedPhotos))
                {
                    if (Directory.Exists(livePhotos)) Directory.Delete(livePhotos, true);
                    Directory.Move(savedPhotos, livePhotos);
                }
            }
            catch (Exception rollbackError)
            {
                retainRecoveryFiles = true;
                throw new InvalidOperationException("Recovery could not finish. Preserve the before-restore ZIP and recovery files at " + staging
                    + " and " + savedPhotos + ". Ask a maintainer to recover them before continuing.", new AggregateException(restoreError, rollbackError));
            }
            throw;
        }
        finally { if (!retainRecoveryFiles) Directory.Delete(staging, true); }
    }
}
