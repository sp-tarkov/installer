param(
    [string]$source,
    [string]$destination
)

clear

Write-Host "Stopping installer ..."

$installer = Stop-Process -Name "SPTInstaller" -ErrorAction SilentlyContinue

if ($installer -ne $null) {
    Write-Host "Something went wrong, couldn't stop installer process'"
    return;
}

Write-Host "Copying new installer ..."

Import-Module BitsTransfer

Start-BitsTransfer -Source $source -Destination $destination -DisplayName "Updating" -Description "Copying new installer"

Remove-Module -Name BitsTransfer

Start-Process $destination

Write-Host "Done"