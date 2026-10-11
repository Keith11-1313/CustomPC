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

            // Finish login before creating the homepage, so only login appears at startup.
            // Guest access also returns OK. Closing login ends the app here.
            using (LoginForm loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
            }

            // Run keeps the app listening for button clicks until the dashboard closes.
            Application.Run(new DashboardForm());
        }
    }
}
