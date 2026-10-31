[CmdletBinding()]
param([switch]$IncludeUi)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$exe = Join-Path $projectRoot 'build\AirPods.exe'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$instanceTest = Join-Path $projectRoot 'build\SingleInstanceTests.exe'
& $compiler /nologo /target:exe "/out:$instanceTest" "$projectRoot\src\SingleInstance.cs" "$projectRoot\tests\SingleInstanceTests.cs"
if ($LASTEXITCODE -ne 0) { throw 'Single-instance test compilation failed.' }
& $instanceTest
if ($LASTEXITCODE -ne 0) { throw "Single-instance tests failed (exit $LASTEXITCODE)." }
if (!(Test-Path $exe)) { throw 'Run scripts/build.ps1 first.' }
$modes = @('--test')
if ($IncludeUi) { $modes += '--test-ui' }
foreach ($mode in $modes) {
    $result = Start-Process -FilePath $exe -ArgumentList $mode -Wait -PassThru -WindowStyle Hidden
    if ($result.ExitCode -ne 0) { throw "$mode failed (exit $($result.ExitCode))" }
    Write-Output "$mode passed"
}
