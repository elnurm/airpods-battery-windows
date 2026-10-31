[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
& "$PSScriptRoot\restore-tools.ps1"
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$metadata = Join-Path $env:WINDIR 'System32\WinMetadata'
$output = Join-Path $projectRoot 'build'
New-Item -ItemType Directory -Force $output | Out-Null
$assemblies = @('System.dll','System.Web.Extensions.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Runtime.dll','System.Runtime.InteropServices.WindowsRuntime.dll','System.Runtime.WindowsRuntime.dll','System.ObjectModel.dll')
$references = $assemblies | ForEach-Object { '/r:' + (Join-Path $framework $_) }
$references += @('Windows.Foundation.winmd','Windows.Devices.winmd','Windows.Storage.winmd') | ForEach-Object { '/r:' + (Join-Path $metadata $_) }
$dependencies = @("$projectRoot\build\dependencies\velopack\lib\net472\Velopack.dll", "$projectRoot\build\dependencies\json\lib\net45\Newtonsoft.Json.dll")
$references += $dependencies | ForEach-Object { '/r:' + $_ }
if (!(Test-Path "$framework\csc.exe") -or !(Test-Path "$metadata\Windows.Devices.winmd")) { throw 'Build requires 64-bit Windows with .NET Framework and Windows WinMetadata.' }
$sources = @(Get-ChildItem (Join-Path $projectRoot 'src') -Filter '*.cs' | ForEach-Object FullName)
$icon = Join-Path $projectRoot 'assets\AirPods.ico'
& "$framework\csc.exe" /nologo /target:winexe /platform:x64 /optimize+ "/out:$output\AirPods.exe" "/win32icon:$icon" $references $sources
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed. Close the running build before rebuilding.' }
Copy-Item -LiteralPath $icon -Destination $output
Copy-Item -LiteralPath "$projectRoot\src\AirPods.exe.config" -Destination $output -Force
Copy-Item -LiteralPath $dependencies -Destination $output -Force
Write-Output "Built $output\AirPods.exe"
