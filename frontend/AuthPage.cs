using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Path = System.IO.Path;

namespace frontend
{
    public class AuthPage : ContentPage
    {
        private readonly MediaElement backgroundVideo;

        private readonly VerticalStackLayout _loginLayout;
        private readonly VerticalStackLayout _signUpLayout;
        private readonly VerticalStackLayout _passwordResetLayout;
        public AuthPage()
        {
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                IsVisible = false,
                IsEnabled = false
            });

            backgroundVideo = new MediaElement
            {
                ShouldAutoPlay = true,
                ShouldLoopPlayback = true,
                ShouldShowPlaybackControls = false,
                Aspect = Aspect.AspectFill,
                Volume = 0,
                InputTransparent = true,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            Loaded += async (_, __) =>
            {
                var stream = await FileSystem.OpenAppPackageFileAsync("background.mp4");
                var filePath = Path.Combine(FileSystem.CacheDirectory, "background.mp4");

                using (var fileStream = File.Create(filePath))
                    await stream.CopyToAsync(fileStream);

                backgroundVideo.Source = MediaSource.FromFile(filePath);
                backgroundVideo.Play();
            };

            _loginLayout = CreateLoginLayout();

            _signUpLayout = CreateRegistrationLayout();
            _signUpLayout.Opacity = 0;
            _signUpLayout.IsVisible = false;

            _passwordResetLayout = CreatePasswordResetLayout();
            _passwordResetLayout.Opacity = 0;
            _passwordResetLayout.IsVisible = false;

            Content = new Grid
            {
                Children =
                {
                    backgroundVideo,
                    _loginLayout,
                    _signUpLayout,
                    _passwordResetLayout
                }
            };
        }

        private VerticalStackLayout CreateLoginLayout()
        {
            Entry _loginEmailEntry = new Entry { Placeholder = "Email", TextColor = Colors.White, FontFamily = "Retro", BackgroundColor = Colors.Transparent };
            Border _loginEmailBorder = new Border { WidthRequest = 200, BackgroundColor = Color.FromArgb("#1E1E1E"), StrokeShape = new RoundRectangle { CornerRadius = 14 }, Padding = 14, Content = _loginEmailEntry };
            Label _loginEmailErrorLabel = new Label { Text = "Invalid email format", FontFamily = "Retro", TextColor = Color.FromArgb("#FF4D4F"), FontSize = 12, IsVisible = false, Margin = new Thickness(6, 0, 0, 0), HorizontalOptions = LayoutOptions.Center };
            VerticalStackLayout _loginEmailLayout = new VerticalStackLayout { Spacing = 4, Children = { _loginEmailBorder, _loginEmailErrorLabel } };

            Entry _loginPasswordEntry = new Entry { Placeholder = "Password", IsPassword = true, TextColor = Colors.White, FontFamily = "Retro", BackgroundColor = Colors.Transparent };
            Border _loginPasswordBorder = new Border { WidthRequest = 200, BackgroundColor = Color.FromArgb("#1E1E1E"), StrokeShape = new RoundRectangle { CornerRadius = 14 }, Padding = 14, Content = _loginPasswordEntry };
            Label _loginPasswordErrorLabel = new Label { Text = "Invalid password format.", FontFamily = "Retro", TextColor = Color.FromArgb("#FF4D4F"), FontSize = 12, IsVisible = false, Margin = new Thickness(6, 0, 0, 0), HorizontalOptions = LayoutOptions.Center };
            VerticalStackLayout _loginPasswordLayout = new VerticalStackLayout { Spacing = 4, Children = { _loginPasswordBorder, _loginPasswordErrorLabel } };

            Button _loginButton = new Button { Text = "Login", BackgroundColor = Color.FromArgb("#4F46E5"), FontFamily = "Retro", TextColor = Colors.White, CornerRadius = 14, WidthRequest = 200, HeightRequest = 55 };
            _loginButton.Clicked += async (s, e) =>
            {
                if (_loginButton == null) return;

                await _loginButton.ScaleToAsync(0.95, 80);
                await _loginButton.ScaleToAsync(1, 80);

                bool isEmailValid = IsEmailValid(_loginEmailEntry?.Text ?? string.Empty, _loginEmailBorder, _loginEmailErrorLabel);
                bool isPasswordValid = IsPasswordlValid(_loginPasswordEntry?.Text ?? string.Empty, _loginPasswordBorder, _loginPasswordErrorLabel);

                if (!isEmailValid || !isPasswordValid)
                {
                    if (!isEmailValid && _loginEmailBorder != null)
                        await Shake(_loginEmailBorder);

                    if (!isPasswordValid && _loginPasswordBorder != null)
                        await Shake(_loginPasswordBorder);

                    return;
                }

                _loginButton.Text = "Logging in...";
                _loginButton.IsEnabled = false;

                await Task.Delay(1200);

                //User? user = await AuthClient.Login(_loginEmailEntry?.Text ?? string.Empty, _loginPasswordEntry?.Text ?? string.Empty);
                //if (user == null)
                //{
                //    await DisplayAlertAsync("Error", "Invalid credentials", "OK");
                //    _loginButton.Text = "Login";
                //    _loginButton.IsEnabled = true;
                //    return;
                //}

                //await DisplayAlertAsync("Success", $"Welcome, {user.username}!", "OK");
            };

            Label _loginPasswordResetLabel = new Label { Text = "Forgot password? Reset here", FontSize = 12, FontFamily = "Retro", TextColor = Colors.FloralWhite, HorizontalOptions = LayoutOptions.Center, TextDecorations = TextDecorations.Underline };
            _loginPasswordResetLabel.GestureRecognizers.Add(
            new TapGestureRecognizer
            {
                Command = new Command(async () =>
                {
                    _passwordResetLayout.IsVisible = true;
                    _passwordResetLayout.Opacity = 0;
                    _passwordResetLayout.TranslationY = 30;

                    _loginLayout.Opacity = 1;
                    _loginLayout.TranslationY = 0;

                    await Task.WhenAll(
                        _loginLayout.FadeToAsync(0, 300, Easing.CubicIn),
                        _loginLayout.TranslateToAsync(0, -30, 300, Easing.CubicIn),

                        _passwordResetLayout.FadeToAsync(1, 300, Easing.CubicOut),
                        _passwordResetLayout.TranslateToAsync(0, 0, 300, Easing.CubicOut)
                    );

                    _loginLayout.IsVisible = false;
                })
            });

            Label _newUserLabel = new Label { Text = "New user? Sign up here", FontSize = 12, FontFamily = "Retro", TextColor = Colors.FloralWhite, HorizontalOptions = LayoutOptions.Center, TextDecorations = TextDecorations.Underline };
            _newUserLabel.GestureRecognizers.Add(
                new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                    {
                        _signUpLayout.IsVisible = true;
                        _signUpLayout.Opacity = 0;
                        _signUpLayout.TranslationY = 30;

                        _loginLayout.Opacity = 1;
                        _loginLayout.TranslationY = 0;

                        await Task.WhenAll(
                            _loginLayout.FadeToAsync(0, 300, Easing.CubicIn),
                            _loginLayout.TranslateToAsync(0, -30, 300, Easing.CubicIn),

                            _signUpLayout.FadeToAsync(1, 300, Easing.CubicOut),
                            _signUpLayout.TranslateToAsync(0, 0, 300, Easing.CubicOut)
                        );

                        _loginLayout.IsVisible = false;
                    })
                });
#if WINDOWS
            _loginPasswordResetLabel.EnableHoverCursor(CursorIcon.Hand);
            _loginButton.EnableHoverCursor(CursorIcon.Hand);
            _newUserLabel.EnableHoverCursor(CursorIcon.Hand);
#endif

            return new VerticalStackLayout
            {
                Padding = 30,
                Spacing = 20,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = "LOGIN", FontSize = 38, FontFamily = "Retro", FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center },
                    _loginEmailLayout,
                    _loginPasswordLayout,
                    _loginButton,
                    _loginPasswordResetLabel,
                    _newUserLabel
                }
            };
        }

        private VerticalStackLayout CreateRegistrationLayout()
        {
            Entry _signUpUserNameEntry = new Entry { Placeholder = "Username", TextColor = Colors.White, FontFamily = "Retro", BackgroundColor = Colors.Transparent };
            Border _signUpUserNameBorder = new Border { WidthRequest = 200, BackgroundColor = Color.FromArgb("#1E1E1E"), StrokeShape = new RoundRectangle { CornerRadius = 14 }, Padding = 14, Content = _signUpUserNameEntry };
            Label _signUpUserNameErrorLabel = new Label { Text = "Invalid username", FontFamily = "Retro", TextColor = Color.FromArgb("#FF4D4F"), FontSize = 12, IsVisible = false, Margin = new Thickness(6, 0, 0, 0), HorizontalOptions = LayoutOptions.Center };
            VerticalStackLayout _signUpUserNameLayout = new VerticalStackLayout { Spacing = 4, Children = { _signUpUserNameBorder, _signUpUserNameErrorLabel } };

            Entry _signUpEmailEntry = new Entry { Placeholder = "Email", TextColor = Colors.White, FontFamily = "Retro", BackgroundColor = Colors.Transparent };
            Border _signUpEmailBorder = new Border { WidthRequest = 200, BackgroundColor = Color.FromArgb("#1E1E1E"), StrokeShape = new RoundRectangle { CornerRadius = 14 }, Padding = 14, Content = _signUpEmailEntry };
            Label _signUpEmailErrorLabel = new Label { Text = "Invalid email format", FontFamily = "Retro", TextColor = Color.FromArgb("#FF4D4F"), FontSize = 12, IsVisible = false, Margin = new Thickness(6, 0, 0, 0), HorizontalOptions = LayoutOptions.Center };
            VerticalStackLayout _signUpEmailLayout = new VerticalStackLayout { Spacing = 4, Children = { _signUpEmailBorder, _signUpEmailErrorLabel } };

            Entry _signUpPasswordEntry = new Entry { Placeholder = "Password", IsPassword = true, TextColor = Colors.White, FontFamily = "Retro", BackgroundColor = Colors.Transparent };
            Border _signUpPasswordBorder = new Border { WidthRequest = 200, BackgroundColor = Color.FromArgb("#1E1E1E"), StrokeShape = new RoundRectangle { CornerRadius = 14 }, Padding = 14, Content = _signUpPasswordEntry };
            Label _signUpPasswordErrorLabel = new Label { Text = "Password must be at least 8 characters long and include an uppercase letter, a digit, and a special character.", FontFamily = "Retro", TextColor = Color.FromArgb("#FF4D4F"), FontSize = 12, IsVisible = false, Margin = new Thickness(6, 0, 0, 0), HorizontalOptions = LayoutOptions.Center };
            VerticalStackLayout _signUpPasswordLayout = new VerticalStackLayout { Spacing = 4, Children = { _signUpPasswordBorder, _signUpPasswordErrorLabel } };

            Button _signUpButton = new Button { Text = "Register", BackgroundColor = Color.FromArgb("#4F46E5"), TextColor = Colors.White, FontFamily = "Retro", CornerRadius = 14, WidthRequest = 200, HeightRequest = 55 };
            _signUpButton.Clicked += async (s, e) =>
            {
                if (_signUpButton == null) return;

                if (_signUpUserNameEntry?.Text == null || _signUpEmailEntry?.Text == null || _signUpPasswordEntry?.Text == null)
                {
                    return;
                }

                await _signUpButton.ScaleToAsync(0.95, 80);
                await _signUpButton.ScaleToAsync(1, 80);

                bool isEmailValid = IsEmailValid(_signUpEmailEntry?.Text ?? string.Empty, _signUpEmailBorder, _signUpEmailErrorLabel);
                bool isPasswordValid = IsPasswordlValid(_signUpPasswordEntry?.Text ?? string.Empty, _signUpPasswordBorder, _signUpPasswordErrorLabel);

                if (!isEmailValid || !isPasswordValid)
                {
                    if (!isEmailValid && _signUpEmailBorder != null)
                        await Shake(_signUpEmailBorder);

                    if (!isPasswordValid && _signUpPasswordBorder != null)
                        await Shake(_signUpPasswordBorder);

                    return;
                }

                _signUpButton.Text = "Account created successfully!";
                _signUpButton.IsEnabled = false;

                await Task.Delay(1200);

                //bool success = await AuthClient.Register(_signUpUserNameEntry.Text, _signUpEmailEntry.Text, _signUpPasswordEntry.Text);
                //if (!success)
                //{
                //    await DisplayAlertAsync("Error", "Registration failed", "OK");
                //    return;
                //}

                //await Navigation.PushAsync(new ChatPage());
            };

            Label _backLabel = new Label { Text = "Back to Login", FontSize = 12, TextColor = Colors.FloralWhite, FontFamily = "Retro", HorizontalOptions = LayoutOptions.Center, TextDecorations = TextDecorations.Underline };
            _backLabel.GestureRecognizers.Add(
                new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                    {
                        _loginLayout.IsVisible = true;
                        _loginLayout.Opacity = 0;
                        _loginLayout.TranslationY = 30;

                        _signUpLayout.Opacity = 1;
                        _signUpLayout.TranslationY = 0;

                        await Task.WhenAll(
                            _signUpLayout.FadeToAsync(0, 300, Easing.CubicIn),
                            _signUpLayout.TranslateToAsync(0, -30, 300, Easing.CubicIn),

                            _loginLayout.FadeToAsync(1, 300, Easing.CubicOut),
                            _loginLayout.TranslateToAsync(0, 0, 300, Easing.CubicOut)
                        );

                        _signUpLayout.IsVisible = false;
                    })
                });
#if WINDOWS
            _signUpButton.EnableHoverCursor(CursorIcon.Hand);
            _backLabel.EnableHoverCursor(CursorIcon.Hand);
#endif

            return new VerticalStackLayout
            {
                Padding = 30,
                Spacing = 20,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = "SIGN UP", FontSize = 38, FontFamily = "Retro", FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center },
                    _signUpUserNameLayout,
                    _signUpEmailLayout,
                    _signUpPasswordLayout,
                    _signUpButton,
                    _backLabel
                }
            };
        }

        private VerticalStackLayout CreatePasswordResetLayout()
        {
            Entry _signUpEmailEntry = new Entry { Placeholder = "Email", TextColor = Colors.White, FontFamily = "Retro", BackgroundColor = Colors.Transparent };
            Border _signUpEmailBorder = new Border { WidthRequest = 200, BackgroundColor = Color.FromArgb("#1E1E1E"), StrokeShape = new RoundRectangle { CornerRadius = 14 }, Padding = 14, Content = _signUpEmailEntry };
            Label _signUpEmailErrorLabel = new Label { Text = "Invalid email format", FontFamily = "Retro", TextColor = Color.FromArgb("#FF4D4F"), FontSize = 12, IsVisible = false, Margin = new Thickness(6, 0, 0, 0), HorizontalOptions = LayoutOptions.Center };
            VerticalStackLayout _signUpEmailLayout = new VerticalStackLayout { Spacing = 4, Children = { _signUpEmailBorder, _signUpEmailErrorLabel } };

            Entry _resetCodeEntry = new Entry { Placeholder = "Reset Code", TextColor = Colors.White, FontFamily = "Retro", BackgroundColor = Colors.Transparent };
            Border _resetCodeBorder = new Border { WidthRequest = 200, BackgroundColor = Color.FromArgb("#1E1E1E"), StrokeShape = new RoundRectangle { CornerRadius = 14 }, Padding = 14, Content = _resetCodeEntry };
            Label _resetCodeErrorLabel = new Label { Text = "Invalid code", FontFamily = "Retro", TextColor = Color.FromArgb("#FF4D4F"), FontSize = 12, IsVisible = false, Margin = new Thickness(6, 0, 0, 0), HorizontalOptions = LayoutOptions.Center };
            VerticalStackLayout _resetCodeLayout = new VerticalStackLayout { Spacing = 4, Children = { _resetCodeBorder, _resetCodeErrorLabel } };

            Entry _signUpPasswordEntry = new Entry { Placeholder = "New password", IsPassword = true, TextColor = Colors.White, FontFamily = "Retro", BackgroundColor = Colors.Transparent };
            Border _signUpPasswordBorder = new Border { WidthRequest = 200, BackgroundColor = Color.FromArgb("#1E1E1E"), StrokeShape = new RoundRectangle { CornerRadius = 14 }, Padding = 14, Content = _signUpPasswordEntry };
            Label _signUpPasswordErrorLabel = new Label { Text = "Password must be at least 8 characters long and include an uppercase letter, a digit, and a special character.", FontFamily = "Retro", TextColor = Color.FromArgb("#FF4D4F"), FontSize = 12, IsVisible = false, Margin = new Thickness(6, 0, 0, 0), HorizontalOptions = LayoutOptions.Center };
            VerticalStackLayout _signUpPasswordLayout = new VerticalStackLayout { Spacing = 4, Children = { _signUpPasswordBorder, _signUpPasswordErrorLabel } };

            Button _signUpButton = new Button { Text = "Reset", BackgroundColor = Color.FromArgb("#4F46E5"), TextColor = Colors.White, FontFamily = "Retro", CornerRadius = 14, WidthRequest = 200, HeightRequest = 55 };
            _signUpButton.Clicked += async (s, e) =>
            {
                if (_signUpButton == null) return;

                if (_resetCodeEntry?.Text == null || _signUpEmailEntry?.Text == null || _signUpPasswordEntry?.Text == null)
                {
                    return;
                }

                await _signUpButton.ScaleToAsync(0.95, 80);
                await _signUpButton.ScaleToAsync(1, 80);

                bool isEmailValid = IsEmailValid(_signUpEmailEntry?.Text ?? string.Empty, _signUpEmailBorder, _signUpEmailErrorLabel);
                bool isPasswordValid = IsPasswordlValid(_signUpPasswordEntry?.Text ?? string.Empty, _signUpPasswordBorder, _signUpPasswordErrorLabel);

                if (!isEmailValid || !isPasswordValid)
                {
                    if (!isEmailValid && _signUpEmailBorder != null)
                        await Shake(_signUpEmailBorder);

                    if (!isPasswordValid && _signUpPasswordBorder != null)
                        await Shake(_signUpPasswordBorder);

                    return;
                }

                _signUpButton.Text = "Password reset success!";
                _signUpButton.IsEnabled = false;

                await Task.Delay(1200);

                //bool success = await AuthClient.Register(_signUpUserNameEntry.Text, _signUpEmailEntry.Text, _signUpPasswordEntry.Text);
                //if (!success)
                //{
                //    await DisplayAlertAsync("Error", "Registration failed", "OK");
                //    return;
                //}

                //await Navigation.PushAsync(new ChatPage());
            };

            Label _backLabel = new Label { Text = "Back to Login", FontSize = 12, TextColor = Colors.FloralWhite, FontFamily = "Retro", HorizontalOptions = LayoutOptions.Center, TextDecorations = TextDecorations.Underline };
            _backLabel.GestureRecognizers.Add(
                new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                    {
                        _loginLayout.IsVisible = true;
                        _loginLayout.Opacity = 0;
                        _loginLayout.TranslationY = 30;

                        _passwordResetLayout.Opacity = 1;
                        _passwordResetLayout.TranslationY = 0;

                        await Task.WhenAll(
                            _passwordResetLayout.FadeToAsync(0, 300, Easing.CubicIn),
                            _passwordResetLayout.TranslateToAsync(0, -30, 300, Easing.CubicIn),

                            _loginLayout.FadeToAsync(1, 300, Easing.CubicOut),
                            _loginLayout.TranslateToAsync(0, 0, 300, Easing.CubicOut)
                        );

                        _passwordResetLayout.IsVisible = false;
                    })
                });
#if WINDOWS
            _signUpButton.EnableHoverCursor(CursorIcon.Hand);
            _backLabel.EnableHoverCursor(CursorIcon.Hand);
#endif

            return new VerticalStackLayout
            {
                Padding = 30,
                Spacing = 20,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label { Text = "RESET", FontSize = 38, FontFamily = "Retro", FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center },
                    _signUpEmailLayout,
                    _resetCodeLayout,
                    _signUpPasswordLayout,
                    _signUpButton,
                    _backLabel
                }
            };
        }

        protected override async void OnAppearing()
        {
            //base.OnAppearing();

            //await MainLayout.FadeToAsync(1, 600, Easing.CubicOut);
            //await MainLayout.TranslateToAsync(0, 0, 600, Easing.CubicOut);
        }

        private async Task Shake(Border border)
        {
            const int delta = 10;
            await border.TranslateToAsync(-delta, 0, 50);
            await border.TranslateToAsync(delta, 0, 50);
            await border.TranslateToAsync(-delta, 0, 50);
            await border.TranslateToAsync(0, 0, 50);
        }

        private bool IsEmailValid(string email, Border border, Label label)
        {

            if (string.IsNullOrWhiteSpace(email)) return false;

            bool isValid = Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

            if (border != null)
                border.Stroke = isValid ? Colors.Green : Colors.Red;
            if (label != null)
            {
                label.IsVisible = !isValid;
                label.Text = isValid ? string.Empty : "Invalid email format";
            }

            return isValid;
        }

        private bool IsPasswordlValid(string password, Border border, Label label)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            bool has8length = password.Length >= 8;
            bool hasUpper = password.Any(char.IsUpper);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            bool isValid = has8length && hasUpper && hasDigit && hasSpecial;

            if (border != null)
                border.Stroke = isValid ? Colors.Green : Colors.Red;
            if (label != null)
            {
                label.IsVisible = !isValid;
                label.Text = isValid ? string.Empty : "Password must be at least 8 characters long and include an uppercase letter," +
                    " a digit, and a special character.";
            }

            return isValid;
        }
    }
}
