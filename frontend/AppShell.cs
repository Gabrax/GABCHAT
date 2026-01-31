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
                ContentTemplate = new DataTemplate(typeof(AuthPage)),
                Route = "MainPage"
            };
            Shell.SetNavBarIsVisible(loginPage, false);

            Items.Add(loginPage);
        }
    }
}