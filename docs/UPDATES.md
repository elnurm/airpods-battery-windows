# Installer and update releases

The application uses Velopack 1.2.161 (net472) and Newtonsoft.Json 13.0.4. Downloaded build dependencies are pinned by cryptographic hash in `scripts/restore-tools.ps1`. Users do not need the build tooling.

## Publish a release

1. Update `src/AssemblyInfo.cs`, the changelog and release notes. Use an increasing three-part version, for example `0.3.1`.
2. Exit the running local build. Run `scripts/package.ps1` and `scripts/test.ps1 -IncludeUi`.
3. Run `scripts/test-installed-updates.ps1` to exercise the generated feed and package locally.
4. Commit and push. Publish **all files** from `dist/v<version>/` together in one GitHub release tagged `v<version>`. Create a draft first; publish it only after every asset is uploaded. Do not rename the generated artifacts.

The release must include `AirPodsBatteryDesktop-win-Setup.exe`, the full `.nupkg`, `releases.win.json`, `RELEASES`, `assets.win.json`, and `SHA256SUMS.txt`. Only the setup EXE is a user-facing download. Do not upload old portable ZIPs or mix artifacts from different builds. Existing releases are left intact.

The app uses the fixed public repository `elnurm/airpods-battery-windows`, channel `win`, and includes GitHub prereleases. No token is embedded. Publishing a prerelease therefore makes it available to existing installed users. Publishing an unrelated release without its update feed may make checks fail until a complete installer release is published.

## Update behavior

Startup/daily checks do not install anything. Users confirm downloading and then restarting. Velopack validates the downloaded package hash, stages it in its package cache, and uses its separate updater to wait for the app to exit, apply the package and restart. The app disables Velopack's default automatic application of pending updates at startup. If restart is deferred, manually checking again offers the downloaded update. Download cancellation or failure does not replace the running executable.

The installer uses app ID `AirPodsBatteryDesktop`, separate from the existing settings directory `%LOCALAPPDATA%\AirPodsBattery`. Language and notification preferences therefore survive migration and updates. The setup installs per user with a Start menu shortcut; Windows Apps provides uninstall. The portable ZIP is discontinued from 0.3.0. Users of 0.2.5 and earlier must install once manually because their checker requires a ZIP release asset.

## Trust and limits

The source is fixed to GitHub over HTTPS. Package SHA-256 checksums detect corruption against the downloaded feed; they are not independent publisher signatures. The release is unsigned. Protect repository write access. Public code signing can be added later through Velopack signing options; a certificate or signing service is not configured here. Never describe the current release as signed or SmartScreen-approved.

Local automated tests cover feed selection, successful package download, same-version/downgrade refusal, and corrupted-package rejection. Before public rollout, also verify a real installation, update/restart and uninstall in a Windows test account. GitHub-hosted delivery of a newer version can only be verified once that release is published.
