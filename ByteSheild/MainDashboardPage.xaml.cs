using System;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;

namespace ByteSheild
{
    public partial class MainDashboardPage : ContentPage
    {
        public MainDashboardPage()
        {
            InitializeComponent();
        }

        private void OnPasswordTextChanged(object? sender, TextChangedEventArgs e)
        {
            var password = e.NewTextValue ?? string.Empty;
            int score = 0;

            bool hasLength = password.Length >= 8;
            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasDigit = Regex.IsMatch(password, @"[0-9]");
            bool hasSymbol = Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]");

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

            double dashLength = percentage * 0.4921825; // Math.PI * 2 * 94 (radius) / 12 (thickness) / 100
            ScoreRing.StrokeDashArray = new DoubleCollection { dashLength, 100 };

            StrengthProgressBar.Progress = score / 5.0;

            if (score == 0)
            {
                StrengthProgressBar.ProgressColor = Colors.Transparent;
                ScoreRing.Stroke = Colors.Transparent;
                StrengthLabel.Text = "Strength: None";
                StrengthLabel.TextColor = Color.FromArgb("#6A7A90");
            }
            else if (score <= 2)
            {
                StrengthProgressBar.ProgressColor = Color.FromArgb("#F44336");
                ScoreRing.Stroke = Color.FromArgb("#F44336");
                StrengthLabel.Text = "Strength: Weak";
                StrengthLabel.TextColor = Color.FromArgb("#F44336");
            }
            else if (score <= 4)
            {
                StrengthProgressBar.ProgressColor = Color.FromArgb("#FF9800");
                ScoreRing.Stroke = Color.FromArgb("#FF9800");
                StrengthLabel.Text = "Strength: Fair";
                StrengthLabel.TextColor = Color.FromArgb("#FF9800");
            }
            else
            {
                StrengthProgressBar.ProgressColor = Color.FromArgb("#00D4AA");
                ScoreRing.Stroke = Color.FromArgb("#00D4AA");
                StrengthLabel.Text = "Strength: Strong";
                StrengthLabel.TextColor = Color.FromArgb("#00D4AA");
            }
        }

        private void OnTogglePasswordVisibility(object? sender, EventArgs e)
        {
            PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
            VisibilityToggle.Text = PasswordEntry.IsPassword ? "👁" : "🙈";
        }

        private void UpdateCriterion(Label label, bool isMet, string text)
        {
            if (isMet)
            {
                label.Text = $"● {text}";
                label.TextColor = Color.FromArgb("#00D4AA");
            }
            else
            {
                label.Text = $"○ {text}";
                label.TextColor = Color.FromArgb("#6A7A90");
            }
        }
    }
}