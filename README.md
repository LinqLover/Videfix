# Videfix
_**Videfix**_ (**Vi**rtual **De**sktop **Fi**x) is a small library & GUI utility that improves the convenience when working with many open windows or virtual desktops on Windows 10.

<p align="center"><img src="https://github.com/LinqLover/Videfix/blob/master/screenshots/taskbar.png?raw=true" alt="Screenshot" /></p>

Since Windows 10, Microsoft finally offers the [Task View](https://en.wikipedia.org/wiki/Task_View) feature, which allows you to arrange your windows on multiple virtual desktops. Unfortunately, after rebooting your system, most of your windows that are reopened automatically, such as Word, Paint, Chrome or Edge, won't remember their virtual desktop position but remain on the first virtual desktop. This small tool provides a solution for that situation by frequently taking automatical backups of all windows' positions and displaying a button in the system tray to restore such a backed up arrangement. You can also create manual backups. Still works with Win 2004.

Features:
- Save window arrangements
- Backup window arrangements automatically
- Restore window arrangement later
- Automatical start with Windows (optional)

Supported languages:
- English
- German

## Installation
**End-users** please go to the **[Releases](//github.com/LinqLover/Videfix/releases)** selection and download the latest `VidefixTray.zip` archive. Extract it into any persistent directory on your system and execute `VidefixTray.exe`. An icon will appear in the system tray which you can right-click and choose an item. If you see the icon, your arrangements will be automatically backed up. To set up the autostart with windows, double-click the item and tick the "Start Videfix with Windows" box.

**For developers:** Provided that I did not make any configuration errors, you should be able to clone this project, open the solution with Visual Studio (tested on VS 2017) and create the build. If you run into any problems, please let me know.

## Development notes
The underlying API for dealing with virtual desktops on Windows has been copied from [@MScholtes](https://github.com/MScholtes)' [VirtualDesktop](https://github.com/MScholtes/VirtualDesktop) implementation. Thank him or her for creating & updating this demo!
This tool was originally developed in 2018 but has been successfully used by me until today. However, some aspects of the implementation are kind of hacky and could need a refactoring. Any contribution is highly welcome and will be honored & supported!

If you have any questions or problems, please create an issue.

### Todos
#### Internal (code quality)
- Real testing
- Upgrade code to .NET Core and C# 8
- Replace `lib/HelperLib.dll`, which is a proprietary precompiled and partially deprecated library.
- Automate build process
#### Feature Ideas
- Heuristics to enhance differentiation between similar windows (for example, could we get the tabs of a chrome window?)
- Respect window position, extent, state (min/max) for identification
- _(much bigger extent)_ Instead of only restoring the open windows' arrangements, also restore the windows itself.  
  Tricky, as different programs use many different ways to indicate their session state, if any (command-line arguments, registry, properietary cache files, no persistency at all).
