namespace ByteSheild
{
    public partial class SplashPage : ContentPage
    {
        private readonly string[] _loadingMessages =
        {
            "Initializing Security Protocols...",
            "Encrypting Local Database...",
            "Verifying Security Keys...",
            "Loading Secure Environment...",
            "Activating ByteShield Protection..."
        };

        private CancellationTokenSource? _animationCts;

        public SplashPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _animationCts = new CancellationTokenSource();
            try
            {
                await StartSplashSequence(_animationCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected when page is disappearing and animations are cancelled
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Splash screen error: {ex}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Cancel animations to prevent memory leaks if user navigates away
            _animationCts?.Cancel();
        }

        private async Task StartSplashSequence(CancellationToken token)
        {
            // Set initial states using direct field references instead of FindByName runtime lookup
            AppTitle.Opacity = 0;
            Subtitle.Opacity = 0;
            SecurityIndicator.Opacity = 0;
            LoadingSection.Opacity = 0;

            ShieldFrame.Scale = 0.8;
            ShieldFrame.Opacity = 1; // Keep visible to smoothly transition from native splash screen

            await Task.Delay(200, token);
            if (token.IsCancellationRequested) return;

            await ShieldFrame.ScaleToAsync(1, 800, Easing.BounceOut);

            await Task.Delay(200, token);
            if (token.IsCancellationRequested) return;

            await AppTitle.FadeToAsync(1, 600, Easing.CubicOut);

            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;

            await Task.WhenAll(
                Subtitle.FadeToAsync(1, 500, Easing.CubicOut),
                SecurityIndicator.FadeToAsync(1, 500, Easing.CubicOut)
            );

            await Task.Delay(500, token);
            if (token.IsCancellationRequested) return;

            await LoadingSection.FadeToAsync(1, 400);

            // Execute parallel UI tasks
            await Task.WhenAll(
                StartLoadingDotsAnimation(token),
                StartProgressBarAnimation(token),
                CycleLoadingMessages(token)
            );
        }

        private async Task StartLoadingDotsAnimation(CancellationToken token)
        {
            var dots = new[] { Dot1, Dot2, Dot3 };

            while (LoadingProgressBar.Progress < 1.0 && !token.IsCancellationRequested)
            {
                foreach (var dot in dots)
                {
                    if (token.IsCancellationRequested) break;

                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await dot.FadeToAsync(1, 300);
                        await dot.FadeToAsync(0.3, 300);
                    });

                    await Task.Delay(100, token);
                }
            }
        }

        private async Task StartProgressBarAnimation(CancellationToken token)
        {
            const int totalSteps = 100;
            const int stepDelay = 35;

            for (int i = 0; i <= totalSteps; i++)
            {
                if (token.IsCancellationRequested) return;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LoadingProgressBar.Progress = (double)i / totalSteps;
                });

                await Task.Delay(stepDelay, token);
            }

            await Task.Delay(800, token);
            if (!token.IsCancellationRequested)
            {
                await MainThread.InvokeOnMainThreadAsync(NavigateToMainPage);
            }
        }

        private async Task CycleLoadingMessages(CancellationToken token)
        {
            int messageIndex = 0;

            while (LoadingProgressBar.Progress < 0.9 && !token.IsCancellationRequested)
            {
                if (messageIndex < _loadingMessages.Length)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        LoadingText.Text = _loadingMessages[messageIndex];
                    });
                    messageIndex++;
                }

                await Task.Delay(1200, token);
            }

            if (!token.IsCancellationRequested)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    LoadingText.Text = "ByteShield Ready!";
                });
            }
        }

        private async Task NavigateToMainPage()
        {
            try
            {
                await Task.WhenAll(
                    this.FadeToAsync(0, 500),
                    ShieldFrame.ScaleToAsync(1.2, 500, Easing.CubicIn)
                );

                await Shell.Current.GoToAsync("//BiometricPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.GoToAsync("//BiometricPage");
            }
        }

        protected override bool OnBackButtonPressed() => true;
    }
}