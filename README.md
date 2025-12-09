# 🎵 SpotifyConnector (C# / .NET 8)
*A minimal, educational Spotify Web API client demonstrating OAuth 2.0 Authorization Code Flow, structured service design, and authenticated API calls.*

---

## 🚀 Overview

This project is a clean, focused implementation of:

- OAuth 2.0 Authorization Code Flow  
- Token exchange + secure app-side secret handling  
- Typed `HttpClient` integration  
- Options binding and dependency injection  
- Token persistence (via pluggable store interface)  
- Authenticated calls to the Spotify Web API  

The goal is to demonstrate **how to correctly authenticate with Spotify** and **make authorized API requests** using modern .NET 8 patterns.

This project forms the **C# half** of a two-language comparison.  
A Go version (`SpotifyConnector_Go`) will follow, showcasing the same workflow in a different ecosystem.

---

## 🧱 Technologies & Concepts

| Feature | Description |
|--------|-------------|
| **.NET 8 Web API (Minimal APIs)** | Lightweight, modern API style |
| **OAuth 2.0 Authorization Code Flow** | Full login → redirect → token exchange |
| **Typed HttpClient via DI** | Clean HTTP service architecture |
| **Options pattern** | Binds Spotify settings securely |
| **User Secrets** | Keeps client ID/secret out of source control |
| **Custom Token Store** | In-memory `ISpotifyTokenStore` abstraction |
| **JSON model binding** | `System.Text.Json` with `JsonPropertyName` |

---

## 📂 Project Structure

```
SpotifyConnector_CSharp/
│
├── src/
│   ├── SpotifyConnector.Api/           # HTTP endpoints (/auth/login, /auth/callback, /me)
│   └── SpotifyConnector.Spotify/       # OAuth service + models + token store
│
├── tests/
│   └── SpotifyConnector.Tests/         # Future test expansion point
│
└── Directory.Packages.props            # Centralized NuGet package versioning
```

---

## 🔐 Authentication Flow (High-Level)

1. **User hits** `/auth/login`  
   → Redirects to Spotify’s consent page.

2. **Spotify redirects back** to `/auth/callback`  
   → Includes an authorization `code`.

3. **Server exchanges the code** with Spotify  
   → Retrieves `access_token`, `refresh_token`, scopes, expiration.

4. **Token stored** in `ISpotifyTokenStore` (in-memory for now)

5. **Authenticated calls** (e.g., `/me`) use `Bearer {access_token}`  

This demonstrates the same flow real production systems use.

---

## 🔧 Endpoints

### `GET /auth/login`
Redirects to Spotify's authorization endpoint with the required scopes.

### `GET /auth/callback`
Handles Spotify's redirect, exchanges the auth code for tokens, and stores them.

### `GET /me`
Retrieves the authenticated user’s Spotify profile.

Example response:

```json
{
  "display_name": "Your Name",
  "email": "you@example.com",
  "id": "your-spotify-id",
  "country": "US"
}
```

---

## 🛠️ Running the Project

### 1. Clone the repo

```bash
git clone https://github.com/MWardenDev/SpotifyConnector_CSharp
cd SpotifyConnector_CSharp
```

### 2. Configure Spotify app (Spotify Developer Dashboard)

Set:

- **Redirect URI**: `https://127.0.0.1:5001/auth/callback`  
  (must be *exactly* HTTPS + 127.0.0.1)

Gather:

- Client ID  
- Client Secret  

### 3. Add user secrets

Run inside `SpotifyConnector.Api` project folder:

```bash
dotnet user-secrets set "Spotify:ClientId" "your-client-id"
dotnet user-secrets set "Spotify:ClientSecret" "your-client-secret"
dotnet user-secrets set "Spotify:RedirectUri" "https://127.0.0.1:5001/auth/callback"
```

### 4. Run the API

```bash
dotnet run --project src/SpotifyConnector.Api/SpotifyConnector.Api.csproj
```

### 5. Authenticate

Visit:

```
https://127.0.0.1:5001/auth/login
```

Then call:

```
https://127.0.0.1:5001/me
```

---

## 🧩 Future Enhancements

- Add refresh-token exchange logic  
- Support additional Spotify API endpoints (playlists, tracks, playback)  
- Add persistent token store (SQL, Redis, etc.)  
- Add unit tests for OAuth flow + service layer  
- Create Go implementation (`SpotifyConnector_Go`) for comparison  
- Build frontend UI demonstrating login + playback control  

---

## 💡 Why This Project Exists

This repo is part of a wider effort to:

- Demonstrate practical API integration skills  
- Show strong command of .NET 8 architecture  
- Provide a reference for OAuth 2.0 flows  
- Build a multi-language comparative set of Spotify clients  

It’s intentionally clean, minimal, and readable for engineers and interviewers alike.

---

## 📜 License


