using Microsoft.Maui.Controls.Shapes;

namespace frontend;

public class ChatPage : ContentPage
{
    CollectionView conversationsList;
    VerticalStackLayout messagesLayout;
    Label chatTitle;

    public ChatPage()
    {
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            IsVisible = false,
            IsEnabled = false
        });

        BackgroundColor = Color.FromArgb("#121212");

        // ==========
        // LEFT PANEL
        // ==========

        var avatar = new Image
        {
            Source = "default_avatar.jpg",
            HeightRequest = 38,
            WidthRequest = 38,
            Aspect = Aspect.AspectFill
        };

        var username = new Label
        {
            Text = "Gab",
            FontFamily = "Retro",
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };

        var optionsButton = new ImageButton
        {
            Source = "options.png",
            HeightRequest = 24,
            WidthRequest = 24,
            BackgroundColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center
        };

        var menuFlyout = new MenuFlyout();
        menuFlyout.Add(new MenuFlyoutItem
        {
            Text = "Logout",
            Command = new Command(async () =>
            {
                await Shell.Current.GoToAsync("///MainPage");
            })
        });

        FlyoutBase.SetContextFlyout(optionsButton, menuFlyout);

        optionsButton.Clicked += async (_, __) =>
        {
            string action = await DisplayActionSheetAsync(
                "Options",
                "Cancel",
                null,
                "Logout"
            );

            if (action == "Logout")
            {
                await Shell.Current.GoToAsync("///MainPage");
            }
        };

        var profileRow = new Grid
        {
            Padding = 12,
            ColumnSpacing = 12,
            IsClippedToBounds = true,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto }, // avatar
                new ColumnDefinition { Width = GridLength.Star }, // username
                new ColumnDefinition { Width = GridLength.Auto }  // options
            }
        };

        profileRow.Add(avatar, 0, 0);
        profileRow.Add(username, 1, 0);
        profileRow.Add(optionsButton, 2, 0);

        conversationsList = new CollectionView
        {
            ItemsSource = new[]
            {
                "Alice",
                "Bob",
                "Charlie",
                "Dev Team"
            },
            SelectionMode = SelectionMode.Single,
            ItemTemplate = new DataTemplate(() =>
            {
                var name = new Label
                {
                    TextColor = Colors.White,
                    FontFamily = "Retro",
                    FontSize = 14
                };
                name.SetBinding(Label.TextProperty, ".");

                return new Border
                {
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = 10
                    },
                    BackgroundColor = Color.FromArgb("#1E1E1E"),
                    Padding = 12,
                    Margin = new Thickness(6, 4),
                    Content = name
                };
            })
        };

        conversationsList.SelectionChanged += OnConversationSelected;

        var leftPanel = new VerticalStackLayout
        {
            WidthRequest = 260,
            BackgroundColor = Color.FromArgb("#181818"),
            Children =
            {
                profileRow,
                new BoxView { HeightRequest = 1, BackgroundColor = Colors.Gray },
                conversationsList
            }
        };

        // ===========
        // RIGHT PANEL
        // ===========

        chatTitle = new Label
        {
            Text = "Select a conversation",
            FontFamily = "Retro",
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            FontSize = 18,
            Padding = 12
        };

        messagesLayout = new VerticalStackLayout
        {
            Spacing = 8,
            Padding = 12
        };

        var messagesScroll = new ScrollView
        {
            Content = messagesLayout
        };

        var messageEntry = new Entry
        {
            Placeholder = "Type a message...",
            FontFamily = "Retro",
            TextColor = Colors.White,
            PlaceholderColor = Colors.Gray,
            BackgroundColor = Color.FromArgb("#1E1E1E")
        };

        var sendButton = new Button
        {
            Text = "Send",
            FontFamily = "Retro",
            BackgroundColor = Color.FromArgb("#4F46E5"),
            TextColor = Colors.White
        };

        sendButton.Clicked += (_, __) =>
        {
            if (!string.IsNullOrWhiteSpace(messageEntry.Text))
            {
                AddMessage("You", messageEntry.Text);
                messageEntry.Text = string.Empty;
            }
        };

        var inputRow = new HorizontalStackLayout
        {
            Padding = 8,
            Spacing = 8,
            Children = { messageEntry, sendButton }
        };

        var rightPanel = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            BackgroundColor = Color.FromArgb("#121212")
        };

        rightPanel.Add(chatTitle, 0, 0);
        rightPanel.Add(messagesScroll, 0, 1);
        rightPanel.Add(inputRow, 0, 2);

        // ========
        // THE GRID
        // ========

        var mainGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            }
        };

        mainGrid.Add(leftPanel, 0, 0);
        mainGrid.Add(rightPanel, 1, 0);

        Content = mainGrid;
    }

    // ======
    // EVENTS
    // ======

    void OnConversationSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string name)
        {
            chatTitle.Text = name;
            messagesLayout.Children.Clear();

            AddMessage(name, "Hello ");
            AddMessage("You", "Hi!");
        }
    }

    void AddMessage(string author, string text)
    {
        messagesLayout.Children.Add(new Border
        {
            StrokeShape = new RoundRectangle
            {
                CornerRadius = 12
            },
            Padding = 10,
            BackgroundColor = author == "You"
                ? Color.FromArgb("#4F46E5")
                : Color.FromArgb("#1E1E1E"),
            Content = new Label
            {
                Text = $"{author}: {text}",
                TextColor = Colors.White,
                FontFamily = "Retro",
            }
        });
    }
}
