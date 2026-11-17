<div align="center">

<img src="docs/icon.png" alt="Winmizer" width="128" height="128">

# Winmizer

**English** | [Polski](README.pl.md)

**A console Windows 10/11 optimizer for gamers: safe, fully reversible, in English and Polish.**

[![CI](https://github.com/Qbiterv/winmizer/actions/workflows/ci.yml/badge.svg)](https://github.com/Qbiterv/winmizer/actions/workflows/ci.yml)
[![Windows 10/11](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?logo=windows&logoColor=white)](#windows-10-and-11-compatibility)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Latest release](https://img.shields.io/github/v/release/Qbiterv/winmizer?label=release&color=cyan)](https://github.com/Qbiterv/winmizer/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/Qbiterv/winmizer/total?label=downloads&color=blueviolet)](https://github.com/Qbiterv/winmizer/releases)

[**Download the latest release**](https://github.com/Qbiterv/winmizer/releases/latest) · [Installation](#installation) · [Safety](#safety) · [FAQ](#faq)

</div>

---

## About

**Winmizer** is a colorful console app (Spectre.Console) that improves Windows gaming performance and removes unnecessary bloat. Every tweak has a description and a risk level, a system restore point is created before any change, and the original values are saved, so **everything can be undone**.

> The interface is in English by default and also available in Polish (see [Language](#language)); code and comments are in English. Requires Windows 10 1809 (build 17763) or newer (x64, ARM64 or x86) and administrator rights to change the system.

## Features

- **Quick optimize** - the recommended safe set in one click, including removal of the recommended bloatware (Cortana, Copilot, Bing apps, consumer Teams, promo apps; you confirm the list first).
- **Custom selection** - categories first, then individual tweaks (Safe and Moderate preselected, Advanced unchecked).
- **Gaming** - Game Mode, HAGS, MMCSS, mouse acceleration and Game DVR turned off.
- **Power** - Ultimate Performance / High performance plan, USB suspend and PCIe power saving turned off.
- **Privacy & telemetry** - less telemetry, ads, Cortana, Copilot and Bing search in Start.
- **Services, network, appearance, storage, cleanup** - over 40 additional settings (list below).
- **App debloat and startup apps** - removes preinstalled Appx apps and disables startup entries, with restore.
- **System info** - CPU, GPU, RAM, power plan and the status of every tweak.
- **Undo / restore** - undo everything or selected changes from `backup.json`.
- **CLI mode** - `--list`, `--dry-run`, `--quick`, `--apply`, `--revert-all` for automation.
- **English and Polish** - English by default; switch with `--lang` or the menu.

After applying changes the program shows a summary (applied / skipped / errors, space freed) and offers a restart.

## Safety

- A **system restore point** is created before every batch of changes. System protection is enabled on C:, and the 24 h limit is temporarily lifted (`SystemRestorePointCreationFrequency=0`) and then restored. If that fails, the program asks whether to continue.
- A **backup of the originals** of every tweak goes to `%ProgramData%\Winmizer\backup.json` (registry values, service start types, task states, power plans, DNS...). Only the *first* original is stored, so re-applying never overwrites the true state.
- **Log** of actions: `%ProgramData%\Winmizer\logs\winmizer-*.log`.
- Every tweak runs inside `try/catch` - one failure doesn't stop the rest. Tweaks detect whether they are already applied and are then skipped.
- **`--dry-run`** shows what would happen without changing anything in the system.

### What Winmizer never touches

| Area | What stays untouched |
|------|----------------------|
| **Printing** | Spooler, PrintNotify, printer drivers, "Print to PDF" (only the *Fax* service is disabled) |
| **Network** | Wi-Fi, Bluetooth, DHCP, DNS Client (protected services; optionally only a DNS server change, with a backup) |
| **Windows Defender** | stays on (`WinDefend`, `Sense`, `SecurityHealthService`) |
| **Microsoft Store** | Store and App Installer/winget stay (needed to reinstall apps) |
| **Xbox** | Xbox identity and Gaming Services stay (Game Pass games); optionally only the Game Bar overlay |
| **Windows Update** | updates work normally (we only disable P2P sharing - Delivery Optimization) |
| **Everything else** | audio, USB, drivers, Windows Hello, UAC, Spectre/Meltdown mitigations |

The code **refuses to change protected services** and **doesn't let you add protected apps** (Store, winget, Calculator, Photos, Notepad, Terminal, Paint, Snipping Tool, Camera, Xbox/Gaming Services, SecHealthUI, codecs) to the removal list.

### What can't be undone automatically

- **Removed Appx apps** - restore them from the Microsoft Store or with `winget install` (the program also tries to re-register them if the files still exist).
- **Cleanup actions** (temp files, Recycle Bin, caches) - these delete data.
- **OneDrive uninstall** - undo it by reinstalling. Files that exist only in the cloud are not deleted, but local sync stops.

## Windows 10 and 11 compatibility

The system is detected by build number (>= 22000 = Windows 11); version, edition and build are shown in the banner. Tweaks that are unavailable on a given system are hidden in the menu, skipped in batch mode and marked in `--list` as **N/A on this system** - they never end in an error.

| Tweak | Windows 11 | Windows 10 |
|-------|:----------:|:----------:|
| Copilot policy, Widgets (`privacy.copilot`, `visual.widgets`) | yes | N/A |
| Appx: Clipchamp, Teams, Copilot, Bing Search | yes | N/A |
| Cortana: lock screen and consent (`privacy.cortana.win10`) | N/A | yes |
| News and interests (`privacy.newsfeed`, build 19041+) | N/A | yes |
| People bar, Meet Now (`visual.peoplebar`, `visual.meetnow`) | N/A | yes |
| Appx: 3D Viewer, Paint 3D, Print 3D | N/A | yes |
| HAGS (`gaming.hags`) | yes (WDDM 2.7+ GPU) | from 2004 (19041) and a WDDM 2.7+ GPU |
| Windowed-game optimizations / VRR (`gaming.vrr`) | yes | from 1903 (18362), partial |
| Power plans, services, network, cleanup | yes | yes |

## Installation

### Recommended: MSI installer

1. Go to [**Releases**](https://github.com/Qbiterv/winmizer/releases/latest) and download the MSI that matches your PC:
   - `Winmizer-1.0.0-x64.msi` - Windows 10/11 64-bit (Intel/AMD, the most common choice),
   - `Winmizer-1.0.0-arm64.msi` - Windows on ARM,
   - `Winmizer-1.0.0-x86.msi` - Windows 10 32-bit.
2. Run the downloaded file and click through the installer.
3. Winmizer installs to `C:\Program Files\Winmizer` and creates shortcuts on the **desktop** and in the **Start menu** with the program icon. The shortcut asks for administrator rights (UAC).

The installer appears in *Apps & features* (publisher: Qbiterv) and uninstalls cleanly (removes the program and shortcuts; backups and logs in `%ProgramData%\Winmizer` stay). Newer versions upgrade the installed one in place.

Silent install, skipping the desktop shortcut:

```powershell
msiexec /i Winmizer-1.0.0-x64.msi /qb INSTALLDESKTOPSHORTCUT=0
```

### Portable version (zip)

Download `Winmizer-1.0.0-win-x64.zip` (or `win-arm64` / `win-x86`) from Releases, extract it and run `Winmizer.exe`. Nothing needs installing, not even .NET (the app is self-contained).

### Build from source

Requirements: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (`winget install Microsoft.DotNet.SDK.8`). The MSI installers are built with WiX Toolset v4 (NuGet package `WixToolset.Sdk 4.0.5`, downloaded automatically).

```powershell
git clone https://github.com/Qbiterv/winmizer.git
cd winmizer

dotnet build                       # plain build
.\build.ps1                        # exe + zip + MSI for x64, arm64, x86 -> publish\
.\build.ps1 -Runtime win-x64       # a single runtime only
.\build.ps1 -NoInstaller           # skip building the MSI
```

`build.ps1` publishes a single-file self-contained Release build for each RID and writes to `publish\`:

| File | For |
|------|-----|
| `Winmizer-<version>-<arch>.msi` | installer (x64, arm64, x86) |
| `Winmizer-<version>-win-<arch>.zip` | portable version |
| `win-<arch>\Winmizer.exe` | ready-to-run executable |

### Releases

Releases are built by GitHub Actions (`.github/workflows/release.yml`) when a `v*` tag is pushed. The version comes from the tag (without the `v`) and ends up in the exe, zips and MSI.

```powershell
git tag v1.0.1
git push origin v1.0.1
```

The workflow builds the exe, zips and MSI for x64, arm64 and x86, generates `SHA256SUMS.txt` and publishes everything as a GitHub Release with automatic notes. Locally the same: `.\build.ps1 -Version 1.0.1`. A regular build (CI) runs for every push to `main` and every pull request: `.github/workflows/ci.yml`.

### Alternative for developers: `install.ps1`

An MSI-free script: copies the exe matching your PC's architecture to `%LOCALAPPDATA%\Programs\Winmizer` (no admin rights needed) and creates shortcuts for the current user. It doesn't launch anything.

```powershell
.\build.ps1 -NoInstaller    # build first
.\install.ps1               # install
.\install.ps1 -Uninstall    # remove shortcuts and the copy
```

> Don't mix the two methods: if you have the MSI version, don't also install `install.ps1` (you would get two shortcuts with the same name).

## Usage

Start **Winmizer** from the shortcut (or `Winmizer.exe` as administrator). Main menu:

1. **Quick optimize** - the recommended safe set; also uninstalls the recommended bloatware after you confirm the list (and offers to restart Explorer so taskbar changes show).
2. **Custom selection** - categories and individual tweaks.
3. **App debloat and startup apps**.
4. **Power plans** - Winmizer Ultimate, High performance, back to Balanced.
5. **System info / status**.
6. **Undo / restore** - also a shortcut to System Restore.
7. **Language / Język** - switch between English and Polish.
8. **Exit**.

### Language

English is the default; Winmizer never switches language based on your system settings. Polish is available in three ways, checked in this order:

1. the `--lang pl` (or `--lang en`) command-line flag, for a single run,
2. the `WINMIZER_LANG` environment variable (`pl` or `en`),
3. the choice saved with the **Language / Język** menu item, stored in `%ProgramData%\Winmizer\settings.json` (a missing or corrupt file is ignored; `--dry-run` never writes it).

### Command-line options

| Option | What it does |
|--------|--------------|
| *(none)* | interactive menu (needs administrator; the program asks for UAC itself) |
| `--list` | list all tweaks and their detected status (changes nothing, no admin needed) |
| `--list-md` | tweak tables in Markdown |
| `--dry-run` | menu in preview mode - no changes to the system |
| `--dry-run --quick` | preview the Quick optimize set |
| `--dry-run --apply id1,id2` / `all` | preview selected tweaks |
| `--quick --yes` | apply Quick optimize without prompts (needs admin) |
| `--apply id1,id2 --yes` | apply selected tweaks without prompts |
| `--revert-all --yes` | undo everything from `backup.json` |
| `--lang en` / `--lang pl` | UI language for this run (default: English) |
| `--help` | help |

Non-interactive modes (`--quick`, `--apply`, `--revert-all`) require `--dry-run` or `--yes`.

## Tweak list

Risk levels: **Safe**, **Moderate**, **Advanced** (unchecked by default). The *Quick* column marks tweaks included in Quick optimize (on laptops, settings that drain the battery are skipped). Quick now also removes the recommended bloatware (the *App debloat* rows marked in the Quick column: Cortana, Copilot, Bing News/Weather, Get Help, Tips, Solitaire, Feedback Hub, Clipchamp, consumer Teams, Power Automate, promo apps...). Optional apps (Phone Link, Zune, Alarms, Maps, Xbox Game Bar) and OneDrive stay out of Quick; protected apps (Store, winget, Calculator, Photos, Xbox identity...) are never removed. With `--yes` the confirmation is skipped and Explorer is restarted automatically when a tweak needs it.

<details>
<summary><b>Gaming (10)</b></summary>

| ID | Tweak | Risk | Quick | Description |
|----|-------|--------|:-----:|------|
| `gaming.gamemode` | Enable Game Mode | Safe | yes | Windows prioritizes the game and limits background tasks (Windows Update won't install drivers while you play). |
| `gaming.hags` | Hardware-accelerated GPU scheduling (HAGS) | Moderate |  | Sets HwSchMode=2. Can reduce latency and CPU load; needs a supported GPU and driver. Requires a restart. **Warning:** On older GPUs/drivers it can cause artifacts - use Revert if you run into problems. *[Needs Windows 10 2004+ (build 19041), a WDDM 2.7+ GPU and a recent driver; otherwise the setting is ignored.]* |
| `gaming.gamedvr` | Disable Game DVR / background recording | Safe | yes | Turns off Xbox Game Bar background recording (GameDVR_Enabled=0, AllowGameDVR=0). Game Bar itself and the Xbox services stay. |
| `gaming.fso` | Disable fullscreen optimizations globally | Advanced |  | Sets the global FSE behavior (GameDVR_FSEBehaviorMode=2). Some games run smoother in true fullscreen, others worse (alt-tab). |
| `gaming.mmcss` | MMCSS: system responsiveness and network | Safe | yes | SystemResponsiveness=10 (not 0 - leaves headroom for audio/system) and NetworkThrottlingIndex=0xFFFFFFFF (disables multimedia network throttling). |
| `gaming.mmcss.games` | MMCSS: "Games" task profile | Safe | yes | GPU Priority=8, Priority=6, Scheduling Category=High, SFIO Priority=High for game processes. |
| `gaming.prioritysep` | Win32PrioritySeparation = 0x26 | Moderate |  | Gives the foreground application (the game) a larger share of CPU time compared to background processes. |
| `gaming.mouse` | Disable mouse acceleration | Safe | yes | Turns off "Enhance pointer precision" (MouseSpeed/Threshold = 0) - constant, predictable sensitivity in FPS games. |
| `gaming.stickykeys` | Disable Sticky/Filter/Toggle Keys prompts | Safe | yes | Turns off the shortcuts that open accessibility dialogs (5x Shift etc.), which can minimize a game. |
| `gaming.vrr` | Windowed-game optimizations + VRR | Safe | yes | Enables SwapEffectUpgradeEnable and VRROptimizeEnable (DirectX) - lower latency in windowed games and variable refresh rate. |

</details>

<details>
<summary><b>Power (7)</b></summary>

| ID | Tweak | Risk | Quick | Description |
|----|-------|--------|:-----:|------|
| `power.ultimate` | Power plan: Winmizer Ultimate (Ultimate Performance) | Moderate | yes | Creates and activates the Ultimate Performance plan (lowest latency). Your original plan is remembered, so you can switch back. **Warning:** Laptops: this plan drains the battery much faster - recommended on AC power only. |
| `power.highperf` | Power plan: High performance | Safe |  | Activates the built-in High performance plan (a lighter alternative to Ultimate). You can switch back to Balanced. **Warning:** Laptops: shorter battery life. |
| `power.usbsuspend` | Disable USB selective suspend | Moderate | yes | Stops USB ports from going to sleep (fewer mouse/keyboard/gamepad stutters). On laptops, AC power only. |
| `power.pcie` | Disable PCIe link power saving | Moderate | yes | Link State Power Management = Off - lower GPU/NVMe latency. On laptops, AC only. |
| `power.procmin` | Minimum processor state 100% (AC) | Moderate | yes | The CPU never drops its clock speed on AC power - steadier FPS, higher idle power draw and temperature. |
| `power.faststartup` | Disable Fast Startup | Moderate |  | Full shutdown on "Shut down" - clean driver state and fewer problems after updates. Boot may be slightly slower. |
| `power.hibernate` | Disable hibernation (frees disk space) | Advanced |  | powercfg -h off deletes hiberfil.sys (several to a dozen or so GB). Also disables Fast Startup and hibernation. **Warning:** Laptops: you lose the ability to hibernate. |

</details>

<details>
<summary><b>Privacy & telemetry (13)</b></summary>

| ID | Tweak | Risk | Quick | Description |
|----|-------|--------|:-----:|------|
| `privacy.telemetry` | Telemetry: basic level | Safe | yes | AllowTelemetry=1 (Basic/Required). Value 0 only works on Enterprise, so it isn't used. Turn off the DiagTrack services in the Services category. |
| `privacy.adid` | Disable advertising ID | Safe | yes | Apps can no longer use the Advertising ID to profile you for ads. |
| `privacy.tailored` | Disable tailored experiences | Safe | yes | Microsoft doesn't use diagnostic data to personalize tips and ads. |
| `privacy.activity` | Disable activity history | Safe | yes | Stops collecting and uploading activity history (Timeline). |
| `privacy.consumer` | Disable consumer features and suggested apps | Safe | yes | DisableWindowsConsumerFeatures=1 - Windows no longer installs promoted apps automatically (Candy Crush etc.). |
| `privacy.startads` | Disable ads and suggestions in the Start menu | Safe | yes | Removes suggested apps/ads in Start and Settings, plus recommended items. |
| `privacy.lockscreen` | Disable tips and ads on the lock screen | Safe | yes | A lock screen without fun facts, Spotlight ads and tips. |
| `privacy.tasks` | Disable telemetry tasks | Safe | yes | Disables the scheduled tasks Compatibility Appraiser, ProgramDataUpdater, Consolidator, UsbCeip and DmClient. Windows Update is not affected. |
| `privacy.cortana` | Disable and remove Cortana (policy + app) | Safe | yes | Turns Cortana off via the AllowCortana=0 policy and uninstalls the Cortana app (Microsoft.549981C3F5F10). Windows Search keeps working normally. Counts as applied only when the app is gone too. |
| `privacy.copilot` | Disable and remove Windows Copilot (policy, taskbar button, app) | Safe | yes | TurnOffWindowsCopilot=1 (user and machine policy), ShowCopilotButton=0 (taskbar button) and removal of the Copilot app packages. Restart Explorer for the taskbar button to disappear. *[Windows 11 only]* |
| `privacy.cortana.win10` | Windows 10: restrict Cortana (lock screen, consent) | Safe | yes | On Windows 10 Cortana is a separate component: this blocks it on the lock screen (AllowCortanaAboveLock=0) and withdraws user consent (CortanaConsent=0). Complements the AllowCortana=0 policy. *[Windows 10 only]* |
| `privacy.newsfeed` | Windows 10: disable "News and interests" on the taskbar | Safe | yes | The EnableFeeds=0 policy removes the weather/news widget from the taskbar (MSN ads and tracking, RAM used by Edge WebView). *[Windows 10 only]* |
| `privacy.websearch` | Disable web search (Bing) in Start | Safe | yes | Search in Start shows local results only (BingSearchEnabled=0, DisableSearchBoxSuggestions=1). |

</details>

<details>
<summary><b>Services (10)</b></summary>

| ID | Tweak | Risk | Quick | Description |
|----|-------|--------|:-----:|------|
| `services.diagtrack` | Telemetry: Connected User Experiences and Telemetry (DiagTrack) | Safe | yes | The main service that sends diagnostic data to Microsoft. Nothing in the system depends on it. |
| `services.dmwappushservice` | Telemetry: WAP Push Message Routing (dmwappushservice) | Safe | yes | Helper service for telemetry/MDM. Not needed on a home PC. |
| `services.fax` | Fax (Fax) | Safe | yes | The FAX service - unrelated to printing. Printers and "Print to PDF" work as before. |
| `services.retaildemo` | Retail demo mode (RetailDemo) | Safe | yes | Only used on store display units. |
| `services.mapsbroker` | Downloaded maps (MapsBroker) -> manual | Safe | yes | Offline map updates; started only when needed. |
| `services.wersvc` | Windows Error Reporting (WerSvc) -> manual | Safe | yes | Sends crash reports to Microsoft; starts on demand. |
| `services.remoteregistry` | Remote Registry (RemoteRegistry) | Safe | yes | Remote registry editing over the network - a security risk, unnecessary at home. (Already disabled by default on Windows 11.) |
| `services.sysmain` | SysMain (Superfetch) -> disabled | Moderate |  | Preloads apps into RAM. Pointless on an SSD/NVMe and causes extra writes; on an HDD it may slow app launches. |
| `services.lfsvc` | Geolocation service (lfsvc) | Moderate |  | Turns off the location service - apps (Maps, Weather) can't determine your position. |
| `services.wsearch` | Windows Search (WSearch) -> manual | Advanced |  | File indexing. Manual start = slower search in Explorer/Start, less disk load. |

</details>

<details>
<summary><b>Network (3)</b></summary>

| ID | Tweak | Risk | Quick | Description |
|----|-------|--------|:-----:|------|
| `network.nagle` | Disable Nagle's algorithm (lower ping) | Moderate |  | TcpAckFrequency=1 and TCPNoDelay=1 on every interface - game packets are sent immediately. The effect on ping is small; doesn't touch Wi-Fi or drivers. |
| `network.dop2p` | Delivery Optimization: disable P2P sharing | Safe | yes | DODownloadMode=0 - updates are downloaded only from Microsoft servers (and not uploaded to other PCs). Windows Update works normally. |
| `network.dns` | Cloudflare DNS (1.1.1.1 / 1.0.0.1) | Advanced |  | Sets a fast public DNS on active network adapters. Previous settings are remembered. **Warning:** Changes your DNS provider - on corporate or parental-filtered networks it may block something. |

</details>

<details>
<summary><b>Appearance & UI (8)</b></summary>

| ID | Tweak | Risk | Quick | Description |
|----|-------|--------|:-----:|------|
| `visual.effects` | Visual effects tuned for performance (keeps fonts and thumbnails) | Safe | yes | Turns off window and taskbar animations plus transparent shadows/selection. Font smoothing (ClearType) and thumbnails stay on. |
| `visual.transparency` | Disable transparency effects | Safe |  | Turns off acrylic/transparency in Start and windows - slightly less GPU load. |
| `visual.startupdelay` | Remove the startup-app launch delay | Safe | yes | Serialize\StartupDelayInMSec=0 - startup programs launch right after sign-in. |
| `visual.menudelay` | No menu delay (MenuShowDelay=0) | Safe | yes | Drop-down menus appear instantly. |
| `visual.bgapps` | Block background apps | Moderate |  | Policy LetAppsRunInBackground=2 - Store apps don't run in the background (saves RAM/CPU). Notifications from such apps may stop arriving. |
| `visual.widgets` | Windows 11: disable Widgets (policy) | Safe |  | Policy AllowNewsAndInterests=0 turns off the Widgets panel (weather, news, ads) and the Widgets.exe background process. *[Windows 11 only]* |
| `visual.peoplebar` | Windows 10: hide the People bar | Safe | yes | Removes the People icon from the taskbar (PeopleBand=0, HidePeopleBar=1). This element doesn't exist on Windows 11. *[Windows 10 only]* |
| `visual.meetnow` | Windows 10: hide the Meet Now icon | Safe | yes | Hides the Meet Now (Skype) button in the taskbar tray (HideSCAMeetNow=1). *[Windows 10 only]* |

</details>

<details>
<summary><b>Storage (2)</b></summary>

| ID | Tweak | Risk | Quick | Description |
|----|-------|--------|:-----:|------|
| `storage.trim` | Make sure TRIM is enabled | Safe | yes | fsutil behavior set DisableDeleteNotify 0 - the OS tells the SSD which blocks are free (performance and lifespan). |
| `storage.lastaccess` | Disable NTFS last-access timestamps | Safe | yes | fsutil behavior set disablelastaccess 1 - fewer disk writes every time a file is read. |

</details>

<details>
<summary><b>Cleanup (4)</b></summary>

| ID | Tweak | Risk | Quick | Description |
|----|-------|--------|:-----:|------|
| `cleanup.temp` | Clean temporary files (%TEMP% and Windows\Temp) | Safe | yes | Deletes user and system temporary files. Files in use by running programs are skipped. Prefetch is NOT cleaned. |
| `cleanup.shader` | Clear shader caches (DirectX/NVIDIA/AMD) | Moderate |  | Deletes D3DSCache, NVIDIA DXCache/GLCache and AMD DxCache. Games recompile shaders from scratch - the first launch may stutter a little. |
| `cleanup.recyclebin` | Empty the Recycle Bin | Moderate |  | Permanently deletes everything in the Recycle Bin. **Warning:** This cannot be undone - check the Recycle Bin contents first. |
| `cleanup.dism` | DISM: component cleanup (StartComponentCleanup) | Advanced |  | DISM /Online /Cleanup-Image /StartComponentCleanup - removes old versions of system components. Safe, but takes several to a dozen or so minutes. |

</details>

<details>
<summary><b>App debloat (38)</b></summary>

| ID | Tweak | Risk | Quick | Description |
|----|-------|--------|:-----:|------|
| `debloat.microsoft.549981c3f5f10` | Cortana | Safe | yes | The old Cortana voice assistant. On Windows 10 this removes only the app; the system component is limited by a Privacy policy. |
| `debloat.microsoft.bingnews` | Microsoft News (Bing News) | Safe | yes | News app. |
| `debloat.microsoft.bingweather` | Weather (Bing Weather) | Safe | yes | MSN weather app. |
| `debloat.microsoft.bingfinance` | Finance (Bing Finance) | Safe | yes | MSN finance app. |
| `debloat.microsoft.bingsports` | Sports (Bing Sports) | Safe | yes | MSN sports app. |
| `debloat.microsoft.bingsearch` | Bing Search (Start add-on) | Safe | yes | Bing search integration. *[Windows 11 only]* |
| `debloat.microsoft.gethelp` | Get Help | Safe | yes | Support app. |
| `debloat.microsoft.getstarted` | Tips | Safe | yes | Windows tips app. |
| `debloat.microsoft.microsoftofficehub` | Microsoft 365 (Office Hub) | Safe | yes | Advertising shortcut to the Office suite. |
| `debloat.microsoft.microsoftsolitairecollection` | Microsoft Solitaire Collection | Safe | yes | Card games with ads. |
| `debloat.microsoft.people` | People | Safe | yes | Address book. |
| `debloat.microsoft.windowsfeedbackhub` | Feedback Hub | Safe | yes | Sends feedback to Microsoft. |
| `debloat.microsoft.windowsmaps` | Windows Maps | Moderate |  | Maps app. |
| `debloat.clipchamp.clipchamp` | Clipchamp | Safe | yes | Microsoft video editor. *[Windows 11 only]* |
| `debloat.microsoft.todos` | Microsoft To Do | Safe | yes | To-do list. |
| `debloat.microsoft.powerautomatedesktop` | Power Automate | Safe | yes | Task automation. |
| `debloat.microsoftteams` | Microsoft Teams (consumer) | Safe | yes | Built-in Teams (Chat). *[Windows 11 only]* |
| `debloat.msteams` | Microsoft Teams (new) | Safe | yes | The new Teams version preinstalled with the system. *[Windows 11 only]* |
| `debloat.microsoft.copilot` | Microsoft Copilot | Safe | yes | The Copilot app. *[Windows 11 only]* |
| `debloat.microsoft.windows.ai.copilot.provider` | Windows Copilot provider | Safe | yes | Copilot component. *[Windows 11 only]* |
| `debloat.microsoftwindows.client.copilot` | Windows Copilot (system package) | Safe | yes | The Copilot package of Windows 11 (taskbar button / sidebar). *[Windows 11 only]* |
| `debloat.microsoft.skypeapp` | Skype | Safe | yes | Skype messenger. |
| `debloat.microsoft.mixedreality.portal` | Mixed Reality Portal | Safe | yes | For Windows MR VR headsets. |
| `debloat.microsoft.microsoft3dviewer` | 3D Viewer | Safe | yes | Windows 10: 3D model viewer. *[Windows 10 only]* |
| `debloat.microsoft.mspaint` | Paint 3D | Safe | yes | Windows 10: Paint 3D (not the classic Paint - that one stays). *[Windows 10 only]* |
| `debloat.microsoft.print3d` | Print 3D | Safe | yes | Windows 10: 3D printing app. *[Windows 10 only]* |
| `debloat.disney` | Disney+ | Safe | yes | Preinstalled promotional app. |
| `debloat.spotifyab.spotifymusic` | Spotify | Safe | yes | Preinstalled promotional app (you can reinstall it). |
| `debloat.tiktok` | TikTok | Safe | yes | Preinstalled promotional app. |
| `debloat.candycrush` | Candy Crush and similar games | Safe | yes | Promotional games (Candy Crush Saga/Soda etc.). |
| `debloat.facebook` | Facebook | Safe | yes | Preinstalled promotional app. |
| `debloat.instagram` | Instagram | Safe | yes | Preinstalled promotional app. |
| `debloat.twitter` | Twitter / X | Safe | yes | Preinstalled promotional app. |
| `debloat.microsoft.zunemusic` | Media Player (Groove/ZuneMusic) | Moderate |  | On Windows 11 this is the new Media Player - remove it only if you use another player. |
| `debloat.microsoft.zunevideo` | Movies & TV (ZuneVideo) | Moderate |  | Movie app. |
| `debloat.microsoft.yourphone` | Phone Link | Moderate |  | Remove it if you don't connect your phone to Windows. |
| `debloat.microsoft.windowsalarms` | Clock & Alarms | Moderate |  | Alarms, timers, stopwatch. |
| `debloat.microsoft.xboxgamingoverlay` | Xbox Game Bar (overlay) | Moderate |  | The Win+G overlay. Xbox identity and Gaming Services stay (needed by Game Pass games). **Warning:** Game Pass games keep working, but you lose the Win+G overlay. |

</details>

<details>
<summary><b>OneDrive (4)</b></summary>

| ID | Tweak | Risk | Quick | Description |
|----|-------|--------|:-----:|------|
| `onedrive.policy` | OneDrive: disable sync via policy (DisableFileSyncNGSC) | Moderate |  | A policy that blocks OneDrive file sync from running. Local files are left untouched. **Warning:** Cloud sync will stop. Online-only files are not deleted. |
| `onedrive.navpane` | OneDrive: remove from the File Explorer navigation pane | Safe |  | Hides the OneDrive icon in the left pane of File Explorer. |
| `onedrive.startup` | OneDrive: remove from startup | Safe |  | Removes the Run entries that launch OneDrive at sign-in (the backup lets you restore them). |
| `onedrive.uninstall` | OneDrive: full uninstall | Moderate |  | Closes the OneDrive process and runs the official uninstaller (OneDriveSetup.exe /uninstall, winget as a fallback). **Warning:** Local sync will stop working. Files that exist ONLY in the cloud are not deleted, but become unavailable from this PC until you reinstall. Make sure your files are downloaded locally first. |

</details>

## Startup apps

Registry entries (`HKCU/HKLM ...\Run`, including 32-bit) are moved to `HKxx\SOFTWARE\Winmizer\DisabledStartup\...`, and shortcuts from the Startup folders to `%ProgramData%\Winmizer\DisabledStartup`. The originals are recorded in `backup.json`, so *Undo / restore* puts them back.

## FAQ

<details>
<summary><b>How do I undo the changes?</b></summary>

Start Winmizer and choose **Undo / restore** (everything or selected changes from `backup.json`), or from the command line: `Winmizer.exe --revert-all --yes`. As a last resort use the restore point created before the changes (*System Restore* in Windows).
</details>

<details>
<summary><b>SmartScreen warns about an unknown publisher. Is it safe?</b></summary>

`Winmizer.exe` and the MSI are not signed with a certificate (a cost open source projects usually can't afford), so Windows SmartScreen may show "Windows protected your PC". Click **More info**, then **Run anyway**. The source code is in this repository and you can build the program yourself (`build.ps1`).
</details>

<details>
<summary><b>I removed an app with Winmizer. How do I get it back?</b></summary>

Reinstall it from the **Microsoft Store** or with `winget install <name>`. The Store and winget are never removed.
</details>

<details>
<summary><b>Does Winmizer disable Windows Defender, Windows Update or printing?</b></summary>

No. Defender, Windows Update (apart from P2P sharing), printers, networking and the Xbox services are protected and the code refuses to change them.
</details>

<details>
<summary><b>Why does the program ask for administrator rights?</b></summary>

Changes to the registry (HKLM), services, power plans and the restore point need admin rights. The `--list` and `--dry-run` modes work without UAC.
</details>

<details>
<summary><b>Will it increase my FPS?</b></summary>

It depends on your hardware. Some effects (HAGS, Nagle, priorities) can be small; the biggest difference is usually in system responsiveness, fewer background processes and steadier frame times. Everything is reversible, so you can simply try.
</details>

<details>
<summary><b>How do I switch the language?</b></summary>

Use **Language / Język** in the main menu (the choice is saved), or run `Winmizer.exe --lang pl` for a single session. English is the default.
</details>

## Known limitations

- Detecting Appx apps without administrator rights (e.g. `--list`) only covers the current user.
- The Ultimate Performance plan is unavailable on some configurations - use the High performance plan instead.
- HAGS, Nagle and other hardware-related settings have different effects depending on the hardware; all of them are reversible.

## Architecture

```
Winmizer.sln
build.ps1                  publish exe + zip + MSI
install.ps1                per-user install (alternative to the MSI)
installer/                 WiX v4: Winmizer.Installer.wixproj, Package.wxs
tools/make-icon.ps1        icon generator
src/Winmizer/
  Program.cs               entry point, CLI options
  Core/                    TweakBase, TweakEngine, BackupStore (backup.json), Restorer,
                           Reg/Proc, StartupManager
  Core/Localization/       L (English-keyed lookup), PolishStrings, settings.json
  Tweaks/<Category>/       Gaming, Power, Privacy, Services, Network, Visual, Storage, Cleanup, Debloat, OneDrive
  Tweaks/TweakCatalog.cs   list of all tweaks
  UI/                      Spectre.Console: menus and rendering
```

Texts are written in English in the code and act as keys; `PolishStrings.cs` holds the Polish translations. `Winmizer.exe --lang-check` lists tweak texts that lack a translation.

The icon (`src/Winmizer/Assets/winmizer.ico`, preview `docs/icon.png`) is generated by the `tools/make-icon.ps1` script.

## Disclaimer

Winmizer changes system settings. Although it creates a restore point and a copy of the originals, the software is provided **"as is"**, without any warranty; you use it at your own risk. It is not a Microsoft product and is not affiliated with Microsoft. We recommend making your own backup of important data before use.

## License

[MIT](LICENSE) © 2026 Qbiterv

---

<p align="center">
  <a href="https://claude.com/claude-code">
    <img src="https://img.shields.io/badge/This%20project%20is-Claude%20Coded-D97757?style=for-the-badge&logo=claude&logoColor=white" alt="This project is Claude Coded">
  </a>
</p>
