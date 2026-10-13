namespace CustomPC;

partial class ProductThumbnail
{
    private PictureBox photo = null!;
    private Label placeholder = null!;
    private void InitializeComponent()
    {
        photo = new PictureBox();
        placeholder = new Label();
        photo.Dock = DockStyle.Fill;
        photo.SizeMode = PictureBoxSizeMode.Zoom;
        photo.BackColor = Color.White;
        photo.Visible = false;
        placeholder.Dock = DockStyle.Fill;
        placeholder.Text = "Photo unavailable";
        placeholder.TextAlign = ContentAlignment.MiddleCenter;
        placeholder.ForeColor = Color.FromArgb(92, 105, 98);
        placeholder.BackColor = Color.FromArgb(243, 245, 247);
        Controls.Add(photo);
        Controls.Add(placeholder);
        Size = new Size(72, 72);
        Font = new Font("Roboto", 8F);
    }
}
