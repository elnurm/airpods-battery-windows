# 0.2.1 — Nearby broadcast cleanup

Download the portable Windows x64 ZIP, extract it, and run AirPods.exe. Keep AirPods.ico alongside the executable. Exit the previous version from its tray menu before starting this version.

- Nearby broadcasts show last-seen time and signal strength.
- Entries expire after two minutes without a supported battery broadcast, preventing old addresses from accumulating.
- If the selected entry expires, battery values clear and Device settings opens. The app never silently switches to another pair.

This remains an unsigned community preview for Windows 11 x64. Battery estimates use 10% steps. AirPods Pro 3 battery reception has been tested on one PC; eight other recognized earbud variants are experimental. Case battery and AirPods Max are not included.

Local decoder, connection-transition, UI state and scaled-layout checks pass. Bluetooth scanning and connection monitoring are unchanged. Real-device behavior varies with firmware and adapter; community feedback is welcome.

[Report compatibility](https://github.com/elnurm/airpods-battery-windows/issues/new?template=compatibility.yml)
