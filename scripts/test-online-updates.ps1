[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$compiler=Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $compiler /nologo "/out:$root\build\OnlineUpdateTests.exe" "/r:$root\build\Velopack.dll" "/r:$root\build\Newtonsoft.Json.dll" "$root\tests\OnlineUpdateTests.cs"
if($LASTEXITCODE -ne 0){throw 'Online test compilation failed.'}
& "$root\build\OnlineUpdateTests.exe"
if($LASTEXITCODE -ne 0){throw 'Live GitHub update check/download failed.'}
