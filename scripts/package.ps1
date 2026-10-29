[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
& "$PSScriptRoot\build.ps1"
& "$PSScriptRoot\test.ps1"
$dist = Join-Path $projectRoot 'dist'
New-Item -ItemType Directory -Force $dist | Out-Null
$stage = Join-Path $projectRoot 'build\portable'
New-Item -ItemType Directory -Force $stage | Out-Null
$files = @("$projectRoot\build\AirPods.exe", "$projectRoot\build\AirPods.ico", "$projectRoot\README.md", "$projectRoot\LICENSE", "$projectRoot\NOTICE.md", "$projectRoot\CONTRIBUTING.md")
Copy-Item -LiteralPath $files -Destination $stage -Force
foreach ($directory in @('assets','docs')) {
    New-Item -ItemType Directory -Force (Join-Path $stage $directory) | Out-Null
    Copy-Item -Path "$projectRoot\$directory\*" -Destination (Join-Path $stage $directory) -Force
}
$archive = Join-Path $dist 'airpods-battery-windows-0.1.0-win-x64.zip'
Compress-Archive -Path "$stage\*" -DestinationPath $archive -Force
$hash = (Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText("$archive.sha256", "$hash  $([IO.Path]::GetFileName($archive))`n")
Write-Output $archive
