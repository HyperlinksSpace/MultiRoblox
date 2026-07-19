$ErrorActionPreference = "Continue"
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$player = (Get-ChildItem "$env:LOCALAPPDATA\Roblox\Versions" -Recurse -Filter RobloxPlayerBeta.exe |
    Where-Object { $_.FullName -notmatch "WindowsApps" } |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName
$cookie = "$env:LOCALAPPDATA\Roblox\LocalStorage\RobloxCookies.dat"
$held = "$cookie.mrhold"

function Shot($name) {
    $b = [System.Windows.Forms.SystemInformation]::VirtualScreen
    $bmp = New-Object System.Drawing.Bitmap $b.Width, $b.Height
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.CopyFromScreen($b.X, $b.Y, 0, 0, $bmp.Size)
    $bmp.Save("C:\1\1\1\1\1\MultiRoblox\$name", [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
}

# Clean slate
Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep 3

# Instance 1: saved account
Write-Host "Launching instance 1 (saved account)..."
Start-Process $player -ArgumentList "-app"
Start-Sleep 20

$existing = @{}
Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue | ForEach-Object { $existing[$_.Id] = $true }

# Instance 2: fresh login — mirror the new controller logic
Write-Host "Launching instance 2 (fresh login)..."
$moved = $false
if (Test-Path $cookie) {
    if (Test-Path $held) { Remove-Item $held -Force }
    Move-Item $cookie $held -Force
    $moved = $true
    Write-Host "cookie moved aside"
}
Start-Process $player -ArgumentList "-app"

# Wait for the NEW window to appear (read is done by then), THEN restore
$deadline = (Get-Date).AddSeconds(60)
$found = $false
while ((Get-Date) -lt $deadline -and -not $found) {
    foreach ($p in (Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue)) {
        if (-not $existing.ContainsKey($p.Id) -and $p.MainWindowHandle -ne 0 -and $p.MainWindowTitle) {
            $found = $true; break
        }
    }
    Start-Sleep 1
}
Write-Host "new window appeared: $found"
Start-Sleep 3
if ($moved) {
    if (Test-Path $cookie) { Remove-Item $cookie -Force }
    Move-Item $held $cookie -Force
    Write-Host "cookie restored"
}

Start-Sleep 8
Shot "shot-fresh.png"
Write-Host "Instances: $((Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue).Count)"
