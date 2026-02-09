using Microsoft.Maui.Controls.Shapes;


namespace frontend;

public class ChatPage : ContentPage
{
    class ChatMessage
    {
        public string Author { get; set; } = "";
        public string Text { get; set; } = "";
    }

    Dictionary<string, List<ChatMessage>> _conversationBuffers = new();
    Dictionary<string, Border> _openedChatPanels = new();

    string? _activeConversation;
    CollectionView _conversationsList;

    HorizontalStackLayout _openedChatsLayout;

    public ChatPage()
    {
        Shell.SetBackButtonBehavior(this, new BackButtonBehavior{IsVisible = false,IsEnabled = false });

        BackgroundColor = Color.FromArgb("#121212");

        // =========
        // LEFT PANEL
        // =========
        var avatar = new Image { Source = "default_avatar.jpg", HeightRequest = 38, WidthRequest = 38, Aspect = Aspect.AspectFill };
        var username = new Label { Text = "Gab", FontFamily = "Retro", TextColor = Colors.White, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
        var optionsButton = new ImageButton { Source = "options.png", HeightRequest = 24, WidthRequest = 24, BackgroundColor = Colors.Transparent, VerticalOptions = LayoutOptions.Center };
        optionsButton.Clicked += async (_, __) =>
        {
            optionsButton.Source = "options_pressed.png";
            await Task.Delay(100);
            optionsButton.Source = "options.png";

            string action = await DisplayActionSheetAsync(
                "Options","Cancel",null,"Logout"
            );

            if (action == "Logout") await Shell.Current.GoToAsync("///MainPage");
        };

        var profileRow = new Grid { Padding = 12, ColumnSpacing = 12, IsClippedToBounds = true, 
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        profileRow.Add(avatar, 0, 0);
        profileRow.Add(username, 1, 0);
        profileRow.Add(optionsButton, 2, 0);

        Entry searchConv = new Entry { Placeholder = "Search or Add", TextColor = Colors.White, FontFamily = "Retro", BackgroundColor = Colors.Transparent };
        ImageButton searchorAddButton = new ImageButton { Source = "enter.png", HeightRequest = 24, WidthRequest = 24, BackgroundColor = Colors.Transparent };
        searchorAddButton.Clicked += async (_, __) =>
        {
            searchorAddButton.Source = "enter_pressed.png";
            await Task.Delay(100);
            searchorAddButton.Source = "enter.png";
        };

        var searchRow = new Grid { Padding = 12,ColumnSpacing = 12,IsClippedToBounds = true,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };
        searchRow.Add(searchConv, 1, 0);
        searchRow.Add(searchorAddButton, 2, 0);
#if WINDOWS
        optionsButton.EnableHoverCursor(CursorIcon.Hand);
        searchorAddButton.EnableHoverCursor(CursorIcon.Hand);
#endif

        _conversationsList = new CollectionView
        {
            ItemsSource = new[] { "Alice", "Bob", "Charlie", "Dev Team" },
            SelectionMode = SelectionMode.Single,
            ItemTemplate = new DataTemplate(() =>
            {
                var label = new Label { FontFamily = "Retro", TextColor = Colors.White, Padding = 12 };
                label.SetBinding(Label.TextProperty, ".");

                return new Border
                {
                    BackgroundColor = Color.FromArgb("#1E1E1E"),
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    Margin = new Thickness(6, 4),
                    Content = label
                };
            }),
        };
#if WINDOWS
        _conversationsList.EnableHoverCursor(CursorIcon.Hand);
#endif
        _conversationsList.SelectionChanged += OnConversationSelected;

        var leftPanel = new VerticalStackLayout
        {
            WidthRequest = 260,
            BackgroundColor = Color.FromArgb("#181818"),
            Children = { profileRow, searchRow, _conversationsList }
        };

        // ===========
        // RIGHT PANEL
        // ===========
        _openedChatsLayout = new HorizontalStackLayout { Spacing = 12, Padding = 12 };
        var openedChatsScroll = new ScrollView{ Orientation = ScrollOrientation.Horizontal,Content = _openedChatsLayout };

        // ===========
        // ROOT GRID
        // ===========
        var mainGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = 260 }, // left panel
                new ColumnDefinition { Width = GridLength.Star } // chats
            }
        };

        mainGrid.Add(leftPanel, 0, 0);
        mainGrid.Add(openedChatsScroll, 1, 0);

        Content = mainGrid;
    }

    Border CreateChatPanel(string conversation)
    {
        var chatTitle = new Label{Text = conversation,FontFamily = "Retro",FontSize = 18,FontAttributes = FontAttributes.Bold,TextColor = Colors.White,HorizontalOptions = LayoutOptions.Center,VerticalOptions = LayoutOptions.Center};

        var messagesLayout = new VerticalStackLayout { Spacing = 8, Padding = 12 };
        var messagesScroll = new ScrollView{ Content = messagesLayout };
        var messageEntry = new Entry {Placeholder = "Type a message...", HorizontalOptions = LayoutOptions.Fill, FontFamily = "Retro",TextColor = Colors.White,PlaceholderColor = Colors.Gray,BackgroundColor = Color.FromArgb("#1E1E1E")};

        var sendButton = new Button{Text = "Send",FontFamily = "Retro",BackgroundColor = Color.FromArgb("#4F46E5"),TextColor = Colors.White};
        sendButton.Clicked += async (_, __) =>
        {
            if (string.IsNullOrWhiteSpace(messageEntry.Text))
                return;

            ChatMessage msg = new ChatMessage { Author = "You", Text = messageEntry.Text };

            _conversationBuffers[conversation].Add(msg);
            messageEntry.Text = string.Empty;

            await AddMessageAnimated(messagesLayout, messagesScroll, msg.Author, msg.Text);
        };

        var inputRow = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } }
        };
        inputRow.Add(messageEntry, 0, 0);
        inputRow.Add(sendButton, 1, 0);

        var closeBuffer = new ImageButton{Source = "exit.png",HeightRequest = 24,WidthRequest = 24,BackgroundColor = Colors.Transparent,HorizontalOptions = LayoutOptions.End,VerticalOptions = LayoutOptions.Center};
        closeBuffer.Clicked += async (_, __) =>
        {
            closeBuffer.Source = "exit_pressed.png";
            await Task.Delay(100);
            closeBuffer.Source = "exit.png";

            if (_openedChatPanels.TryGetValue(conversation, out Border? panel))
            {
                _openedChatsLayout.Children.Remove(panel);
                _openedChatPanels.Remove(conversation);

                if (_activeConversation == conversation)
                {
                    _activeConversation = null;
                    _conversationsList.SelectedItem = null;
                }
            }
        };
#if WINDOWS
        closeBuffer.EnableHoverCursor(CursorIcon.Hand);
        sendButton.EnableHoverCursor(CursorIcon.Hand);
#endif

        var headerRow = new HorizontalStackLayout{Padding = new Thickness(12, 0),Spacing = 6,VerticalOptions = LayoutOptions.Center,HorizontalOptions = LayoutOptions.Center};
        headerRow.Children.Add(chatTitle);
        headerRow.Children.Add(closeBuffer);

        var chatPanelGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = 56 }, // header
                new RowDefinition { Height = GridLength.Star }, // messages
                new RowDefinition { Height = GridLength.Auto }  // input
            },
            BackgroundColor = Colors.Transparent
        };

        var chatPanel = new Border{ WidthRequest = 250, StrokeShape = new RoundRectangle { CornerRadius = 16 },BackgroundColor = Color.FromArgb("#1E1E1E"),Padding = 0, StrokeThickness = 2, Stroke = Colors.Transparent, Content = chatPanelGrid};

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_,__) => { SetActiveConversation(conversation); };
        chatPanel.GestureRecognizers.Add(tap);

        chatPanelGrid.Add(headerRow, 0, 0);
        chatPanelGrid.Add(messagesScroll, 0, 1);
        chatPanelGrid.Add(inputRow, 0, 2);

        // Render existing messages from buffer
        if (_conversationBuffers.ContainsKey(conversation))
        {
            foreach (var msg in _conversationBuffers[conversation])
                messagesLayout.Children.Add(CreateStaticBubble(msg.Author, msg.Text));
        }

        return chatPanel;
    }

    void SetActiveConversation(string conversation)
    {
        _activeConversation = conversation;

        foreach (var kv in _openedChatPanels)
            kv.Value.Stroke = Colors.Transparent;

        if (_openedChatPanels.TryGetValue(conversation, out var panel))
            panel.Stroke = Color.FromArgb("#4F46E5");

        _conversationsList.SelectedItem = conversation;
    }

    View CreateStaticBubble(string author, string text)
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = 10,
            BackgroundColor = author == "You" ? Color.FromArgb("#4F46E5") : Color.FromArgb("#1E1E1E"),
            HorizontalOptions = author == "You" ? LayoutOptions.End : LayoutOptions.Start,
            Content = new Label { Text = text, FontFamily = "Retro", TextColor = Colors.White }
        };
    }

    async Task AddMessageAnimated(VerticalStackLayout messagesLayout, ScrollView messagesScroll, string author, string text)
    {
        var bubble = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Padding = 10,
            BackgroundColor = author == "You" ? Color.FromArgb("#4F46E5") : Color.FromArgb("#1E1E1E"),
            HorizontalOptions = author == "You" ? LayoutOptions.End : LayoutOptions.Start,
            Opacity = 0,
            TranslationY = 12,
            Scale = 0.95,
            Content = new Label { Text = text,FontFamily = "Retro",TextColor = Colors.White }
        };

        messagesLayout.Children.Add(bubble);

        await Task.WhenAll(
            bubble.FadeToAsync(1, 160, Easing.CubicOut),
            bubble.TranslateToAsync(0, 0, 160, Easing.CubicOut),
            bubble.ScaleToAsync(1, 160, Easing.CubicOut)
        );

        await Task.Delay(30);
        await messagesScroll.ScrollToAsync(0, messagesLayout.Height, true);
    }

    void OnConversationSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not string name)
            return;

        if (!_conversationBuffers.ContainsKey(name))
            _conversationBuffers[name] = new List<ChatMessage>();

        if (!_openedChatPanels.ContainsKey(name))
        {
            var panel = CreateChatPanel(name);
            _openedChatPanels[name] = panel;
            _openedChatsLayout.Children.Add(panel);
        }

        SetActiveConversation(name);
    }
}
