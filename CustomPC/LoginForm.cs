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

        var privacy = ShopPolicies.Link("Privacy Notice", "privacy", this);
        privacy.Name = "loginPrivacyLink"; privacy.AutoSize = false;
        privacy.TextAlign = ContentAlignment.MiddleCenter;
        authSurface.Controls.Add(privacy);
        ApplyBrandAppearance();
        Load += (_, _) => RestoreLoginLayout();
        SizeChanged += (_, _) => { if (IsHandleCreated) RestoreLoginLayout(); };
        AcceptButton = login;
        login.Click += LoginButton_Click;
        register.LinkClicked += RegisterButton_Click;
        guest.LinkClicked += GuestButton_Click;
    }

    private void RestoreLoginLayout()
    {
        float scale = DeviceDpi / 96F;
        int Px(int value) => (int)(value * scale);
        int leftWidth = (int)(ClientSize.Width * .42F);
        brandPanel.SetBounds(0, 0, leftWidth, ClientSize.Height);
        authSurface.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        authSurface.SetBounds(leftWidth + Px(24), Px(24), Math.Max(Px(300), ClientSize.Width - leftWidth - Px(48)), Math.Max(Px(552), ClientSize.Height - Px(48)));
        int width = Math.Max(1, authSurface.Width - Px(64));
        heading.SetBounds(Px(32), Px(42), width, Px(60));
        emailLabel.SetBounds(Px(32), Px(144), width, Px(24));
        email.SetBounds(Px(32), Px(176), width, Px(44));
        passwordLabel.SetBounds(Px(32), Px(244), width, Px(24));
        password.SetBounds(Px(32), Px(276), width, Px(44));
        login.SetBounds(Px(32), Px(354), width, Px(48));
        register.SetBounds(Px(32), Px(422), width, Px(34));
        guest.SetBounds(Px(32), Px(464), width, Px(34));
        authSurface.Controls["loginPrivacyLink"]?.SetBounds(Px(32), Px(506), width, Px(34));
        logo.SetBounds(Px(32), Px(32), Px(64), Px(43));
        brand.SetBounds(Px(110), Px(29), Math.Max(1, leftWidth - Px(128)), Px(48));
        brandCaption.SetBounds(Px(38), Px(88), Math.Max(1, leftWidth - Px(64)), Px(46));
        loginHero.SetBounds(Px(38), Px(215), Math.Max(1, leftWidth - Px(64)), Px(146));
        brandFooter.SetBounds(Px(38), Math.Max(Px(380), brandPanel.Height - Px(138)), Math.Max(1, leftWidth - Px(64)), Px(100));
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
