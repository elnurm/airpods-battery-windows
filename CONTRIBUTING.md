# Contributing

Compatibility reports are especially helpful for this first release. Use the compatibility issue form, even if everything works. Include the exact AirPods model, firmware if available, Windows version, Bluetooth adapter if known, and what you tested. Do not post serial numbers, Bluetooth addresses, account information, or pairing keys.

For bugs, describe what you expected, what happened, and how to reproduce it. Cropped screenshots are useful if device names and addresses are removed.

## Development

Run scripts/build.ps1 and scripts/test.ps1 -IncludeUi on Windows. The non-UI tests cover known model IDs, unknown models, pairing-mode/encrypted frames, battery bounds, ear orientation, charging bits, disconnect transitions, notification deduplication, and reconnection freshness. UI tests verify clearing values, bars, and tray text. UI tests use synthetic readings and do not connect to hardware.

- `src/Decoder.cs`: manufacturer payload parsing and its checks.
- `src/Models.cs`: recognized earbud models and experimental labels.
- `src/ConnectionState.cs`: connection/notification state and tests.
- `src/AirPodsBattery.cs`: Windows Forms UI, Windows Bluetooth integration, UI tests.
- `src/Artwork.cs`: original vector icon and illustrations.

Keep unknown values unknown. Do not treat nearby packets as a Windows audio connection. Do not infer identity from model or signal strength alone. Adding a model requires a known model ID, parser coverage, and an experimental label until a hardware report supports stronger claims.

## Manual release check

Test connection, battery reception, disconnection while broadcasts continue, reconnection, radio off/on, missing/old data, tray reopen/exit, and notification behavior on a real PC. Check the window at 100%, 125%, and 150% display scaling. Record Windows version, firmware, and adapter details in the release notes. Publish known gaps rather than claiming universal compatibility.
