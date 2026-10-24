using CandeDisk;
using Xunit;

namespace CandeDisk.Tests;

public class ModelsTests
{
    private const ulong MiB = 1024UL * 1024UL;
    private const ulong GiB = 1024UL * MiB;

    private static PartitionInfo Part(ulong offset, ulong size) =>
        new() { PartitionNumber = 1, Offset = offset, Size = size };

    private static DiskInfo Disk(string style, ulong size, params PartitionInfo[] parts)
    {
        var disk = new DiskInfo { PartitionStyle = style, Size = size, Partitions = parts.ToList() };
        disk.BuildRows();
        return disk;
    }

    [Fact]
    public void Empty_gpt_disk_has_one_free_row_between_first_and_last_mib()
    {
        var disk = Disk("GPT", 10 * GiB);

        var free = Assert.Single(disk.Rows);
        Assert.True(free.IsFree);
        Assert.Equal(MiB, free.Offset);
        Assert.Equal(10 * GiB - MiB, free.End);
    }

    [Fact]
    public void Empty_mbr_disk_uses_the_whole_size()
    {
        var disk = Disk("MBR", 10 * GiB);

        Assert.Equal(10 * GiB, Assert.Single(disk.Rows).End);
    }

    [Fact]
    public void Free_space_after_the_last_partition_is_listed()
    {
        var disk = Disk("GPT", 10 * GiB, Part(MiB, 4 * GiB));

        Assert.Equal(2, disk.Rows.Count);
        Assert.False(disk.Rows[0].IsFree);
        Assert.True(disk.Rows[1].IsFree);
        Assert.Equal(MiB + 4 * GiB, disk.Rows[1].Offset);
    }

    [Fact]
    public void Gap_between_two_partitions_is_listed_in_offset_order()
    {
        var disk = Disk("GPT", 10 * GiB,
            Part(6 * GiB, 2 * GiB),   // fornite fuori ordine di proposito
            Part(MiB, 2 * GiB));

        Assert.Equal(new[] { false, true, false, true },
            disk.Rows.Select(r => r.IsFree).ToArray());
        Assert.Equal(MiB, disk.Rows[0].Offset);
        Assert.Equal(2 * GiB + MiB, disk.Rows[1].Offset);
    }

    [Fact]
    public void Gaps_smaller_than_one_mib_are_ignored()
    {
        var disk = Disk("GPT", 10 * GiB,
            Part(MiB, 2 * GiB),
            Part(MiB + 2 * GiB + MiB / 2, 7 * GiB));   // buco di 512 KiB: allineamento, non spazio libero

        Assert.DoesNotContain(disk.Rows, r => r.IsFree && r.Offset < 3 * GiB);
    }

    [Fact]
    public void Fully_used_disk_has_no_free_rows()
    {
        var disk = Disk("MBR", 4 * GiB, Part(MiB, 4 * GiB - MiB));

        Assert.DoesNotContain(disk.Rows, r => r.IsFree);
    }

    [Fact]
    public void Demo_disks_never_overlap()
    {
        foreach (var disk in DemoData.Disks())
        {
            ulong cursor = 0;
            foreach (var row in disk.Rows)
            {
                Assert.True(row.Offset >= cursor, $"Disco {disk.Number}: righe sovrapposte");
                cursor = row.End;
            }
        }
    }

    [Theory]
    [InlineData(0UL, "0 B")]
    [InlineData(512UL, "512 B")]
    [InlineData(1024UL, "1 KB")]
    [InlineData(1536UL, "1,5 KB")]
    [InlineData(1024UL * 1024 * 1024, "1 GB")]
    public void SizeFormat_uses_binary_units(ulong bytes, string expected)
    {
        // Il separatore decimale dipende dalla cultura del sistema: si confronta su una forma normalizzata.
        Assert.Equal(expected.Replace(',', '.'), SizeFormat.Bytes(bytes).Replace(',', '.'));
    }

    [Theory]
    [InlineData("System", true)]
    [InlineData("Reserved", true)]
    [InlineData("Recovery", true)]
    [InlineData("OEM", true)]
    [InlineData("Basic", false)]
    [InlineData("IFS", false)]
    [InlineData("Unknown", false)]   // es. partizioni Linux: dati, non partizioni da proteggere
    public void IsSpecial_matches_protected_partition_types(string type, bool expected)
    {
        var p = new PartitionInfo { Type = type };

        Assert.Equal(expected, p.IsSpecial);
    }

    [Fact]
    public void Free_rows_are_never_special()
    {
        Assert.False(new PartitionInfo { IsFree = true, IsSystem = true }.IsSpecial);
    }
}
