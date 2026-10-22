namespace CustomPC;

public partial class LoginForm
{
    private TableLayoutPanel? loginLayout;
    private Panel? authViewport;
    private void BuildLoginLayout()
    {
        loginLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
        loginLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42)); loginLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        loginLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        authViewport = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.White, Margin = new Padding(0) };
        brandPanel.Dock = DockStyle.Fill; brandPanel.Margin = new Padding(0);
        authSurface.Parent = authViewport; authSurface.Anchor = AnchorStyles.Top | AnchorStyles.Left; authSurface.Dock = DockStyle.None;
        loginLayout.Controls.Add(brandPanel, 0, 0); loginLayout.Controls.Add(authViewport, 1, 0);
        Controls.Add(loginLayout); loginLayout.BringToFront();
        MinimumSize = new Size(600, 500); ClientSize = new Size(1120, 740);
        SizeChanged += (_, _) => FitLoginLayout(); authViewport.SizeChanged += (_, _) => FitLoginLayout();
        Shown += (_, _) => FitLoginLayout(); FitLoginLayout();
        heading.Font = AppFonts.Heading(30);
    }
    private void FitLoginLayout()
    {
        if (loginLayout == null || authViewport == null) return;
        float scale = DeviceDpi / 96F;
        int Px(int value) => (int)(value * scale);
        bool compact = ClientSize.Width < Px(900);
        loginLayout.ColumnStyles[0].SizeType = compact ? SizeType.Absolute : SizeType.Percent;
        loginLayout.ColumnStyles[0].Width = compact ? 0 : 42;
        brandPanel.Visible = !compact;
        int width = Math.Max(Px(300), Math.Min(Px(440), authViewport.ClientSize.Width - Px(64)));
        authSurface.SetBounds(Math.Max(Px(20), (authViewport.ClientSize.Width - width) / 2), Math.Max(Px(24), (authViewport.ClientSize.Height - Px(540)) / 2), width, Px(540));
        heading.SetBounds(0, 0, width, Px(62));
        emailLabel.SetBounds(0, Px(114), width, Px(24)); email.SetBounds(0, Px(148), width, Px(46));
        passwordLabel.SetBounds(0, Px(218), width, Px(24)); password.SetBounds(0, Px(252), width, Px(46));
        login.SetBounds(0, Px(338), width, Px(48));
        register.SetBounds(0, Px(406), (width - Px(12)) / 2, Px(44)); guest.SetBounds(register.Right + Px(12), register.Top, register.Width, register.Height);
        logo.SetBounds(Px(36), Px(40), Px(48), Px(48)); brand.SetBounds(Px(100), Px(38), Math.Max(1, brandPanel.Width - Px(120)), Px(52));
        brandCaption.SetBounds(Px(36), Px(110), Math.Max(1, brandPanel.Width - Px(72)), Px(34));
        loginHero.SetBounds(Px(36), Px(260), Math.Max(1, brandPanel.Width - Px(72)), Px(160));
        brandFooter.SetBounds(Px(36), Math.Max(Px(430), brandPanel.Height - Px(150)), Math.Max(1, brandPanel.Width - Px(72)), Px(94));
    }
}
