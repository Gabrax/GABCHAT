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
            Source = "dotnet_bot.png",
            HeightRequest = 48,
            WidthRequest = 48,
            Aspect = Aspect.AspectFill
        };

        var username = new Label
        {
            Text = "Gab",
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };

        var profileRow = new HorizontalStackLayout
        {
            Spacing = 12,
            Padding = 12,
            Children = { avatar, username }
        };

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
                    FontSize = 14
                };
                name.SetBinding(Label.TextProperty, ".");

                return new Frame
                {
                    Padding = 12,
                    Margin = new Thickness(6, 4),
                    CornerRadius = 10,
                    BackgroundColor = Color.FromArgb("#1E1E1E"),
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
            TextColor = Colors.White,
            PlaceholderColor = Colors.Gray,
            BackgroundColor = Color.FromArgb("#1E1E1E")
        };

        var sendButton = new Button
        {
            Text = "Send",
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
        messagesLayout.Children.Add(new Frame
        {
            CornerRadius = 12,
            Padding = 10,
            BackgroundColor = author == "You"
                ? Color.FromArgb("#4F46E5")
                : Color.FromArgb("#1E1E1E"),
            Content = new Label
            {
                Text = $"{author}: {text}",
                TextColor = Colors.White
            }
        });
    }
}
