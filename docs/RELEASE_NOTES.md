# 0.3.5 — Battery signal recovery

Reduce unnecessary waiting for battery data after AirPods change their Bluetooth advertising address.

- Consider only matching signals seen within the last 15 seconds that contain usable battery data when automatically selecting a source. Older addresses remain cached but no longer block a fresh signal for two minutes.
- Keep the current reading during quiet scans, with the usual stale labels and expiry rules. Quiet scans alone no longer clear the display.
- Ignore candidates without usable battery data after reconnecting. Disconnects still clear readings, and reconnects require fresh battery data.
- Preserve the detected model name while waiting and use a shorter waiting message that fits the widget.
- Add regression checks for address changes, quiet scans, empty candidates, and disconnect/reconnect recovery.

**Download:** run **AirPodsBatteryDesktop-win-Setup.exe**, or choose **Check for updates** in the installed app and confirm the restart after downloading. Older portable ZIP users need to run the installer once.

Multiple actively broadcasting pairs of the same model can still be ambiguous without identity keys. Battery values remain approximate 10% steps. AirPods Pro 3 is tested; other recognized models remain experimental. This is an unsigned preview installer.
