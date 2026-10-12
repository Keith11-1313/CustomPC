namespace CustomPC
{
    internal static class Program
    {
        // Windows Forms starts here, on one UI thread.
        [STAThread]
        static void Main()
        {
            // Apply desktop display settings before any form or control is created.
            ApplicationConfiguration.Initialize();
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) =>
            {
                Ui.ShowError(new NativeWindow(), e.Exception);
                Application.Exit();
            };

            string path = Environment.GetEnvironmentVariable("CUSTOMPC_DATABASE_PATH") ??
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CustomPC", "custompc.db");
            string key = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(Path.GetFullPath(path).ToUpperInvariant())));
            using Mutex instance = new Mutex(true, @"Local\CustomPC-" + key, out bool firstInstance);
            if (!firstInstance)
            {
                MessageBox.Show("CustomPC is already running for this database. Switch to its existing window.", "CustomPC");
                return;
            }

            try
            {
                if (AppStore.Current.NeedsSetup)
                {
                    using SetupForm setup = new SetupForm();
                    if (setup.ShowDialog() != DialogResult.OK) return;
                }

                // Finish login before creating the homepage. Closing login ends the app.
                using (LoginForm loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() != DialogResult.OK) return;
                }

                // Run listens for button clicks until the dashboard closes.
                Application.Run(new DashboardForm());
            }
            catch (Exception exception)
            {
                Ui.ShowError(new NativeWindow(), exception);
            }
        }
    }
}
