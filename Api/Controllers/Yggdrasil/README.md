# How to use

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