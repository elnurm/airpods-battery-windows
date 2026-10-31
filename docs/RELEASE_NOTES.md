# 0.3.2 — Fix GitHub update checks

Fixes the misleading internet error in 0.3.0 and 0.3.1: the installed updater inherited old .NET TLS defaults, which GitHub rejected. This release explicitly enables TLS 1.2 and includes a .NET Framework 4.8 runtime configuration. Certificate validation remains enabled.

- Installed update checks now use a modern secure connection.
- Manual failures show the actual error instead of blaming the internet connection.
- Added a live GitHub check and verified-download test; local feed tests alone did not expose this issue.

**For affected installed users:** download and run **AirPodsBatteryDesktop-win-Setup.exe** once if the existing updater cannot connect. Existing language settings are preserved. Future releases can then be downloaded and applied from the app.

This remains an unsigned installer. Battery behavior and supported languages are unchanged.
