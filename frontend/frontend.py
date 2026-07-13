import flet as ft

from pages.auth import authPage


def main(page: ft.Page):

    page.add(
        authPage(page)
    )


ft.run(main)
