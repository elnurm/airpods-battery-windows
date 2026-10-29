# 0.2.0 — Companion design

Download the portable Windows x64 ZIP, extract it, and run AirPods.exe. Keep AirPods.ico alongside the executable. Exit version 0.1.0 from its tray menu before starting this version.

- Light Companion layout with original earbud illustrations and rounded cards.
- iPhone-style battery indicators with percentages inside.
- Hover or keyboard-focus a battery indicator to see when it updated.
- Stale readings still show Last known; disconnecting clears the values.
- Expandable device settings and a model-aware heading.

This remains an unsigned community preview for Windows 11 x64. Battery estimates use 10% steps. AirPods Pro 3 battery reception has been tested on one PC; eight other recognized earbud variants are experimental. Case battery and AirPods Max are not included.

Local decoder, connection-transition, UI state and scaled-layout checks pass. Bluetooth scanning and connection monitoring are unchanged. Real-device behavior varies with firmware and adapter; community feedback is welcome.

[Report compatibility](https://github.com/elnurm/airpods-battery-windows/issues/new?template=compatibility.yml)
