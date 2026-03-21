# TO-DO

This is a list of things that need to be done for this project. If you want to contribute, please feel free to pick any item from the list and work on it.

> Please note that this project is no longer maintained, so any contributions are for educational purposes only. 
> The following list of tasks are mostly for reference what should have been done during active development.

## UI
- [ ] Resize the update window and its background image because they took too much transparent space on the screen and the user might be confused why they can't click through it.
- [ ] Add a mods tab to the settings page where users can choose which mods they want to be enabled and which ones they want to be disabled.
- [ ] Add remember me functionality to the login page so that users don't have to log in every time they open the launcher.
- [ ] Adjust authentication so the users don't have to log in every time even if it is more secure to not store any credentials.

## Backend

- [x] Remove some configuration options such as:
  - [x] "Use custom JVM arguments" because it is not needed and it can cause issues if the user doesn't know what they are doing.
  - [x] "Environment variables" because it is not needed and it can cause issues if the user doesn't know what they are doing.
  - [x] "Ignore system Java" because the launcher should always use its own Java runtime to avoid compatibility issues.
- [x] Make java mirrors static and not changeable by the user.
- [x] Remove "manifests" directory and download them to the assets directory instead.
- [x] Rename "logs" directory to "launcher-logs" to avoid confusion with the logs directory of the game.
- [x] "versions" directory should be changed entirely and how the launcher handles game versions.
- [ ] Anti-tamper and anti-cheat measures to make it **harder** to modify the launcher and cheat in the game.
  - [ ] Code obfuscation to make it harder to reverse engineer the launcher.
  - [ ] Integrity checks to ensure that the launcher files have not been modified.
  - [ ] Anti-debugging techniques to make it harder to debug the launcher.
  - [ ] Memory protection to prevent memory dumping and modification of the launcher's memory.
  - [ ] Server-side connection validation.
  - [ ] Improve authentication complexity to make it harder to reproduce.
- [ ] Remove and/or refactor the code related to KonkordLauncher (Core project) since it was made in mind to be a customizable and multi-version supporting launcher, and it is not needed for this project.

## API

- [ ] Add new controllers
  - [ ] ProductController to handle product related operations.
  - [ ] CartController to handle cart related operations.
  - [ ] PaymentController to handle payment related operations.
  - [ ] OrderController to handle order related operations.
  - [ ] CouponsController to handle coupon related operations.
  - [ ] LuckyWheelController to handle lucky wheel related operations.
  - [ ] UserExtrasController to handle user extra features such as prefixes and welcome messages.
  - [ ] FriendsController to handle friend related operations.
  - [ ] NotificationsController to handle notifications related operations.
- [ ] Review rate limiting and authentication for the API to ensure it is secure and efficient.
- [ ] Make launcher login endpoint more secure and utilize deviceID and other parameters to prevent abuse and unauthorized access.

## Client

Note that making a secure client and also respecting Mojang's EULA and terms of service is a very difficult task.
> Client was not developed at all, these tasks are mostly for reference.

- [ ] Use MCP-Reborn to deobfuscate the Minecraft client and make it easier to modify and add features to the game.
- [ ] Custom menu background
- [ ] Custom loading screen
- [ ] Custom title screen
- [ ] Customize authlib or replace it entirely.
- [ ] Add support for HD skins and capes.
- [ ] Add support for extra player cosmetics such as hats, wings, tails and bands.

## Installer

- [ ] Add logic to detect if the launcher is already installed and if it is, ask the user if they want to repair or uninstall the launcher.
  - Note: This was planned to be done after the beta testing phase, but was never implemented due to the project being discontinued.
- [ ] Add logic to detect if the launcher is already running and if it is, ask the user if they want to close the running instance and continue with the installation or cancel the installation.