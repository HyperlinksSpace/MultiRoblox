# MultiRoblox

Run **two Roblox clients at the same time** on a single Windows PC, so two
accounts can play together from one machine.

**Org:** [HyperlinksSpace](https://github.com/HyperlinksSpace) · **Site:** https://hyperlinksspace.github.io/MultiRoblox/

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

- **Portable app:** [MultiRobloxController.exe](https://github.com/HyperlinksSpace/MultiRoblox/releases/latest/download/MultiRobloxController.exe) — just run it.
- **Installer (auto-update + shortcuts):** [Install-MultiRoblox.cmd](https://github.com/HyperlinksSpace/MultiRoblox/releases/latest/download/Install-MultiRoblox.cmd)

Or from PowerShell:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "iwr -useb https://raw.githubusercontent.com/HyperlinksSpace/MultiRoblox/main/installer/Install-MultiRoblox.ps1 | iex"
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

Every push to `main` triggers `.github/workflows/release.yml`, which:

1. Builds `MultiRobloxController.exe`
2. Packages the installer scripts + zip
3. Publishes a new GitHub Release (`v1.<run>.0`) with those assets

The download site is served by GitHub Pages from the `docs/` folder on `main`.

## Disclaimer

MultiRoblox is an independent tool and is not affiliated with or endorsed by
Roblox Corporation. Use it in accordance with Roblox's Terms of Use.
