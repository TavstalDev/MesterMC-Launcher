# Packaging

This document describes the recommended packaging steps used in this project for creating releases on Windows and Linux, and notes for macOS. It includes the exact `dotnet publish` flags we use, examples for generating a local development code-signing certificate and signing executables (PowerShell examples), archive and checksum steps, and where to put generated artifacts so the installer build can pick them up.

> The primary development was on Linux so some commands might need adjustments for Windows/macOS. The principles and general steps are the same across platforms.

## Table of Contents
- [Principles and rules](#principles-and-rules)
- [Build / Publish commands](#1-build--publish-commands)
- [Code signing (Windows only)](#2-code-signing-windows-only)
- [Create archive and copy into installer input folder](#3-create-archive-and-copy-into-installer-input-folder)
- [Build the installer](#4-build-the-installer)

## Principles and rules
- We build self-contained, single-file publishes for each runtime so users don't need a preinstalled .NET runtime.
- Built binaries and archives must NOT be committed to git. Put them into `./MMC-Installer/Software` (or a separate release folder) before building the installer.
- Use reproducible flags where possible. Always run the same publish commands used here.
- Windows executables should be signed (we sign with a development cert for CI/local dev testing; production signing uses a real CA certificate).
- Linux packages use `.tar.gz` and are not signed with Authenticode.
- macOS: packaging is untested but publish commands are included.

## 1. Build / Publish commands

General publish flags used by the project:
- `-c Release` — Release configuration
- `-r <RID>` — Runtime identifier (see RIDs below)
- `--self-contained true` — Produce a self-contained executable
- `-p:PublishSingleFile=true` — Produce a single-file publish (one exe or single-binary)

RIDs we use as examples:
- Windows x64: `win-x64`
- Linux x64: `linux-x64`
- macOS x64: `osx-x64` (or `osx-arm64` for Apple Silicon)

Example (Windows, two apps: Launcher and Updater):
```bash
# Run from repo root
dotnet publish ./MMC-Launcher/MMC-Launcher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish ./MMC-Updater/MMC-Updater.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Linux example:
```bash
# Run from repo root
dotnet publish ./MMC-Launcher/MMC-Launcher.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish ./MMC-Updater/MMC-Updater.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

MacOS example:
```bash
# Run from repo root
dotnet publish ./MMC-Launcher/MMC-Launcher.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
dotnet publish ./MMC-Updater/MMC-Updater.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
```

Notes:
- Published files will be located under
- - ./MMC-Launcher/bin/Release/net<version>/<RID>/publish/
- - ./MMC-Updater/bin/Release/net<version>/<RID>/publish/
- - ./MMC-Installer/bin/Release/net<version>/<RID>/publish/

## 2. Code signing (Windows only)

You can create a local development code-signing certificate, export it to a .pfx, and sign EXEs locally for testing.
A. Create a self-signed code-signing certificate and export to PFX:
> Run in an elevated PowerShell or pwsh session if required
### 1) Create a code signing cert in CurrentUser store
```powershell
$cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=MMC-Dev" -CertStoreLocation "Cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(5)
```

### 2) Export the cert to a .pfx file (choose a secure password in real use)

Use signtool to sign the built EXEs with the generated certificate:
```powershell
signtool sign /a /fd SHA256 /t http://timestamp.digicert.com <path_to_exe>
```

## 3. Create archive and copy into installer input folder
After signing, create an archive containing the Launcher and Updater EXEs:
- Windows archive name: `MMCLauncher_windows_x64.zip`
- Linux archive name: `MMCLauncher_linux_x64.tar.gz`
- MacOS archive name: `MMCLauncher_macos_x64.tar.gz` (or `MMCLauncher_macos_arm64.tar.gz`)

Then copy the archive to `./MMC-Installer/Software/` so the installer project can include it in the final installer.

## 4. Build the installer

Finally, build the installer using the following command from the repo root:
### Windows:
```bash
dotnet publish ./MMC-Installer/MMC-Installer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```
> Do not forget to sign the installer EXE with the same certificate used for the Launcher and Updater EXEs, using signtool as shown above.
### Linux:
```bash
dotnet publish ./MMC-Installer/MMC-Installer.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

### MacOS:
```bash
dotnet publish ./MMC-Installer/MMC-Installer.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
```