![icon](https://github.com/OpenByteDev/EZBlocker3/blob/master/EZBlocker3/Icon/Icon128.png)
# EZBlocker 3

EZBlocker 3 is a Spotify Ad Muter/Blocker for Windows written in C#.
It mutes Spotify while an advertisement is playing.
It is a based on the original [EZBlocker](https://github.com/Xeroday/Spotify-Ad-Blocker).

Comparing to the original, EZBlocker 3...
- ... fixes some muting bugs (e.g. [#215](https://github.com/Xeroday/Spotify-Ad-Blocker/pull/215))
- ... does not include any sort of analytics ([#103](https://github.com/Xeroday/Spotify-Ad-Blocker/issues/103)).
- ... is built using a [modern UI](https://github.com/Kinnara/ModernWpf)
- ... offers automatic updating
- ... uses an event based approach instead of polling for a smaller performance footprint (using [WinEventHooks](https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook))

## Setup

**[Download for Windows](https://github.com/OpenByteDev/EZBlocker3/releases/latest/)**

No need to install as EZBlocker 3 is a portable application.

## Credits

- [Eric Zhang](https://github.com/Xeroday) for the original [EZBlocker](https://github.com/Xeroday/Spotify-Ad-Blocker)
- [Yimeng Wu](https://github.com/Kinnara) for the beautiful [ModernWPF UI Library](https://github.com/Kinnara/ModernWpf))
