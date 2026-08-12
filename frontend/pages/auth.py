from __future__ import annotations

from collections.abc import Callable

import flet as ft


def auth_page(
    page: ft.Page,
    on_login: Callable,
    on_register: Callable,
) -> ft.Control:
    login_email = ft.TextField(
        label="E-mail",
        prefix_icon=ft.Icons.MAIL_OUTLINE,
        keyboard_type=ft.KeyboardType.EMAIL,
        autofocus=True,
    )
    login_password = ft.TextField(
        label="Password",
        password=True,
        can_reveal_password=True,
        prefix_icon=ft.Icons.LOCK_OUTLINE,
    )

    register_name = ft.TextField(label="Username", prefix_icon=ft.Icons.PERSON_OUTLINE)
    register_email = ft.TextField(
        label="E-mail", prefix_icon=ft.Icons.MAIL_OUTLINE, keyboard_type=ft.KeyboardType.EMAIL
    )
    register_password = ft.TextField(
        label="Password (at least 6 characters)",
        password=True,
        can_reveal_password=True,
        prefix_icon=ft.Icons.LOCK_OUTLINE,
    )
    register_confirm = ft.TextField(
        label="Confirm password",
        password=True,
        can_reveal_password=True,
        prefix_icon=ft.Icons.LOCK_RESET,
    )

    status = ft.Text(color=ft.Colors.RED_600, text_align=ft.TextAlign.CENTER)

    async def login_clicked(_):
        await on_login(login_email, login_password, login_button, status)

    async def register_clicked(_):
        await on_register(
            register_name,
            register_email,
            register_password,
            register_confirm,
            register_button,
            status,
        )

    login_password.on_submit = login_clicked
    login_button = ft.Button(
        content="Sign in",
        icon=ft.Icons.LOGIN,
        height=48,
        on_click=login_clicked,
    )
    register_button = ft.Button(
        content="Create account",
        icon=ft.Icons.PERSON_ADD_OUTLINED,
        height=48,
        on_click=register_clicked,
    )

    login_tab = ft.Container(
        padding=ft.Padding.only(left=28, right=28, top=24),
        content=ft.Column(
            spacing=14,
            controls=[
                ft.Text("Good to see you", size=24, weight=ft.FontWeight.BOLD),
                ft.Text("Sign in to return to your conversations.", color=ft.Colors.GREY_600),
                login_email,
                login_password,
                login_button,
            ],
        ),
    )

    register_tab = ft.Container(
        padding=ft.Padding.only(left=28, right=28, top=24),
        content=ft.Column(
            spacing=12,
            controls=[
                ft.Text("Create an account", size=24, weight=ft.FontWeight.BOLD),
                register_name,
                register_email,
                register_password,
                register_confirm,
                register_button,
            ],
        ),
    )

    tabs = ft.Tabs(
        length=2,
        expand=True,
        content=ft.Column(
            expand=True,
            controls=[
                ft.TabBar(tabs=[ft.Tab(label="Sign in"), ft.Tab(label="Register")]),
                ft.TabBarView(expand=True, controls=[login_tab, register_tab]),
            ],
        ),
    )

    return ft.Container(
        expand=True,
        alignment=ft.Alignment.CENTER,
        gradient=ft.LinearGradient(
            begin=ft.Alignment.TOP_LEFT,
            end=ft.Alignment.BOTTOM_RIGHT,
            colors=["#EEF2FF", "#F8FAFC", "#ECFEFF"],
        ),
        content=ft.Card(
            elevation=16,
            content=ft.Container(
                width=440,
                height=650,
                padding=ft.Padding.only(top=28, bottom=16),
                content=ft.Column(
                    horizontal_alignment=ft.CrossAxisAlignment.CENTER,
                    controls=[
                        ft.Container(
                            width=64,
                            height=64,
                            bgcolor="#4F46E5",
                            border_radius=18,
                            alignment=ft.Alignment.CENTER,
                            content=ft.Icon(ft.Icons.FORUM_ROUNDED, color=ft.Colors.WHITE, size=34),
                        ),
                        ft.Text("GABCHAT", size=28, weight=ft.FontWeight.BOLD, color="#1E1B4B"),
                        ft.Text("Chat. Share. Stay close.", color=ft.Colors.GREY_600),
                        ft.Container(height=8),
                        ft.Container(expand=True, content=tabs),
                        ft.Container(padding=ft.Padding.symmetric(horizontal=24), content=status),
                    ],
                ),
            ),
        ),
    )


# Compatibility with the original name used in the project.
authPage = auth_page
