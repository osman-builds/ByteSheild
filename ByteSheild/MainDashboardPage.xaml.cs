using System.Text.RegularExpressions;

namespace ByteSheild
{
    public partial class MainDashboardPage : ContentPage
    {
        // Cached colors to prevent parsing on every keystroke
        private static readonly Color InactiveColor = Color.FromArgb("#6A7A90");
        private static readonly Color DangerColor = Color.FromArgb("#F44336");
        private static readonly Color WarningColor = Color.FromArgb("#FF9800");
        private static readonly Color SuccessColor = Color.FromArgb("#00D4AA");

        // Math.PI * 2 * 94 (radius) / 12 (thickness) / 100
        private const double RingDashMultiplier = 0.4921825;

        // Source-generated regexes for high-performance pattern matching
        [GeneratedRegex(@"[A-Z]")]
        private static partial Regex UpperCaseRegex();

        [GeneratedRegex(@"[a-z]")]
        private static partial Regex LowerCaseRegex();

        [GeneratedRegex(@"[0-9]")]
        private static partial Regex DigitRegex();

        [GeneratedRegex(@"[!@#$%^&*(),.?""':{}|<>]")]
        private static partial Regex SymbolRegex();

        public MainDashboardPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await UpdateRealtimeDataAsync();
        }

        private async Task UpdateRealtimeDataAsync()
        {
            try
            {
                var dbService = Handler?.MauiContext?.Services.GetService<Services.DatabaseService>()
                    ?? new Services.DatabaseService();

                var items = await dbService.GetVaultItemsAsync();
                VaultCountLabel.Text = items.Count.ToString();
            }
            catch
            {
                VaultCountLabel.Text = "0";
            }

            var emailStatus = Preferences.Default.Get("EmailBreachStatus", "UNKNOWN");
            var isSafe = Preferences.Default.Get("EmailIsSafe", false);

            EmailStatusLabel.Text = emailStatus;

            if (emailStatus == "UNKNOWN")
            {
                EmailStatusLabel.TextColor = WarningColor;
            }
            else if (isSafe)
            {
                EmailStatusLabel.TextColor = SuccessColor;
            }
            else
            {
                EmailStatusLabel.TextColor = DangerColor;
            }
        }

        /// <summary>
        /// Evaluates the strength of the entered password and dynamically updates the UI to reflect the score.
        /// Uses high-performance regex matching for validation.
        /// </summary>
        /// <param name="sender">The object that fired the event.</param>
        /// <param name="e">Event arguments containing the new text value.</param>
        private void OnPasswordTextChanged(object? sender, TextChangedEventArgs e)
        {
            var password = e.NewTextValue ?? string.Empty;
            int score = 0;

            bool hasLength = password.Length >= 8;
            bool hasUpper = UpperCaseRegex().IsMatch(password);
            bool hasLower = LowerCaseRegex().IsMatch(password);
            bool hasDigit = DigitRegex().IsMatch(password);
            bool hasSymbol = SymbolRegex().IsMatch(password);

            UpdateCriterion(LengthCriterion, hasLength, "8+ chars");
            UpdateCriterion(UpperCriterion, hasUpper, "Uppercase");
            UpdateCriterion(LowerCriterion, hasLower, "Lowercase");
            UpdateCriterion(DigitCriterion, hasDigit, "Number");
            UpdateCriterion(SymbolCriterion, hasSymbol, "Symbol");

            if (hasLength) score++;
            if (hasUpper) score++;
            if (hasLower) score++;
            if (hasDigit) score++;
            if (hasSymbol) score++;

            // Update percentage dynamically based ONLY on input
            int percentage = score * 20;
            MainScoreLabel.Text = percentage.ToString();

            double dashLength = percentage * RingDashMultiplier;
            
            // Current value to start the animation from
            double startValue = ScoreRing.StrokeDashArray != null && ScoreRing.StrokeDashArray.Count > 0 ? ScoreRing.StrokeDashArray[0] : 0;
            
            // Adding a simple animation instead of instant snap
            ScoreRing.Animate("RingProgress", new Animation(v => ScoreRing.StrokeDashArray = new DoubleCollection { v, 100 }, startValue, dashLength, Easing.CubicOut), 16, 250);
            _ = StrengthProgressBar.ProgressTo(score / 5.0, 250, Easing.CubicOut);

            UpdateVisualStrength(score);
        }

        private void UpdateVisualStrength(int score)
        {
            if (score == 0)
            {
                StrengthProgressBar.ProgressColor = Colors.Transparent;
                ScoreRing.Stroke = Colors.Transparent;
                StrengthLabel.Text = "Strength: None";
                StrengthLabel.TextColor = InactiveColor;
            }
            else if (score <= 2)
            {
                StrengthProgressBar.ProgressColor = DangerColor;
                ScoreRing.Stroke = DangerColor;
                StrengthLabel.Text = "Strength: Weak";
                StrengthLabel.TextColor = DangerColor;
            }
            else if (score <= 4)
            {
                StrengthProgressBar.ProgressColor = WarningColor;
                ScoreRing.Stroke = WarningColor;
                StrengthLabel.Text = "Strength: Fair";
                StrengthLabel.TextColor = WarningColor;
            }
            else
            {
                StrengthProgressBar.ProgressColor = SuccessColor;
                ScoreRing.Stroke = SuccessColor;
                StrengthLabel.Text = "Strength: Strong";
                StrengthLabel.TextColor = SuccessColor;
            }
        }

        private void OnTogglePasswordVisibility(object? sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            VisibilityToggle.Source = PasswordEntry.IsPassword ? "settings_icon.svg" : "vault_icon.svg";
        }

        private void UpdateCriterion(Label label, bool isMet, string text)
        {
            label.Text = isMet ? $"✅ {text}" : $"❌ {text}";
            label.TextColor = isMet ? SuccessColor : InactiveColor;
        }
    }
}