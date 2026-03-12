using System;
using Microsoft.Maui.Controls;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using System.Threading.Tasks;

namespace ByteSheild
{
    public partial class BiometricPage : ContentPage
    {
        public BiometricPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await AuthenticateUser();
        }

        private async void OnRetryClicked(object? sender, EventArgs e)
        {
            await AuthenticateUser();
        }

        private async Task AuthenticateUser()
        {
            var request = new AuthenticationRequestConfiguration("Biometric Gatekeeper", "Scanning for Authenticated User...");
            var result = await CrossFingerprint.Current.AuthenticateAsync(request);

            if (result.Authenticated)
            {
                // Go to Main Dashboard
                await Shell.Current.GoToAsync("//MainDashboardPage");
            }
            else
            {
                await DisplayAlertAsync("Authentication Failed", "Please try again.", "OK");
            }
        }
    }
}   