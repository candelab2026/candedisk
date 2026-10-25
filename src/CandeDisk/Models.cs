namespace CandeDisk;

public static class SizeFormat
{
    public static string Bytes(ulong bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double value = bytes;
        int i = 0;
        while (value >= 1024 && i < units.Length - 1)
        {
            value /= 1024;
            i++;
        }
        return $"{value:0.##} {units[i]}";
    }
}

public sealed class PartitionInfo
{
    public int PartitionNumber { get; set; }
    public string DriveLetter { get; set; } = "";
    public ulong Offset { get; set; }
    public ulong Size { get; set; }
    public string Type { get; set; } = "";
    public bool IsSystem { get; set; }
    public bool IsBoot { get; set; }
    public bool IsActive { get; set; }
    public bool IsHidden { get; set; }
    public string FileSystem { get; set; } = "";
    public string Label { get; set; } = "";
    public ulong SizeRemaining { get; set; }

    /// <summary>Riga calcolata: spazio non allocato, non una partizione reale.</summary>
    public bool IsFree { get; set; }

    public ulong End => Offset + Size;

    /// <summary>EFI, MSR, Recovery, OEM: partizioni che Gestione disco spesso protegge.</summary>
    public bool IsSpecial =>
        !IsFree && (IsSystem || Type is "System" or "Reserved" or "Recovery" or "OEM");

    public string Number => IsFree ? "" : PartitionNumber.ToString();
    public string Letter => string.IsNullOrEmpty(DriveLetter) ? "" : DriveLetter + ":";
    public string SizeText => SizeFormat.Bytes(Size);
    public string FreeText => IsFree || string.IsNullOrEmpty(FileSystem) ? "" : SizeFormat.Bytes(SizeRemaining);
    public string OffsetText => SizeFormat.Bytes(Offset);

    public string TypeText => IsFree
        ? Loc.T("Unallocated")
        : Type switch
        {
            "System" => Loc.T("TypeEfi"),
            "Reserved" => Loc.T("TypeMsr"),
            "Recovery" => Loc.T("TypeRecovery"),
            "Basic" => Loc.T("TypeData"),
            "IFS" => Loc.T("TypeDataIfs"),
            "Logical" => Loc.T("TypeLogical"),
            "Extended" => Loc.T("TypeExtended"),
            "" => "",
            _ => Type,
        };

    public string FlagsText
    {
        get
        {
            if (IsFree) return "";
            var flags = new List<string>();
            if (IsBoot) flags.Add(Loc.T("FlagWindows"));
            if (IsSystem) flags.Add(Loc.T("FlagBoot"));
            if (IsActive) flags.Add(Loc.T("FlagActive"));
            if (IsHidden) flags.Add(Loc.T("FlagHidden"));
            return string.Join(", ", flags);
        }
    }

    public string BarLabel => IsFree
        ? $"{Loc.T("Unallocated")}\n{SizeText}"
        : $"{(Letter != "" ? Letter + " " : "")}{Label}\n{SizeText}\n{FileSystem}".Trim();
}

public sealed class DiskInfo
{
    public int Number { get; set; }
    public string FriendlyName { get; set; } = "";
    public ulong Size { get; set; }
    public string PartitionStyle { get; set; } = "";
    public string BusType { get; set; } = "";
    public bool IsSystem { get; set; }
    public bool IsBoot { get; set; }
    public bool IsOffline { get; set; }
    public bool IsReadOnly { get; set; }
    public List<PartitionInfo> Partitions { get; set; } = new();

    /// <summary>Partizioni reali più le righe di spazio non allocato, in ordine di offset.</summary>
    public List<PartitionInfo> Rows { get; private set; } = new();

    public bool IsRaw => PartitionStyle.Equals("RAW", StringComparison.OrdinalIgnoreCase);
    public string Title => Loc.F("DiskTitle", Number);
    public string Subtitle =>
        $"{FriendlyName} · {SizeFormat.Bytes(Size)} · {PartitionStyle}" +
        (IsBoot ? " · " + Loc.T("DiskFlagSystem") : "") +
        (IsOffline ? " · " + Loc.T("DiskFlagOffline") : "") +
        (IsReadOnly ? " · " + Loc.T("DiskFlagReadOnly") : "");

    private const ulong MiB = 1024UL * 1024UL;

    public void BuildRows()
    {
        var sorted = Partitions.OrderBy(p => p.Offset).ToList();
        var rows = new List<PartitionInfo>();

        // Il primo MiB è riservato alla tabella delle partizioni; su GPT lo è anche l'ultimo.
        ulong cursor = MiB;
        ulong limit = PartitionStyle.Equals("GPT", StringComparison.OrdinalIgnoreCase) && Size > MiB
            ? Size - MiB
            : Size;

        foreach (var p in sorted)
        {
            AddGap(rows, cursor, p.Offset);
            rows.Add(p);
            cursor = Math.Max(cursor, p.End);
        }
        AddGap(rows, cursor, limit);
        Rows = rows;
    }

    private static void AddGap(List<PartitionInfo> rows, ulong from, ulong to)
    {
        if (to > from && to - from >= MiB)
            rows.Add(new PartitionInfo { IsFree = true, Offset = from, Size = to - from });
    }
}
