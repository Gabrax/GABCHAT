using Microsoft.Maui.Controls;

namespace frontend
{
    public class AppShell : Shell
    {
        public AppShell()
        {
            Title = "GABCHAT";
            FlyoutBehavior = FlyoutBehavior.Disabled;

            // LOGIN
            var loginPage = new ShellContent
            {
                ContentTemplate = new DataTemplate(typeof(LoginPage)),
                Route = "MainPage"
            };
            Shell.SetNavBarIsVisible(loginPage, false);

            // REGISTRATION
            var registrationPage = new ShellContent
            {
                ContentTemplate = new DataTemplate(typeof(RegistrationPage)),
                Route = "RegistrationPage"
            };
            Shell.SetNavBarIsVisible(registrationPage, false);

            Items.Add(loginPage);
            Items.Add(registrationPage);

            Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
        }
    }
}