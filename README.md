# EZBlocker 3

EZBlocker 3 is a Spotify Ad Muter/Blocker for Windows written in C#.
It mutes Spotify while an advertisement is playing.
It is a complete rewrite of the original [EZBlocker](https://github.com/Xeroday/Spotify-Ad-Blocker), as it did not mute all the ads and Spotify was always muted on startup.

## Features
Comparing to the original, EZBlocker 3...
- ... provides better muting (e.g. [#215](https://github.com/Xeroday/Spotify-Ad-Blocker/pull/215))
- ... does not include any sort of analytics ([#103](https://github.com/Xeroday/Spotify-Ad-Blocker/issues/103)).
- ... is built using a [modern UI](https://github.com/Kinnara/ModernWpf)
- ... offers automatic updating
- ... uses an event based approach instead of polling for a smaller performance footprint

![Dark Theme](https://raw.githubusercontent.com/OpenByteDev/EZBlocker3/master/screenshots/Screenshot-01.png)
![Light Theme](https://raw.githubusercontent.com/OpenByteDev/EZBlocker3/master/screenshots/Screenshot-02.png)

## Setup

**[Download for Windows](https://github.com/OpenByteDev/EZBlocker3/releases/latest/)**

No need to install as EZBlocker 3 is a portable application.

## Remove application

To remove EZBlocker 3 the "Uninstall" button in the settings menu should be used.

## Technical overview

### UI
The EZBlocker 3 UI ist built using the [Windows Presentation Foundation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/introduction-to-wpf?view=netframeworkdesktop-4.8) and the [ModernWPF UI Library](https://github.com/Kinnara/ModernWpf)) for a modern Windows 10 look and system theme awareness.

### Ad detection
Ads are detected by checking the spotify window title. On startup EZBlocker scans for running processes named "spotify" that have a window and extracts the title. It then listens for title changes by handling the `EVENT_OBJECT_NAMECHANGE` event using [`SetWinEventHook`](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook).
Spotify startup and shutdown are detected by handling the `EVENT_OBJECT_DESTROY`and `EVENT_OBJECT_SHOW` events.

### Auto update
EZBlocker 3 checks for new releases on startup using the [Github REST API](https://docs.github.com/en/free-pro-team@latest/rest). If a newer version is found, it is downloaded next to the app. The `EZBlocker3.exe` file is then switched out while it is still running and then restarted.

## Credits

- [Eric Zhang](https://github.com/Xeroday) for the original [EZBlocker](https://github.com/Xeroday/Spotify-Ad-Blocker)
- [Yimeng Wu](https://github.com/Kinnara) for the beautiful [ModernWPF UI Library](https://github.com/Kinnara/ModernWpf)
- [James Newton-King](https://github.com/JamesNK) for his omnipresent [Json.NET](https://www.newtonsoft.com/json)
