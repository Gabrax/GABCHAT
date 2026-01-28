using Microsoft.Maui.Controls.Shapes;

namespace frontend;

public class RegistrationPage : ContentPage
{
    private Entry UserNameEntry;
    private Border UserNameBorder;
    private Label UserNameErrorLabel;

    private Entry EmailEntry;
    private Border EmailBorder;
    private Label EmailErrorLabel;

    private Entry PasswordEntry;
    private Border PasswordBorder;
    private Label PasswordErrorLabel;

    private Button SignUpButton;
    private Button BackButton;

    private VerticalStackLayout MainLayout;
    public RegistrationPage()
	{
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsVisible = false,
            IsEnabled = false
        });

        BackgroundColor = Color.FromArgb("#121212");

        // USERNAME
        UserNameEntry = new Entry
        {
            Placeholder = "Username",
            Keyboard = Keyboard.Email,
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#888"),
            BackgroundColor = Colors.Transparent
        };

        UserNameBorder = new Border
        {
            WidthRequest = 200,
            BackgroundColor = Color.FromArgb("#1E1E1E"),
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = 14,
            Content = UserNameEntry
        };

        UserNameErrorLabel = new Label
        {
            Text = "Invalid user name",
            TextColor = Color.FromArgb("#FF4D4F"),
            FontSize = 12,
            IsVisible = false,
            Margin = new Thickness(6, 0, 0, 0),
            HorizontalOptions = LayoutOptions.Center
        };

        var usernameLayout = new VerticalStackLayout
        {
            Spacing = 4,
            Children = { UserNameBorder, UserNameErrorLabel }
        };

        // EMAIL
        EmailEntry = new Entry
        {
            Placeholder = "Email",
            Keyboard = Keyboard.Email,
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#888"),
            BackgroundColor = Colors.Transparent
        };

        EmailBorder = new Border
        {
            WidthRequest = 200,
            BackgroundColor = Color.FromArgb("#1E1E1E"),
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = 14,
            Content = EmailEntry
        };

        EmailErrorLabel = new Label
        {
            Text = "Invalid email address",
            TextColor = Color.FromArgb("#FF4D4F"),
            FontSize = 12,
            IsVisible = false,
            Margin = new Thickness(6, 0, 0, 0),
            HorizontalOptions = LayoutOptions.Center
        };

        var emailLayout = new VerticalStackLayout
        {
            Spacing = 4,
            Children = { EmailBorder, EmailErrorLabel }
        };

        // PASSWORD
        PasswordEntry = new Entry
        {
            Placeholder = "Password",
            IsPassword = true,
            TextColor = Colors.White,
            PlaceholderColor = Color.FromArgb("#888"),
            BackgroundColor = Colors.Transparent
        };

        PasswordBorder = new Border
        {
            WidthRequest = 200,
            BackgroundColor = Color.FromArgb("#1E1E1E"),
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = 14,
            Content = PasswordEntry
        };

        PasswordErrorLabel = new Label
        {
            TextColor = Color.FromArgb("#FF4D4F"),
            FontSize = 12,
            IsVisible = false,
            Margin = new Thickness(6, 0, 0, 0),
            HorizontalOptions = LayoutOptions.Center
        };

        var passwordLayout = new VerticalStackLayout
        {
            Spacing = 4,
            Children = { PasswordBorder, PasswordErrorLabel }
        };

        // LOGIN BUTTON
        BackButton = new Button
        {
            WidthRequest = 200,
            HeightRequest = 55,
            Text = "Back",
            CornerRadius = 14,
            BackgroundColor = Color.FromArgb("#4F46E5"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold
        };
        BackButton.Clicked += async (s, e) =>
        {
            await Navigation.PopAsync();
        };
#if WINDOWS
            BackButton.EnableHoverCursor(CursorIcon.Hand);
#endif

        SignUpButton = new Button
        {
            WidthRequest = 200,
            HeightRequest = 55,
            Text = "Sign Up",
            CornerRadius = 14,
            BackgroundColor = Color.FromArgb("#4F46E5"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold
        };
        SignUpButton.Clicked += async (s, e) =>
        {
            SignUpButton.Text = "Account created successfully!";
            await Task.Delay(1000);
            await Navigation.PushAsync(new ChatPage());
        };
#if WINDOWS
            SignUpButton.EnableHoverCursor(CursorIcon.Hand);
#endif

        var buttonLayout = new HorizontalStackLayout
        {
            HorizontalOptions = LayoutOptions.Center,
            Children = { BackButton, SignUpButton }
        };

        MainLayout = new VerticalStackLayout
        {
            Padding = 30,
            Spacing = 20,
            VerticalOptions = LayoutOptions.Center,
            Opacity = 0,
            TranslationY = 40,
            Children =
                {
                    new Image
                    {
                        Source = "dotnet_bot.png",
                        HeightRequest = 120,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = "GABCHAT",
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    usernameLayout,
                    emailLayout,
                    passwordLayout,
                    buttonLayout,
                }
        };

        Content = MainLayout;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await MainLayout.FadeToAsync(1, 600, Easing.CubicOut);
        await MainLayout.TranslateToAsync(0, 0, 600, Easing.CubicOut);
    }
}