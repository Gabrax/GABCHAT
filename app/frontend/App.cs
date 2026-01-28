using Microsoft.Maui.Controls;

namespace frontend
{
    public class App : Application
    {
        public App()
        {
            Resources = new ResourceDictionary();

            Resources.Add("PrimaryColor", Color.FromArgb("#4F46E5"));
            Resources.Add("SecondaryColor", Color.FromArgb("#888"));
            Resources.Add("ErrorColor", Color.FromArgb("#FF4D4F"));

            Resources.Add("Headline", new Style(typeof(Label))
            {
                Setters =
                {
                    new Setter { Property = Label.FontSizeProperty, Value = 28 },
                    new Setter { Property = Label.FontAttributesProperty, Value = FontAttributes.Bold },
                    new Setter { Property = Label.TextColorProperty, Value = Colors.White }
                }
            });

            Resources.Add("SubHeadline", new Style(typeof(Label))
            {
                Setters =
                {
                    new Setter { Property = Label.FontSizeProperty, Value = 16 },
                    new Setter { Property = Label.TextColorProperty, Value = Colors.Gray }
                }
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
