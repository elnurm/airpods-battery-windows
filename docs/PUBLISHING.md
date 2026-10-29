# First public release

Planned repository: `elnurm/airpods-battery-windows` (public).

Suggested About description:

> Free AirPods battery monitor for Windows 11. Left/right battery levels, system tray icon and disconnect notifications. Tested on AirPods Pro 3; other earbuds experimental.

Suggested topics: `airpods`, `airpods-pro`, `windows`, `windows-11`, `battery-monitor`, `bluetooth`, `bluetooth-low-energy`, `system-tray`, `csharp`, `winforms`.

1. Create an empty public repository under the intended account. Do not initialize another README or license there.
2. Review the local source, MIT license, screenshots, and README. Push the local main branch to `https://github.com/elnurm/airpods-battery-windows.git` using the account's configured credentials.
3. Verify the Windows build workflow, then run the physical-device checks in CONTRIBUTING.md.
4. Create a `v0.1.0` prerelease titled **AirPods Battery for Windows 0.1.0 — first preview**. Attach the ZIP and checksum produced by scripts/package.ps1. Use RELEASE_NOTES.md as the starting description.
5. Add the About description and topics, and enable Issues for compatibility feedback. The README already links to the planned releases and issue forms; these become available once the repository is published.

A descriptive repository name, readable README, topics, screenshots, and an accessible release make the project easier to find. GitHub documents topics at https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/classifying-your-repository-with-topics . A public repository may be indexed by Google, but neither indexing time nor ranking is guaranteed. No keyword stuffing or fake compatibility claims are needed.

The included workflow builds artifacts only. It has read-only repository permissions and does not create a release or publish the project.
