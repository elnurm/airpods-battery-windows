# 0.2.4 — AirPods model filter

Download the portable Windows x64 ZIP, extract it, and run AirPods.exe. Keep AirPods.ico alongside the executable. Exit the previous version from its tray menu before starting this version.

- Selecting a nearby broadcast hides other AirPods models for the current session, including new broadcasts received later.
- Enable **Show all AirPods models** in Device settings to select a different model. Selecting a broadcast reapplies the filter.
- Nearby pairs of the same model remain visible; filtering does not verify ownership.
- README badges show Windows build status, latest release, release asset downloads, contribution information, and the MIT license.

Version 0.2.3 and later can notify you of this update. Earlier versions require a manual download. Installation remains manual in all versions.

Automatic matching cannot verify ownership: a single matching broadcast could belong to someone else. Nearby broadcasts still show last-seen time and signal strength and expire after two minutes without updates.

This remains an unsigned community preview for Windows 11 x64. Battery estimates use 10% steps. AirPods Pro 3 battery reception has been tested on one PC; eight other recognized earbud variants are experimental. Case battery and AirPods Max are not included.

Local decoder, connection-transition, UI state and scaled-layout checks pass. Bluetooth scanning and connection monitoring are unchanged. Real-device behavior varies with firmware and adapter; community feedback is welcome.

[Report compatibility](https://github.com/elnurm/airpods-battery-windows/issues/new?template=compatibility.yml)
