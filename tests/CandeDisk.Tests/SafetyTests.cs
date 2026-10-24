using CandeDisk;
using Xunit;

namespace CandeDisk.Tests;

public class SafetyTests
{
    private static DiskInfo NormalDisk() => new() { Number = 1, PartitionStyle = "GPT" };

    // ---- etichette: unico campo di testo libero che finisce in uno script PowerShell ----

    [Theory]
    [InlineData("")]
    [InlineData("Dati")]
    [InlineData("Backup 2026")]
    [InlineData("foto_vacanze-agosto.1")]
    [InlineData("Città")]
    public void Valid_labels_are_accepted(string label) => Assert.True(Safety.IsValidLabel(label));

    [Theory]
    [InlineData("x'; Remove-Item C:\\ -Recurse #")]
    [InlineData("a'b")]
    [InlineData("a\"b")]
    [InlineData("a;b")]
    [InlineData("a|b")]
    [InlineData("$(whoami)")]
    [InlineData("a`b")]
    [InlineData("a\nb")]
    [InlineData("a&b")]
    [InlineData("a<b")]
    public void Labels_with_shell_metacharacters_are_rejected(string label) =>
        Assert.False(Safety.IsValidLabel(label));

    [Fact]
    public void Labels_longer_than_32_chars_are_rejected()
    {
        Assert.True(Safety.IsValidLabel(new string('a', 32)));
        Assert.False(Safety.IsValidLabel(new string('a', 33)));
    }

    [Fact]
    public async Task Injection_through_the_label_is_stopped_before_powershell_runs()
    {
        // La validazione avviene prima di costruire lo script: nessun processo viene avviato.
        await Assert.ThrowsAsync<DiskOperationException>(() =>
            DiskService.FormatAsync(1, 1, "NTFS", "x'; Remove-Item C:\\ -Recurse #"));
        await Assert.ThrowsAsync<DiskOperationException>(() =>
            DiskService.CreateAsync(1, 1024 * 1024, 1024 * 1024 * 10, ulong.MaxValue, "NTFS", "a;b"));
    }

    [Theory]
    [InlineData("ReFS")]
    [InlineData("NTFS; Get-Process")]
    [InlineData("")]
    public async Task Unknown_file_systems_are_rejected(string fileSystem) =>
        await Assert.ThrowsAsync<DiskOperationException>(() => DiskService.FormatAsync(1, 1, fileSystem, "ok"));

    // ---- regole di blocco ----

    [Fact]
    public void Running_windows_partition_cannot_be_formatted_or_deleted()
    {
        var disk = new DiskInfo { Number = 0, IsBoot = true };
        var windows = new PartitionInfo { PartitionNumber = 3, IsBoot = true };

        Assert.NotNull(Safety.BlockReason(disk, windows, PartitionAction.Format));
        Assert.NotNull(Safety.BlockReason(disk, windows, PartitionAction.Delete));
    }

    [Fact]
    public void Running_windows_partition_can_still_be_resized()
    {
        var disk = new DiskInfo { Number = 0, IsBoot = true };
        var windows = new PartitionInfo { PartitionNumber = 3, IsBoot = true };

        Assert.Null(Safety.BlockReason(disk, windows, PartitionAction.Resize));
    }

    [Fact]
    public void Efi_partition_of_the_boot_disk_is_blocked()
    {
        var disk = new DiskInfo { Number = 0, IsBoot = true };
        var efi = new PartitionInfo { PartitionNumber = 1, IsSystem = true, Type = "System" };

        Assert.NotNull(Safety.BlockReason(disk, efi, PartitionAction.Delete));
    }

    [Fact]
    public void Efi_partition_of_another_disk_is_allowed_but_warns()
    {
        var disk = NormalDisk();
        var efi = new PartitionInfo { PartitionNumber = 1, IsSystem = true, Type = "System" };

        Assert.Null(Safety.BlockReason(disk, efi, PartitionAction.Delete));
        Assert.NotNull(Safety.Warning(disk, efi));
    }

    [Fact]
    public void Ordinary_data_partition_is_allowed_without_warning()
    {
        var disk = NormalDisk();
        var data = new PartitionInfo { PartitionNumber = 1, Type = "Basic", DriveLetter = "Z" };

        Assert.Null(Safety.BlockReason(disk, data, PartitionAction.Format));
        Assert.Null(Safety.BlockReason(disk, data, PartitionAction.Delete));
        Assert.Null(Safety.Warning(disk, data));
    }

    [Theory]
    [InlineData(PartitionAction.Format)]
    [InlineData(PartitionAction.Delete)]
    [InlineData(PartitionAction.Resize)]
    public void Offline_and_read_only_disks_are_never_modified(PartitionAction action)
    {
        var part = new PartitionInfo { PartitionNumber = 1, Type = "Basic" };

        Assert.NotNull(Safety.BlockReason(new DiskInfo { IsOffline = true }, part, action));
        Assert.NotNull(Safety.BlockReason(new DiskInfo { IsReadOnly = true }, part, action));
    }

    [Fact]
    public void Free_space_rows_cannot_be_targeted_by_partition_actions()
    {
        var free = new PartitionInfo { IsFree = true };

        Assert.NotNull(Safety.BlockReason(NormalDisk(), free, PartitionAction.Delete));
    }

    [Fact]
    public void System_drive_letter_is_blocked_even_without_boot_flag()
    {
        string? systemDrive = Environment.GetEnvironmentVariable("SystemDrive")?.TrimEnd(':');
        if (string.IsNullOrEmpty(systemDrive)) return;   // ambiente non Windows: nulla da verificare

        var part = new PartitionInfo { PartitionNumber = 2, Type = "Basic", DriveLetter = systemDrive };

        Assert.NotNull(Safety.BlockReason(NormalDisk(), part, PartitionAction.Format));
    }
}
