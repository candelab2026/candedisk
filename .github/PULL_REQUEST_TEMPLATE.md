## What changes

<!-- Describe the change and the reason. Link the issue, if any: "Closes #123". -->

## How I tested it

<!-- For example: `--demo` for the interface, a virtual disk (VHDX) for disk operations. -->

## Checklist

- [ ] `dotnet build` and `dotnet test` finish with no errors or warnings
- [ ] I tried disk operations **only** on virtual or spare disks
- [ ] New operations go through `Safety`, ask for confirmation and write to the log
- [ ] New user-facing text is in the `.lang` files (all languages, or English with a note)
- [ ] I updated `docs/guide/en.md` and `CHANGELOG.md` if the change is visible to users
