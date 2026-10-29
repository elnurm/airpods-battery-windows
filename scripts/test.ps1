[CmdletBinding()]
param([switch]$IncludeUi)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$exe = Join-Path $projectRoot 'build\AirPods.exe'
if (!(Test-Path $exe)) { throw 'Run scripts/build.ps1 first.' }
$modes = @('--test')
if ($IncludeUi) { $modes += '--test-ui' }
foreach ($mode in $modes) {
    $result = Start-Process -FilePath $exe -ArgumentList $mode -Wait -PassThru -WindowStyle Hidden
    if ($result.ExitCode -ne 0) { throw "$mode failed (exit $($result.ExitCode))" }
    Write-Output "$mode passed"
}
