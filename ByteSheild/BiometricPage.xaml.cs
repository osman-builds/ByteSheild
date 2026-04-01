using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace ByteSheild
{
    public partial class BiometricPage : ContentPage
    {
        public BiometricPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            // Small delay to ensure the Android window token is fully attached before showing any dialogs
            // This prevents Android.Runtime.JavaProxyThrowable / BadTokenException
            await Task.Delay(150);

            await CheckAndAskForName();
            await AuthenticateUser();
        }

        private async Task CheckAndAskForName()
        {
            if (!Microsoft.Maui.Storage.Preferences.Default.ContainsKey("UserName"))
            {
                string name = await DisplayPromptAsync("Welcome", "Please enter your name for a personalized experience:", "Save", "Cancel");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    Microsoft.Maui.Storage.Preferences.Default.Set("UserName", name);
                }
            }
        }

        private async void OnRetryClicked(object? sender, EventArgs e)
        {
            await AuthenticateUser();
        }

        private async Task AuthenticateUser()
        {
            RetryButton.IsVisible = false;
            var userName = Microsoft.Maui.Storage.Preferences.Default.Get("UserName", Environment.UserName);

            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                // Bypass biometric authentication on Windows
                await DisplayAlertAsync("Welcome", $"Welcome, {userName}!", "OK");
                await Shell.Current.GoToAsync("//MainDashboardPage");
                return;
            }

            var request = new AuthenticationRequestConfiguration("Biometric Gatekeeper", "Scanning for Authenticated User...");
            var result = await CrossFingerprint.Current.AuthenticateAsync(request);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (result.Authenticated)
                {
                    await DisplayAlertAsync("Welcome", $"Welcome, {userName}!", "OK");
                    // Go to Main Dashboard
                    await Shell.Current.GoToAsync("//MainDashboardPage");
                }
                else
                {
                    var savedPasscode = await Microsoft.Maui.Storage.SecureStorage.Default.GetAsync("AppPasscode");
                    if (!string.IsNullOrEmpty(savedPasscode))
                    {
                        var entered = await DisplayPromptAsync("Biometrics Failed", "Enter your 4-digit passcode:", keyboard: Keyboard.Numeric);
                        if (entered == savedPasscode)
                        {
                            await DisplayAlertAsync("Welcome", $"Welcome, {userName}!", "OK");
                            await Shell.Current.GoToAsync("//MainDashboardPage");
                            return;
                        }
                        else if (entered != null)
                        {
                            await DisplayAlertAsync("Error", "Incorrect Passcode.", "OK");
                        }
                    }

                    RetryButton.IsVisible = true;
                    await DisplayAlertAsync("Authentication Failed", "Please try again.", "OK");
                }
            });
        }
    }
}