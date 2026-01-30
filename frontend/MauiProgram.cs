using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace frontend
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().UseMauiCommunityToolkit().ConfigureFonts(static fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("BigBlueTermPlusNerdFont-Regular.ttf", "Retro");
            });
#if ANDROID || IOS || MACCATALYST || TIZEN || WINDOWS
            // Ogranicz wywołanie do obsługiwanych platform
            builder.UseMauiCommunityToolkitMediaElement();
#endif
#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}