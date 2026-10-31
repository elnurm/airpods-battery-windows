[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ExpectedCommit,[switch]$CheckOnly)
$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$repository='elnurm/airpods-battery-windows'
Push-Location $root
try {
    $head=git rev-parse HEAD
    if($LASTEXITCODE -ne 0 -or $head -ne $ExpectedCommit){throw 'Checkout differs from the prepared release commit.'}
    $changes=git status --porcelain
    if($LASTEXITCODE -ne 0 -or $changes){throw 'Commit working-tree changes and repackage before publishing.'}
    $origin=git remote get-url origin
    if($LASTEXITCODE -ne 0 -or $origin -ne 'https://github.com/elnurm/airpods-battery-windows.git'){throw 'Unexpected Git origin.'}
    $version=([Version](Get-Item build\AirPods.exe).VersionInfo.FileVersion).ToString(3)
    $dist=Join-Path $root "dist\v$version"
    $names=@('AirPodsBatteryDesktop-win-Setup.exe',"AirPodsBatteryDesktop-$version-full.nupkg",'releases.win.json','RELEASES','assets.win.json')
    $hashes=Get-Content (Join-Path $dist 'SHA256SUMS.txt')
    foreach($name in $names){
        $file=Join-Path $dist $name
        if(!(Test-Path -LiteralPath $file -PathType Leaf)){throw "Missing asset: $name"}
        $hash=(Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant()
        if($hashes -notcontains "$hash  $name"){throw "Checksum mismatch: $name"}
    }
    $feed=Get-Content (Join-Path $dist 'releases.win.json') -Raw | ConvertFrom-Json
    $full=@($feed.Assets | Where-Object {$_.Type -eq 'Full' -and $_.Version -eq $version -and $_.PackageId -eq 'AirPodsBatteryDesktop'})
    if($full.Count -ne 1 -or $full[0].FileName -ne "AirPodsBatteryDesktop-$version-full.nupkg"){throw 'Release feed does not match the build.'}
    if($full[0].SHA256 -ine (Get-FileHash (Join-Path $dist $full[0].FileName)).Hash){throw 'Release feed package checksum mismatch.'}
    Write-Output "Verified installer and update feed for v$version at $head"
    if($CheckOnly){Write-Output 'Validation only: nothing pushed or published.';return}
    $command=Get-Command gh -ErrorAction SilentlyContinue
    $gh=if($command){$command.Source}else{'C:\Users\elnur\Documents\Codex\tools\github-cli\bin\gh.exe'}
    if(!(Test-Path $gh)){throw 'GitHub CLI not found.'}
    git push origin HEAD:main
    if($LASTEXITCODE -ne 0){throw 'Push failed; no release created.'}
    $assets=@($names+@('SHA256SUMS.txt') | ForEach-Object {Join-Path $dist $_})
    # Drafts are invisible to update checks until all upload operations finish.
    & $gh release create "v$version" @assets --repo $repository --target $head --title "AirPods Battery for Windows $version" --notes-file (Join-Path $root 'docs\RELEASE_NOTES.md') --prerelease --draft
    if($LASTEXITCODE -ne 0){throw 'Release upload failed. Inspect GitHub drafts; do not overwrite an existing release.'}
    & $gh release edit "v$version" --repo $repository --draft=false
    if($LASTEXITCODE -ne 0){throw 'Assets uploaded, but publishing failed. The release may remain a draft.'}
    & $gh release view "v$version" --repo $repository --json url,isDraft,isPrerelease,assets
    if($LASTEXITCODE -ne 0){throw 'Publishing returned success, but verification failed. Check the Releases page.'}
} finally {Pop-Location}
