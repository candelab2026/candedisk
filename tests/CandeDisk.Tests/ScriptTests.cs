using CandeDisk;
using Xunit;

namespace CandeDisk.Tests;

/// <summary>Controlla gli script PowerShell costruiti, senza eseguirli: nessun disco viene toccato.</summary>
public class ScriptTests
{
    private const ulong MiB = 1024UL * 1024UL;
    private const ulong GiB = 1024UL * MiB;

    [Fact]
    public void Create_makes_the_partition_without_a_drive_letter()
    {
        string script = DiskService.BuildCreateScript(3, MiB, 1 * GiB, 10 * GiB, "NTFS", "aaa");

        // Una lettera su una partizione ancora vuota scatena "È necessario formattare il disco".
        Assert.DoesNotContain("New-Partition -DiskNumber 3 -Offset 1048576 -Size 1073741824 -AssignDriveLetter", script);
        Assert.Matches(@"New-Partition [^;]*;", script);
        Assert.DoesNotMatch(@"New-Partition [^;]*-AssignDriveLetter", script);
    }

    [Fact]
    public void Create_assigns_the_letter_only_after_formatting()
    {
        string script = DiskService.BuildCreateScript(3, MiB, 1 * GiB, 10 * GiB, "NTFS", "aaa");

        int format = script.IndexOf("Format-Volume", StringComparison.Ordinal);
        int assign = script.IndexOf("Add-PartitionAccessPath", StringComparison.Ordinal);
        Assert.True(format >= 0 && assign > format, "la lettera va assegnata dopo la formattazione");
    }

    [Fact]
    public void Create_without_formatting_leaves_the_partition_without_letter()
    {
        string script = DiskService.BuildCreateScript(3, MiB, 1 * GiB, 10 * GiB, "", "");

        Assert.DoesNotContain("Format-Volume", script);
        Assert.DoesNotContain("Add-PartitionAccessPath", script);
    }

    [Fact]
    public void Create_near_the_end_of_the_gap_lets_windows_pick_the_size()
    {
        string max = DiskService.BuildCreateScript(3, MiB, 10 * GiB - 3 * MiB, 10 * GiB, "NTFS", "x");
        string fixedSize = DiskService.BuildCreateScript(3, MiB, 1 * GiB, 10 * GiB, "NTFS", "x");

        Assert.Contains("-UseMaximumSize", max);
        Assert.DoesNotContain("-UseMaximumSize", fixedSize);
        Assert.Contains("-Size 1073741824", fixedSize);
    }

    [Fact]
    public void Offset_is_aligned_up_to_one_mib()
    {
        string script = DiskService.BuildCreateScript(3, MiB + 512, 1 * GiB, 10 * GiB, "NTFS", "x");

        Assert.Contains("-Offset 2097152", script);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Scripts_always_restore_the_hardware_detection_service(bool create)
    {
        string script = create
            ? DiskService.BuildCreateScript(3, MiB, 1 * GiB, 10 * GiB, "NTFS", "x")
            : DiskService.BuildFormatScript(3, 2, "NTFS", "x", quick: true);

        int stop = script.IndexOf("Stop-Service ShellHWDetection", StringComparison.Ordinal);
        int tryAt = script.IndexOf("try {", StringComparison.Ordinal);
        int finallyAt = script.IndexOf("finally {", StringComparison.Ordinal);
        int start = script.IndexOf("Start-Service ShellHWDetection", StringComparison.Ordinal);

        Assert.True(stop >= 0 && tryAt > stop && finallyAt > tryAt && start > finallyAt,
            "il servizio va fermato prima del try e riavviato nel finally");
        // Si riavvia solo se era in esecuzione: lo stato iniziale del sistema viene rispettato.
        Assert.Contains("$wasRunning", script);
    }

    [Theory]
    [InlineData(true, "")]
    [InlineData(false, " -Full")]
    public void Format_is_quick_unless_a_full_format_is_requested(bool quick, string flag)
    {
        string script = DiskService.BuildFormatScript(3, 2, "NTFS", "dati", quick);

        Assert.Equal(flag.Length > 0, script.Contains(" -Full"));
        Assert.Contains("-FileSystem NTFS", script);
        Assert.Contains("-NewFileSystemLabel 'dati'", script);
    }

    [Fact]
    public void Single_quotes_in_labels_are_doubled_even_if_validation_is_bypassed()
    {
        // Seconda linea di difesa: la validazione li rifiuta già, ma lo script non deve dipenderne.
        string script = DiskService.BuildFormatScript(3, 2, "NTFS", "a'b", quick: true);

        Assert.Contains("'a''b'", script);
    }
}
