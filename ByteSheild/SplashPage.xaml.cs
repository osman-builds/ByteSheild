using System;
using Microsoft.Maui.Controls.Shapes;

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

        public SplashPage()
        {
            InitializeComponent();
            _ = StartSplashSequence();
        }

        private async Task StartSplashSequence()
        {
            // Set initial states
            if (FindByName("AppTitle") is Label appTitle)
                appTitle.Opacity = 0;

            if (FindByName("Subtitle") is Label subtitle)
                subtitle.Opacity = 0;

            if (FindByName("SecurityIndicator") is StackLayout securityIndicator)
                securityIndicator.Opacity = 0;

            if (FindByName("LoadingSection") is StackLayout loadingSection)
                loadingSection.Opacity = 0;

            if (FindByName("ShieldFrame") is Border shieldFrame)
            {
                shieldFrame.Scale = 0.8;
                shieldFrame.Opacity = 1; // Keep visible to smoothly transition from native splash screen
            }

            await Task.Delay(200);

            if (FindByName("ShieldFrame") is Border shieldFrameAnim)
            {
                await Task.WhenAll(
                    shieldFrameAnim.ScaleToAsync(1, 800, Easing.BounceOut)
                );
            }

            await Task.Delay(200);

            if (FindByName("AppTitle") is Label titleAnim)
                await titleAnim.FadeToAsync(1, 600, Easing.CubicOut);

            await Task.Delay(300);

            var animations = new List<Task>();
            if (FindByName("Subtitle") is Label subtitleAnim)
                animations.Add(subtitleAnim.FadeToAsync(1, 500, Easing.CubicOut));
            if (FindByName("SecurityIndicator") is StackLayout securityAnim)
                animations.Add(securityAnim.FadeToAsync(1, 500, Easing.CubicOut));

            if (animations.Any())
                await Task.WhenAll(animations);

            await Task.Delay(500);

            if (FindByName("LoadingSection") is StackLayout loadingSectionAnim)
                await loadingSectionAnim.FadeToAsync(1, 400);

            await Task.WhenAll(
                StartLoadingDotsAnimation(),
                StartProgressBarAnimation(),
                CycleLoadingMessages()
            );
        }

        private async Task StartLoadingDotsAnimation()
        {
            var dot1 = FindByName("Dot1") as Ellipse;
            var dot2 = FindByName("Dot2") as Ellipse;
            var dot3 = FindByName("Dot3") as Ellipse;
            var dots = new[] { dot1, dot2, dot3 }.OfType<Ellipse>().ToArray();

            var loadingProgressBar = FindByName("LoadingProgressBar") as ProgressBar;
            if (loadingProgressBar == null) return;

            while (loadingProgressBar.Progress < 1.0)
            {
                foreach (var dot in dots)
                {
                    _ = Task.Run(async () =>
                    {
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await dot.FadeToAsync(1, 300);
                            await dot.FadeToAsync(0.3, 300);
                        });
                    });
                    await Task.Delay(200);
                }
            }
        }

        private async Task StartProgressBarAnimation()
        {
            var loadingProgressBar = FindByName("LoadingProgressBar") as ProgressBar;
            if (loadingProgressBar == null) return;

            const int totalSteps = 100;
            const int stepDelay = 35;

            for (int i = 0; i <= totalSteps; i++)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    loadingProgressBar.Progress = (double)i / totalSteps;
                });

                await Task.Delay(stepDelay);
            }

            await Task.Delay(800);
            await NavigateToMainPage();
        }

        private async Task CycleLoadingMessages()
        {
            var loadingText = FindByName("LoadingText") as Label;
            var loadingProgressBar = FindByName("LoadingProgressBar") as ProgressBar;
            if (loadingText == null || loadingProgressBar == null) return;

            int messageIndex = 0;

            while (loadingProgressBar.Progress < 0.9)
            {
                if (messageIndex < _loadingMessages.Length)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        loadingText.Text = _loadingMessages[messageIndex];
                    });
                    messageIndex++;
                }

                await Task.Delay(1200);
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                loadingText.Text = "ByteShield Ready!";
            });
        }

        private async Task NavigateToMainPage()
        {
            try
            {
                var shieldFrame = FindByName("ShieldFrame") as Border;

                var animations = new List<Task> { this.FadeToAsync(0, 500) };
                if (shieldFrame != null)
                    animations.Add(shieldFrame.ScaleToAsync(1.2, 500, Easing.CubicIn));

                await Task.WhenAll(animations);
                await Shell.Current.GoToAsync("//BiometricPage");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                await Shell.Current.GoToAsync("//BiometricPage");
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}