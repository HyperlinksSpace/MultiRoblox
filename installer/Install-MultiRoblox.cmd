@echo off
REM MultiRoblox one-click web installer.
REM Downloads the installer script and the latest release, then installs.
setlocal
echo Installing MultiRoblox...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12; $u='https://raw.githubusercontent.com/HyperlinksSpace/MultiRoblox/main/installer/Install-MultiRoblox.ps1'; $o=Join-Path $env:TEMP 'Install-MultiRoblox.ps1'; Invoke-WebRequest -UseBasicParsing -Uri $u -OutFile $o; & $o"
echo.
pause
endlocal
