using System.Text.Json;

namespace CandeDisk;

public sealed record SupportedSize(ulong Min, ulong Max);

public static class DiskService
{
    public const ulong MiB = 1024UL * 1024UL;

    public static readonly string[] FileSystems = { "NTFS", "exFAT", "FAT32" };

    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    // ---- v1: lettura ------------------------------------------------------

    public static async Task<List<DiskInfo>> ListAsync()
    {
        string json = await PowerShellRunner.RunAsync("""
            $disks = @(Get-Disk | Sort-Object Number | ForEach-Object {
              $d = $_
              $parts = @(Get-Partition -DiskNumber $d.Number -ErrorAction SilentlyContinue | Sort-Object Offset | ForEach-Object {
                $p = $_
                $v = $null
                try { $v = $p | Get-Volume -ErrorAction Stop } catch {}
                [pscustomobject]@{
                  PartitionNumber = [int]$p.PartitionNumber
                  DriveLetter     = $(if ([int]$p.DriveLetter -ne 0) { [string]$p.DriveLetter } else { '' })
                  Offset          = [uint64]$p.Offset
                  Size            = [uint64]$p.Size
                  Type            = [string]$p.Type
                  IsSystem        = [bool]$p.IsSystem
                  IsBoot          = [bool]$p.IsBoot
                  IsActive        = [bool]$p.IsActive
                  IsHidden        = [bool]$p.IsHidden
                  FileSystem      = $(if ($v) { [string]$v.FileSystem } else { '' })
                  Label           = $(if ($v) { [string]$v.FileSystemLabel } else { '' })
                  SizeRemaining   = $(if ($v) { [uint64]$v.SizeRemaining } else { [uint64]0 })
                }
              })
              [pscustomobject]@{
                Number         = [int]$d.Number
                FriendlyName   = [string]$d.FriendlyName
                Size           = [uint64]$d.Size
                PartitionStyle = [string]$d.PartitionStyle
                BusType        = [string]$d.BusType
                IsSystem       = [bool]$d.IsSystem
                IsBoot         = [bool]$d.IsBoot
                IsOffline      = [bool]$d.IsOffline
                IsReadOnly     = [bool]$d.IsReadOnly
                Partitions     = $parts
              }
            })
            ConvertTo-Json -InputObject $disks -Depth 4 -Compress
            """);

        var disks = JsonSerializer.Deserialize<List<DiskInfo>>(json, Json) ?? new List<DiskInfo>();
        foreach (var disk in disks) disk.BuildRows();
        return disks;
    }

    // ---- v2: formatta, crea, elimina -------------------------------------

    public static Task InitializeAsync(int diskNumber) =>
        PowerShellRunner.RunAsync($"Initialize-Disk -Number {diskNumber} -PartitionStyle GPT | Out-Null");

    public static Task FormatAsync(int diskNumber, int partitionNumber, string fileSystem, string label, bool quick = true)
    {
        Validate(fileSystem, label);
        // Senza -Full, Format-Volume esegue la formattazione rapida; con -Full scrive tutto il volume (lento).
        string full = quick ? "" : " -Full";
        return PowerShellRunner.RunAsync(
            $"Get-Partition -DiskNumber {diskNumber} -PartitionNumber {partitionNumber} | " +
            $"Format-Volume -FileSystem {fileSystem} -NewFileSystemLabel '{Quote(label)}'{full} -Confirm:$false -Force | Out-Null");
    }

    /// <summary>Crea una partizione in uno spazio non allocato. fileSystem vuoto = non formattare.</summary>
    public static Task CreateAsync(int diskNumber, ulong offset, ulong size, ulong gapEnd, string fileSystem, string label)
    {
        if (fileSystem.Length > 0) Validate(fileSystem, label);
        ulong alignedOffset = (offset + MiB - 1) / MiB * MiB;

        // Se si chiede tutto lo spazio libero, lascia decidere a Windows: i byte persi
        // nell'allineamento fanno altrimenti fallire con "Not enough available capacity".
        // Il margine di 4 MiB copre lo spazio che Windows tiene per sé (es. fine disco MBR).
        bool wantsMaximum = alignedOffset + size + 4 * MiB >= gapEnd;
        string sizeArg = wantsMaximum ? "-UseMaximumSize" : $"-Size {size}";
        string script =
            $"$p = New-Partition -DiskNumber {diskNumber} -Offset {alignedOffset} {sizeArg} -AssignDriveLetter; ";
        if (fileSystem.Length > 0)
            script += $"$p | Format-Volume -FileSystem {fileSystem} -NewFileSystemLabel '{Quote(label)}' -Confirm:$false -Force | Out-Null";
        return PowerShellRunner.RunAsync(script);
    }

    /// <param name="isProtectedType">
    /// True solo per partizioni speciali (EFI, OEM, ripristino): sono le uniche per cui si ripiega su diskpart.
    /// Per tutte le altre un errore di Remove-Partition (es. volume in uso) deve fermare l'operazione:
    /// "override" cancellerebbe comunque e aggirerebbe la protezione di Windows.
    /// </param>
    public static async Task DeleteAsync(int diskNumber, int partitionNumber, bool isProtectedType = false)
    {
        try
        {
            await PowerShellRunner.RunAsync(
                $"Remove-Partition -DiskNumber {diskNumber} -PartitionNumber {partitionNumber} -Confirm:$false");
        }
        catch (DiskOperationException) when (isProtectedType)
        {
            await PowerShellRunner.RunDiskpartAsync(
                $"select disk {diskNumber}\r\nselect partition {partitionNumber}\r\ndelete partition override\r\n");
        }
    }

    // ---- v3: ridimensiona -------------------------------------------------

    public static async Task<SupportedSize> GetSupportedSizeAsync(int diskNumber, int partitionNumber)
    {
        string json = await PowerShellRunner.RunAsync(
            $"$s = Get-PartitionSupportedSize -DiskNumber {diskNumber} -PartitionNumber {partitionNumber}; " +
            "ConvertTo-Json -InputObject ([pscustomobject]@{ Min = [uint64]$s.SizeMin; Max = [uint64]$s.SizeMax }) -Compress");
        using var doc = JsonDocument.Parse(json);
        return new SupportedSize(doc.RootElement.GetProperty("Min").GetUInt64(),
                                 doc.RootElement.GetProperty("Max").GetUInt64());
    }

    public static Task ResizeAsync(int diskNumber, int partitionNumber, ulong newSize) =>
        PowerShellRunner.RunAsync(
            $"Resize-Partition -DiskNumber {diskNumber} -PartitionNumber {partitionNumber} -Size {newSize}");

    // ---- utilità ----------------------------------------------------------

    private static void Validate(string fileSystem, string label)
    {
        if (!FileSystems.Contains(fileSystem))
            throw new DiskOperationException($"File system non supportato: {fileSystem}");
        if (!Safety.IsValidLabel(label))
            throw new DiskOperationException("L'etichetta può contenere solo lettere, numeri, spazio, _ - . (max 32).");
    }

    private static string Quote(string value) => value.Replace("'", "''");
}
