<#
.SYNOPSIS
  Creates or removes a virtual disk (VHDX) to try CandeDisk without touching real disks.

.DESCRIPTION
  The disk is created empty and uninitialized (RAW), so "Initialize" can be tried too.
  Requires PowerShell started as administrator.

.EXAMPLE
  .\scripts\TestDisk.ps1 -Action Create -SizeMB 2048
  .\scripts\TestDisk.ps1 -Action Remove
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('Create', 'Remove')]
    [string]$Action,

    [ValidateRange(64, 102400)]
    [int]$SizeMB = 1024
)

$ErrorActionPreference = 'Stop'

$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $admin) { throw 'Start PowerShell as administrator.' }

# Fixed, dedicated path: the script never touches any other file.
$vhd = Join-Path $env:TEMP 'CandeDiskTest.vhdx'

function Invoke-DiskPart([string]$commands) {
    $script = Join-Path $env:TEMP "candedisk-test-$([guid]::NewGuid().ToString('N')).txt"
    Set-Content -Path $script -Value $commands -Encoding ASCII
    try {
        $out = diskpart /s $script
        if ($LASTEXITCODE -ne 0) { throw "diskpart failed:`n$($out -join "`n")" }
    }
    finally { Remove-Item $script -Force -ErrorAction SilentlyContinue }
}

switch ($Action) {
    'Create' {
        if (Test-Path $vhd) { throw "$vhd already exists. Run first: -Action Remove" }
        Invoke-DiskPart "create vdisk file=`"$vhd`" maximum=$SizeMB type=expandable`nattach vdisk"
        Write-Host "Virtual disk of $SizeMB MB created and attached: $vhd"
        Write-Host 'Open it in CandeDisk (Refresh): it appears as a RAW, uninitialized disk.'
    }
    'Remove' {
        if (-not (Test-Path $vhd)) { Write-Host 'No test disk to remove.'; return }
        Invoke-DiskPart "select vdisk file=`"$vhd`"`ndetach vdisk"
        Remove-Item $vhd -Force
        Write-Host 'Virtual disk detached and deleted.'
    }
}
