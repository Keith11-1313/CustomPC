namespace CustomPC;

public partial class LoginForm : StoreWindow
{
    public LoginForm()
    {
        InitializeComponent();

        if (Ui.IsDesign)
        {
            return;
        }

        ApplyBrandAppearance();
        BuildLoginLayout();
        AcceptButton = login;
        login.Click += LoginButton_Click;
        register.Click += RegisterButton_Click;
        guest.Click += GuestButton_Click;
    }

    private void ApplyBrandAppearance()
    {
        brand.UseCompatibleTextRendering = true;
        loginHero.UseCompatibleTextRendering = true;
        brand.Font = AppFonts.Brand(24F);
        loginHero.Font = AppFonts.Brand(29F);
        using Stream stream = typeof(LoginForm).Assembly.GetManifestResourceStream("CustomPC.Assets.backdrop.png")!;
        using Image photo = Image.FromStream(stream);
        // Dark green stays dominant; the PC photograph adds a quiet texture behind the text.
        Bitmap backdrop = new(photo.Width, photo.Height);
        using (Graphics graphics = Graphics.FromImage(backdrop))
        {
            graphics.DrawImage(photo, new Rectangle(0, 0, photo.Width, photo.Height),
                0, 0, photo.Width, photo.Height, GraphicsUnit.Pixel);
            using SolidBrush tint = new(Color.FromArgb(218, 24, 43, 30));
            graphics.FillRectangle(tint, 0, 0, backdrop.Width, backdrop.Height);
        }
        // Keep the photo proportional. Crop excess width; only enlarge uniformly if
        // the panel is taller than the original, so no empty strip appears below it.
        brandPanel.Paint += (_, e) =>
        {
            float scale = Math.Max(1F, Math.Max(brandPanel.ClientSize.Width / (float)backdrop.Width,
                brandPanel.ClientSize.Height / (float)backdrop.Height));
            float width = brandPanel.ClientSize.Width / scale;
            float height = brandPanel.ClientSize.Height / scale;
            e.Graphics.DrawImage(backdrop, brandPanel.ClientRectangle,
                new RectangleF((backdrop.Width - width) / 2, (backdrop.Height - height) / 2, width, height), GraphicsUnit.Pixel);
        };
        brandPanel.Resize += (_, _) => brandPanel.Invalidate(true);
        foreach (Label label in brandPanel.Controls.OfType<Label>()) label.BackColor = Color.Transparent;
        logo.BackColor = Color.Transparent;
        Disposed += (_, _) => backdrop.Dispose();
    }

    private void LoginButton_Click(object? sender, EventArgs e)
    {
        try
        {
            AppStore.Current.Login(email.Text, password.Text);
            DialogResult = DialogResult.OK;
        }
        catch (Exception exception)
        {
            Ui.ShowError(this, exception);
        }
    }

    private void RegisterButton_Click(object? sender, EventArgs e)
    {
        using (RegisterForm registrationForm = new RegisterForm())
        {
            registrationForm.ShowDialog(this);
        }
    }

    private void GuestButton_Click(object? sender, EventArgs e)
    {
        AppStore.Current.Logout();
        DialogResult = DialogResult.OK;
    }
}
