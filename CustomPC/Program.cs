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
            // Run keeps the app listening for button clicks until the dashboard closes.
            Application.Run(new DashboardForm());
        }
    }
}
