# Architecture

CandeDisk is a WPF application (C#, .NET 9) for Windows. It never writes directly to disk sectors: it delegates every operation to the Windows *Storage* module, exposed through PowerShell, and uses `diskpart` only as a fallback.

## Code layout

```
src/CandeDisk/
  MainWindow.xaml(.cs)    main UI: menus, toolbar, disk map, table, actions
  OptionsDialog.xaml(.cs) single dialog for format / create / resize
  HelpWindow, AboutWindow guide (from docs/guide/xx.md) and About
  DiskService.cs          disk operations (list, format, create, delete, resize)
  PowerShellRunner.cs     runs PowerShell and diskpart, cleans up errors
  Safety.cs               protection rules and label validation
  Models.cs               DiskInfo, PartitionInfo, unallocated-space calculation
  Loc.cs                  translations: loads Languages/*.lang, {l:T Key} markup extension
  Languages/              one .lang file per language (key = text)
  DemoData.cs             sample disks for --demo and screenshots
  MarkdownView.cs         shows the guide (minimal Markdown) in a FlowDocument
  Logger.cs               operation log
  Icons.xaml              stroke-based vector icons
  Assets/                 application logo and icon
```

Outside `src/`: `tests/CandeDisk.Tests` (xUnit tests, they never touch disks), `scripts/TestDisk.ps1` (creates/removes a virtual test disk), `docs/guide/` (the guide, one file per language), `.github/` (build and release workflows, templates).

## Flow of an operation

1. The user selects a partition and picks a command (menu, toolbar, shortcut).
2. `Safety.BlockReason` checks whether the action is forbidden (running Windows, offline disk, ...).
3. A dialog collects the parameters; values are validated (`Safety.IsValidLabel`, file system from a closed list).
4. The app shows one or more confirmations, with the disk name.
5. `DiskService` builds the PowerShell script and runs it with `PowerShellRunner`.
6. The result and details are written to the log; the disk list is read again.

## Design choices

- **PowerShell instead of native APIs.** The Storage module is the one Windows' own tools use; it is documented and stable. The cost is starting a process per call, which is fine for rare operations.
- **`-EncodedCommand`.** Scripts are passed as Base64 UTF-16 to avoid any quoting problem. Variable values are numbers or strings validated against an allow-list of characters, and single quotes are doubled.
- **Error messages.** PowerShell with redirected stderr produces XML (CLIXML): `PowerShellRunner` extracts only the error text and disables the progress bar.
- **Unallocated space is computed in C#.** Windows does not list it: `DiskInfo.BuildRows` derives the free intervals from the partition positions. Windows reserves a few MB more (for example the end of an MBR disk), so when the user asks for almost all the space, `New-Partition` gets `-UseMaximumSize` and decides by itself.
- **Quiet creation.** A new partition is created *without* a drive letter, formatted, and only then given a letter; the shell hardware-detection service is paused meanwhile and always restored. This avoids Windows' "You need to format the disk" and "E:\ is not accessible" pop-ups.
- **Protected deletion.** `Remove-Partition` refuses some partitions (EFI, OEM): only for those (`PartitionInfo.IsSpecial`) does it fall back to `diskpart` with `delete partition override`. For the others an error stops the operation, because "override" would delete anyway, even a volume in use.
- **Elevation.** The Release build embeds a `requireAdministrator` manifest; Debug does not, and shows a banner with a button to restart as administrator.
- **Translations.** Each language is an embedded `Languages/xx.lang` file; XAML uses `{l:T Key}` and code uses `Loc.T` / `Loc.F`. Changing language opens a new main window in the same process, so it needs no restart and no new UAC prompt. The guide is chosen the same way (`docs/guide/xx.md`, embedded as `guide.xx.md`). English is the fallback for any missing key. Logs are always in English.

## Command-line options

| Option | Effect |
|---|---|
| `--demo` | Uses sample data; no operation is performed |
| `--lang <code>` | Starts in that language for this run only (for example `--lang ja`) |
| `--screenshot <file.png>` | Saves a screenshot of the window and exits (used for the docs) |
| `--disk <n>`, `--row <n>` | With `--screenshot`: disk and row to select |
| `--window help\|about` | With `--screenshot`: captures the Guide or About window |

Example to regenerate the README screenshot:

```powershell
dotnet run --project src/CandeDisk -- --demo --lang en --screenshot docs/images/screenshot.png --disk 0 --row 2
```

## Logo and icon

`assets/logo.svg` is the vector logo. The PNG images and `src/CandeDisk/Assets/app.ico` are derived from the same drawing: a ring of coloured partitions (blue, green, amber and grey for free space) around the disk hub.
