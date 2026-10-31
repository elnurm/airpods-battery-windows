[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
Add-Type -AssemblyName System.Windows.Forms, System.Drawing
[System.Windows.Forms.Application]::EnableVisualStyles()
$assembly = [Reflection.Assembly]::LoadFrom((Join-Path $projectRoot 'build\AirPods.exe'))
$windowType = $assembly.GetType('BatteryWindow')
$privateInstance = [Reflection.BindingFlags]'Instance,NonPublic'
$windowType.GetField('previewMode', [Reflection.BindingFlags]'Static,NonPublic').SetValue($null, $true)
$window = [Activator]::CreateInstance($windowType, [object[]]@($true))
$appIcon = [Drawing.Icon]::new((Join-Path $projectRoot 'assets\AirPods.ico'))
$window.Icon = $appIcon
try {
    $window.Show()
    [System.Windows.Forms.Application]::DoEvents()
    $windowType.GetMethod('SetConnected', $privateInstance).Invoke($window, [object[]]@($true)) | Out-Null
    $reading = [Activator]::CreateInstance($assembly.GetType('Reading'), $true)
    $reading.Address = 1
    $reading.LastSeen = [DateTime]::UtcNow
    $reading.Rssi = -48
    $reading.Level[0] = 80
    $reading.Level[1] = 70
    $reading.Updated[0] = $reading.Updated[1] = [DateTime]::UtcNow
    $picker = $windowType.GetField('picker', $privateInstance).GetValue($window)
    $picker.Items.Add($reading) | Out-Null
    $picker.SelectedIndex = 0
    $windowType.GetMethod('SetSettingsVisible', $privateInstance).Invoke($window, [object[]]@($false)) | Out-Null
    foreach ($russian in @($false, $true)) {
        $windowType.GetMethod('ChangeLanguage', $privateInstance).Invoke($window, [object[]]@($russian, $false)) | Out-Null
        $window.ActiveControl = $null
        [System.Windows.Forms.Application]::DoEvents()
        $bitmap = New-Object System.Drawing.Bitmap($window.Width, $window.Height)
        try {
            $window.DrawToBitmap($bitmap, [Drawing.Rectangle]::new(0, 0, $bitmap.Width, $bitmap.Height))
            $name = if ($russian) { 'screenshot-ru.png' } else { 'screenshot.png' }
            $bitmap.Save((Join-Path $projectRoot "docs\$name"), [Drawing.Imaging.ImageFormat]::Png)
        } finally { $bitmap.Dispose() }
    }
} finally { $window.Dispose(); $appIcon.Dispose() }
