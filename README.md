<p align="center">
  <img src="assets/logo.png" alt="CandeDisk logo" width="128" />
</p>

<h1 align="center">CandeDisk</h1>

<p align="center">
  A disk and partition manager for Windows: format, create, delete and resize partitions with a simple interface.
</p>

<p align="center">
  <a href="https://github.com/candelab2026/candedisk/actions/workflows/build.yml"><img alt="Build" src="https://github.com/candelab2026/candedisk/actions/workflows/build.yml/badge.svg" /></a>
  <a href="LICENSE"><img alt="MIT License" src="https://img.shields.io/badge/license-MIT-blue.svg" /></a>
  <img alt="Platform" src="https://img.shields.io/badge/platform-Windows%2011-0078D6.svg" />
  <img alt=".NET 9" src="https://img.shields.io/badge/.NET-9-512BD4.svg" />
</p>

![CandeDisk screenshot](docs/images/screenshot.png)

> The screenshot uses **demo mode** with sample data, not real disks.

## Why CandeDisk

Windows *Disk Management* works, but it sometimes gets in the way: "Delete Volume" greyed out on some partitions, "Extend" disabled without explanation, unclear errors. CandeDisk uses the same Windows storage functions, but:

- shows **all disks, partitions and unallocated space** in one view, with a colour-coded map;
- can delete **protected partitions** (EFI, OEM, recovery) through `diskpart`, after explicit confirmations;
- explains errors in readable language and keeps an **operation log**;
- **protects** you from the worst mistakes (it won't touch the running Windows or the boot partition).

## Features

| Feature | Description |
|---|---|
| Disk list | Disks, partitions, file systems, free space, status (Windows, EFI, active, hidden) |
| Disk map | Clickable proportional bar, colour-coded by partition type |
| Initialize | Prepare a new disk with a GPT partition table |
| Create | New partition in unallocated space, with file system and label |
| Format | NTFS, exFAT, FAT32; quick or full format |
| Resize | Shrink or extend a partition within the real limits reported by Windows |
| Delete | Including protected partitions, with double/triple confirmation |
| Guide and log | Built-in guide (`F1`), operation log, copy the summary to the clipboard |
| Languages | English, Italiano, Español, Français, Deutsch, Português, Русский, 简体中文, 日本語 |

## Languages

CandeDisk starts in your Windows language (English if yours is not available) and you can switch at any time from the **Language** menu: the choice is immediate and remembered. The in-app guide is translated too.

| Language | Guide |
|---|---|
| English | [docs/guide/en.md](docs/guide/en.md) |
| Italiano | [docs/guide/it.md](docs/guide/it.md) |
| Español | [docs/guide/es.md](docs/guide/es.md) |
| Français | [docs/guide/fr.md](docs/guide/fr.md) |
| Deutsch | [docs/guide/de.md](docs/guide/de.md) |
| Português | [docs/guide/pt.md](docs/guide/pt.md) |
| Русский | [docs/guide/ru.md](docs/guide/ru.md) |
| 简体中文 | [docs/guide/zh.md](docs/guide/zh.md) |
| 日本語 | [docs/guide/ja.md](docs/guide/ja.md) |

Want to add or improve a translation? See [CONTRIBUTING.md](CONTRIBUTING.md#translations).

## Requirements

- Windows 11 (64-bit), where it has been tested. Windows 10 should work because it uses the same storage functions, but it is **not verified**.
- **Administrator** rights to change disks (without them the program runs in read-only mode).

## Installation

1. Download the latest version from the [**Releases**](https://github.com/candelab2026/candedisk/releases) page.
2. Extract the zip and run `CandeDisk.exe`: Windows will ask for administrator confirmation (UAC).

The release executable includes the .NET runtime, so you don't need to install anything else.

> **Windows SmartScreen** may show a "Windows protected your PC" warning the first time, because the program is not code-signed and is new. Click **More info** > **Run anyway**, and check the SHA-256 checksum published with each release first.

To build it yourself, see [Build from source](#build-from-source).

## Quick start

1. Select a disk on the left.
2. Select a partition (or an **Unallocated** row) in the table or in the map.
3. Use the toolbar, the menus or the shortcuts:

| Shortcut | Action |
|---|---|
| `F5` | Refresh |
| `Ctrl+N` | Create partition |
| `Ctrl+Shift+F` | Format |
| `Ctrl+R` | Resize |
| `Del` | Delete (with the focus on the table) |
| `F1` | Guide |

Full guide: [docs/guide/en.md](docs/guide/en.md).

## Safety

Disk operations **permanently erase data**. Always back up first.

- The running Windows partition, the system drive and the EFI partition of the boot disk cannot be formatted or deleted.
- Every destructive operation needs a double confirmation showing the disk name, and a third one for special partitions.
- Offline or read-only disks are never modified.
- The software is provided **without any warranty** (see [LICENSE](LICENSE)). Before using it on important data, try it on a virtual disk or a spare USB drive.

To report a vulnerability, read [SECURITY.md](SECURITY.md).

## Demo mode

To try the interface without touching any disk:

```powershell
CandeDisk.exe --demo
```

Other options: `--lang <code>` (for example `--lang de`) starts in a given language for that run only.

## Build from source

Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download).

```powershell
git clone https://github.com/candelab2026/candedisk.git
cd candedisk
dotnet build src/CandeDisk -c Release
dotnet run --project src/CandeDisk
```

A **Debug** build starts without asking for administrator rights (read-only, with a button to restart with permissions). A **Release** build asks for UAC at startup.

Automated tests (they never touch a disk):

```powershell
dotnet test
```

To try real operations use a virtual disk, not your own disks: see [CONTRIBUTING.md](CONTRIBUTING.md) and `scripts/TestDisk.ps1`.

To produce the standalone executable:

```powershell
dotnet publish src/CandeDisk -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist
```

The resulting executable is about 60 MB because it includes the .NET runtime.

## How it works

CandeDisk is a WPF application (C#, .NET 9) that drives the Windows *Storage* module through PowerShell (`Get-Disk`, `Get-Partition`, `New-Partition`, `Resize-Partition`, `Format-Volume`, `Remove-Partition`) and, as a fallback for protected partitions, `diskpart`. It never writes directly to disk sectors. Details in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Known limitations and future ideas

- **Moving** partitions (to merge non-adjacent space) is not supported yet: it is the riskiest operation.
- Dynamic disks and software RAID are not handled.
- Ideas: partition cloning, SMART status, dark theme.

See the [CHANGELOG](CHANGELOG.md) and the [issues](https://github.com/candelab2026/candedisk/issues).

## Privacy

CandeDisk works offline and has no account. Once a day, at start-up, it sends one anonymous count to `https://candelab.net/stats` (the project's statistics portal) to know how many people use which version: app name, version, `windows` and the interface language. It never sends disk, partition, file or computer information, and no identifier. The hosting provider may keep technical server logs (including IP addresses) for a limited time, as every web server does. You can turn it off any time from **File → Send anonymous usage count**, or by writing `stats=off` in `%LOCALAPPDATA%\CandeDisk\settings.ini`. The code is in `src/CandeDisk/UsageStats.cs`.

## Contributing

Contributions, reports and ideas are welcome. Read [CONTRIBUTING.md](CONTRIBUTING.md) and the [Code of Conduct](CODE_OF_CONDUCT.md).

## License

Released under the [MIT](LICENSE) license. Copyright © 2026 CandeLab.
