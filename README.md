# Videfix
_**Videfix**_ (**Vi**rtual **De**sktop **Fi**x) is a small library & GUI utility that improves the convenience when working with many open windows or virtual desktops on Windows 10.

Since Windows 10, Microsoft finally offers the Task View feature, which allows you to arrange your windows on multiple virtual desktops. Unfortunately, after rebooting your system most of your windows that are reopened automatically, such as Word, Paint, Chrome or Edge, won't remember their virtual desktop position but remain on the first virtual desktop. This tool provides a fix for that situation by frequently taking automatical backups of all windows' positions and displaying a button in the system tray to restore such a backed up arrangement. You can also create manual backups.

Features:
- Save window arrangements
- Backup window arrangements automatically
- Restore window arrangement later
Supported languages:
- English
- German

## Development
The underlying API for dealing with virtual desktops on Windows has been copied from [@MScholtes](https://github.com/MScholtes)' [VirtualDesktop](https://github.com/MScholtes/VirtualDesktop) implementation. Thank him for creating & updating this demo!
This tool was originally developed in 2018 but has been successfully used by me until today. However, some aspects of the implementation are kind of hacky and could need a refactoring. Any contribution is highly welcome and will be honored & supported!

If you have any questions or problems, please create an issue.
