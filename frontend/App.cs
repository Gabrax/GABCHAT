using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace frontend
{
    public class App : Application
    {
        public App()
        {
            Resources = new ResourceDictionary
            {
                { "PrimaryColor", Color.FromArgb("#4F46E5") },
                { "SecondaryColor", Color.FromArgb("#888") },
                { "ErrorColor", Color.FromArgb("#FF4D4F") },
            };

            InitializeAsync().GetAwaiter().GetResult();
        }

        private async Task InitializeAsync()
        { 
            var config = new ConfigurationBuilder()
                .AddJsonStream(await FileSystem.OpenAppPackageFileAsync("appsettings.json"))
                .Build();

            var baseUrl = config["HTTPS_URL"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("No HTTPS_URL set in appsettings.json.");

            BaseClient.Initialize(baseUrl: baseUrl);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(new AppShell());
#if WINDOWS
            window.Width = 1000;
            window.Height = 800;

            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            window.X = (displayInfo.Width / displayInfo.Density  - window.Width) / 2;
            window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2;

            if (window != null)
            {
                window.MinimumWidth = window.Width;
                window.MinimumHeight = window.Height;

                //window.MaximumWidth = 1200;
                //window.MaximumHeight = 800;
            }
#endif
            return window ?? new Window(new AppShell());
        }
    }
}
