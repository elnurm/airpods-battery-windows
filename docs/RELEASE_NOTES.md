# 0.1.0 — first preview

AirPods Battery for Windows brings approximate left/right battery levels to a small Windows 11 tray app.

- Original earbud illustrations and mint app icon.
- Charging state and per-earbud reading age.
- Windows connection monitoring: clear readings on disconnect, request one notification, and wait for fresh values on reconnect.
- AirPods Pro 3 battery reception tested on one Windows 11 PC.
- Other recognized AirPods earbuds enabled experimentally; please submit compatibility reports.

This is an unsigned early preview for Windows 11 x64. Readings use 10% steps. Case battery, AirPods Max, exact 1% readings, automatic identity matching, and automatic startup are not included. Select the paired Windows device and its nearby battery broadcast separately.

Automated decoder, connection, and UI tests pass locally. CI and complete real-device disconnect/reconnect notification delivery still need release validation. See the README for limitations and troubleshooting.
