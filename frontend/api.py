from __future__ import annotations

import mimetypes
import os
from typing import Any
from urllib.parse import quote

import requests


class ApiError(Exception):
    def __init__(self, message: str, status_code: int = 0):
        super().__init__(message)
        self.status_code = status_code


class ApiClient:
    def __init__(self, base_url: str | None = None):
        self.base_url = (base_url or os.getenv("GABCHAT_API_URL") or "http://localhost:5046").rstrip("/")
        self.token: str | None = None
        self.session = requests.Session()

    def _request(self, method: str, path: str, **kwargs: Any) -> Any:
        headers = dict(kwargs.pop("headers", {}))
        if self.token:
            headers["Authorization"] = f"Bearer {self.token}"

        try:
            response = self.session.request(
                method,
                f"{self.base_url}{path}",
                headers=headers,
                timeout=20,
                **kwargs,
            )
        except requests.RequestException as exc:
            raise ApiError(
                "Could not connect to the server. Check that the backend is running on port 5046."
            ) from exc

        if not response.ok:
            message = f"Server error ({response.status_code})."
            try:
                payload = response.json()
                if isinstance(payload, dict) and payload.get("message"):
                    message = str(payload["message"])
                elif isinstance(payload, str):
                    message = payload
            except ValueError:
                if response.text.strip():
                    message = response.text.strip()
            raise ApiError(message, response.status_code)

        if response.status_code == 204 or not response.content:
            return None
        return response.json()

    def register(self, username: str, email: str, password: str) -> dict[str, Any]:
        payload = self._request(
            "POST",
            "/api/auth/register",
            json={"username": username, "email": email, "password": password},
        )
        self.token = payload["token"]
        return payload

    def login(self, email: str, password: str) -> dict[str, Any]:
        payload = self._request(
            "POST", "/api/auth/login", json={"email": email, "password": password}
        )
        self.token = payload["token"]
        return payload

    def logout(self) -> None:
        try:
            self._request("POST", "/api/auth/logout")
        finally:
            self.token = None

    def contacts(self) -> list[dict[str, Any]]:
        return self._request("GET", "/api/users/contacts")

    def search_users(self, query: str) -> list[dict[str, Any]]:
        return self._request("GET", f"/api/users/search?query={quote(query)}")

    def add_contact(self, user_id: int) -> dict[str, Any]:
        return self._request("POST", f"/api/users/contacts/{user_id}")

    def remove_contact(self, user_id: int) -> bool:
        self._request("DELETE", f"/api/users/contacts/{user_id}")
        return True

    def conversation(self, user_id: int, after_id: int = 0) -> list[dict[str, Any]]:
        return self._request("GET", f"/api/messages/{user_id}?afterId={after_id}")

    def send_message(
        self, user_id: int, text: str, selected_file: Any | None = None
    ) -> dict[str, Any]:
        # Putting text in `files` with a filename of None forces
        # multipart/form-data even for messages without an image. The ASP.NET
        # endpoint accepts this form format.
        files: dict[str, tuple] = {"text": (None, text)}
        if selected_file is not None:
            content = selected_file.bytes
            if content is None and selected_file.path:
                with open(selected_file.path, "rb") as source:
                    content = source.read()
            mime_type = mimetypes.guess_type(selected_file.name)[0] or "application/octet-stream"
            files["image"] = (selected_file.name, content, mime_type)
        return self._request("POST", f"/api/messages/{user_id}", files=files)

    def upload_avatar(self, selected_file: Any) -> dict[str, Any]:
        content = selected_file.bytes
        if content is None and selected_file.path:
            with open(selected_file.path, "rb") as source:
                content = source.read()
        mime_type = mimetypes.guess_type(selected_file.name)[0] or "application/octet-stream"
        return self._request(
            "POST",
            "/api/users/me/avatar",
            files={"image": (selected_file.name, content, mime_type)},
        )

    def heartbeat(self) -> bool:
        self._request("POST", "/api/presence/heartbeat")
        return True

    def media_url(self, path: str | None) -> str | None:
        return f"{self.base_url}{path}" if path else None
