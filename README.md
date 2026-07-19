# MultiRoblox

Run **as many Roblox instances as you want** on a single Windows PC — one
button per instance, so several accounts can play together from one machine.

**Org:** [HyperlinksSpace](https://github.com/HyperlinksSpace) · **Site:** https://hyperlinksspace.github.io/MultiRoblox/

Site languages: **EN** · **РУ** · **中文** (switcher at the bottom of the first screen; follows browser language by default).

## How it works

Roblox normally allows only one client per computer by holding two named
kernel objects (`ROBLOX_singletonMutex` and `ROBLOX_singletonEvent`).
MultiRoblox owns those objects itself while its window is open, so every
additional client you launch is allowed to stay alive. Nothing is patched or
modified on disk, and no administrator rights are needed.

## Download

**[MultiRoblox.exe](https://github.com/HyperlinksSpace/MultiRoblox/releases/latest/download/MultiRoblox.exe)** — a single portable .exe. Download, run, press **Launch Instance**.

Prefer Start Menu shortcuts and auto-update? Run the installer from PowerShell:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -Command "iwr -useb https://raw.githubusercontent.com/HyperlinksSpace/MultiRoblox/main/installer/Install-MultiRoblox.ps1 | iex"
```

## Usage

1. Run **MultiRoblox.exe**.
2. Press **Launch Instance** — once for every Roblox window you want open.
3. Sign into a different Roblox account in each window.
4. Keep MultiRoblox open while playing. Click **Stop All** to close every client.

## Requirements

- Windows 10 or 11.
- The Roblox player installed (from roblox.com).

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
