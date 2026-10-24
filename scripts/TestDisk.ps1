<#
.SYNOPSIS
  Crea o rimuove un disco virtuale (VHDX) per provare CandeDisk senza toccare i dischi reali.

.DESCRIPTION
  Il disco viene creato vuoto e non inizializzato (RAW), così si può provare anche "Inizializza".
  Richiede PowerShell avviato come amministratore.

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
if (-not $admin) { throw 'Avvia PowerShell come amministratore.' }

# Percorso fisso e dedicato: lo script non tocca mai altri file.
$vhd = Join-Path $env:TEMP 'CandeDiskTest.vhdx'

function Invoke-DiskPart([string]$commands) {
    $script = Join-Path $env:TEMP "candedisk-test-$([guid]::NewGuid().ToString('N')).txt"
    Set-Content -Path $script -Value $commands -Encoding ASCII
    try {
        $out = diskpart /s $script
        if ($LASTEXITCODE -ne 0) { throw "diskpart ha fallito:`n$($out -join "`n")" }
    }
    finally { Remove-Item $script -Force -ErrorAction SilentlyContinue }
}

switch ($Action) {
    'Create' {
        if (Test-Path $vhd) { throw "Esiste già $vhd. Esegui prima: -Action Remove" }
        Invoke-DiskPart "create vdisk file=`"$vhd`" maximum=$SizeMB type=expandable`nattach vdisk"
        Write-Host "Disco virtuale da $SizeMB MB creato e collegato: $vhd"
        Write-Host 'Aprilo in CandeDisk (Aggiorna): compare come disco RAW, non inizializzato.'
    }
    'Remove' {
        if (-not (Test-Path $vhd)) { Write-Host 'Nessun disco di prova da rimuovere.'; return }
        Invoke-DiskPart "select vdisk file=`"$vhd`"`ndetach vdisk"
        Remove-Item $vhd -Force
        Write-Host 'Disco virtuale scollegato e cancellato.'
    }
}
