# DaggerLog

DaggerLog is a utility mod for Daggerfall Unity that enhances error handling by logging unhandled exceptions and providing players with an in-game prompt to manage exceptions and crashes more gracefully. It helps both players and mod developers diagnose issues without diving through log files after every session.

## Features
- __Logs unhandled exceptions__ and displays an in-game prompt.
- __Ignore exceptions__: Users can choose to ignore specific exceptions — they'll still be logged but won't pop-up again.
- __Persistent logs__: Keeps the last 5 log files to avoid clutter.
- __Quick access to logs__: The in-game prompt includes a button that opens the log folder in the user's preferred file browser.
- __Clipboard report__: The prompt allows users to copy a detailed exception report to the clipboard, including:
    - Modlist
    - Location
    - Stacktrace

## Installation
1. Download the latest release from [GitHub](https://github.com/Z-Machine/dfu-daggerlog/releases).
2. Extract the contents for your OS into your `DaggerfallUnity_Data/StreamingAssets/Mods` folder.
3. Enable the mod in the Mod Manager
4. Set the mod's load priority to zero, it should be all the way at the top.

## Usage
Whenever an unhandled exception occurs an in-game prompt will appear with the following options:
- __Ignore__: Supresses future pop-ups for this specific error. This does not prevent logging of said error.
- __Copy to Clipboard__: Copies a report of the current exception to the user's system clipboard.
- __Quit__: Immediately closes the game without saving to prevent potential savefile corruption.

The prompt also includes a hamburger button on the top left which will open the user's log directory in their preferred file manager.

## Logging
Logs are stored in your persistent data folder for Daggerfall Unity and works out of the box with Portable installations of the game.

The logs will be located within the `DaggerLog` folder and currently the mod keeps the last 5 files, rotating out older ones.

## Compatibility
DaggerLog doesn't interfere with gameplay or save files and was for v1.1.1 and above.

## License
This mod is released under the MIT License.