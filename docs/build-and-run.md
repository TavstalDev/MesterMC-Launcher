# Building and Running

This document explains the simplest way to build and run the project locally. It focuses on the developer workflow used during active development: building the solution in an IDE and running the pieces in the correct order so the Launcher can authenticate against the local API.

> Short summary: In the IDE press "Build the whole solution" (or run dotnet build from the repo root), run the `Api` project, start your Minecraft server (with authlib-injector), then run the generated MMC-Launcher executable.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Depends on](#depends-on)
- [Build](#build)
- [Running](#running)

## Prerequisites

- .NET 9 SDK installed.
- An IDE that supports .NET (Visual Studio, Rider, Visual Studio Code + C# extension). The docs below assume you usually use the IDE.
- Java + Minecraft server for testing the launcher.
- authlib-injector on the Minecraft server (if you test the custom auth flow).
- (Optional) dev HTTPS certificate trusted locally — see the "Dev HTTPS certificate" note below.

## Depends on
This document assumes you have already read the following documentation:
- [Database documentation](./database.md) 
- [Yggdrasil documentation](./yggdrasil-auth.md)

## Build

Preferred (IDE):
- Open the solution `MMC-Launcher.sln` in your IDE.
- Use the IDE's "Build the whole solution" button/command. This will build all projects and produce the debug binaries you can run from the IDE or find on disk.

Alternatively, use the following command from the repo root to build all projects:
```bash
dotnet build ./MMC-Launcher.sln -c Debug
```

## Running
1. Run the `Api` (`Api: Development`) project first. The default endpoint is `https://localhost:36767/docs`
```bash
dotnet run --project ./MMC-Api/MMC-Api.csproj
```
2. Start your Minecraft server with authlib-injector as described.
3. Run the generated `MMC-Launcher` executable from disk. You can find it in `./MMC-Launcher/bin/Debug/net9.0/<RID>/publish/` after building. 
The launcher should start, connect to the API, and allow you to authenticate and launch the game.