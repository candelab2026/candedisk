namespace CandeDisk;

/// <summary>Dischi di esempio per --demo: permettono screenshot e prove della grafica senza toccare nulla.</summary>
public static class DemoData
{
    private const ulong MiB = 1024UL * 1024UL;
    private const ulong GiB = 1024UL * MiB;

    public static List<DiskInfo> Disks()
    {
        var disks = new List<DiskInfo>
        {
            new()
            {
                Number = 0, FriendlyName = "NVMe SSD 1 TB", Size = 1000 * GiB, PartitionStyle = "GPT",
                BusType = "NVMe", IsBoot = true, IsSystem = true,
                Partitions = Sequence(1000 * GiB,
                    Part(1, "", 260 * MiB, "System", "FAT32", "EFI", system: true),
                    Part(2, "", 16 * MiB, "Reserved", "", ""),
                    Part(3, "C", 930 * GiB, "Basic", "NTFS", "Windows", boot: true, free: 410 * GiB),
                    Part(4, "", 1 * GiB, "Recovery", "NTFS", "Ripristino", free: 300 * MiB)),
            },
            new()
            {
                Number = 1, FriendlyName = "Disco esterno 2 TB", Size = 2000 * GiB, PartitionStyle = "GPT",
                BusType = "USB",
                Partitions = Sequence(2000 * GiB,
                    Part(1, "D", 1200 * GiB, "Basic", "NTFS", "Archivio", free: 540 * GiB),
                    Part(2, "E", 600 * GiB, "Basic", "exFAT", "Backup", free: 210 * GiB)),
            },
            new()
            {
                Number = 2, FriendlyName = "Chiavetta USB 64 GB", Size = 64 * GiB, PartitionStyle = "MBR",
                BusType = "USB",
                Partitions = Sequence(64 * GiB,
                    Part(1, "F", 32 * GiB, "IFS", "FAT32", "CHIAVETTA", free: 20 * GiB, active: true)),
            },
        };

        foreach (var disk in disks) disk.BuildRows();
        return disks;
    }

    private static PartitionInfo Part(int number, string letter, ulong size, string type, string fs, string label,
        bool system = false, bool boot = false, bool active = false, ulong free = 0) => new()
    {
        PartitionNumber = number, DriveLetter = letter, Size = size, Type = type, FileSystem = fs, Label = label,
        IsSystem = system, IsBoot = boot, IsActive = active, SizeRemaining = free,
    };

    /// <summary>Assegna gli offset in sequenza, a partire da 1 MiB, come farebbe Windows.</summary>
    private static List<PartitionInfo> Sequence(ulong diskSize, params PartitionInfo[] parts)
    {
        ulong offset = MiB;
        foreach (var p in parts)
        {
            p.Offset = offset;
            offset += p.Size;
        }
        return parts.ToList();
    }
}
