namespace ByteSheild
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ThemeSwitch.IsToggled = Microsoft.Maui.Storage.Preferences.Default.Get("AppTheme", "Dark") == "Dark";
        }

        private void OnThemeSwitchToggled(object? sender, ToggledEventArgs e)
        {
            var isDark = e.Value;
            var themeStr = isDark ? "Dark" : "Light";
            Microsoft.Maui.Storage.Preferences.Default.Set("AppTheme", themeStr);
            if (Application.Current != null)
                Application.Current.UserAppTheme = isDark ? AppTheme.Dark : AppTheme.Light;
        }

        private async void OnSetupPasscodeTapped(object? sender, TappedEventArgs e)
        {
            var passcode = await DisplayPromptAsync("Setup Passcode", "Enter a 4-digit passcode:", keyboard: Keyboard.Numeric, maxLength: 4);
            if (!string.IsNullOrWhiteSpace(passcode) && passcode.Length == 4)
            {
                await Microsoft.Maui.Storage.SecureStorage.Default.SetAsync("AppPasscode", passcode);
                await DisplayAlertAsync("Success", "Passcode enabled. You can now use it when biometric authentication fails.", "OK");
            }
            else if (passcode != null)
            {
                await DisplayAlertAsync("Error", "Passcode must be exactly 4 digits.", "OK");
            }
        }

        private async void OnPrivacyPolicyTapped(object? sender, TappedEventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new PrivacyPolicyPage());
            }
            catch (Exception ex)
            {
                // Log exception in case of diagnostic tracking later
                System.Diagnostics.Debug.WriteLine($"Navigation failed: {ex.Message}");
                await DisplayAlertAsync("Error", "Could not open the Privacy Policy.", "OK");
            }
        }

        private async void OnOpenSourceLicensesTapped(object? sender, TappedEventArgs e)
        {
            // Use modern C# 11 raw string literals for cleaner multiline text
            const string licenses = """
                SQLitePCLRaw: Apache-2.0 / MIT 
                sqlite-net-pcl: MIT 
                Plugin.Fingerprint: MIT 
                .NET MAUI: MIT
                """;
            await DisplayAlertAsync("Open Source Licenses", licenses, "OK");
        }
    }
}