# 0.2.3 — Update notifications

Download the portable Windows x64 ZIP, extract it, and run AirPods.exe. Keep AirPods.ico alongside the executable. Exit the previous version from its tray menu before starting this version.

- Check GitHub Releases at startup and daily, including preview versions with a Windows ZIP.
- Receive one notification per newer version; click it to open the release page. Installation remains manual.
- Use **Check for updates** or **GitHub releases** in the tray menu at any time.
- Automatic checks stay quiet while offline. GitHub receives your IP address and app version, but no Bluetooth or battery data. Notification history is saved locally.

Older versions do not contain the updater: install this version manually to receive future update notifications.

Automatic matching cannot verify ownership: a single matching broadcast could belong to someone else. Nearby broadcasts still show last-seen time and signal strength and expire after two minutes without updates.

This remains an unsigned community preview for Windows 11 x64. Battery estimates use 10% steps. AirPods Pro 3 battery reception has been tested on one PC; eight other recognized earbud variants are experimental. Case battery and AirPods Max are not included.

Local decoder, connection-transition, UI state and scaled-layout checks pass. Bluetooth scanning and connection monitoring are unchanged. Real-device behavior varies with firmware and adapter; community feedback is welcome.

[Report compatibility](https://github.com/elnurm/airpods-battery-windows/issues/new?template=compatibility.yml)
