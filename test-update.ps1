$ErrorActionPreference = 'Continue'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

$dir = Join-Path $env:TEMP 'mrtest-update'
if (Test-Path $dir) { Remove-Item $dir -Recurse -Force }
New-Item -ItemType Directory -Path $dir | Out-Null
$exe = Join-Path $dir 'MultiRoblox.exe'
Copy-Item (Join-Path $root 'dist\MultiRobloxController.exe') $exe

$before = (Get-FileHash $exe).Hash
Write-Host "before hash: $before"

$proc = Start-Process $exe -PassThru
Write-Host "started pid $($proc.Id)"

# The updater renames the running exe to .old and puts the new one in place.
$deadline = (Get-Date).AddSeconds(90)
$updated = $false
while ((Get-Date) -lt $deadline) {
    if (Test-Path "$exe.old") { $updated = $true; break }
    Start-Sleep 2
}
Write-Host "swap happened: $updated"

if ($updated) {
    Start-Sleep 2
    $after = (Get-FileHash $exe).Hash
    Write-Host "after hash:  $after"
    Write-Host ("hash changed: {0}" -f ($after -ne $before))
    $len = (Get-Item $exe).Length
    Write-Host "new size: $len"
}

Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
Start-Sleep 1

if ($updated) {
    # Second start should be the downloaded release build (not "dev").
    $v = (Get-Item $exe).VersionInfo.FileVersion
    Write-Host "new file version: $v"
    Write-Host 'RESULT: PASS'
    exit 0
}
Write-Host 'RESULT: FAIL - no update swap within 90s'
exit 1
