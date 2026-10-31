# 0.3.1 — Translation roadmap and update-delivery test

This small release adds a note about planned interface languages to the English and Russian README files. No additional translations are included yet. Battery behavior and updater logic are unchanged.

It also provides a real newer package for testing the in-app update flow from 0.3.0 to 0.3.1. This prerelease is offered to all installed users, not just private testers.

## Update from 0.3.0

1. Keep your installed 0.3.0 copy running; do not install this setup manually if you want to test the updater.
2. Right-click its tray icon and select **Check for updates** / **Проверить обновления**.
3. Accept the download, then confirm **Restart and update** / **Перезапустить и обновить**.
4. After restart, use **Check for updates** again. It should report version **0.3.1** as current. Check that your language preference is preserved.

New users can download **AirPodsBatteryDesktop-win-Setup.exe**. Users of ZIP versions through 0.2.5 must install manually once; those versions cannot detect installer-only releases.

The installer remains unsigned. The additional .nupkg and feed assets are required by the updater and are not manual downloads for users.
