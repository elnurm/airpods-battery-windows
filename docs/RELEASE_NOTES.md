# 0.2.5 — Russian support and window improvements

Download the portable Windows x64 ZIP, extract it, and run AirPods.exe. Keep AirPods.ico alongside the executable. Exit the previous version from its tray menu before starting this version.

- Russian interface, tray menu, tooltips, dialogs, and notifications. Choose **Language / Язык → Русский** from the tray menu; your preference is saved.
- Resize or maximize the window. Content stays centered, and the minimum size keeps controls visible.
- Reopening the app shows the existing window instead of creating a second process and tray icon. Diagnostic runs remain separate.
- English and Russian README files, matching screenshots, and prominent download links. Both README files are included in the ZIP.

Before upgrading, exit every running older copy from its tray menu. Older versions do not participate in the new single-instance check.

Version 0.2.3 and later can notify you of this update. Earlier versions require a manual download. Installation remains manual in all versions.

Automatic matching cannot verify ownership: a single matching broadcast could belong to someone else. Nearby broadcasts still show last-seen time and signal strength and expire after two minutes without updates.

This remains an unsigned community preview for Windows 11 x64. Battery estimates use 10% steps. AirPods Pro 3 battery reception has been tested on one PC; eight other recognized earbud variants are experimental. Case battery and AirPods Max are not included.

Local decoder, connection-transition, update-selection, single-instance, language-switching, UI state, resize, and scaled-layout checks pass. Bluetooth scanning and connection monitoring are unchanged. Real-device behavior varies with firmware and adapter; community feedback is welcome.

[Report compatibility](https://github.com/elnurm/airpods-battery-windows/issues/new?template=compatibility.yml)
