<#
    Builds MultiRobloxController.exe from src\MultiRobloxController.cs using the
    .NET Framework C# compiler (csc.exe). This compiler ships with Windows and
    is present on GitHub's windows-latest runners, so local and CI builds match.

    -Version 1.42.0  embeds that assembly version (used by CI releases and the
    in-app updater). Local builds default to 0.0.0, shown in the app as "dev".
#>
param(
    [string]$Version = "0.0.0"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src = Join-Path $root "src\MultiRobloxController.cs"
$outDir = Join-Path $root "dist"
$outExe = Join-Path $outDir "MultiRobloxController.exe"

if (-not (Test-Path -LiteralPath $src)) {
    throw "Source not found: $src"
}

if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Version must look like 1.42.0 (got '$Version')"
}

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

# Version is embedded through a generated AssemblyInfo file.
$assemblyInfo = Join-Path $outDir "AssemblyInfo.g.cs"
@"
using System.Reflection;
[assembly: AssemblyTitle("MultiRoblox")]
[assembly: AssemblyProduct("MultiRoblox")]
[assembly: AssemblyVersion("$Version.0")]
[assembly: AssemblyFileVersion("$Version.0")]
"@ | Set-Content -LiteralPath $assemblyInfo -Encoding UTF8
Write-Host "Version: $Version"

$candidates = @(
    (Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"),
    (Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe")
)
$csc = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $csc) {
    throw "csc.exe not found. Install the .NET Framework developer tools."
}

Write-Host "Using compiler: $csc"

$icon = Join-Path $root "assets\MultiRoblox.ico"
$iconArgs = @()
if (Test-Path -LiteralPath $icon) {
    $iconArgs = @("/win32icon:$icon")
    Write-Host "Embedding icon: $icon"
} else {
    Write-Warning "Icon not found at $icon - building without /win32icon"
}

& $csc `
    /nologo `
    /target:winexe `
    /optimize+ `
    /out:"$outExe" `
    @iconArgs `
    /reference:System.dll `
    /reference:System.Core.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    "$src" `
    "$assemblyInfo"

if ($LASTEXITCODE -ne 0) {
    throw "Compilation failed with exit code $LASTEXITCODE"
}

Remove-Item -LiteralPath $assemblyInfo -Force -ErrorAction SilentlyContinue
Write-Host "Built: $outExe"
