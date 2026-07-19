<#
    MultiRoblox web installer.

    Downloads the latest MultiRobloxController.exe from GitHub Releases,
    installs it to the user's local programs folder, and creates Start Menu
    and Desktop shortcuts. No administrator rights required.

    Uninstall by running:  Install-MultiRoblox.ps1 -Uninstall
#>
[CmdletBinding()]
param(
    [switch]$Uninstall,
    [switch]$NoShortcutLaunch
)

$ErrorActionPreference = "Stop"
$Repo = "HyperlinksSpace/MultiRoblox"
$AppName = "MultiRoblox"
$ExeName = "MultiRobloxController.exe"

$installDir = Join-Path $env:LOCALAPPDATA "Programs\$AppName"
$exePath = Join-Path $installDir $ExeName
$startMenuDir = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs"
$startMenuLink = Join-Path $startMenuDir "$AppName.lnk"
$desktopLink = Join-Path ([Environment]::GetFolderPath("Desktop")) "$AppName.lnk"

function Write-Step([string]$text) {
    Write-Host ""
    Write-Host ">> $text" -ForegroundColor Cyan
}

function New-Shortcut([string]$linkPath, [string]$target) {
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($linkPath)
    $shortcut.TargetPath = $target
    $shortcut.WorkingDirectory = Split-Path -Parent $target
    $shortcut.Description = "Run as many Roblox instances as you want"
    # Multi-resolution icon embedded in the EXE (16..256) — Start Menu, Desktop,
    # and the taskbar each pick the frame that matches their pixel size.
    $shortcut.IconLocation = "$target,0"
    $shortcut.Save()
}

if ($Uninstall) {
    Write-Step "Uninstalling $AppName"
    Get-Process MultiRobloxController -ErrorAction SilentlyContinue |
        Stop-Process -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $startMenuLink -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $desktopLink -Force -ErrorAction SilentlyContinue
    if (Test-Path -LiteralPath $installDir) {
        Remove-Item -LiteralPath $installDir -Recurse -Force
    }
    Write-Host "$AppName removed." -ForegroundColor Green
    return
}

Write-Step "Finding the latest $AppName release"
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$headers = @{ "User-Agent" = "MultiRoblox-Installer"; "Accept" = "application/vnd.github+json" }
$release = Invoke-RestMethod -Uri "https://api.github.com/repos/$Repo/releases/latest" -Headers $headers
$asset = $release.assets | Where-Object { $_.name -eq $ExeName } | Select-Object -First 1
if (-not $asset) {
    throw "Could not find $ExeName in the latest release ($($release.tag_name))."
}
Write-Host "Latest version: $($release.tag_name)"

Write-Step "Downloading $ExeName"
New-Item -ItemType Directory -Force -Path $installDir | Out-Null
Get-Process MultiRobloxController -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue
Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $exePath -Headers $headers
Write-Host "Installed to: $exePath"

Write-Step "Creating shortcuts"
New-Shortcut -linkPath $startMenuLink -target $exePath
New-Shortcut -linkPath $desktopLink -target $exePath
Write-Host "Start Menu and Desktop shortcuts created."

Write-Host ""
Write-Host "$AppName installed successfully!" -ForegroundColor Green
Write-Host "Launch it from the Desktop or Start Menu, then press 'Launch Instance' once per Roblox window you want."

if (-not $NoShortcutLaunch) {
    Start-Process -FilePath $exePath
}
