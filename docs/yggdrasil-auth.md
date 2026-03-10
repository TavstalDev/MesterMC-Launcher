# Yggdrasil Authentication

This document provides instructions on how to set up and use a custom Yggdrasil authentication server for Minecraft. This allows you to authenticate users using your own server instead of the official Mojang servers.

> Please note that the Yggdrasil authentication was implemented after I parted from the MesterMC network, so I had no intention to make it a complicated and hardly reproducible process.

## Table of Contents
- [Server](#server)
- [Client](#client)
- [HTTPS Development certificate](#https-development-certificate)
  - [Generating certificate](#generating-certificate)
  - [Using pregenerated certificate](#using-pregenerated-certificate)

## Server

1. Authlib-Injector is required to run the server. You can download it from [here](https://github.com/yushijinhun/authlib-injector).
2. Place the `authlib-injector.jar` file in the same directory as the server.jar / start script.
3. Example script to run the server:
```bash
#!/bin/bash

java -javaagent:authlib-injector.jar=https://localhost:36767/yggdrasil/ -Dauthlibinjector.disableHttpd -Dauthlibinjector.usernameCheck=enabled -Xms2048M -Xmx2048M -jar server.jar --nogui
```

4. Optionally, add the following JVM arguments to enable debug logging:
```bash
-Dauthlibinjector.debug
```

## Client
The client requires the following JVM arguments to be able to use the custom Yggdrasil server:
```bash
-Dminecraft.api.env=custom
-Dminecraft.api.auth.host=https://localhost:36767/yggdrasil/
-Dminecraft.api.account.host=https://localhost:36767/yggdrasil/
-Dminecraft.api.session.host=https://localhost:36767/yggdrasil/
-Dminecraft.api.services.host=https://localhost:36767/yggdrasil/
```

Optionally for HD skin support, it needs the CustomSkinLoader mod installed, you can download it from [here](https://modrinth.com/mod/customskinloader).
> Other mods may work as well, but this is the only one that has been tested and confirmed to work with the custom Yggdrasil server.

## HTTPS Development certificate
Java is very strict about trusting certificates, so you will need to add the development certificate to your Java trust store.

First of all, the client uses custom java installation not the system one, so you will need to find the `cacerts` file in the `lib/security` folder of the custom java installation.
You can find a pre-generated certificate in the Api project, or you can generate your own self-signed certificate using OpenSSL or a similar tool. I suggest using mkcert.

> Please note that the commands provided in this section are for Linux and may need to be adjusted for other operating systems.

### Generating certificate

Usage of mkcert:
```bash
mkcert -install
mkcert localhost 127.0.0.1 ::1
```

Creating a .pfx file from the generated certificate and private key:
```bash
openssl pkcs12 -export
  -out localhost.pfx
  -inkey localhost+2-key.pem
  -in localhost+2.pem
  -password pass:changeit
```

After that copy the `localhost.pfx` file into the Api project, also don't forget to adjust the password in the `appsettings.json` and the `appsettings.Development.json` file if you used a different password.

Finally import it to the launcher Java trust store.
The java installation is located in the `<launcher_root>/java/jdk-21.0.8+9` directory.
In debug mode the launcher will create its directory (`LauncherDebug`) to the same directory where the executable is located, the path of the executable is `<path_to_project>/MMC-Launcher/bind/Debug/net9.0/<arch>/MMC-Launcher`.

```bash
cd <launcher_root>/java/jdk-21.0.8+9/bin
keytool -importcert
  -file <path_to_certificate>/localhost+2.pem
  -keystore ../lib/security/cacerts
  -alias mmc-dev
  -storepass changeit
```

### Using pregenerated certificate

Getting the necessary files from the .pfx file:
```bash
openssl pkcs12 -in localhost.pfx -clcerts -nokeys -out localhost+2.pem
```

Import the certificate to the system Java trust store
```bash
keytool -importcert
  -file localhost+2.pem
  -keystore /usr/lib/jvm/java-21-openjdk/lib/security/cacerts
  -alias mmc-dev
  -storepass changeit
```

Then import the certificate to the launcher Java trust store
The java installation is located in the `<launcher_root>/java/jdk-21.0.8+9` directory.
In debug mode the launcher will create its directory (`LauncherDebug`) to the same directory where the executable is located, the path of the executable is `<path_to_project>/MMC-Launcher/bind/Debug/net9.0/<arch>/MMC-Launcher`.

```bash
cd <launcher_root>/java/jdk-21.0.8+9/bin
keytool -importcert
  -file <path_to_certificate>/localhost+2.pem
  -keystore ../lib/security/cacerts
  -alias mmc-dev
  -storepass changeit
```