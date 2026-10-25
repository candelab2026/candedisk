# Security policy

CandeDisk runs with administrator rights and modifies disks, so security matters.

## Supported versions

Only the latest version published on the [Releases](https://github.com/candelab2026/candedisk/releases) page is maintained.

## Reporting a vulnerability

**Do not open a public issue.** Use GitHub's private reporting:

1. Go to the **Security** tab of the repository.
2. Choose **Report a vulnerability**.
3. Describe the problem, how to reproduce it and its impact.

We will reply as soon as possible and keep you updated until it is fixed. Once the fix is released, the issue can be made public, crediting you if you wish.

## What we consider a security issue

- Execution of unintended commands (for example injection into the PowerShell or `diskpart` commands through labels, names or other fields).
- Bypassing the protections that stop you deleting the running system.
- Writing files or sensitive data to unsafe locations.

## A note on usage

CandeDisk is provided without warranty (see [LICENSE](LICENSE)). Data loss caused by misuse is not a vulnerability, but a suggestion to improve the protections is always welcome as an issue.
