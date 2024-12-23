﻿param (
    [string]$installPath
)

$desktop = Join-Path $env:USERPROFILE "Desktop"

$launcherExe = gci $installPath | where {$_.Name -like "*.Launcher.exe"} | select -ExpandProperty FullName
$serverExe = gci $installPath | where {$_.Name -like "*.Server.exe"} | select -ExpandProperty FullName

$launcherShortcut = Join-Path $desktop "SPT.Launcher.lnk"
$serverShortcut = Join-Path $desktop "SPT.Server.lnk"

$WshShell = New-Object -comObject WScript.Shell

$launcher = $WshShell.CreateShortcut($launcherShortcut)
$launcher.TargetPath = $launcherExe
$launcher.WorkingDirectory = $installPath
$launcher.Save()

$server = $WshShell.CreateShortcut($serverShortcut)
$server.TargetPath = $serverExe
$server.WorkingDirectory = $installPath
$server.Save()