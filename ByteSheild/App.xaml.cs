namespace ByteSheild
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var savedTheme = Microsoft.Maui.Storage.Preferences.Default.Get("AppTheme", "Dark");
            Current!.UserAppTheme = savedTheme == "Dark" ? AppTheme.Dark : AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new AppShell();

            // Find the splash page item - handle the IMPL_ prefix that MAUI adds
            var splashItem = shell.Items.FirstOrDefault(item =>
                item.Route == "SplashPage" || item.Route == "IMPL_SplashPage");

            // Set current item if found, otherwise let Shell use its default
            if (splashItem != null)
            {
                shell.CurrentItem = splashItem;
            }

            return new Window(shell);
        }
    }
}