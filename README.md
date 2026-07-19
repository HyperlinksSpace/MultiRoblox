# MultiRoblox

Run **two Roblox clients at the same time** on a single Windows PC, so two
accounts can play together from one machine.

Website & downloads: **https://anriltine.github.io/MultiRoblox/**

## How it works

Roblox normally allows only one client per computer by holding two named
kernel objects (`ROBLOX_singletonMutex` and `ROBLOX_singletonEvent`).
MultiRoblox owns those objects itself while the controller window is open,
so additional clients are allowed to launch. Nothing is patched or modified
on disk, and no administrator rights are needed.

The reliable pairing is the **classic desktop player** (from roblox.com)
together with the **Microsoft Store Roblox app**. The controller launches one
of each.

## Download

- **Portable app:** [MultiRobloxController.exe](https://github.com/anriltine/MultiRoblox/releases/latest/download/MultiRobloxController.exe) &mdash; just run it.
- **Installer (auto-update + shortcuts):** [Install-MultiRoblox.cmd](https://github.com/anriltine/MultiRoblox/releases/latest/download/Install-MultiRoblox.cmd)

Or from PowerShell:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "iwr -useb https://raw.githubusercontent.com/anriltine/MultiRoblox/main/installer/Install-MultiRoblox.ps1 | iex"
```

## Usage

1. Run **MultiRobloxController.exe**.
2. Click **Launch 2 Clients**.
3. Sign into a different Roblox account in each window.
4. Keep the controller open while playing. Click **Stop All Clients** to close them.

## Requirements

- Windows 10 or 11.
- Both the classic Roblox player and the Microsoft Store Roblox app installed.

## Build from source

```powershell
./build.ps1   # produces dist/MultiRobloxController.exe
```

Uses the .NET Framework `csc.exe` that ships with Windows, so no extra SDK is
required.

## Releases & CI

- `.github/workflows/release.yml` builds the controller and publishes a
  GitHub Release whenever a `v*` tag is pushed (or via manual dispatch).
- `.github/workflows/pages.yml` deploys the `docs/` site to GitHub Pages.

## Disclaimer

MultiRoblox is an independent tool and is not affiliated with or endorsed by
Roblox Corporation. Use it in accordance with Roblox's Terms of Use.
