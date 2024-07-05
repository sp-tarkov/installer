param(
    [string]$source,
    [string]$destination
)

Clear-Host

Write-Host "Stopping installer ... " -ForegroundColor cyan -NoNewLine

$installer = Stop-Process -Name "SPTInstaller" -ErrorAction SilentlyContinue

if ($installer -ne $null)
{
    Write-Warning "Something went wrong, couldn't stop installer process'"
    return;
}

Write-Host "OK" -ForegroundColor green

if (-not(Test-Path $source) -and -not(Test-Path $destination)) {
    Write-Warning "Can't find a required file"
    Write-host ""
    Write-Host "Press [enter] to close ..."
    Read-Host
    exit
}

Write-Host "Copying new installer ... " -ForegroundColor cyan

$maxAttempts = 10
$copied = $false

while (-not $copied) {

    $maxAttempts--
    
    Write-Host "  > Please wait ... " -NoNewLine
    
    if ($maxAttempts -le 0) {
        Write-Host "Couldn't copy new installer :(   Please re-download the installer"
        Write-Host ""
        Write-Host "Press [enter] to close ..."
        Read-Host
        exit
    }
    
    try {
        Remove-Item $destination -ErrorAction SilentlyContinue
        Copy-Item $source $destination -ErrorAction SilentlyContinue
    }
    catch {
        Write-Host "file locked, retrying ..." -ForegroundColor yellow
        sleep(2)
        continue
    }
    
    if (Test-Path $destination) {
        $sLength = (Get-Item $source).Length
        $dLength = (Get-Item $destination).Length
        
        if ($sLength -eq $dLength) {
            $copied = $true
            Write-Host "OK" -ForegroundColor green
            break
        }
        
        Write-Host "sizes differ, retrying ..." -ForegroundColor yellow
        sleep(2)
    }
}

# remove the new installer from the cache folder after it is copied
Remove-Item -Path $source

Start-Process $destination

Write-Host "Done"