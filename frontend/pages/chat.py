from __future__ import annotations

import asyncio
from datetime import datetime
from typing import Any, Callable

import flet as ft

from api import ApiClient, ApiError


class ChatPage:
    def __init__(
        self,
        page: ft.Page,
        api: ApiClient,
        current_user: dict[str, Any],
        on_logout: Callable,
        on_session_expired: Callable,
    ):
        self.page = page
        self.api = api
        self.current_user = current_user
        self.on_logout = on_logout
        self.on_session_expired = on_session_expired
        self.running = True
        self.selected_user: dict[str, Any] | None = None
        self.selected_image: Any | None = None
        self.last_message_id = 0

        self.search_field = ft.TextField(
            hint_text="Search by name or email…",
            prefix_icon=ft.Icons.SEARCH,
            dense=True,
            expand=True,
            on_submit=self.search_users,
        )
        self.search_results = ft.Column(spacing=3, scroll=ft.ScrollMode.AUTO)
        self.search_panel = ft.Container(
            visible=False,
            height=260,
            padding=8,
            border=ft.Border.all(1, "#E2E8F0"),
            border_radius=12,
            bgcolor=ft.Colors.WHITE,
            content=self.search_results,
        )
        self.contacts_list = ft.Column(expand=True, spacing=4, scroll=ft.ScrollMode.AUTO)
        self.contacts_empty = ft.Container(
            padding=24,
            alignment=ft.Alignment.CENTER,
            content=ft.Column(
                horizontal_alignment=ft.CrossAxisAlignment.CENTER,
                controls=[
                    ft.Icon(ft.Icons.PERSON_SEARCH_OUTLINED, size=42, color=ft.Colors.GREY_400),
                    ft.Text("No contacts", weight=ft.FontWeight.W_600),
                    ft.Text(
                        "Search for a user above and add them to your conversations.",
                        text_align=ft.TextAlign.CENTER,
                        color=ft.Colors.GREY_600,
                    ),
                ],
            ),
        )

        self.chat_header = ft.Container(
            height=76,
            padding=ft.Padding.symmetric(horizontal=22),
            border=ft.Border.only(bottom=ft.BorderSide(1, "#E2E8F0")),
            bgcolor=ft.Colors.WHITE,
        )
        self.messages_list = ft.Column(
            expand=True,
            spacing=8,
            scroll=ft.ScrollMode.AUTO,
            auto_scroll=True,
        )
        self.message_field = ft.TextField(
            hint_text="Type a message…",
            multiline=True,
            min_lines=1,
            max_lines=4,
            shift_enter=True,
            expand=True,
            on_submit=self.send_message,
        )
        self.attachment_preview = ft.Container(visible=False)
        self.send_button = ft.FilledIconButton(
            icon=ft.Icons.SEND_ROUNDED,
            tooltip="Send",
            on_click=self.send_message,
        )
        self.conversation_panel = ft.Column(
            expand=True,
            visible=False,
            spacing=0,
            controls=[
                self.chat_header,
                ft.Container(
                    expand=True,
                    bgcolor="#F8FAFC",
                    padding=ft.Padding.symmetric(horizontal=22, vertical=16),
                    content=self.messages_list,
                ),
                self.attachment_preview,
                ft.Container(
                    padding=ft.Padding.symmetric(horizontal=16, vertical=12),
                    border=ft.Border.only(top=ft.BorderSide(1, "#E2E8F0")),
                    bgcolor=ft.Colors.WHITE,
                    content=ft.Row(
                        vertical_alignment=ft.CrossAxisAlignment.END,
                        controls=[
                            ft.IconButton(
                                icon=ft.Icons.ADD_PHOTO_ALTERNATE_OUTLINED,
                                tooltip="Attach an image",
                                on_click=self.pick_message_image,
                            ),
                            self.message_field,
                            self.send_button,
                        ],
                    ),
                ),
            ],
        )
        self.empty_conversation = ft.Container(
            expand=True,
            alignment=ft.Alignment.CENTER,
            bgcolor="#F8FAFC",
            content=ft.Column(
                tight=True,
                horizontal_alignment=ft.CrossAxisAlignment.CENTER,
                controls=[
                    ft.Container(
                        width=88,
                        height=88,
                        alignment=ft.Alignment.CENTER,
                        bgcolor="#E0E7FF",
                        border_radius=26,
                        content=ft.Icon(ft.Icons.CHAT_BUBBLE_OUTLINE_ROUNDED, size=44, color="#4F46E5"),
                    ),
                    ft.Text("Select a conversation", size=24, weight=ft.FontWeight.BOLD),
                    ft.Text("Select someone from your contacts or find a new person.", color=ft.Colors.GREY_600),
                ],
            ),
        )

        self.profile_avatar_holder = ft.Container()
        self.profile_name = ft.Text(current_user["username"], weight=ft.FontWeight.BOLD, max_lines=1)
        self.root = self._build()

    def _build(self) -> ft.Control:
        self._refresh_profile_header()
        sidebar = ft.Container(
            width=350,
            bgcolor=ft.Colors.WHITE,
            border=ft.Border.only(right=ft.BorderSide(1, "#E2E8F0")),
            content=ft.Column(
                spacing=0,
                controls=[
                    ft.Container(
                        height=86,
                        padding=ft.Padding.symmetric(horizontal=16),
                        border=ft.Border.only(bottom=ft.BorderSide(1, "#E2E8F0")),
                        content=ft.Row(
                            controls=[
                                self.profile_avatar_holder,
                                ft.Column(
                                    expand=True,
                                    tight=True,
                                    spacing=2,
                                    controls=[
                                        self.profile_name,
                                        ft.Text("online", size=12, color=ft.Colors.GREEN_600),
                                    ],
                                ),
                                ft.IconButton(
                                    icon=ft.Icons.PHOTO_CAMERA_OUTLINED,
                                    tooltip="Change profile picture",
                                    on_click=self.pick_avatar,
                                ),
                                ft.IconButton(
                                    icon=ft.Icons.LOGOUT,
                                    tooltip="Sign out",
                                    on_click=self.on_logout,
                                ),
                            ],
                        ),
                    ),
                    ft.Container(
                        padding=ft.Padding.only(left=14, right=14, top=14, bottom=8),
                        content=ft.Row(
                            controls=[
                                self.search_field,
                                ft.IconButton(icon=ft.Icons.SEARCH, tooltip="Search", on_click=self.search_users),
                            ]
                        ),
                    ),
                    ft.Container(padding=ft.Padding.symmetric(horizontal=14), content=self.search_panel),
                    ft.Container(
                        padding=ft.Padding.only(left=18, right=14, top=12, bottom=8),
                        content=ft.Row(
                            controls=[
                                ft.Text("KONTAKTY", size=12, weight=ft.FontWeight.BOLD, color=ft.Colors.GREY_600),
                                ft.Container(expand=True),
                                ft.IconButton(
                                    icon=ft.Icons.REFRESH,
                                    icon_size=18,
                                    tooltip="Refresh contacts",
                                    on_click=self.refresh_contacts,
                                ),
                            ]
                        ),
                    ),
                    ft.Container(
                        expand=True,
                        padding=ft.Padding.symmetric(horizontal=8),
                        content=ft.Stack(expand=True, controls=[self.contacts_empty, self.contacts_list]),
                    ),
                ],
            ),
        )
        return ft.Row(
            expand=True,
            spacing=0,
            controls=[
                sidebar,
                ft.Stack(
                    expand=True,
                    controls=[self.empty_conversation, self.conversation_panel],
                ),
            ],
        )

    async def start(self) -> None:
        await self.refresh_contacts()
        self.page.run_task(self._poll_loop)

    def stop(self) -> None:
        self.running = False

    def _avatar(self, user: dict[str, Any], radius: int = 21, show_status: bool = True) -> ft.Control:
        name = user.get("username") or "?"
        initials = "".join(part[0] for part in name.split()[:2]).upper() or "?"
        avatar = ft.CircleAvatar(
            radius=radius,
            bgcolor="#E0E7FF",
            color="#3730A3",
            foreground_image_src=self.api.media_url(user.get("avatarUrl")),
            content=ft.Text(initials, weight=ft.FontWeight.BOLD),
        )
        if not show_status:
            return avatar
        return ft.Stack(
            width=radius * 2 + 4,
            height=radius * 2 + 4,
            controls=[
                avatar,
                ft.Container(
                    alignment=ft.Alignment.BOTTOM_RIGHT,
                    content=ft.CircleAvatar(
                        radius=6,
                        bgcolor=ft.Colors.GREEN_500 if user.get("isOnline") else ft.Colors.GREY_400,
                    ),
                ),
            ],
        )

    def _refresh_profile_header(self) -> None:
        self.profile_avatar_holder.content = self._avatar(self.current_user, radius=23, show_status=False)
        self.profile_name.value = self.current_user["username"]

    async def _api_call(self, function: Callable, *args: Any) -> Any:
        try:
            return await asyncio.to_thread(function, *args)
        except ApiError as exc:
            if exc.status_code == 401:
                self.running = False
                self.on_session_expired()
            else:
                self._snack(str(exc), error=True)
            return None

    def _snack(self, message: str, error: bool = False) -> None:
        self.page.show_dialog(
            ft.SnackBar(
                ft.Text(message, color=ft.Colors.WHITE),
                bgcolor=ft.Colors.RED_700 if error else ft.Colors.GREEN_700,
                behavior=ft.SnackBarBehavior.FLOATING,
                show_close_icon=True,
            )
        )

    async def refresh_contacts(self, _=None) -> None:
        contacts = await self._api_call(self.api.contacts)
        if contacts is None or not self.running:
            return

        self.contacts_list.controls = [self._contact_tile(user) for user in contacts]
        self.contacts_list.visible = bool(contacts)
        self.contacts_empty.visible = not contacts

        if self.selected_user:
            refreshed = next((u for u in contacts if u["id"] == self.selected_user["id"]), None)
            if refreshed:
                self.selected_user = refreshed
                self._update_chat_header()
        self.page.update()

    def _contact_tile(self, user: dict[str, Any]) -> ft.Control:
        status = "online" if user.get("isOnline") else self._last_seen(user.get("lastSeenAt"))
        return ft.ListTile(
            leading=self._avatar(user),
            title=ft.Text(user["username"], weight=ft.FontWeight.W_600, max_lines=1),
            subtitle=ft.Text(
                status,
                size=12,
                color=ft.Colors.GREEN_600 if user.get("isOnline") else ft.Colors.GREY_500,
                max_lines=1,
            ),
            trailing=ft.IconButton(
                icon=ft.Icons.PERSON_REMOVE_OUTLINED,
                icon_size=19,
                tooltip="Remove contact",
                on_click=self._user_handler(self.remove_contact, user),
            ),
            selected=bool(self.selected_user and self.selected_user["id"] == user["id"]),
            selected_tile_color="#EEF2FF",
            shape=ft.RoundedRectangleBorder(radius=12),
            on_click=self._user_handler(self.select_contact, user),
        )

    async def search_users(self, _=None) -> None:
        query = (self.search_field.value or "").strip()
        if len(query) < 2:
            self.search_results.controls = [
                ft.Text("Enter at least 2 characters.", size=12, color=ft.Colors.GREY_600)
            ]
            self.search_panel.visible = True
            self.page.update()
            return

        users = await self._api_call(self.api.search_users, query)
        if users is None:
            return
        self.search_results.controls = [self._search_result(user) for user in users]
        if not users:
            self.search_results.controls = [
                ft.Text("No users found.", size=12, color=ft.Colors.GREY_600)
            ]
        self.search_panel.visible = True
        self.page.update()

    def _search_result(self, user: dict[str, Any]) -> ft.Control:
        action = (
            ft.Icon(ft.Icons.CHECK_CIRCLE, color=ft.Colors.GREEN_600)
            if user.get("isContact")
            else ft.IconButton(
                icon=ft.Icons.PERSON_ADD_ALT_1,
                tooltip="Add contact",
                on_click=self._user_handler(self.add_contact, user),
            )
        )
        return ft.ListTile(
            dense=True,
            leading=self._avatar(user, radius=17),
            title=ft.Text(user["username"], max_lines=1),
            subtitle=ft.Text(user["email"], size=11, max_lines=1),
            trailing=action,
        )

    @staticmethod
    def _user_handler(action: Callable, user: dict[str, Any]) -> Callable:
        async def handler(_) -> None:
            await action(user)

        return handler

    async def add_contact(self, user: dict[str, Any]) -> None:
        added = await self._api_call(self.api.add_contact, user["id"])
        if added is None:
            return
        self.search_field.value = ""
        self.search_panel.visible = False
        await self.refresh_contacts()
        await self.select_contact(added)
        self._snack(f"Contact added: {user['username']}")

    async def remove_contact(self, user: dict[str, Any]) -> None:
        result = await self._api_call(self.api.remove_contact, user["id"])
        if result is None or not self.running:
            return
        if self.selected_user and self.selected_user["id"] == user["id"]:
            self.selected_user = None
            self.conversation_panel.visible = False
            self.empty_conversation.visible = True
        await self.refresh_contacts()
        self._snack(f"Contact removed: {user['username']}")

    async def select_contact(self, user: dict[str, Any]) -> None:
        self.selected_user = user
        self.last_message_id = 0
        self.selected_image = None
        self.message_field.value = ""
        self.messages_list.controls.clear()
        self.attachment_preview.visible = False
        self.empty_conversation.visible = False
        self.conversation_panel.visible = True
        self._update_chat_header()
        self.page.update()
        await self._load_messages(initial=True, expected_user_id=user["id"])
        await self.refresh_contacts()

    def _update_chat_header(self) -> None:
        if not self.selected_user:
            return
        status = (
            "online"
            if self.selected_user.get("isOnline")
            else self._last_seen(self.selected_user.get("lastSeenAt"))
        )
        self.chat_header.content = ft.Row(
            controls=[
                self._avatar(self.selected_user, radius=23),
                ft.Column(
                    tight=True,
                    spacing=2,
                    controls=[
                        ft.Text(self.selected_user["username"], size=17, weight=ft.FontWeight.BOLD),
                        ft.Text(
                            status,
                            size=12,
                            color=(
                                ft.Colors.GREEN_600
                                if self.selected_user.get("isOnline")
                                else ft.Colors.GREY_500
                            ),
                        ),
                    ],
                ),
            ]
        )

    async def _load_messages(self, initial: bool, expected_user_id: int | None = None) -> None:
        if not self.selected_user:
            return
        user_id = expected_user_id or self.selected_user["id"]
        after_id = 0 if initial else self.last_message_id
        messages = await self._api_call(self.api.conversation, user_id, after_id)
        if messages is None or not self.selected_user or self.selected_user["id"] != user_id:
            return

        if initial:
            self.messages_list.controls.clear()
        elif messages and self.last_message_id == 0:
            self.messages_list.controls.clear()
        for message in messages:
            self.messages_list.controls.append(self._message_bubble(message))
            self.last_message_id = max(self.last_message_id, message["id"])
        if initial and not messages:
            self.messages_list.controls.append(
                ft.Container(
                    alignment=ft.Alignment.CENTER,
                    padding=24,
                    content=ft.Text(
                        "This is the beginning of your conversation. Send the first message!",
                        color=ft.Colors.GREY_500,
                    ),
                )
            )
        self.page.update()

    def _message_bubble(self, message: dict[str, Any]) -> ft.Control:
        mine = message["senderId"] == self.current_user["id"]
        content: list[ft.Control] = []
        if message.get("imageUrl"):
            content.append(
                ft.Container(
                    border_radius=12,
                    clip_behavior=ft.ClipBehavior.ANTI_ALIAS,
                    content=ft.Image(
                        src=self.api.media_url(message["imageUrl"]),
                        width=320,
                        height=240,
                        fit=ft.BoxFit.COVER,
                    ),
                )
            )
        if message.get("text"):
            content.append(ft.Text(message["text"], selectable=True))

        meta_controls: list[ft.Control] = [
            ft.Text(self._message_time(message.get("sentAt")), size=10, color=ft.Colors.GREY_500)
        ]
        if mine:
            meta_controls.append(
                ft.Icon(
                    ft.Icons.DONE_ALL if message.get("readAt") else ft.Icons.CHECK,
                    size=14,
                    color=ft.Colors.BLUE_500 if message.get("readAt") else ft.Colors.GREY_500,
                )
            )
        content.append(ft.Row(alignment=ft.MainAxisAlignment.END, spacing=3, controls=meta_controls))
        return ft.Row(
            alignment=ft.MainAxisAlignment.END if mine else ft.MainAxisAlignment.START,
            controls=[
                ft.Container(
                    width=370 if message.get("imageUrl") else None,
                    padding=10,
                    border_radius=ft.BorderRadius.only(
                        top_left=16,
                        top_right=16,
                        bottom_left=16 if mine else 4,
                        bottom_right=4 if mine else 16,
                    ),
                    bgcolor="#E0E7FF" if mine else ft.Colors.WHITE,
                    border=None if mine else ft.Border.all(1, "#E2E8F0"),
                    content=ft.Column(tight=True, spacing=7, controls=content),
                )
            ],
        )

    async def pick_message_image(self, _=None) -> None:
        files = await ft.FilePicker().pick_files(
            allow_multiple=False,
            with_data=True,
            file_type=ft.FilePickerFileType.CUSTOM,
            allowed_extensions=["jpg", "jpeg", "png", "webp", "gif"],
        )
        if not files:
            return
        if files[0].size and files[0].size > 8 * 1024 * 1024:
            self._snack("The image can be up to 8 MB.", error=True)
            return
        self.selected_image = files[0]
        self.attachment_preview.visible = True
        self.attachment_preview.padding = ft.Padding.only(left=18, right=18, top=8)
        self.attachment_preview.content = ft.Container(
            padding=10,
            bgcolor="#EEF2FF",
            border_radius=10,
            content=ft.Row(
                controls=[
                    ft.Icon(ft.Icons.IMAGE_OUTLINED, color="#4F46E5"),
                    ft.Text(self.selected_image.name, expand=True, max_lines=1),
                    ft.IconButton(
                        icon=ft.Icons.CLOSE,
                        icon_size=18,
                        tooltip="Remove attachment",
                        on_click=self.clear_attachment,
                    ),
                ]
            ),
        )
        self.page.update()

    def clear_attachment(self, _=None) -> None:
        self.selected_image = None
        self.attachment_preview.visible = False
        self.page.update()

    async def send_message(self, _=None) -> None:
        if not self.selected_user:
            return
        text = (self.message_field.value or "").strip()
        if not text and self.selected_image is None:
            return
        self.send_button.disabled = True
        self.page.update()
        recipient_id = self.selected_user["id"]
        message = await self._api_call(
            self.api.send_message, recipient_id, text, self.selected_image
        )
        self.send_button.disabled = False
        if message is None:
            self.page.update()
            return
        if self.messages_list.controls and self.last_message_id == 0:
            self.messages_list.controls.clear()
        self.messages_list.controls.append(self._message_bubble(message))
        self.last_message_id = max(self.last_message_id, message["id"])
        self.message_field.value = ""
        self.clear_attachment()
        self.page.update()

    async def pick_avatar(self, _=None) -> None:
        files = await ft.FilePicker().pick_files(
            allow_multiple=False,
            with_data=True,
            file_type=ft.FilePickerFileType.CUSTOM,
            allowed_extensions=["jpg", "jpeg", "png", "webp", "gif"],
        )
        if not files:
            return
        if files[0].size and files[0].size > 8 * 1024 * 1024:
            self._snack("The image can be up to 8 MB.", error=True)
            return
        user = await self._api_call(self.api.upload_avatar, files[0])
        if user is None:
            return
        self.current_user.update(user)
        self._refresh_profile_header()
        self.page.update()
        self._snack("Your profile picture has been updated.")

    async def _poll_loop(self) -> None:
        ticks = 0
        while self.running:
            await asyncio.sleep(3)
            if not self.running:
                break
            if self.selected_user:
                await self._load_messages(initial=False)
            ticks += 1
            if ticks % 5 == 0:
                await self._api_call(self.api.heartbeat)
                await self.refresh_contacts()

    @staticmethod
    def _parse_date(value: str | None) -> datetime | None:
        if not value:
            return None
        try:
            return datetime.fromisoformat(value.replace("Z", "+00:00")).astimezone()
        except ValueError:
            return None

    def _last_seen(self, value: str | None) -> str:
        parsed = self._parse_date(value)
        if not parsed:
            return "offline"
        now = datetime.now().astimezone()
        if parsed.date() == now.date():
            return f"offline · today at {parsed:%H:%M}"
        return f"offline · {parsed:%b %d, %Y at %H:%M}"

    def _message_time(self, value: str | None) -> str:
        parsed = self._parse_date(value)
        return parsed.strftime("%H:%M") if parsed else ""
