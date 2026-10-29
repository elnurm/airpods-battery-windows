# Changelog

## 0.2.1 — Nearby broadcast cleanup

- Remove nearby broadcast entries after two minutes without a supported battery broadcast.
- Show last-seen time and signal strength in each entry.
- Clear an expired selection and open Device settings without silently switching to another pair.

## 0.2.0 — Companion design

- Light macOS-inspired Companion layout, original earbud artwork and native Windows window controls.
- iPhone-style battery indicators with percentages inside. The footer identifies estimates; tray percentages retain the approximation marker.
- Reading age and charging information on hover or keyboard focus of each battery indicator.
- Fresh timestamps no longer occupy permanent space. Charging, last-known, missing-data and disconnected states remain visible.
- Device selectors in expandable settings, open initially for setup and collapsed after selecting a battery broadcast.
- Model-aware window heading.

Battery decoding, model support and Windows connection monitoring are unchanged. Dark mode and automatic startup are not included.

## 0.1.0 — First preview

- Portable Windows 11 x64 battery monitor with tray icon.
- Pro 3 battery reception tested; eight other earbud variants experimental.
- Separate Windows connection monitoring and nearby BLE battery selection.
