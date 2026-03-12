using Microsoft.Extensions.DependencyInjection;

namespace ByteSheild
{
    public partial class App : Application
    {
        private static Services.DatabaseService? _database;

        public static Services.DatabaseService Database
        {
            get
            {
                if (_database == null)
                {
                    _database = new Services.DatabaseService();
                }
                return _database;
            }
        }

        public App()
        {
            InitializeComponent();
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