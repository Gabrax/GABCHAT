import httpx
import flet as ft

from pages.chat import chatPage


API_BASE_URL = "http://localhost:5046/api"


async def authPage(page: ft.Page):
    page.clean()

    page.title = "GABCHAT — logowanie i rejestracja"
    page.padding = 0
    page.theme_mode = ft.ThemeMode.LIGHT

    page.window.width = 460
    page.window.height = 720
    page.window.alignment = ft.Alignment.CENTER
    page.update()
    await page.window.center()

    login_status = ft.Text(visible=False)
    register_status = ft.Text(visible=False)

    def show_status(control: ft.Text, message: str, is_error: bool = True):
        control.value = message
        control.color = (
            ft.Colors.RED_600
            if is_error
            else ft.Colors.GREEN_600
        )
        control.visible = True

    login_email = ft.TextField(
        label="Email",
        hint_text="Wprowadź adres email",
        width=340,
        prefix_icon=ft.Icons.EMAIL_OUTLINED,
        keyboard_type=ft.KeyboardType.EMAIL,
        autofocus=True,
    )

    async def focus_login_password(event=None):
        await login_password.focus()

    async def login(event=None):
        email = (login_email.value or "").strip()
        password = (login_password.value or "").strip()

        if not email or not password:
            show_status(login_status, "Podaj email i hasło.")
            page.update()

            if not email:
                await login_email.focus()
            else:
                await login_password.focus()
            return

        login_status.visible = False
        page.update()

        # Logowanie zostanie podłączone do endpointu /api/auth/login.
        await chatPage(page)

    login_email.on_submit = focus_login_password

    login_password = ft.TextField(
        label="Hasło",
        hint_text="Wprowadź hasło",
        width=340,
        password=True,
        can_reveal_password=True,
        prefix_icon=ft.Icons.LOCK_OUTLINE,
        on_submit=login,
    )

    register_name = ft.TextField(
        label="Nazwa użytkownika",
        hint_text="Wprowadź nazwę użytkownika",
        width=340,
        prefix_icon=ft.Icons.PERSON_OUTLINE,
    )

    register_email = ft.TextField(
        label="Email",
        hint_text="Wprowadź adres email",
        width=340,
        prefix_icon=ft.Icons.EMAIL_OUTLINED,
        keyboard_type=ft.KeyboardType.EMAIL,
    )

    register_password = ft.TextField(
        label="Hasło",
        hint_text="Minimum 8 znaków",
        width=340,
        password=True,
        can_reveal_password=True,
        prefix_icon=ft.Icons.LOCK_OUTLINE,
    )

    register_password_confirm = ft.TextField(
        label="Powtórz hasło",
        hint_text="Wprowadź hasło ponownie",
        width=340,
        password=True,
        can_reveal_password=True,
        prefix_icon=ft.Icons.LOCK_RESET,
    )

    async def register(event=None):
        username = (register_name.value or "").strip()
        email = (register_email.value or "").strip()
        password = register_password.value or ""
        password_confirm = register_password_confirm.value or ""

        if not all((username, email, password, password_confirm)):
            show_status(register_status, "Uzupełnij wszystkie pola.")
        elif "@" not in email or "." not in email.rsplit("@", 1)[-1]:
            show_status(register_status, "Podaj poprawny adres email.")
        elif len(password) < 8:
            show_status(register_status, "Hasło musi mieć co najmniej 8 znaków.")
        elif password != password_confirm:
            show_status(register_status, "Podane hasła nie są takie same.")
        else:
            register_status.visible = False
            register_button.disabled = True
            register_button.content = "Rejestrowanie..."
            page.update()

            try:
                async with httpx.AsyncClient(
                    timeout=8.0,
                    follow_redirects=True,
                    verify=False,
                ) as client:
                    response = await client.post(
                        f"{API_BASE_URL}/auth/register",
                        json={
                            "username": username,
                            "email": email,
                            "password": password,
                        },
                    )

                if response.is_success:
                    login_email.value = email
                    login_password.value = ""
                    register_password.value = ""
                    register_password_confirm.value = ""
                    show_status(
                        login_status,
                        "Konto zostało utworzone. Możesz się zalogować.",
                        is_error=False,
                    )
                    auth_tabs.selected_index = 0
                    page.update()
                    await login_password.focus()
                    return

                if response.status_code == 400:
                    show_status(
                        register_status,
                        "Użytkownik o tej nazwie lub adresie email już istnieje.",
                    )
                else:
                    show_status(
                        register_status,
                        "Nie udało się utworzyć konta. Spróbuj ponownie.",
                    )
            except httpx.RequestError:
                show_status(
                    register_status,
                    "Brak połączenia z serwerem. Uruchom backend i spróbuj ponownie.",
                )
            finally:
                register_button.disabled = False
                register_button.content = "Utwórz konto"
                page.update()

        page.update()

    register_password_confirm.on_submit = register

    register_button = ft.Button(
        content="Utwórz konto",
        icon=ft.Icons.PERSON_ADD,
        width=340,
        on_click=register,
    )

    login_view = ft.Container(
        padding=ft.Padding.only(top=18),
        content=ft.Column(
            spacing=14,
            horizontal_alignment=ft.CrossAxisAlignment.CENTER,
            controls=[
                ft.Text(
                    "Zaloguj się do swojego konta",
                    color=ft.Colors.GREY_600,
                ),
                login_email,
                login_password,
                login_status,
                ft.Button(
                    content="Zaloguj",
                    icon=ft.Icons.LOGIN,
                    width=340,
                    on_click=login,
                ),
            ],
        ),
    )

    register_view = ft.Container(
        padding=ft.Padding.only(top=12),
        content=ft.Column(
            spacing=10,
            horizontal_alignment=ft.CrossAxisAlignment.CENTER,
            scroll=ft.ScrollMode.AUTO,
            controls=[
                ft.Text(
                    "Utwórz nowe konto",
                    color=ft.Colors.GREY_600,
                ),
                register_name,
                register_email,
                register_password,
                register_password_confirm,
                register_status,
                register_button,
            ],
        ),
    )

    auth_tabs = ft.Tabs(
        length=2,
        expand=True,
        content=ft.Column(
            expand=True,
            spacing=0,
            controls=[
                ft.TabBar(
                    scrollable=False,
                    tabs=[
                        ft.Tab(label="Logowanie", icon=ft.Icons.LOGIN),
                        ft.Tab(label="Rejestracja", icon=ft.Icons.PERSON_ADD),
                    ],
                ),
                ft.TabBarView(
                    expand=True,
                    controls=[login_view, register_view],
                ),
            ],
        ),
    )

    page.add(
        ft.Container(
            expand=True,
            padding=ft.Padding.only(left=40, right=40, top=24, bottom=20),
            content=ft.Column(
                spacing=10,
                horizontal_alignment=ft.CrossAxisAlignment.CENTER,
                controls=[
                    ft.Icon(
                        ft.Icons.CHAT,
                        size=58,
                        color=ft.Colors.BLUE_400,
                    ),
                    ft.Text(
                        "GABCHAT",
                        size=30,
                        weight=ft.FontWeight.BOLD,
                    ),
                    auth_tabs,
                ],
            ),
        )
    )

    page.update()
