# Compatibility and technical details

[← Back to README](../README.md)

## Compatibility

| Model | Status |
| --- | --- |
| AirPods Pro 3 | Battery reception tested on one Windows 11 PC |
| AirPods 1, 2, 3 | Experimental; hardware reports needed |
| AirPods 4 / AirPods 4 with ANC | Experimental; hardware reports needed |
| AirPods Pro / AirPods Pro 2 / Pro 2 USB-C | Experimental; hardware reports needed |
| AirPods Max, Beats, other headphones | Not supported by this version |

Requires **Windows 11 x64**, .NET Framework 4.8 or later, and a Bluetooth adapter/driver that supports BLE scanning. Windows 10, ARM64, and other operating systems have not been validated. Model recognition does not guarantee that every firmware or adapter combination works.

Using an experimental model? [Report whether it works](https://github.com/elnurm/airpods-battery-windows/issues/new?template=compatibility.yml). Include the model, firmware if known, Windows version, and whether each earbud and disconnect notification worked.

## What to expect

**Battery precision:** this app reads the unencrypted Bluetooth Low Energy proximity broadcast. Values are approximate **10% steps**, identified as approximate in the footer (and with `~` in the tray). It does not extract encryption keys or provide exact 1% readings. Case battery is intentionally omitted.

**Connection vs. proximity:** a nearby AirPods broadcast does not mean the headphones are connected to Windows. The app separately checks the selected paired Windows device's connection state. A disconnect clears the stored percentages and bars; broadcasts received while disconnected cannot restore them. Windows may delay reporting the change, and Do Not Disturb or notification preferences may suppress the popup.

**Automatic discovery:** AirPods rotate their BLE address. The app looks for a single fresh battery signal compatible with the selected Windows device name, observes it for five seconds, and remembers its model for the session. Discovery considers only signals seen within the last 15 seconds that contain usable battery readings. Older addresses remain in the display cache but do not block selection of a fresh source. A quiet scan alone does not clear the current reading; the normal stale/expiry rules still apply. If multiple matching signals are nearby, it clears the automatic selection and waits instead of asking you to choose a broadcast. Changing the Windows device resets the remembered model. BLE signals cannot be authenticated as belonging to the Windows device; even a sole matching signal can belong to another pair.



**Old readings:** while Windows still reports a connection, a missing signal means missing battery data, not a disconnect. Readings older than 60 seconds are muted and labeled Last known. Reading details remain available to screen readers. After two minutes without a supported battery signal, the entry expires and the display clears while automatic discovery continues.

## Troubleshooting

| Symptom | Try this |
| --- | --- |
| No paired device listed | Pair in Windows first, then click Rescan. |
| Disconnected even with earbuds nearby | Connect the selected device to this PC; proximity is not a connection. |
| No battery broadcasts | Open the case near the PC, check Bluetooth, and click Rescan. |
| Updates stop | Check the reading age, keep your AirPods near the PC, and allow automatic discovery to find their new signal. |
| No popup | Check Windows notification preferences; the disconnected state still appears in the app. |
| Experimental model does not work | Submit a compatibility report; avoid posting serial numbers or Bluetooth addresses. |

## Build from source

On a Windows 11 x64 machine, open PowerShell in the repository root:

```powershell
./scripts/build.ps1
./scripts/test.ps1 -IncludeUi
./scripts/package.ps1
```

The build uses the Windows .NET Framework compiler and system WinRT metadata. The first build downloads pinned, checksum-verified Velopack and Newtonsoft.Json dependencies. Packaging also downloads a private .NET runtime and Velopack CLI into `build/dependencies`; no machine-wide SDK install is needed. The executable is written to `build/AirPods.exe`; packaging produces the installer, full update package, feed metadata and `SHA256SUMS.txt` under `dist/v<version>/`. Close a running build before rebuilding it. See [release publishing](UPDATES.md).

GitHub Actions is configured to build, run decoder/connection tests, and produce downloadable build artifacts. It does not publish releases automatically. Hardware Bluetooth tests require a real PC and AirPods; hosted CI cannot establish device compatibility.

## Privacy

The installed app checks this project's public GitHub Releases API at startup and once every 24 hours while running, including preview releases with a Windows update feed. **Check for updates** also checks manually. A notification opens the in-app update flow. Downloading and restarting require your confirmation. Automatic notifications appear once per new version. Windows notification settings may suppress them; **GitHub releases** opens the manual installer download page.

Update checks and downloads contact GitHub over HTTPS. GitHub receives your IP address and request metadata, but no Bluetooth addresses, device names or battery data. No authentication token is used. The last notified version is saved in `%LOCALAPPDATA%\AirPodsBattery\notified-version.txt` to avoid repeating notifications after restarting. Offline or failed automatic checks stay quiet and retry at the next daily check. Velopack verifies package checksums before installation. Trust is based on HTTPS and control of this GitHub repository; packages do not yet have independent publisher signatures or Authenticode signing.

Bluetooth advertisements are read locally. Battery values and device selections are held in memory. The optional `--diagnose` mode writes packet counts and errors beside the executable and does not check for updates; it does not intentionally log raw packets or addresses. Error details may contain local paths, so review logs before sharing them.

Your chosen interface language is stored in `%LOCALAPPDATA%\AirPodsBattery\language.txt`. Device names and model names remain unchanged when switching languages.

## Contributing and license

More interface languages are planned. Translation suggestions and contributions are welcome; English and Russian are the currently supported languages.

See [CONTRIBUTING.md](../CONTRIBUTING.md) for bug reports, compatibility feedback, and development notes. The project is released under the [MIT license](../LICENSE). Protocol references and trademark information are in [NOTICE.md](../NOTICE.md).
