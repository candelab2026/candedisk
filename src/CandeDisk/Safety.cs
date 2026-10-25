namespace CandeDisk;

public enum PartitionAction
{
    Format,
    Delete,
    Resize,
}

/// <summary>Regole di protezione: cosa non si può toccare e quando serve un avviso in più.</summary>
public static class Safety
{
    /// <summary>Motivo per cui l'azione è vietata, oppure null se consentita.</summary>
    public static string? BlockReason(DiskInfo disk, PartitionInfo partition, PartitionAction action)
    {
        if (partition.IsFree) return Loc.T("SafetySelectPartition");
        if (disk.IsOffline) return Loc.T("SafetyOffline");
        if (disk.IsReadOnly) return Loc.T("SafetyReadOnly");

        // Ridimensionare la partizione di Windows è un'operazione normale e supportata.
        if (action == PartitionAction.Resize) return null;

        if (partition.IsBoot)
            return Loc.T("SafetyRunningWindows");
        if (disk.IsBoot && partition.IsSystem)
            return Loc.T("SafetyBootEfi");

        string? systemDrive = Environment.GetEnvironmentVariable("SystemDrive")?.TrimEnd(':');
        if (!string.IsNullOrEmpty(systemDrive) &&
            string.Equals(partition.DriveLetter, systemDrive, StringComparison.OrdinalIgnoreCase))
            return Loc.F("SafetySystemDrive", systemDrive);

        return null;
    }

    /// <summary>Avviso aggiuntivo da mostrare in conferma, oppure null.</summary>
    public static string? Warning(DiskInfo disk, PartitionInfo partition)
    {
        if (partition.IsFree) return null;
        if (partition.IsSpecial)
            return Loc.T("WarnSpecial");
        if (disk.IsBoot || disk.IsSystem)
            return Loc.T("WarnSystemDisk");
        return null;
    }

    private static readonly System.Text.RegularExpressions.Regex LabelPattern =
        new(@"^[\p{L}\p{N} _\-\.]{0,32}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public static bool IsValidLabel(string label) => LabelPattern.IsMatch(label);
}
