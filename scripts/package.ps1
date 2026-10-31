[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
& "$PSScriptRoot\build.ps1"
& "$PSScriptRoot\test.ps1"
& "$PSScriptRoot\restore-tools.ps1" -Packaging
$version = ([Version](Get-Item "$projectRoot\build\AirPods.exe").VersionInfo.FileVersion).ToString(3)
$dist = Join-Path $projectRoot "dist\v$version"
$generated = Join-Path $projectRoot ('build\release-'+$version+'-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force $dist | Out-Null
$stage = Join-Path $projectRoot "build\installer-$version"
New-Item -ItemType Directory -Force $stage | Out-Null
$files = @("$projectRoot\build\AirPods.exe", "$projectRoot\build\AirPods.exe.config", "$projectRoot\build\AirPods.ico", "$projectRoot\build\Velopack.dll", "$projectRoot\build\Newtonsoft.Json.dll", "$projectRoot\README.md", "$projectRoot\README.ru.md", "$projectRoot\LICENSE", "$projectRoot\NOTICE.md", "$projectRoot\CONTRIBUTING.md")
Copy-Item -LiteralPath $files -Destination $stage -Force
foreach ($directory in @('assets','docs')) {
    New-Item -ItemType Directory -Force (Join-Path $stage $directory) | Out-Null
    Copy-Item -Path "$projectRoot\$directory\*" -Destination (Join-Path $stage $directory) -Force -Recurse
}
$runtime = Join-Path $projectRoot 'build\dependencies\dotnet\dotnet.exe'
$vpk = Join-Path $projectRoot 'build\dependencies\vpk\tools\net8.0\any\vpk.dll'
$previousTemp = $env:VELOPACK_TEMP
try {
    $env:VELOPACK_TEMP = Join-Path $projectRoot 'build\packaging-temp'
    & $runtime $vpk pack --packId AirPodsBatteryDesktop --packVersion $version --packDir $stage --mainExe AirPods.exe --packTitle 'AirPods Battery for Windows' --packAuthors 'Elnur Mammadov' --icon "$projectRoot\assets\AirPods.ico" --releaseNotes "$projectRoot\docs\RELEASE_NOTES.md" --runtime win-x64 --channel win --framework net48 --shortcuts StartMenuRoot --delta None --noPortable --outputDir $generated
    if ($LASTEXITCODE -ne 0) { throw 'Installer packaging failed.' }
} finally { $env:VELOPACK_TEMP = $previousTemp }
Copy-Item -Path "$generated\*" -Destination $dist -Force
$checksums = @(Get-ChildItem $dist -File | Where-Object { $_.Name -ne 'SHA256SUMS.txt' } | Sort-Object Name | ForEach-Object { (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + $_.Name })
[IO.File]::WriteAllLines((Join-Path $dist 'SHA256SUMS.txt'),$checksums)
Write-Output "Installer and update feed: $dist"
