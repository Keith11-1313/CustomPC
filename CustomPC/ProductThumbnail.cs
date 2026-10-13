using System.ComponentModel;

namespace CustomPC;

public partial class ProductThumbnail : UserControl
{
    private string displayedKey = "";
    public ProductThumbnail() { InitializeComponent(); }
    [DefaultValue("Photo unavailable")]
    public string PlaceholderText { get => placeholder.Text; set => placeholder.Text = value; }
    public void Display(Part? part, string databasePath)
    {
        string fileName = part?.PhotoFileName ?? "";
        string key = databasePath + "|" + fileName;
        if (fileName.Length > 0)
        {
            try { key += "|" + File.GetLastWriteTimeUtc(ProductPhotos.Resolve(databasePath, fileName)).Ticks; }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidOperationException) { key += "|unavailable"; }
        }
        if (key == displayedKey) return;
        displayedKey = key;
        photo.Image?.Dispose();
        photo.Image = part == null ? null : ProductPhotos.ReadForDisplay(databasePath, part.PhotoFileName);
        placeholder.Visible = photo.Image == null;
        photo.Visible = photo.Image != null;
        placeholder.Text = part == null ? "Choose part" : "Photo unavailable";
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) photo.Image?.Dispose();
        base.Dispose(disposing);
    }
}
