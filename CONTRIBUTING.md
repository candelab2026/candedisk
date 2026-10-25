# Contributing to CandeDisk

Thanks for your interest! Reports, ideas and pull requests are welcome.

## Reporting a problem

Open an [issue](https://github.com/candelab2026/candedisk/issues/new/choose) and include:

- the CandeDisk version (menu **Help > About**) and your Windows version;
- what you were doing (operation, disk type, file system);
- the full error message;
- the **disk summary** (menu **File > Copy disk summary**) and, if useful, the log file (**Help > Open logs folder**).

Check first that the log or the summary contain no personal information.

For security vulnerabilities do **not** open a public issue: read [SECURITY.md](SECURITY.md).

## Proposing a change

1. Fork the repository and create a dedicated branch (`feature/name` or `fix/name`).
2. Build and test your change (see below).
3. Open a pull request explaining **what** changes and **why**.

For large changes, or ones that touch disk operations, open an issue first to discuss them.

## Development environment

You need the [.NET 9 SDK](https://dotnet.microsoft.com/download) on Windows.

```powershell
dotnet build src/CandeDisk
dotnet run --project src/CandeDisk -- --demo
```

`--demo` uses sample data and never modifies a disk: it is the safest way to work on the interface.

### Trying disk operations

**Never try destructive operations on your real disks.** Use a virtual disk (VHDX), from a PowerShell started as administrator:

```powershell
# create and attach an empty 2 GB virtual disk (it appears in CandeDisk as a RAW disk)
.\scripts\TestDisk.ps1 -Action Create -SizeMB 2048

# ... try CandeDisk (Release build, or Debug started as administrator) ...

# when you are done: detach and delete the virtual disk
.\scripts\TestDisk.ps1 -Action Remove
```

The script only touches the file `%TEMP%\CandeDiskTest.vhdx`. Before opening a pull request that changes the operations, try them on the virtual disk: initialize, create, format, resize (shrink and extend) and delete.

### Automated tests

```powershell
dotnet test
```

The tests never modify a disk: they cover the unallocated-space calculation, the `Safety` rules, input validation, the generated PowerShell scripts, error-message cleanup and the translation files.

## Conventions

- C# with `nullable` enabled, in a style consistent with the existing code (see `.editorconfig`).
- User-facing text lives in the translation files (`src/CandeDisk/Languages/*.lang`), never hard-coded in the C# or XAML. Log lines are in English.
- Comments explain the *why*, not the *what*.
- Every new disk operation must: go through the `Safety` checks, ask for confirmation, write to the log and **not** accept unvalidated input in PowerShell scripts.
- Add or update the tests in `tests/CandeDisk.Tests` for the logic you change.
- Update `docs/guide/en.md` and `CHANGELOG.md` if the change is visible to users.

## Translations

The interface and the guide are translated into English, Italian, Spanish, French, German, Portuguese, Russian, Simplified Chinese and Japanese. To add a language (say Dutch, code `nl`):

1. Copy `src/CandeDisk/Languages/en.lang` to `nl.lang` and translate the values. Keep the keys, the `{0}` placeholders and the `\n` line breaks; a `_` before a letter in a menu text marks its keyboard accelerator.
2. Set `@Name` (the language name in that language) and `@Culture` (for example `nl-NL`, used for number formatting).
3. Copy `docs/guide/en.md` to `docs/guide/nl.md` and translate it.
4. Run `dotnet test`: it checks that your file has every key, the same placeholders as English, and a guide. No C# change is needed; the language shows up in the **Language** menu by itself.

To improve an existing translation, edit its `.lang` file and its guide, and open a pull request. Native speakers' corrections are especially welcome.

## Code of conduct

By participating you agree to the [Code of Conduct](CODE_OF_CONDUCT.md).
