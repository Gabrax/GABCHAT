import flet as ft

from pages.auth import authPage

async def main(page: ft.Page):
    await authPage(page)

if __name__ == "__main__":
    ft.run(main)
