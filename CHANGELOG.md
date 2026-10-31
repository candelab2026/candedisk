# Changelog

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [semantic versioning](https://semver.org/).

## [1.2.0] - 2026-10-31

### Added
- **Help → Check for updates**, and an automatic check at every start: if a newer release exists the app offers to open its download page (nothing is downloaded or installed by the app).
- Anonymous usage count: once a day the app sends its name, version, "windows" and interface language to candelab.net/stats. No disk or computer data, no identifier. It can be turned off from **File → Send anonymous usage count**. See "Privacy" in the README.

## [1.1.0] - 2026-10-25

### Added
- Interface and in-app guide in nine languages: English, Italiano, Español, Français, Deutsch, Português, Русский, 简体中文 and 日本語.
- **Language** menu: the change is immediate (no restart, no new UAC prompt) and is remembered. The first start uses the Windows language, with English as fallback.
- `--lang <code>` command-line option to start in a given language for one run.
- Translations are plain `Languages/xx.lang` files, so adding a language needs no code. Tests check that every language has all the keys, the same placeholders as English, and a guide.

### Changed
- GitHub pages (README, contributing, security, architecture, templates) are now in English. The guide moved to `docs/guide/xx.md`, one file per language.
- Log lines are always written in English, whatever the interface language.

## [1.0.0] - 2026-10-24

First release.

### Added
- List of disks and partitions with a clickable proportional map and unallocated space.
- Disk initialization (GPT).
- Partition creation (NTFS, exFAT, FAT32 or unformatted).
- Quick or full format.
- Resize (shrink and extend) within the limits supported by Windows.
- Deleting partitions, including protected ones (EFI, OEM, recovery) through `diskpart`.
- Safety rules: the running Windows and the boot partition are blocked, double and triple confirmation, offline or read-only disks.
- Menus, toolbar with icons, context menu and keyboard shortcuts.
- Built-in guide (`F1`) and About window.
- Operation log in `%LocalAppData%\CandeDisk\logs`.
- `--demo` mode with sample data.
- Logo and application icon.
- Automated tests (`dotnet test`) and the `scripts/TestDisk.ps1` script to try the app on a virtual disk.

### Fixed
- Creating a partition no longer shows the Windows "You need to format the disk" and "E:\ is not accessible" windows. The partition is created without a letter, formatted, and only then given its letter; the shell hardware detection is paused meanwhile and always restored.

### Security
- The `diskpart ... delete partition override` fallback only runs for special partitions (EFI, OEM, recovery). For the others a Windows error, such as a volume in use, stops the operation.
- Labels and file systems are validated before building the PowerShell scripts, with dedicated tests against command injection.
