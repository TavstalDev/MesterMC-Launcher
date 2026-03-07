# Getting Started

This guide will help you to get started with the project. It will cover the basics of setting up your development environment and understanding the core concepts.

## Table of Contents
- [Essential prerequisites](#essential-prerequisites)
- [Useful links](#useful-links)
- [Setting up the development environment](#setting-up-the-development-environment)

## Essential prerequisites

Before you begin, make sure you have the following installed on your machine:
- .NET 9 SDK
- EF Core 9.0
- ASP.NET Core 9.0
- An IDE of your choice, Rider is recommended
- A database server (MySQL or MariaDB)
- Windows 10+ or Linux
- Git
- Tools to manage certificates (e.g., OpenSSL)
- A web browser for testing the API or a tool like Postman (browser is recommended because of swagger)
- (Optional) a minecraft server to test the authentication

## Useful links
- [Database setup](./database.md)
- [Yggdrasil authentication](./yggdrasil-auth.md)
- [Building and running the project](./build-and-run.md)
- [Packaging](./packaging.md)

## Setting up the development environment
1. Clone the repository:
```bash
git clone https://github.com/TavstalDev/MesterMC-Launcher
```
2. Open the solution in your IDE.
3. Create a .env file in the root of the `Api` project, example:
```env 
JWT_ENCRYPTION_KEY=replace_with_a_secure_key

DB_USER=root
DB_PASSWORD=ascent

EMAIL_ADDRESS=example@localhost
EMAIL_PASSWORD=12345678

YGGDRASIL_CERT_PASSWORD=changeit
```
4. Adjust the `appsettings.json` and the `appsettings.Development.json` files in the `Api` project to match your database configuration and other settings.
5. Continue with the [Database setup](./database.md) guide to set up the database and run migrations.

### Appsettings.json example:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:36767",
        "Certificate": {
          "Path": "localhost.pfx",
          "Password": "changeit"
        }
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Issuer": "https://api.mestermc.com",
    "Audience": "https://api.mestermc.com",
    "EncryptionKey": "$JWT_ENCRYPT"
  },
  "ConnectionStrings": {
    "DefaultConnection": "server=127.0.0.1;port=3306;database=mmc;uid=$DB_USER;pwd=$DB_PASSWORD;"
  },
  "CORS": {
    "Default": {
      "AllowAnyOrigin": true,
      "AllowAnyHeader": true,
      "AllowAnyMethod": true,
      "Sites": [
        "https://api.mestermc.com",
        "https://mestermc.com"
      ]
    }
  },
  "Email": {
    "Provider": "",
    "Port": 587
  },
  "Discord": {
    "GuildId": "000000000000000000",
    "RedirectUri": "https://mestermc.com/oauth/callback?provider=discord"
  },
  "Servers": {
    "API": "https://api.mestermc.com",
    "Website": "https://mestermc.com"
  },
  "UploadDir": "wwwroot/uploads",
  "Yggdrasil": {
    "SkinDomains": [
      ".mestermc.com"
    ],
    "PublicKey": "$YGGDRASIL_PUBLIC_KEY",
    "ServerName": "MesterMC",
    "ImplementationName": "yggdrasil-mock-server",
    "ImplementationVersion": "1.0.0"
  }
}
```