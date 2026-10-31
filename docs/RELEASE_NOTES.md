# 0.3.0 — Installer and in-app updates

Download **AirPodsBatteryDesktop-win-Setup.exe**, exit all previous copies through the tray, and run the installer. Open AirPods Battery for Windows from the Start menu. New releases no longer include a portable ZIP.

- Per-user installation, Start menu shortcut, and Windows uninstall entry.
- Installed copies check GitHub at startup and daily. Click a notification or use **Check for updates** to download, then confirm **Restart and update**.
- Download progress and cancellation, package checksum verification, and refusal to downgrade.
- English and Russian update dialogs. Existing language preferences are preserved.

**Migrating from ZIP versions (0.2.5 and earlier):** install manually once. Those versions require a ZIP asset to detect a release, so they cannot notify about this installer-only release. Stop using the old extracted executable after installing.

The .nupkg and feed files under Assets are used by the updater; users only need the Setup EXE. Future releases must include the entire generated update feed alongside the installer.

The release remains unsigned. Downloads use HTTPS from this repository and checksums detect corruption, but there is no independent publisher signature or Authenticode certificate yet.

Windows 11 x64; approximate 10% battery steps. AirPods Pro 3 tested on one PC; other recognized earbuds experimental. Case battery and AirPods Max are not included.
