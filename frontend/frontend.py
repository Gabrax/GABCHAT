from __future__ import annotations

import asyncio
import os

import flet as ft

from api import ApiClient, ApiError
from pages.auth import auth_page
from pages.chat import ChatPage


class GabChatApp:
    def __init__(self, page: ft.Page):
        self.page = page
        self.api = ApiClient()
        self.chat: ChatPage | None = None
        self._configure_page()

    def _configure_page(self) -> None:
        self.page.title = "GABCHAT"
        self.page.padding = 0
        self.page.spacing = 0
        self.page.bgcolor = "#F8FAFC"
        self.page.theme_mode = ft.ThemeMode.LIGHT
        self.page.theme = ft.Theme(
            color_scheme_seed="#4F46E5",
            font_family="Segoe UI",
            visual_density=ft.VisualDensity.COMFORTABLE,
        )
        self.page.window.width = 1180
        self.page.window.height = 780
        self.page.window.min_width = 820
        self.page.window.min_height = 620

    def show_auth(self, message: str | None = None) -> None:
        if self.chat:
            self.chat.stop()
            self.chat = None
        self.api.token = None
        self.page.clean()
        view = auth_page(self.page, self.login, self.register)
        self.page.add(view)
        if message:
            self.page.show_dialog(
                ft.SnackBar(
                    ft.Text(message, color=ft.Colors.WHITE),
                    bgcolor=ft.Colors.RED_700,
                    behavior=ft.SnackBarBehavior.FLOATING,
                )
            )
        self.page.update()

    async def login(self, email, password, button, status) -> None:
        status.value = ""
        if not (email.value or "").strip() or not password.value:
            status.value = "Enter your email address and password."
            self.page.update()
            return

        button.disabled = True
        self.page.update()
        try:
            payload = await asyncio.to_thread(
                self.api.login, (email.value or "").strip(), password.value
            )
        except ApiError as exc:
            status.value = str(exc)
            button.disabled = False
            self.page.update()
            return
        await self.show_chat(payload["user"])

    async def register(self, name, email, password, confirm, button, status) -> None:
        status.value = ""
        if not all([(name.value or "").strip(), (email.value or "").strip(), password.value, confirm.value]):
            status.value = "Complete all fields."
            self.page.update()
            return
        if password.value != confirm.value:
            status.value = "Passwords do not match."
            self.page.update()
            return

        button.disabled = True
        self.page.update()
        try:
            payload = await asyncio.to_thread(
                self.api.register,
                (name.value or "").strip(),
                (email.value or "").strip(),
                password.value,
            )
        except ApiError as exc:
            status.value = str(exc)
            button.disabled = False
            self.page.update()
            return
        await self.show_chat(payload["user"])

    async def show_chat(self, user: dict) -> None:
        self.page.clean()
        self.chat = ChatPage(
            self.page,
            self.api,
            user,
            self.logout,
            lambda: self.show_auth("Your session has expired. Sign in again."),
        )
        self.page.add(self.chat.root)
        self.page.update()
        await self.chat.start()

    async def logout(self, _=None) -> None:
        if self.chat:
            self.chat.stop()
        try:
            await asyncio.to_thread(self.api.logout)
        except ApiError:
            self.api.token = None
        self.show_auth()


def main(page: ft.Page):
    app = GabChatApp(page)
    app.show_auth()


if __name__ == "__main__":
    # Web mode does not require downloading the native Flet client on first
    # launch. Set GABCHAT_DESKTOP=1 to use a native window instead (Flet will
    # then download its desktop client).
    desktop_mode = os.getenv("GABCHAT_DESKTOP", "").lower() in {"1", "true", "yes"}
    view = ft.AppView.FLET_APP if desktop_mode else ft.AppView.WEB_BROWSER
    ft.run(main, view=view)
