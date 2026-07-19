$ErrorActionPreference = 'Continue'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

function Count-Players {
    @(Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue).Count
}

function Get-SingletonState {
    $mutexAsMutex = $false
    $eventAsEvent = $false
    $eventAsMutex = $false
    try {
        $m = [System.Threading.Mutex]::OpenExisting('ROBLOX_singletonMutex')
        $mutexAsMutex = $true
        $m.Dispose()
    } catch {}
    try {
        $e = [System.Threading.EventWaitHandle]::OpenExisting('ROBLOX_singletonEvent')
        $eventAsEvent = $true
        $e.Dispose()
    } catch {}
    try {
        $m2 = [System.Threading.Mutex]::OpenExisting('ROBLOX_singletonEvent')
        $eventAsMutex = $true
        $m2.Dispose()
    } catch {}
    "mutexAsMutex=$mutexAsMutex eventAsEvent=$eventAsEvent eventAsMutex=$eventAsMutex"
}

Write-Host '=== pre-clean ==='
Get-Process RobloxPlayerBeta, 'MultiRoblox*' -ErrorAction SilentlyContinue |
    ForEach-Object { Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue }
Start-Sleep 2
Write-Host (Get-SingletonState)
Write-Host ("players={0}" -f (Count-Players))

# Hold the same locks MultiRoblox holds
Write-Host '=== acquire guard ==='
$created1 = $false
$created2 = $false
$guard1 = New-Object System.Threading.Mutex($true, 'ROBLOX_singletonMutex', [ref]$created1)
$guard2 = New-Object System.Threading.Mutex($true, 'ROBLOX_singletonEvent', [ref]$created2)
Write-Host ("created mutex={0} eventBlocker={1}" -f $created1, $created2)
Write-Host (Get-SingletonState)

$player = (Get-ChildItem "$env:LOCALAPPDATA\Roblox\Versions" -Recurse -Filter RobloxPlayerBeta.exe |
    Where-Object { $_.FullName -notmatch 'WindowsApps' } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1).FullName
if (-not $player) { throw 'RobloxPlayerBeta.exe not found' }
Write-Host ("player=$player")

$cookie = "$env:LOCALAPPDATA\Roblox\LocalStorage\RobloxCookies.dat"
$held = "$cookie.mrhold"

Write-Host '=== launch 1 (saved account) ==='
Start-Process $player -ArgumentList '-app'
$deadline = (Get-Date).AddSeconds(45)
while ((Get-Date) -lt $deadline -and (Count-Players) -lt 1) { Start-Sleep 1 }
Start-Sleep 8
Write-Host ("after1 players={0}  {1}" -f (Count-Players), (Get-SingletonState))
$pids1 = @(Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
Write-Host ("pids1=$($pids1 -join ',')")

Write-Host '=== launch 2 (fresh login) ==='
$existing = @{}
foreach ($id in $pids1) { $existing[$id] = $true }
$moved = $false
if (Test-Path $cookie) {
    if (Test-Path $held) { Remove-Item $held -Force }
    Move-Item $cookie $held -Force
    $moved = $true
}
Start-Process $player -ArgumentList '-app'

$found = $false
$deadline = (Get-Date).AddSeconds(60)
while ((Get-Date) -lt $deadline -and -not $found) {
    foreach ($p in @(Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue)) {
        if (-not $existing.ContainsKey($p.Id) -and $p.MainWindowHandle -ne [IntPtr]::Zero -and $p.MainWindowTitle) {
            $found = $true
            Write-Host ("new window pid={0} title={1}" -f $p.Id, $p.MainWindowTitle)
            break
        }
    }
    Start-Sleep 1
}
Write-Host ("newWindow=$found")
Start-Sleep 3
if ($moved) {
    if (Test-Path $cookie) { Remove-Item $cookie -Force }
    Move-Item $held $cookie -Force
    Write-Host 'cookie restored'
}

Write-Host '=== watch coexistence ==='
$ok = $true
foreach ($t in 5, 10, 20, 35, 50, 70) {
    Start-Sleep 5
    $n = Count-Players
    $alive1 = @($pids1 | Where-Object {
        Get-Process -Id $_ -ErrorAction SilentlyContinue
    }).Count
    Write-Host ("t+{0}s players={1} firstStillAlive={2}/{3}  {4}" -f $t, $n, $alive1, $pids1.Count, (Get-SingletonState))
    if ($n -lt 2 -or $alive1 -lt $pids1.Count) { $ok = $false }
}

Write-Host '=== final processes ==='
Get-Process RobloxPlayerBeta -ErrorAction SilentlyContinue |
    Select-Object Id, StartTime, MainWindowTitle |
    Format-Table -AutoSize

# Keep guards alive until script ends; dispose after reporting
$guard1.Dispose()
$guard2.Dispose()

if ($ok) {
    Write-Host 'RESULT: PASS - both clients stayed open'
    exit 0
} else {
    Write-Host 'RESULT: FAIL - first client quit or count dropped'
    exit 1
}
