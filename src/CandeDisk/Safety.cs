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
        if (partition.IsFree) return "Seleziona una partizione, non lo spazio non allocato.";
        if (disk.IsOffline) return "Il disco è offline.";
        if (disk.IsReadOnly) return "Il disco è in sola lettura.";

        // Ridimensionare la partizione di Windows è un'operazione normale e supportata.
        if (action == PartitionAction.Resize) return null;

        if (partition.IsBoot)
            return "Questa partizione contiene il Windows attualmente in esecuzione.";
        if (disk.IsBoot && partition.IsSystem)
            return "Questa è la partizione di avvio (EFI) del sistema attualmente in uso.";

        string? systemDrive = Environment.GetEnvironmentVariable("SystemDrive")?.TrimEnd(':');
        if (!string.IsNullOrEmpty(systemDrive) &&
            string.Equals(partition.DriveLetter, systemDrive, StringComparison.OrdinalIgnoreCase))
            return $"{systemDrive}: è l'unità di sistema.";

        return null;
    }

    /// <summary>Avviso aggiuntivo da mostrare in conferma, oppure null.</summary>
    public static string? Warning(DiskInfo disk, PartitionInfo partition)
    {
        if (partition.IsFree) return null;
        if (partition.IsSpecial)
            return "È una partizione speciale (EFI/ripristino/riservata). Cancellarla o formattarla può " +
                   "impedire l'avvio di un sistema o rimuovere l'ambiente di ripristino.";
        if (disk.IsBoot || disk.IsSystem)
            return "Si trova sul disco di sistema.";
        return null;
    }

    private static readonly System.Text.RegularExpressions.Regex LabelPattern =
        new(@"^[\p{L}\p{N} _\-\.]{0,32}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public static bool IsValidLabel(string label) => LabelPattern.IsMatch(label);
}
