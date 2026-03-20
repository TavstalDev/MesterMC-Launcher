# TO-DO

This is a list of things that need to be done for this project. If you want to contribute, please feel free to pick any item from the list and work on it.

> Please note that this project is no longer maintained, so any contributions are for educational purposes only. 
> The following list of tasks are mostly for reference what should have been done.

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