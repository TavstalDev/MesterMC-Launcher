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

- [ ] Remove some configuration options such as:
  - [ ] "Use custom Java path" because it is not needed and it can cause issues if the user doesn't know what they are doing.
  - [ ] "Use custom JVM arguments" because it is not needed and it can cause issues if the user doesn't know what they are doing.
  - [ ] "Environment variables" because it is not needed and it can cause issues if the user doesn't know what they are doing.
  - [ ] "Ignore system Java" because the launcher should always use its own Java runtime to avoid compatibility issues.
- [ ] Make java mirrors static and not changeable by the user.
- [ ] Remove "manifests" directory and download them to the assets directory instead.
- [ ] Rename "logs" directory to "launcher-logs" to avoid confusion with the logs directory of the game.
- [ ] "versions" directory should be changed entirely and how the launcher handles game versions.

## API

*Nothing to do here for now.*