using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.Shapes;
using System.Text.RegularExpressions;
using Microsoft.Maui.Storage;
using Path = System.IO.Path;

namespace frontend
{
    public class LoginPage : ContentPage
    {
        private Entry EmailEntry;
        private Border EmailBorder;
        private Label EmailErrorLabel;

        private Entry PasswordEntry;
        private Border PasswordBorder;
        private Label PasswordErrorLabel;
        private Label ResetPasswordLabel;

        private Button LoginButton;
        private Label NewUserLabel;
        private VerticalStackLayout MainLayout;

        public LoginPage()
        {

            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                IsVisible = false,
                IsEnabled = false
            });

            // EMAIL
            EmailEntry = new Entry
            {
                Placeholder = "Email",
                Keyboard = Keyboard.Email,
                TextColor = Colors.White,
                FontFamily = "Retro",
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
                FontFamily = "Retro",
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
                FontFamily = "Retro",
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
                FontFamily = "Retro",
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
            LoginButton = new Button
            {
                WidthRequest = 200,
                HeightRequest = 55,
                Text = "Login",
                FontFamily = "Retro",
                CornerRadius = 14,
                BackgroundColor = Color.FromArgb("#4F46E5"),
                TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold
            };
            LoginButton.Clicked += OnLoginClicked;
#if WINDOWS
            LoginButton.EnableHoverCursor(CursorIcon.Hand);
#endif

            ResetPasswordLabel = new Label
            {
                Text = "Forgot password? Reset here",
                FontFamily = "Retro",
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.FloralWhite,
                TextDecorations = TextDecorations.Underline
            };
#if WINDOWS
            ResetPasswordLabel.EnableHoverCursor(CursorIcon.Hand);
#endif

            NewUserLabel = new Label
            {
                Text = "New user? Sign up here",
                FontFamily = "Retro",
                FontSize = 12,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.FloralWhite,
                TextDecorations = TextDecorations.Underline
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += OnSignUpTapped;
            NewUserLabel.GestureRecognizers.Add(tapGesture);
#if WINDOWS
            NewUserLabel.EnableHoverCursor(CursorIcon.Hand);
#endif


            MainLayout = new VerticalStackLayout
            {
                Padding = 30,
                Spacing = 20,
                VerticalOptions = LayoutOptions.Center,
                Opacity = 1,
                TranslationY = 40,
                Children =
                {
                    //new Image
                    //{
                    //    Source = "dotnet_bot.png",
                    //    HeightRequest = 120,
                    //    HorizontalOptions = LayoutOptions.Center
                    //},
                    new Label
                    {
                        Text = "GABCHAT",
                        FontFamily = "Retro",
                        FontSize = 38,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.White,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    emailLayout,
                    passwordLayout,
                    LoginButton,
                    ResetPasswordLabel,
                    NewUserLabel
                }
            };

            var backgroundVideo = new MediaElement
            {
                ShouldAutoPlay = true,
                ShouldLoopPlayback = true,
                ShouldShowPlaybackControls = false,
                Aspect = Aspect.AspectFill,
                Volume = 0,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            Content = new Grid
            {
                Children =
                {
                    backgroundVideo,
                    MainLayout
                }
            };

            Loaded += async (_, __) =>
            {
                var stream = await FileSystem.OpenAppPackageFileAsync("background.mp4");

                var filePath = Path.Combine(FileSystem.CacheDirectory, "background.mp4");

                using (var fileStream = File.Create(filePath))
                    await stream.CopyToAsync(fileStream);

                backgroundVideo.Source = MediaSource.FromUri(filePath);

                backgroundVideo.Play();
            };
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await MainLayout.FadeToAsync(1, 600, Easing.CubicOut);
            await MainLayout.TranslateToAsync(0, 0, 600, Easing.CubicOut);
        }

        private async Task Shake(Border border)
        {
            const int delta = 10;
            await border.TranslateToAsync(-delta, 0, 50);
            await border.TranslateToAsync(delta, 0, 50);
            await border.TranslateToAsync(-delta, 0, 50);
            await border.TranslateToAsync(0, 0, 50);
        }

        private bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            bool isValid = Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

            EmailBorder.Stroke = isValid ? Colors.Green : Colors.Red;
            EmailErrorLabel.IsVisible = !isValid;
            EmailErrorLabel.Text = isValid ? string.Empty : "Invalid email format";

            return isValid;
        }

        private bool IsPasswordlValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            bool has8length = password.Length >= 8;
            bool hasUpper = password.Any(char.IsUpper);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            bool isValid = has8length && hasUpper && hasDigit && hasSpecial;

            PasswordBorder.Stroke = isValid ? Colors.Green : Colors.Red;
            PasswordErrorLabel.IsVisible = !isValid;
            PasswordErrorLabel.Text = isValid ? string.Empty : "Password must be at least 8 characters long and include an uppercase letter," +
                " a digit, and a special character.";

            return isValid;
        }

        private async void OnSignUpTapped(object? sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new RegistrationPage());
        }

        private async void OnLoginClicked(object? sender, EventArgs e)
        {
            await LoginButton.ScaleToAsync(0.95, 80);
            await LoginButton.ScaleToAsync(1, 80);

            bool isEmailValid = IsEmailValid(EmailEntry.Text);
            bool isPasswordValid = IsPasswordlValid(PasswordEntry.Text);

            if (!isEmailValid || !isPasswordValid)
            {
                if (!isEmailValid)
                    await Shake(EmailBorder);

                if (!isPasswordValid)
                    await Shake(PasswordBorder);

                return;
            }

            LoginButton.Text = "Logging in...";
            LoginButton.IsEnabled = false;

            await Task.Delay(1200);

            User? user = await AuthClient.Login(EmailEntry.Text, PasswordEntry.Text);
            if (user == null)
            {
                await DisplayAlertAsync("Error", "Invalid credentials", "OK");
                LoginButton.Text = "Login";
                LoginButton.IsEnabled = true;
                return;
            }

            await DisplayAlertAsync("Success", $"Welcome, {user.username}!", "OK");
        }
    }
}
