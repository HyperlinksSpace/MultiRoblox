<#
    Builds MultiRobloxController.exe from src\MultiRobloxController.cs using the
    .NET Framework C# compiler (csc.exe). This compiler ships with Windows and
    is present on GitHub's windows-latest runners, so local and CI builds match.
#>
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src = Join-Path $root "src\MultiRobloxController.cs"
$outDir = Join-Path $root "dist"
$outExe = Join-Path $outDir "MultiRobloxController.exe"

if (-not (Test-Path -LiteralPath $src)) {
    throw "Source not found: $src"
}

New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$candidates = @(
    (Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"),
    (Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe")
)
$csc = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $csc) {
    throw "csc.exe not found. Install the .NET Framework developer tools."
}

Write-Host "Using compiler: $csc"
& $csc `
    /nologo `
    /target:winexe `
    /optimize+ `
    /out:"$outExe" `
    /reference:System.dll `
    /reference:System.Core.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    "$src"

if ($LASTEXITCODE -ne 0) {
    throw "Compilation failed with exit code $LASTEXITCODE"
}

Write-Host "Built: $outExe"
