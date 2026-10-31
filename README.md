<p align="center"><img src="assets/AppIcon.png" width="104" alt="AirPods Battery for Windows icon"></p>

<h1 align="center">AirPods Battery for Windows</h1>

<p align="center"><strong>English</strong> · <a href="README.ru.md">Русский</a></p>

<p align="center">
  <a href="https://github.com/elnurm/airpods-battery-windows/actions/workflows/build.yml"><img src="https://github.com/elnurm/airpods-battery-windows/actions/workflows/build.yml/badge.svg?branch=main" alt="Windows build and tests"></a>
  <a href="https://github.com/elnurm/airpods-battery-windows/releases"><img src="https://img.shields.io/github/v/release/elnurm/airpods-battery-windows?include_prereleases&amp;sort=semver&amp;label=release" alt="Latest release including previews"></a>
  <a href="https://github.com/elnurm/airpods-battery-windows/releases"><img src="https://img.shields.io/github/downloads/elnurm/airpods-battery-windows/total?label=downloads" alt="Total release asset downloads"></a>
  <a href="CONTRIBUTING.md"><img src="https://img.shields.io/badge/PRs-welcome-brightgreen" alt="Pull requests welcome"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue" alt="MIT license"></a>
</p>

<p align="center"><strong>See your AirPods battery level on Windows 11, right from the system tray.</strong><br>A small, free, open-source desktop app showing left and right earbud percentages, charging state, and disconnect notifications.</p>

<p align="center">Built for everyday listening on a Windows PC.<br><strong>Tested with AirPods Pro 3; other recognized AirPods earbuds are enabled experimentally.</strong><br>Community compatibility reports are welcome.</p>

<p align="center">
  <a href="https://github.com/elnurm/airpods-battery-windows/releases"><img src="https://img.shields.io/badge/Download_for_Windows_11-Installer-28a77b?style=for-the-badge" alt="Download AirPods Battery for Windows 11 installer"></a><br>
  Choose AirPodsBatteryDesktop-win-Setup.exe under Assets on the release page.
</p>

<table align="center">
  <tr><th>English</th><th>Русский</th></tr>
  <tr>
    <td><img src="docs/screenshot.png" width="360" alt="AirPods Battery in English with sample battery readings"></td>
    <td><img src="docs/screenshot-ru.png" width="360" alt="AirPods Battery на русском языке с примером уровня заряда"></td>
  </tr>
</table>

*Illustrative app renders with identical synthetic readings, not a claim of precise battery measurement.*

## Features

- Separate left and right earbud battery estimates and charging indicators.
- Light Companion window, iPhone-style battery indicators, and a matching tray icon.
- Resizable window with maximize support, centered content, and a minimum size that keeps controls visible.
- Single instance per Windows user session: launching the app again shows the existing window instead of adding another tray icon.
- English and Russian interface. Right-click the tray icon → **Language / Язык** → **English** or **Русский** to switch immediately. Your choice is saved; on first launch the app uses Russian for a Russian Windows UI language, otherwise English.
- Clears percentages when the selected Windows Bluetooth device disconnects.
- Requests one notification per disconnect; waits for fresh data after reconnecting.
- Hover or keyboard-focus a battery indicator for its update time. Old readings visibly show Last known.
- Per-user installer, Start menu shortcut, and uninstall support. No account or custom drivers. Battery monitoring works offline; updates use the internet.

## Download and use

Get **AirPodsBatteryDesktop-win-Setup.exe** from [GitHub Releases](https://github.com/elnurm/airpods-battery-windows/releases). Starting with 0.3.0, releases are installer-only. The `.nupkg` and JSON files are used by the updater; you do not need to open them.

1. Exit any old copy through its tray menu, run the setup program, and open **AirPods Battery for Windows** from the Start menu.
2. Pair and connect your AirPods using Windows Bluetooth settings.
3. Expand **Device settings**. In the **Windows Bluetooth device** dropdown, select the Windows Bluetooth device to monitor. A single paired device named AirPods is selected automatically; renamed devices can be selected manually.
4. In the **Nearby battery broadcast** dropdown, select your nearby AirPods battery broadcast. Opening the case near the PC can help discovery.
5. Close the window to keep it in the tray. Double-click the tray icon to reopen it; right-click → **Exit** to quit.

Exit any previous copy before opening a new version. The release is unsigned; code signing is not included in this early community release.

**Moving from a portable ZIP:** versions up to 0.2.5 only detect releases containing a ZIP. They cannot notify you about this installer-only migration. Download and run the installer manually once, then stop using the old extracted executable. Your language preference remains in `%LOCALAPPDATA%\AirPodsBattery`; the installed app lives separately under `%LOCALAPPDATA%\AirPodsBatteryDesktop`. Uninstall through Windows Settings → Apps.

**In-app updates:** click an update notification or choose **Check for updates** in the tray menu. Confirm the download, then choose whether to restart and install. Cancelling a download leaves the current app running. Declining restart keeps the downloaded package; check for updates again when ready. Updates are not automatically applied on startup.

## Compatibility

| Model | Status |
| --- | --- |
| AirPods Pro 3 | Battery reception tested on one Windows 11 PC |
| AirPods 1, 2, 3 | Experimental; hardware reports needed |
| AirPods 4 / AirPods 4 with ANC | Experimental; hardware reports needed |
| AirPods Pro / AirPods Pro 2 / Pro 2 USB-C | Experimental; hardware reports needed |
| AirPods Max, Beats, other headphones | Not supported by this version |

Requires **Windows 11 x64**, .NET Framework 4.8 or later, and a Bluetooth adapter/driver that supports BLE scanning. Windows 10, ARM64, and other operating systems have not been validated. Model recognition does not guarantee that every firmware or adapter combination works.

Using an experimental model? [Report whether it works](https://github.com/elnurm/airpods-battery-windows/issues/new?template=compatibility.yml). Include the model, firmware if known, Windows version, and whether each earbud and disconnect notification worked.

## What to expect

**Battery precision:** this app reads the unencrypted Bluetooth Low Energy proximity broadcast. Values are approximate **10% steps**, identified as approximate in the footer (and with `~` in the tray). It does not extract encryption keys or provide exact 1% readings. Case battery is intentionally omitted.

**Connection vs. proximity:** a nearby AirPods broadcast does not mean the headphones are connected to Windows. The app separately checks the selected paired Windows device's connection state. A disconnect clears the stored percentages and bars; broadcasts received while disconnected cannot restore them. Windows may delay reporting the change, and Do Not Disturb or notification preferences may suppress the popup.

**Why two selectors?** AirPods rotate their BLE address. This version cannot authenticate the broadcast as belonging to the Windows device, so you choose both. Use it away from other AirPods when selecting, and choose the new broadcast if the address changes. After you manually choose a broadcast, the app remembers its model for this session. When that address expires, it automatically selects a sole matching model after five seconds of observation, provided its battery broadcast is fresh. Multiple matching models require manual selection; a second match also clears an automatic selection. Changing the Windows device resets the remembered model. This is a convenience heuristic: even a single matching broadcast may belong to someone else. If the two selectors refer to different pairs, the connection status and battery readings will not describe the same device.

**Model filter:** selecting a nearby broadcast hides other AirPods models for the current session. Enable **Show all AirPods models** in Device settings to choose a different model. Choosing a broadcast applies the filter again. Other pairs of the same model remain visible; this filter does not verify ownership.

**Old readings:** while Windows still reports a connection, a missing broadcast is treated as missing battery data, not proof of a disconnect. Readings older than 60 seconds are muted and labeled Last known. Their age is available on hover or keyboard focus. Broadcast entries show their last-seen time and signal strength (dBm), and disappear after two minutes without a supported battery broadcast. If the selected entry expires, its battery display clears and Device settings opens for a new selection. Signal strength does not establish which pair is yours.

## Troubleshooting

| Symptom | Try this |
| --- | --- |
| No paired device listed | Pair in Windows first, then click Rescan. |
| Disconnected even with earbuds nearby | Connect the selected device to this PC; proximity is not a connection. |
| No battery broadcasts | Open the case near the PC, check Bluetooth, and click Rescan. |
| Updates stop | Check reading age and select the new broadcast entry if its address rotated. |
| No popup | Check Windows notification preferences; the disconnected state still appears in the app. |
| Experimental model does not work | Submit a compatibility report; avoid posting serial numbers or Bluetooth addresses. |

## Build from source

On a Windows 11 x64 machine, open PowerShell in the repository root:

```powershell
./scripts/build.ps1
./scripts/test.ps1 -IncludeUi
./scripts/package.ps1
```

The build uses the Windows .NET Framework compiler and system WinRT metadata. The first build downloads pinned, checksum-verified Velopack and Newtonsoft.Json dependencies. Packaging also downloads a private .NET runtime and Velopack CLI into `build/dependencies`; no machine-wide SDK install is needed. The executable is written to `build/AirPods.exe`; packaging produces the installer, full update package, feed metadata and `SHA256SUMS.txt` under `dist/v<version>/`. Close a running build before rebuilding it. See [release publishing](docs/UPDATES.md).

GitHub Actions is configured to build, run decoder/connection tests, and produce downloadable build artifacts. It does not publish releases automatically. Hardware Bluetooth tests require a real PC and AirPods; hosted CI cannot establish device compatibility.

## Privacy

The installed app checks this project's public GitHub Releases API at startup and once every 24 hours while running, including preview releases with a Windows update feed. **Check for updates** also checks manually. A notification opens the in-app update flow. Downloading and restarting require your confirmation. Automatic notifications appear once per new version. Windows notification settings may suppress them; **GitHub releases** opens the manual installer download page.

Update checks and downloads contact GitHub over HTTPS. GitHub receives your IP address and request metadata, but no Bluetooth addresses, device names or battery data. No authentication token is used. The last notified version is saved in `%LOCALAPPDATA%\AirPodsBattery\notified-version.txt` to avoid repeating notifications after restarting. Offline or failed automatic checks stay quiet and retry at the next daily check. Velopack verifies package checksums before installation. Trust is based on HTTPS and control of this GitHub repository; packages do not yet have independent publisher signatures or Authenticode signing.

Bluetooth advertisements are read locally. Battery values and device selections are held in memory. The optional `--diagnose` mode writes packet counts and errors beside the executable and does not check for updates; it does not intentionally log raw packets or addresses. Error details may contain local paths, so review logs before sharing them.

Your chosen interface language is stored in `%LOCALAPPDATA%\AirPodsBattery\language.txt`. Device names and model names remain unchanged when switching languages.

## Contributing and license

More interface languages are planned. Translation suggestions and contributions are welcome; English and Russian are the currently supported languages.

See [CONTRIBUTING.md](CONTRIBUTING.md) for bug reports, compatibility feedback, and development notes. The project is released under the [MIT license](LICENSE). Protocol references and trademark information are in [NOTICE.md](NOTICE.md).
