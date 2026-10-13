namespace CustomPC;

public sealed partial class AppStore
{
    public void SavePartWithPhoto(Part part, string? importPath, bool removePhoto)
    {
        Refresh(); RequireAdmin();
        string? imported = null;
        try
        {
            if (importPath != null) imported = ProductPhotos.Import(DatabasePath, importPath);
            part.PhotoFileName = imported ?? (removePhoto ? "" : part.PhotoFileName);
            SavePart(part);
        }
        catch
        {
            if (imported != null) File.Delete(ProductPhotos.Resolve(DatabasePath, imported));
            throw;
        }
        // Older photos stay in backups and may be referenced by other parts. No eager deletion.
    }
}
