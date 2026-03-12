namespace ByteSheild
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(AddVaultItemPage), typeof(AddVaultItemPage));
        }
    }
}
