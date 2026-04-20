using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace ByteSheild
{
    public partial class BreachCheckerPage : ContentPage
    {
        // Cache UI colors to avoid repeated FromArgb parsing overhead
        private static readonly Color DangerColor = Color.FromArgb("#F44336");
        private static readonly Color SuccessColor = Color.FromArgb("#00D4AA");
        private static readonly Color InactiveColor = Color.FromArgb("#6A7A90");
        private static readonly Color WarningColor = Color.FromArgb("#FF9800");

        // High-performance compiled regex for email validation
        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EmailFormatRegex();

        // API Configuration
        private static string ApiKey => Environment.GetEnvironmentVariable("HIBP_API_KEY") ?? string.Empty;
        private static readonly HttpClient HttpClient = new HttpClient();

        public BreachCheckerPage()
        {
            InitializeComponent();

            // Set up HttpClient headers for HIBP API
            if (!string.IsNullOrEmpty(ApiKey))
            {
                // Clear to ensure no duplicates if constructed multiple times
                HttpClient.DefaultRequestHeaders.Remove("hibp-api-key");
                HttpClient.DefaultRequestHeaders.Remove("User-Agent");

                HttpClient.DefaultRequestHeaders.Add("hibp-api-key", ApiKey);
                HttpClient.DefaultRequestHeaders.Add("User-Agent", "ByteShield-App");
            }
        }

        /// <summary>
        /// Handles the event when the Check button is clicked. 
        /// Validates the email, queries the Have I Been Pwned API, and updates the application preferences with the breach status.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private async void OnCheckClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                await DisplayAlertAsync("Error", "Please enter an email address.", "OK");
                return;
            }

            var email = EmailEntry.Text.Trim();
            if (!email.Contains('@') || !email.Contains('.') || !EmailFormatRegex().IsMatch(email))
            {
                await DisplayAlertAsync("Error", "Please enter a valid email address containing '@' and '.'.", "OK");
                return;
            }

            // Enter loading state
            CheckButton.Text = string.Empty;
            CheckButton.IsEnabled = false;
            CheckLoading.IsVisible = true;
            CheckLoading.IsRunning = true;
            ResultContainer.IsVisible = false;

            if (string.IsNullOrEmpty(ApiKey) || ApiKey == "your_api_key_here")
            {
                await DisplayAlertAsync("Configuration Error", "HIBP API key is missing. Please check your .env file.", "OK");

                // Restore UI state
                CheckLoading.IsRunning = false;
                CheckLoading.IsVisible = false;
                CheckButton.Text = "CHECK NOW";
                CheckButton.IsEnabled = true;
                return;
            }

            try
            {
                // Call haveibeenpwned API
                // Adding padding to URL for k-anonymity (optional for email but good practice for passwords)
                var encodedEmail = Uri.EscapeDataString(email);
                var response = await HttpClient.GetAsync($"https://haveibeenpwned.com/api/v3/breachedaccount/{encodedEmail}?truncateResponse=false");

                if (response.IsSuccessStatusCode)
                {
                    // Breaches found
                    var breaches = await response.Content.ReadFromJsonAsync<List<BreachModel>>();
                    int breachCount = breaches?.Count ?? 0;

                    DomainResult.Text = "AT RISK";
                    DomainResult.TextColor = DangerColor;
                    Preferences.Default.Set("EmailBreachStatus", "AT RISK");
                    Preferences.Default.Set("EmailIsSafe", false);

                    LeaksResult.Text = $"{breachCount} FOUND";
                    LeaksResult.TextColor = DangerColor;

                    RegistryResult.Text = "COMPROMISED";
                    RegistryResult.TextColor = DangerColor;

                    if (breachCount > 0 && breaches != null)
                    {
                        SourcesContainer.IsVisible = true;
                        SourcesResult.Text = string.Join(", ", breaches.Select(b => b.Name));
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // No breaches found
                    DomainResult.Text = "SECURE";
                    DomainResult.TextColor = SuccessColor;
                    Preferences.Default.Set("EmailBreachStatus", "SECURE");
                    Preferences.Default.Set("EmailIsSafe", true);

                    LeaksResult.Text = "0 FOUND";
                    LeaksResult.TextColor = InactiveColor;

                    RegistryResult.Text = "MONITORED";
                    RegistryResult.TextColor = WarningColor;

                    SourcesContainer.IsVisible = false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await DisplayAlertAsync("Error", "Invalid API key. Please check your configuration.", "OK");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    await DisplayAlertAsync("Rate Limited", "Too many requests. Please try again later.", "OK");
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    await DisplayAlertAsync("Error", $"API Error: {response.StatusCode}", "OK");
                }
            }
            catch (Exception)
            {
                // Generic error message to prevent exposing internal exception details/stack traces
                await DisplayAlertAsync("Error", "A connection error occurred. Please check your network and try again later.", "OK");
            }

            // Restore UI state
            CheckLoading.IsRunning = false;
            CheckLoading.IsVisible = false;
            CheckButton.Text = "CHECK NOW";
            CheckButton.IsEnabled = true;
            ResultContainer.IsVisible = true;
        }

        private void OnEmailTextChanged(object? sender, TextChangedEventArgs e)
        {
            // Reset to UNKNOWN when email is modified or cleared
            Preferences.Default.Set("EmailBreachStatus", "UNKNOWN");
            Preferences.Default.Set("EmailIsSafe", false);
            ResultContainer.IsVisible = false;
        }

        // Simple model for deserialization
        public class BreachModel
        {
            public string Name { get; set; } = string.Empty;
            public string Domain { get; set; } = string.Empty;
            public string BreachDate { get; set; } = string.Empty;
        }
    }
}