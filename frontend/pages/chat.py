import flet as ft


async def chatPage(page: ft.Page):
    page.clean()

    page.title = "GABCHAT"
    page.padding = 0
    page.theme_mode = ft.ThemeMode.LIGHT

    page.window.width = 960
    page.window.height = 640
    page.window.alignment = ft.Alignment.CENTER
    page.update()
    await page.window.center()

    current_user = {
        "name": "Jan Kowalski",
        "status": "Dostępny",
        "avatar_url": None,
    }

    contacts = [
        {
            "name": "Anna Nowak",
            "status": "Dostępna",
            "last_message": "Cześć! Jak leci?",
            "avatar_url": None,
        },
        {
            "name": "Piotr Wiśniewski",
            "status": "Offline",
            "last_message": "Wysyłam pliki jutro.",
            "avatar_url": None,
        },
        {
            "name": "Kasia Zielińska",
            "status": "Dostępna",
            "last_message": "Dzięki wielkie :)",
            "avatar_url": None,
        },
        {
            "name": "Tomek Kowalczyk",
            "status": "Offline",
            "last_message": "Widzimy się o 18?",
            "avatar_url": None,
        },
        {
            "name": "Marta Lewandowska",
            "status": "Dostępna",
            "last_message": "Ok, trzymam kciuki.",
            "avatar_url": None,
        },
    ]

    pending_invitations = [
        {
            "name": "Ola Mazur",
            "status": "Dostępna",
            "last_message": "Nowy kontakt",
            "avatar_url": None,
        },
        {
            "name": "Michał Wójcik",
            "status": "Offline",
            "last_message": "Nowy kontakt",
            "avatar_url": None,
        },
    ]
    sent_invitations: set[str] = set()

    conversations: dict[str, list[dict]] = {
        "Anna Nowak": [
            {"text": "Cześć! Jak leci?", "from_me": False},
            {"text": "Hej! Wszystko dobrze.", "from_me": True},
        ],
        "Piotr Wiśniewski": [
            {"text": "Wysyłam pliki jutro.", "from_me": False},
        ],
        "Kasia Zielińska": [
            {"text": "Dzięki wielkie :)", "from_me": False},
        ],
        "Tomek Kowalczyk": [
            {"text": "Widzimy się o 18?", "from_me": False},
        ],
        "Marta Lewandowska": [
            {"text": "Ok, trzymam kciuki.", "from_me": False},
        ],
    }

    selected_contact: dict | None = None

    messages_view = ft.ListView(
        expand=True,
        spacing=8,
        padding=ft.Padding.all(15),
        auto_scroll=True,
    )

    chat_header = ft.Container()

    chat_content = ft.Column(
        expand=True,
        spacing=0,
    )

    def avatar_for(
        name: str,
        url: str | None,
        size: int = 40,
    ) -> ft.CircleAvatar:
        if url:
            return ft.CircleAvatar(
                foreground_image_src=url,
                radius=size / 2,
            )

        initials = "".join(
            part[0].upper()
            for part in name.split()[:2]
            if part
        )

        return ft.CircleAvatar(
            content=ft.Text(initials),
            radius=size / 2,
        )

    def build_message_bubble(
        text: str,
        from_me: bool,
    ) -> ft.Row:
        return ft.Row(
            alignment=(
                ft.MainAxisAlignment.END
                if from_me
                else ft.MainAxisAlignment.START
            ),
            controls=[
                ft.Container(
                    padding=ft.Padding.symmetric(
                        horizontal=14,
                        vertical=8,
                    ),
                    bgcolor=(
                        ft.Colors.BLUE_400
                        if from_me
                        else ft.Colors.GREY_200
                    ),
                    border_radius=16,
                    content=ft.Text(
                        text,
                        color=(
                            ft.Colors.WHITE
                            if from_me
                            else ft.Colors.BLACK
                        ),
                        selectable=True,
                    ),
                ),
            ],
        )

    def show_empty_chat():
        chat_content.controls = [
            ft.Container(
                expand=True,
                alignment=ft.Alignment.CENTER,
                content=ft.Column(
                    alignment=ft.MainAxisAlignment.CENTER,
                    horizontal_alignment=ft.CrossAxisAlignment.CENTER,
                    controls=[
                        ft.Icon(
                            ft.Icons.CHAT_BUBBLE_OUTLINE,
                            size=64,
                            color=ft.Colors.GREY_400,
                        ),
                        ft.Text(
                            "Wybierz kontakt, aby rozpocząć rozmowę",
                            size=16,
                            color=ft.Colors.GREY_500,
                        ),
                    ],
                ),
            ),
        ]

    def refresh_messages():
        if selected_contact is None:
            return

        name = selected_contact["name"]
        messages_view.controls.clear()

        for message in conversations.get(name, []):
            messages_view.controls.append(
                build_message_bubble(
                    message["text"],
                    message["from_me"],
                )
            )

    async def send_message(event=None):
        if selected_contact is None:
            return

        text = (message_input.value or "").strip()

        if not text:
            await message_input.focus()
            return

        name = selected_contact["name"]

        conversations.setdefault(name, []).append(
            {
                "text": text,
                "from_me": True,
            }
        )

        messages_view.controls.append(
            build_message_bubble(text, True)
        )

        message_input.value = ""
        page.update()
        await message_input.focus()

    # Pole jest jednoliniowe, więc Enter wywołuje on_submit.
    message_input = ft.TextField(
        hint_text="Napisz wiadomość...",
        expand=True,
        border_radius=20,
        filled=True,
        on_submit=send_message,
    )

    send_button = ft.IconButton(
        icon=ft.Icons.SEND,
        icon_color=ft.Colors.BLUE_400,
        tooltip="Wyślij",
        on_click=send_message,
    )

    async def open_chat(contact: dict):
        nonlocal selected_contact

        selected_contact = contact
        name = contact["name"]

        conversations.setdefault(name, [])

        chat_header.content = ft.Row(
            controls=[
                avatar_for(
                    name,
                    contact.get("avatar_url"),
                    size=40,
                ),
                ft.Column(
                    spacing=0,
                    controls=[
                        ft.Text(
                            name,
                            size=16,
                            weight=ft.FontWeight.BOLD,
                        ),
                        ft.Text(
                            contact["status"],
                            size=12,
                            color=(
                                ft.Colors.GREEN_600
                                if contact["status"] == "Dostępna"
                                else ft.Colors.GREY_500
                            ),
                        ),
                    ],
                ),
            ],
        )

        refresh_messages()

        chat_content.controls = [
            chat_header,
            ft.Divider(height=1),
            messages_view,
            ft.Container(
                padding=ft.Padding.symmetric(
                    horizontal=10,
                    vertical=8,
                ),
                content=ft.Row(
                    vertical_alignment=ft.CrossAxisAlignment.CENTER,
                    controls=[
                        message_input,
                        send_button,
                    ],
                ),
            ),
        ]

        page.update()
        await message_input.focus()

    def contact_tile(contact: dict) -> ft.Container:
        async def handle_contact_click(event):
            await open_chat(contact)

        return ft.Container(
            padding=ft.Padding.symmetric(
                horizontal=10,
                vertical=7,
            ),
            border_radius=10,
            ink=True,
            on_click=handle_contact_click,
            content=ft.Row(
                controls=[
                    avatar_for(
                        contact["name"],
                        contact["avatar_url"],
                        size=44,
                    ),
                    ft.Column(
                        spacing=1,
                        expand=True,
                        controls=[
                            ft.Row(
                                controls=[
                                    ft.Text(
                                        contact["name"],
                                        size=14,
                                        weight=ft.FontWeight.W_600,
                                        expand=True,
                                    ),
                                    ft.Icon(
                                        ft.Icons.CIRCLE,
                                        size=10,
                                        color=(
                                            ft.Colors.GREEN_500
                                            if contact["status"] == "Dostępna"
                                            else ft.Colors.GREY_400
                                        ),
                                    ),
                                ],
                            ),
                            ft.Text(
                                contact["last_message"],
                                size=12,
                                color=ft.Colors.GREY_600,
                                max_lines=1,
                                overflow=ft.TextOverflow.ELLIPSIS,
                            ),
                        ],
                    ),
                ],
            ),
        )

    def refresh_contacts():
        query = (contact_search.value or "").strip().casefold()
        visible_contacts = [
            contact
            for contact in contacts
            if query in contact["name"].casefold()
        ]

        contacts_list.controls = [
            contact_tile(contact)
            for contact in visible_contacts
        ]

        if not visible_contacts:
            contacts_list.controls.append(
                ft.Container(
                    padding=15,
                    alignment=ft.Alignment.CENTER,
                    content=ft.Text(
                        "Nie znaleziono kontaktów",
                        size=12,
                        color=ft.Colors.GREY_500,
                    ),
                )
            )

    def show_contact_status(message: str, is_error: bool = False):
        contact_status.value = message
        contact_status.color = (
            ft.Colors.RED_600
            if is_error
            else ft.Colors.GREEN_600
        )
        contact_status.visible = True

    def filter_contacts(event=None):
        refresh_contacts()
        page.update()

    def send_contact_invitation(event=None):
        name = (invitation_name.value or "").strip()
        normalized_name = name.casefold()

        if not name:
            show_contact_status("Wpisz nazwę użytkownika.", True)
        elif any(
            contact["name"].casefold() == normalized_name
            for contact in contacts
        ):
            show_contact_status("Ta osoba jest już w kontaktach.", True)
        elif normalized_name in sent_invitations:
            show_contact_status("Zaproszenie zostało już wysłane.", True)
        else:
            sent_invitations.add(normalized_name)
            invitation_name.value = ""
            show_contact_status(f"Wysłano zaproszenie do: {name}")

        page.update()

    def accept_invitation(invitation: dict):
        if invitation not in pending_invitations:
            return

        pending_invitations.remove(invitation)
        contacts.append(invitation)
        conversations.setdefault(invitation["name"], [])
        refresh_contacts()
        refresh_invitations()
        show_contact_status(
            f"{invitation['name']} jest teraz w kontaktach."
        )
        page.update()

    def reject_invitation(invitation: dict):
        if invitation not in pending_invitations:
            return

        pending_invitations.remove(invitation)
        refresh_invitations()
        show_contact_status(
            f"Odrzucono zaproszenie od: {invitation['name']}"
        )
        page.update()

    def invitation_tile(invitation: dict) -> ft.Container:
        def handle_accept(event=None):
            accept_invitation(invitation)

        def handle_reject(event=None):
            reject_invitation(invitation)

        return ft.Container(
            padding=ft.Padding.only(left=10, right=4, top=4, bottom=4),
            border_radius=8,
            bgcolor=ft.Colors.WHITE,
            content=ft.Row(
                spacing=4,
                controls=[
                    avatar_for(
                        invitation["name"],
                        invitation["avatar_url"],
                        size=34,
                    ),
                    ft.Text(
                        invitation["name"],
                        size=12,
                        weight=ft.FontWeight.W_600,
                        expand=True,
                        max_lines=1,
                        overflow=ft.TextOverflow.ELLIPSIS,
                    ),
                    ft.IconButton(
                        icon=ft.Icons.CHECK,
                        icon_color=ft.Colors.GREEN_600,
                        tooltip="Akceptuj",
                        on_click=handle_accept,
                    ),
                    ft.IconButton(
                        icon=ft.Icons.CLOSE,
                        icon_color=ft.Colors.RED_500,
                        tooltip="Odrzuć",
                        on_click=handle_reject,
                    ),
                ],
            ),
        )

    def refresh_invitations():
        invitations_title.value = (
            f"Zaproszenia ({len(pending_invitations)})"
        )
        invitations_list.controls = [
            invitation_tile(invitation)
            for invitation in pending_invitations
        ]
        invitations_section.visible = bool(pending_invitations)

    async def open_auth_page(event=None):
        # Import wewnątrz funkcji zapobiega importowi cyklicznemu.
        from pages.auth import authPage

        await authPage(page)

    profile_section = ft.Container(
        padding=ft.Padding.all(15),
        content=ft.Row(
            controls=[
                avatar_for(
                    current_user["name"],
                    current_user["avatar_url"],
                    size=48,
                ),
                ft.Column(
                    spacing=0,
                    expand=True,
                    controls=[
                        ft.Text(
                            current_user["name"],
                            size=16,
                            weight=ft.FontWeight.BOLD,
                        ),
                        ft.Text(
                            current_user["status"],
                            size=12,
                            color=ft.Colors.GREEN_600,
                        ),
                    ],
                ),
                ft.IconButton(
                    icon=ft.Icons.LOGOUT,
                    tooltip="Wyloguj",
                    on_click=open_auth_page,
                ),
            ],
        ),
    )

    contact_search = ft.TextField(
        hint_text="Szukaj kontaktów...",
        prefix_icon=ft.Icons.SEARCH,
        dense=True,
        border_radius=12,
        on_change=filter_contacts,
    )

    invitation_name = ft.TextField(
        hint_text="Nazwa użytkownika",
        prefix_icon=ft.Icons.PERSON_SEARCH,
        dense=True,
        expand=True,
        on_submit=send_contact_invitation,
    )

    contact_status = ft.Text(
        size=11,
        visible=False,
    )

    contact_actions = ft.Container(
        padding=ft.Padding.symmetric(horizontal=10, vertical=8),
        content=ft.Column(
            spacing=7,
            controls=[
                contact_search,
                ft.Row(
                    spacing=4,
                    controls=[
                        invitation_name,
                        ft.IconButton(
                            icon=ft.Icons.PERSON_ADD,
                            icon_color=ft.Colors.BLUE_500,
                            tooltip="Wyślij zaproszenie",
                            on_click=send_contact_invitation,
                        ),
                    ],
                ),
                contact_status,
            ],
        ),
    )

    invitations_title = ft.Text(
        size=12,
        color=ft.Colors.GREY_600,
        weight=ft.FontWeight.W_600,
    )
    invitations_list = ft.Column(spacing=4)
    invitations_section = ft.Container(
        padding=ft.Padding.only(left=8, right=8, bottom=8),
        content=ft.Column(
            spacing=5,
            controls=[
                invitations_title,
                invitations_list,
            ],
        ),
    )

    contacts_list = ft.ListView(
        expand=True,
        spacing=2,
        padding=ft.Padding.symmetric(horizontal=8),
    )

    refresh_contacts()
    refresh_invitations()

    left_panel = ft.Container(
        width=300,
        bgcolor=ft.Colors.GREY_50,
        content=ft.Column(
            spacing=0,
            controls=[
                profile_section,
                ft.Divider(height=1),
                contact_actions,
                invitations_section,
                ft.Divider(height=1),
                ft.Container(
                    padding=ft.Padding.only(
                        left=15,
                        top=10,
                        bottom=5,
                    ),
                    content=ft.Text(
                        "Kontakty",
                        size=13,
                        color=ft.Colors.GREY_500,
                        weight=ft.FontWeight.W_600,
                    ),
                ),
                contacts_list,
            ],
        ),
    )

    show_empty_chat()

    right_panel = ft.Container(
        expand=True,
        content=chat_content,
    )

    page.add(
        ft.Row(
            expand=True,
            spacing=0,
            controls=[
                left_panel,
                ft.VerticalDivider(width=1),
                right_panel,
            ],
        )
    )

    page.update()
