# ChatApp (C#) - macOS-friendly terminal chat

This repo has two parts:

- `ChatServer`: ASP.NET Core + SignalR server (`/chat`)
- `ChatClient`: Terminal (console) client

## Requirements

- .NET SDK (verify it is installed): `dotnet --version`

## Run

### 1) Start the server

```bash
cd ChatServer
dotnet run
```

By default the server listens on `http://localhost:5294` and the SignalR endpoint is `http://localhost:5294/chat`.

### 2) Run the client

Open a new terminal:

```bash
cd ChatClient
dotnet run
```

- When prompted for server address, you can keep `http://localhost:5294`.
- To quit: `/quit`

## Multiple users at the same time

You can run `ChatClient` in multiple terminals and use different usernames to chat.

## Note (NuGet cache permissions)

If you get a NuGet cache write-permission error, you can run using workspace-local caches like this:

```bash
NUGET_PACKAGES="$PWD/.nuget/packages" \
NUGET_HTTP_CACHE_PATH="$PWD/.nuget/http-cache" \
dotnet run
```

