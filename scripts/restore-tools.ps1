[CmdletBinding()]
param([switch]$Packaging)
$ErrorActionPreference = 'Stop'
$cache = Join-Path (Split-Path $PSScriptRoot -Parent) 'build\dependencies'
New-Item -ItemType Directory -Force $cache | Out-Null
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
function Restore-Pinned($Name, $Url, $Hash, $Marker, $Algorithm = 'SHA256') {
    $archive = Join-Path $cache "$Name.zip"
    $destination = Join-Path $cache $Name
    if (!(Test-Path $archive)) { Invoke-WebRequest $Url -OutFile $archive -UseBasicParsing }
    if ((Get-FileHash $archive -Algorithm $Algorithm).Hash -ine $Hash) { throw "Dependency checksum mismatch: $Name. Remove the cached ZIP and retry." }
    if (!(Test-Path (Join-Path $destination $Marker))) { Expand-Archive $archive -DestinationPath $destination -Force }
}
Restore-Pinned 'velopack' 'https://api.nuget.org/v3-flatcontainer/velopack/1.2.161/velopack.1.2.161.nupkg' 'D57275692B299751A12EE3E18283B5F7FE3E55339F96B1623345DA095DC484ED' 'lib\net472\Velopack.dll'
Restore-Pinned 'json' 'https://api.nuget.org/v3-flatcontainer/newtonsoft.json/13.0.4/newtonsoft.json.13.0.4.nupkg' 'F09081D457405BAF35A973FA0C50D6BF272ED683F2568C5A620A49DA952F6529' 'lib\net45\Newtonsoft.Json.dll'
if ($Packaging) {
    Restore-Pinned 'vpk' 'https://api.nuget.org/v3-flatcontainer/vpk/1.2.161/vpk.1.2.161.nupkg' 'AC9BE73846D9A7A76483F8684ABD1503CD50703B992727429DCDC045589C5E08' 'tools\net8.0\any\vpk.dll'
    Restore-Pinned 'dotnet' 'https://builds.dotnet.microsoft.com/dotnet/Runtime/8.0.33/dotnet-runtime-8.0.33-win-x64.zip' 'CEB56FA91C15AA7C465DB8CD42EE4E978589C58885CD1F164F8FCDE7231EA15C3CFC0BCDDB2E6B452238B3687A3E2BCB3683349B8B92A7172E03154ED280BE67' 'dotnet.exe' 'SHA512'
}
