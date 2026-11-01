<p align="center"><img src="assets/AppIcon.png" width="104" alt="AirPods Battery for Windows icon"></p>

<h1 align="center">AirPods Battery for Windows</h1>

<p align="center"><strong>English</strong> · <a href="README.ru.md">Русский</a></p>

<p align="center">
  <a href="https://github.com/elnurm/airpods-battery-windows/actions/workflows/build.yml"><img src="https://github.com/elnurm/airpods-battery-windows/actions/workflows/build.yml/badge.svg?branch=main" alt="Windows build and tests"></a>
  <a href="https://github.com/elnurm/airpods-battery-windows/releases"><img src="https://img.shields.io/github/v/release/elnurm/airpods-battery-windows?include_prereleases&amp;sort=semver&amp;label=release" alt="Latest release including previews"></a>
  <a href="https://github.com/elnurm/airpods-battery-windows/releases"><img src="https://img.shields.io/github/downloads/elnurm/airpods-battery-windows/AirPodsBatteryDesktop-win-Setup.exe?label=downloads&amp;displayAssetName=false" alt="Installer downloads across all releases"></a>
  <a href="CONTRIBUTING.md"><img src="https://img.shields.io/badge/PRs-welcome-brightgreen" alt="Pull requests welcome"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue" alt="MIT license"></a>
</p>

<p align="center">A free, open-source AirPods battery widget for Windows 11.<br>Left and right battery levels, charging indicators, and disconnect alerts.</p>

<p align="center">
  <a href="https://github.com/elnurm/airpods-battery-windows/releases"><img src="https://img.shields.io/badge/Download_for_Windows_11-Installer-28a77b?style=for-the-badge" alt="Download for Windows 11"></a><br>
  Download <strong>AirPodsBatteryDesktop-win-Setup.exe</strong> from the release assets.
</p>

<table align="center">
  <tr><th>English</th><th>Русский</th></tr>
  <tr>
    <td><img src="docs/screenshot.png" width="264" alt="AirPods Battery in English with sample battery readings"></td>
    <td><img src="docs/screenshot-ru.png" width="264" alt="AirPods Battery на русском языке с примером уровня заряда"></td>
  </tr>
</table>

*Screenshots show sample readings.*

## Features

- Compact, draggable battery-ring widget with model-specific AirPods artwork.
- Light/dark themes, optional always-on-top, and system tray access.
- Automatic battery discovery and disconnect notifications.
- English and Russian interface, with updates available inside the app.

## Get started

1. Install the app and connect your AirPods in Windows Bluetooth settings.
2. Open the app. Battery discovery runs automatically; opening the case near the PC can help.
3. Use **⋯** for settings, language, theme, updates, or **Hide widget**. Double-click the tray icon to reopen it; choose **Exit** to quit.

If your AirPods are renamed or several pairs are connected, choose yours under **⋯ → Device settings**.

## Compatibility

**Tested with AirPods Pro 3.** AirPods 1–4, AirPods Pro, and Pro 2 are experimental. AirPods Max and Beats are not supported.

Requires **Windows 11 x64**, .NET Framework 4.8+, and Bluetooth with BLE scanning support. The installer is currently unsigned.

- Battery readings are approximate **10% steps**; case battery is not shown.
- Old readings show **Last known** and clear after two minutes without a signal.
- Nearby pairs of the same model can make automatic detection ambiguous.

Battery data stays on your PC. Update checks and downloads connect to GitHub.

[Technical details & troubleshooting](docs/DETAILS.md) · [Updates & ZIP migration](docs/UPDATES.md) · [Changelog](docs/CHANGELOG.md)

## Feedback and development

[Report compatibility](https://github.com/elnurm/airpods-battery-windows/issues/new?template=compatibility.yml) or [open an issue](https://github.com/elnurm/airpods-battery-windows/issues). Contributions and translations are welcome.

[Build from source](docs/DETAILS.md#build-from-source) · [Contributing](CONTRIBUTING.md) · [MIT license](LICENSE) · [Credits](NOTICE.md)
