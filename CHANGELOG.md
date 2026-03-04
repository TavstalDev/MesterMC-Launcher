# Changelog

## 0.0.4
- Added settings menu.
- Stricter file management; most files now have integrity verified via SHA-1.
- Enabled "Online" login.
- Re-enabled news retrieval (reverted previous commenting out).
- New ASP.NET web API for improved authentication and management; groundwork for a custom Yggdrasil auth server.

## 0.0.3
- Disabled automatic server connection.
- Updated Minecraft version from 1.21.5 to 1.21.8 due to instability issues on Windows.
- Switched mod management to Modrinth for downloading and updates.
- Fixed automatic Java download; added a config option to ignore system-installed Java versions.
- Fixed Java VM launch on Windows.
- Added splash screen.
- Reworked game launch process.
- Added new Java VM arguments for faster startup.

## 0.0.2
- Fixed drag & drop in the launcher.
- Added Beta title/version and build date display.
- Updated automatic Java detection.
- Added a config option to ignore system-installed Java versions.
- Removed unnecessary mods.
- Disabled Mojang's `command_history.txt`.
- Replaced username text field with a combobox.

## 0.0.1
- Initial release and public BETA testing.
- Minecraft 1.21.5 fabric
- Offline mode only (disabled authentication due to API issues).