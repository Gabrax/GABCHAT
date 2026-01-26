using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace frontend
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
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

        private async void OnLoginClicked(object sender, EventArgs e)
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

            LoginButton.Text = "Login";
            LoginButton.IsEnabled = true;

            await DisplayAlertAsync("Success", "Logged in!", "OK");
        }
    }
}
