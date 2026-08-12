A complete local chat application with a Flet frontend, an ASP.NET Core API,
and a MySQL database.

## Features

- registration, sign-in, and securely hashed passwords,
- user search by username or email address,
- adding and removing contacts,
- online/offline status and last activity time,
- profile pictures,
- text messages and images in conversations,
- message read receipts,
- automatic conversation and presence updates.

## Requirements

- Docker Desktop,
- .NET SDK 10,
- Python 3.11 or newer.

## Running the application

Start the database from the project directory:

```powershell
docker compose -f backend/database/docker-compose.yml up -d
```

Then start the API (available at `http://localhost:5046` by default):

```powershell
dotnet run --project backend/backend.csproj --launch-profile http
```

Prepare and start the frontend in a second terminal:

```powershell
python -m venv frontend/.venv
frontend/.venv/Scripts/Activate.ps1
pip install -r frontend/requirements.txt
python frontend/frontend.py
```

The frontend opens in a web browser by default. This avoids downloading an
additional Flet client on first launch.

To use an optional native window instead, run:

```powershell
$env:GABCHAT_DESKTOP = "1"
python frontend/frontend.py
```

Flet must download its desktop client the first time this mode is used.

When changing the Python version, recreate `.venv` from scratch. Do not run
`python -m venv` over an environment created with another Python version,
because incompatible binary modules may remain in it.

If the API is available at another address, set this variable before starting
the frontend:

```powershell
$env:GABCHAT_API_URL = "http://localhost:5046"
```

JPG, PNG, WEBP, and GIF images are stored in `backend/uploads`. Each file can
be up to 8 MB. On startup, the backend automatically adds any missing tables
and columns to an existing database.
