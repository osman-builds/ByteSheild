namespace ByteSheild
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            try
            {
                VersionLabel.Text = $"Version {AppInfo.Current.VersionString} - Stable Release";
            }
            catch
            {
                VersionLabel.Text = "Version Info Unavailable";
            }
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
                END USER LICENSE AGREEMENT (CLOSED SOURCE)

                Copyright (c) 2026 Abdullahi Osman. All Rights Reserved.

                This software ("ByteShield") is the proprietary intellectual property of Abdullahi Osman. This is a closed-source license. You may not distribute, sub-license, rent, lease, alter, reverse-engineer, modify, or create derivative works from this software without explicit written permission from the author.

                IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY CLAIM, DAMAGES, DATA LOSS, SECURITY BREACHES, OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
                """;
            await DisplayAlertAsync("End User License Agreement", licenses, "OK");
        }

        private async void OnSelfDestructTapped(object? sender, TappedEventArgs e)
        {
            var confirm1 = await DisplayAlertAsync("⚠️ WARNING ⚠️", "Are you absolutely sure you want to delete all your personal data? This will wipe your secure vault, preferences, and all settings.", "Yes, Proceed", "Cancel");
            if (!confirm1) return;

            var confirm2 = await DisplayAlertAsync("🚨 FINAL WARNING 🚨", "This operation is IRREVERSIBLE. ALL data will be permanently destroyed and cannot be recovered. Do you wish to self-destruct?", "DESTROY EVERYTHING", "Cancel");
            if (!confirm2) return;

            try
            {
                // Obtain DatabaseService and wipe data
                var dbService = Handler?.MauiContext?.Services.GetService<Services.DatabaseService>() 
                                ?? new Services.DatabaseService();
                
                await dbService.DeleteAllDataAsync();

                // Clear Preferences
                Microsoft.Maui.Storage.Preferences.Default.Clear();

                // Clear SecureStorage keys
                Microsoft.Maui.Storage.SecureStorage.Default.Remove("AppPasscode");
                Microsoft.Maui.Storage.SecureStorage.Default.Remove("database_encryption_key");
                Microsoft.Maui.Storage.SecureStorage.Default.RemoveAll(); // Attempt full sweep

                await DisplayAlertAsync("Wiped", "All data has been successfully securely erased.", "OK");

                // Exit the app gracefully if possible on current platform
                Application.Current?.Quit();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Error", $"Self-destruct encountered an issue: {ex.Message}", "OK");
            }
        }
    }
}