using System;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;

namespace ByteSheild
{
    public partial class BreachCheckerPage : ContentPage
    {
        public BreachCheckerPage()
        {
            InitializeComponent();
        }

        private async void OnCheckClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                await DisplayAlertAsync("Error", "Please enter an email address.", "OK");
                return;
            }

            var email = EmailEntry.Text.Trim();
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                await DisplayAlertAsync("Error", "Please enter a valid email address.", "OK");
                return;
            }

            ResultContainer.IsVisible = true;

            // Artificial delay to simulate scanning
            await Task.Delay(1000);

            // Give dummy data giving feedback based on entered data
            if (email.ToLower().Contains("test") || email.ToLower().Contains("admin"))
            {
                DomainResult.Text = "AT RISK";
                DomainResult.TextColor = Color.FromArgb("#F44336");

                LeaksResult.Text = "3 FOUND";
                LeaksResult.TextColor = Color.FromArgb("#F44336");

                RegistryResult.Text = "COMPROMISED";
                RegistryResult.TextColor = Color.FromArgb("#F44336");
            }
            else
            {
                DomainResult.Text = "SECURE";
                DomainResult.TextColor = Color.FromArgb("#00D4AA");

                LeaksResult.Text = "0 FOUND";
                LeaksResult.TextColor = Color.FromArgb("#6A7A90");

                RegistryResult.Text = "MONITORED";
                RegistryResult.TextColor = Color.FromArgb("#FF9800");
            }
        }
    }
}