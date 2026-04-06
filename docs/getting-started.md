# Getting Started

This guide will help you to get started with the project. It will cover the basics of setting up your development environment and understanding the core concepts.

## Table of Contents
- [Essential prerequisites](#essential-prerequisites)
- [Useful links](#useful-links)
- [Quick Start](#quick-start)
- [Configuration](#configuration)
- [Development Setup](#development-setup)

## Essential prerequisites

Before you begin, make sure you have the following software and tools installed on your machine:
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

## Quick Start

1. **Copy the example environment file:**
```bash
cd Api
cp .env.example .env
```

2. **Edit `.env` with your values:**
```bash
nano .env  # or use your preferred editor
```

3. **Set values as needed** (see Configuration section below)

## Configuration

### Required Variables

#### `JWT_ENCRYPTION_KEY`
- **Purpose**: Used to sign and encrypt JWT tokens for authentication
- **Type**: String (minimum 32 characters recommended)
- **Example**: `your-super-secret-encryption-key-here`
- **How to generate**: You can use `openssl rand -base64 32` on Linux/Mac

#### `DB_USER`
- **Purpose**: MySQL database username
- **Type**: String
- **Example**: `mmc_user`
- **Default (dev)**: `mmc`

#### `DB_PASSWORD`
- **Purpose**: MySQL database password
- **Type**: String
- **Example**: `your_secure_password_here`
- **Note**: Use a strong password in production

#### `EMAIL_ADDRESS`
- **Purpose**: SMTP account email address (sender for emails)
- **Type**: Email address
- **Example**: `noreply@example.com`
- **Note**: Should match your SMTP provider account

#### `EMAIL_PASSWORD`
- **Purpose**: SMTP account password
- **Type**: String
- **Note**: This is the password for your email provider account, not the application password

### `CERTIFICATE_FINGERPRINT`
- **Purpose**: SHA-1 fingerprint of the SSL certificate used for secure communication
- **Type**: String
- **Example**: `ABCDEF1234567890ABCDEF1234567890ABCDEF`
- **Note**: On Linux and macOS, this field is used as path to the certificate, so it should be the full path to the certificate file (e.g., `/path/to/certificate.pfx`)

#### `CERTIFICATE_PASSWORD`
- **Purpose**: Password for the SSL certificate
- **Type**: String
- **Default**: `replace_certificate_password`

## Development Setup

### Using Mailhog for Email Testing

For local development, you can use **Mailhog** to catch and inspect emails without actually sending them.

1. **Install Mailhog**:
```bash
# Using Docker (recommended)
docker run -d -p 1025:1025 -p 8025:8025 mailhog/mailhog
   
# Or download from https://github.com/mailhog/MailHog
```

2. **Set environment variables**:
```
EMAIL_ADDRESS=test@example.com
EMAIL_PASSWORD=anything
```

3. **Access the web UI**: Open `http://localhost:8025` to view sent emails