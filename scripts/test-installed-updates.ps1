[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$version=([Version](Get-Item "$root\build\AirPods.exe").VersionInfo.FileVersion).ToString(3)
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $compiler /nologo "/out:$root\build\InstalledUpdateTests.exe" "/r:$root\build\Velopack.dll" "/r:$root\build\Newtonsoft.Json.dll" "$root\tests\InstalledUpdateTests.cs"
if($LASTEXITCODE -ne 0){throw 'Update test compilation failed.'}
& "$root\build\InstalledUpdateTests.exe" "$root\dist\v$version" (Join-Path $root ('build\update-tests-'+[Guid]::NewGuid().ToString('N'))) $version
if($LASTEXITCODE -ne 0){throw 'Installed update tests failed.'}
