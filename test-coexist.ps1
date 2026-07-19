$ErrorActionPreference = "Continue"
$player = (Get-ChildItem "$env:LOCALAPPDATA\Roblox\Versions" -Recurse -Filter RobloxPlayerBeta.exe |
    Where-Object { $_.FullName -notmatch "WindowsApps" } |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName
$cookie = "$env:LOCALAPPDATA\Roblox\LocalStorage\RobloxCookies.dat"
$held = "$cookie.mrhold"

function Count { (Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue).Count }

Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep 3

Write-Host "== launch 1 (saved) =="
Start-Process $player -ArgumentList "-app"
Start-Sleep 22
Write-Host "after 1: $(Count)"

$existing = @{}
Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue | ForEach-Object { $existing[$_.Id] = $true }

Write-Host "== launch 2 (fresh) =="
$moved = $false
if (Test-Path $cookie) { if (Test-Path $held) { Remove-Item $held -Force }; Move-Item $cookie $held -Force; $moved = $true }
Start-Process $player -ArgumentList "-app"
$deadline = (Get-Date).AddSeconds(60); $found = $false
while ((Get-Date) -lt $deadline -and -not $found) {
    foreach ($p in (Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue)) {
        if (-not $existing.ContainsKey($p.Id) -and $p.MainWindowHandle -ne 0 -and $p.MainWindowTitle) { $found = $true; break }
    }
    Start-Sleep 1
}
Write-Host "new window: $found"
Start-Sleep 3
if ($moved) { if (Test-Path $cookie) { Remove-Item $cookie -Force }; Move-Item $held $cookie -Force }

foreach ($t in 5,10,20,35) {
    Start-Sleep 5
    Write-Host "t+${t}s count=$(Count)"
}
Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue | Select-Object Id,StartTime,MainWindowTitle | Format-Table -AutoSize
