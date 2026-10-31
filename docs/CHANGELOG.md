# Changelog

## 0.2.5 — Russian support and window improvements

- Prevent duplicate normal app processes and tray icons; a second launch shows the existing window. Diagnostic runs remain separate.
- Add Russian translations for the interface, tray menu, tooltips, update dialogs, and notifications, with a saved English/Russian selection in the tray menu.
- Allow resizing and maximizing the window, keeping the battery display centered and readable.
- Keep manually enlarged windows at their chosen size when toggling Device settings; enforce a minimum size to prevent clipping.
- Add Russian documentation, matching English/Russian screenshots, and prominent download links. Include both README files in the portable ZIP.

## 0.2.4 — AirPods model filter

- Hide other AirPods models after selecting a broadcast, with a Show all AirPods models option for switching pairs.
- Add README badges for build status, releases, downloads, contributions, and license.

## 0.2.3 — Update notifications

- Check public GitHub releases at startup and daily, including downloadable previews.
- Notify once per newer version, with a click opening its release page.
- Add manual update checks and a releases link to the tray menu.
- Keep automatic checks quiet on network failures; document network requests and local notification state.

## 0.2.2 — Automatic broadcast recovery

- Remember the manually selected model for the current session and automatically recover a sole fresh matching broadcast after five seconds.
- Require manual selection when multiple matching models are nearby; clear an automatic selection if it becomes ambiguous.
- Reset the model preference when changing the Windows Bluetooth device.

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
