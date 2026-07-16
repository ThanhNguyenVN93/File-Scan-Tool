# File Scan Tool

A lightweight Windows desktop tool for scanning suspicious files and URLs through multiple online
antivirus sources — **VirusTotal**, **MetaDefender (OPSWAT)**, and **Kaspersky OpenTIP** — without
ever opening a browser. Results (clean/infected, detection ratio, engine names, SHA-256/MD5) show
up right in the app.

![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2%2B-blue)
![Platform](https://img.shields.io/badge/platform-Windows%207%20SP1%2B-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

- **Multi-source scanning** — pick one source per scan (VirusTotal / MetaDefender / OpenTIP); no
  automatic fallback between sources, so quota/errors are always reported directly.
- **Hash lookup before upload** — checks if a result already exists for the file's SHA-256 before
  uploading the actual file content anywhere.
- **URL scanning** — check a URL against VirusTotal or OpenTIP without needing a local file.
- **Batch hash lookup** — paste a list of hashes and check them all against one source, no uploads.
- **Local scan history** — recently scanned files are cached locally so re-scanning the same file
  doesn't burn API quota.
- **Checksum verification** — paste an expected MD5/SHA-1/SHA-256 and compare it against a file.
- **File actions** — delete or quarantine an infected file straight from the results panel.
- **Authenticode signature check** — quick local signal on whether a file carries a digital signature.
- **ZIP password detection** — flags password-protected ZIP archives before scanning.
- **Proxy support** — system proxy by default, or configure a manual proxy (with credentials).
- **System tray integration** — runs quietly in the tray, global hotkey (`Ctrl+Alt+S`) to scan
  whatever file is currently in the clipboard, optional launch at Windows startup.
- **Bring your own API key (BYOK)** — enter your own free-tier API keys for each source; keys are
  encrypted at rest with Windows DPAPI (tied to your Windows user account).
- **.NET Framework bootstrapper** — automatically detects and installs the required .NET Framework
  version on first run, including a repair path for stripped-down Windows 7 installs.

## Requirements

- Windows 7 SP1 or later
- .NET Framework 4.7.2 or later (the app will offer to install it automatically if missing)
- Free API keys from the sources you want to use:
  - [VirusTotal](https://www.virustotal.com/gui/my-apikey)
  - [MetaDefender Cloud](https://my.opswat.com/)
  - [Kaspersky OpenTIP](https://opentip.kaspersky.com/token)

## Getting started

1. Download the latest release from the [Releases](../../releases) page and run `File Scan Tool.exe`.
2. Right-click the pill → **API key settings...** and paste in the API key(s) for whichever source(s)
   you plan to use.
3. Select a file (click the path field or drag & drop), pick a source, and click **Scan**.

No installer is required — it's a single portable executable. Keys are stored locally and encrypted;
nothing is sent anywhere except to the scanning source you explicitly choose.

## Building from source

Requires Visual Studio (or the .NET Framework 4.7.2 targeting pack + MSBuild) and NuGet package
restore for `Guna.UI2.WinForms` and `Newtonsoft.Json`.

```
msbuild frm_scan_online.csproj /p:Configuration=Release
```

The build output is a single self-contained executable (no separate DLLs need to ship alongside it
other than what's already produced in the output folder).

## Privacy notes

- Only the file/URL you explicitly submit for scanning is sent to the source you select — one
  source at a time, never all three.
- API keys are encrypted locally with DPAPI and never leave your machine except as the
  authentication header for your own API calls to the source you configured them for.
- Scan history is stored locally (unencrypted, but contains no file content — just file name, hash,
  and verdict) and can be cleared at any time from the tray menu.

## License

MIT — see [LICENSE](LICENSE).
