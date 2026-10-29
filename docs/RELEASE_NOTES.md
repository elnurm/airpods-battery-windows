# 0.2.2 — Automatic broadcast recovery

Download the portable Windows x64 ZIP, extract it, and run AirPods.exe. Keep AirPods.ico alongside the executable. Exit the previous version from its tray menu before starting this version.

- Select your broadcast once per app session. After its address expires, the app automatically chooses a sole fresh broadcast of the same model after five seconds of observation.
- Multiple matching models require manual selection. If another matching model appears after automatic selection, the app clears that selection.
- Changing the Windows Bluetooth device resets the model preference. Signal strength is not used to choose a pair.

Automatic matching cannot verify ownership: a single matching broadcast could belong to someone else. Nearby broadcasts still show last-seen time and signal strength and expire after two minutes without updates.

This remains an unsigned community preview for Windows 11 x64. Battery estimates use 10% steps. AirPods Pro 3 battery reception has been tested on one PC; eight other recognized earbud variants are experimental. Case battery and AirPods Max are not included.

Local decoder, connection-transition, UI state and scaled-layout checks pass. Bluetooth scanning and connection monitoring are unchanged. Real-device behavior varies with firmware and adapter; community feedback is welcome.

[Report compatibility](https://github.com/elnurm/airpods-battery-windows/issues/new?template=compatibility.yml)
