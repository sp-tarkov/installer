param(
    [string]$source,
    [string]$destination
)

Clear-Host

Write-Host "Stopping installer ..."

$installer = Stop-Process -Name "SPTInstaller" -ErrorAction SilentlyContinue

if ($installer -ne $null)
{
    Write-Host "Something went wrong, couldn't stop installer process'"
    return;
}

if (-not(Test-Path $source) -and -not(Test-Path $destination)) {
    Write-Warning "Can't find a required file"
    Write-host ""
    Write-Host "Press [enter] to close ..."
    Read-Host
    exit
}

Write-Host "Copying new installer ..."

$maxAttempts = 10
$copied = $false

while (-not $copied) {

    $maxAttempts--
    
    Write-Host "Please wait ..."
    
    if ($maxAttempts -le 0) {
        Write-Host "Couldn't copy new installer :(   Please re-download the installer"
        Write-Host ""
        Write-Host "Press [enter] to close ..."
        Read-Host
        exit
    }
    
    Remove-Item $destination -ErrorAction SilentlyContinue
    Copy-Item $source $destination
    
    if (Test-Path $destination) {
        $sLength = (Get-Item $source).Length
        $dLength = (Get-Item $destination).Length
        
        if ($sLength -eq $dLength) {
            $copied = $true
            break
        }
        
        sleep(2)
    }
}

# remove the new installer from the cache folder after it is copied
Remove-Item -Path $source

Start-Process $destination

Write-Host "Done"