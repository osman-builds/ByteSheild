using Microsoft.Extensions.Logging;

namespace ByteSheild
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<Services.DatabaseService>();
            builder.Services.AddTransient<OfflineVaultPage>();
            builder.Services.AddTransient<AddVaultItemPage>();

            // Load environment variables via embedded resource
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream("ByteSheild..env");
                if (stream != null)
                {
                    using var reader = new System.IO.StreamReader(stream);
                    DotNetEnv.Env.LoadContents(reader.ReadToEnd());
                }
            }
            catch { /* Ignore if it doesn't load */ }

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
