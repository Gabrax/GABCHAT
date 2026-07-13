import flet as ft


def authPage(page: ft.Page):
    page.title = "GABCHAT"
    page.window_width = 420
    page.window_height = 620
    page.horizontal_alignment = ft.CrossAxisAlignment.CENTER
    page.vertical_alignment = ft.MainAxisAlignment.CENTER
    page.theme_mode = ft.ThemeMode.LIGHT

    # Login controls
    login_email = ft.TextField(
        label="Email",
        prefix_icon=ft.Icons.EMAIL,
        width=320,
    )

    login_password = ft.TextField(
        label="Password",
        password=True,
        can_reveal_password=True,
        prefix_icon=ft.Icons.LOCK,
        width=320,
    )

    # Register controls
    register_name = ft.TextField(
        label="Full Name",
        prefix_icon=ft.Icons.PERSON,
        width=320,
    )

    register_email = ft.TextField(
        label="Email",
        prefix_icon=ft.Icons.EMAIL,
        width=320,
    )

    register_password = ft.TextField(
        label="Password",
        password=True,
        can_reveal_password=True,
        prefix_icon=ft.Icons.LOCK,
        width=320,
    )

    register_confirm = ft.TextField(
        label="Confirm Password",
        password=True,
        can_reveal_password=True,
        prefix_icon=ft.Icons.LOCK_OUTLINE,
        width=320,
    )

    status = ft.Text(color=ft.Colors.BLUE)

    def login_clicked(e):
        if not login_email.value or not login_password.value:
            status.value = "Please fill in all login fields."
        else:
            status.value = f"Welcome back, {login_email.value}!"
        page.update()

    def register_clicked(e):
        if (
            not register_name.value
            or not register_email.value
            or not register_password.value
            or not register_confirm.value
        ):
            status.value = "Please fill in all registration fields."
        elif register_password.value != register_confirm.value:
            status.value = "Passwords do not match."
        else:
            status.value = "Account created successfully!"
        page.update()

    login_tab = ft.Container(
        padding=20,
        content=ft.Column(
            horizontal_alignment=ft.CrossAxisAlignment.CENTER,
            controls=[
                ft.Icon(ft.Icons.ACCOUNT_CIRCLE, size=90),
                ft.Text("Login", size=28, weight=ft.FontWeight.BOLD),
                login_email,
                login_password,
                ft.FilledButton(
                    "Login",
                    width=320,
                    height=45,
                    on_click=login_clicked,
                ),
                ft.TextButton("Forgot password?")
            ],
        ),
    )

    register_tab = ft.Container(
        padding=20,
        content=ft.Column(
            horizontal_alignment=ft.CrossAxisAlignment.CENTER,
            controls=[
                ft.Icon(ft.Icons.PERSON_ADD, size=90),
                ft.Text("Create Account", size=28, weight=ft.FontWeight.BOLD),
                register_name,
                register_email,
                register_password,
                register_confirm,
                ft.FilledButton(
                    "Register",
                    width=320,
                    height=45,
                    on_click=register_clicked,
                ),
            ],
        ),
    )

    tabs = ft.Tabs(
        length=2,
        expand=True,
        content=ft.Column(
            controls=[
                ft.TabBar(
                    tabs=[
                        ft.Tab(label="Login"),
                        ft.Tab(label="Register"),
                    ],
                ),
                ft.TabBarView(
                    expand=True,
                    controls=[
                        login_tab,
                        register_tab,
                    ],
                ),
            ],
        ),
    )

    return ft.Card(
        elevation=10,
        content=ft.Container(
            width=360,
            height=550,
            padding=20,
            content=ft.Column(
                controls=[
                    tabs,
                    ft.Divider(),
                    status,
                ],
            ),
        ),
    )
