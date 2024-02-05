param(
    [string]$source,
    [string]$destination
)

Clear-Host

Write-Host "Stopping installer ..."

$installer = Stop-Process -Name "SPTInstaller" -ErrorAction SilentlyContinue

if ($installer -ne $null) {
    Write-Host "Something went wrong, couldn't stop installer process'"
    return;
}

Write-Host "Copying new installer ..."

Copy-Item -Path $source -Destination $destination -Force

# remove the new installer from the cache folder after it is copied
Remove-Item -Path $source

Start-Process $destination

Write-Host "Done"