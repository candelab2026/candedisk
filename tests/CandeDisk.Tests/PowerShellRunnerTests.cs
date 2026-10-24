using CandeDisk;
using Xunit;

namespace CandeDisk.Tests;

public class PowerShellRunnerTests
{
    [Fact]
    public void Plain_errors_are_returned_unchanged() =>
        Assert.Equal("Not enough available capacity", PowerShellRunner.CleanError("Not enough available capacity"));

    [Fact]
    public void Clixml_errors_are_reduced_to_their_message()
    {
        const string clixml =
            "#< CLIXML\r\n<Objs Version=\"1.1.0.1\" xmlns=\"http://schemas.microsoft.com/powershell/2004/04\">" +
            "<S S=\"Error\">Not enough available capacity_x000D__x000A_</S>" +
            "<S S=\"Error\">Activity ID: {1234}_x000D__x000A_</S></Objs>";

        string cleaned = PowerShellRunner.CleanError(clixml);

        Assert.Contains("Not enough available capacity", cleaned);
        Assert.DoesNotContain("<", cleaned);
        Assert.DoesNotContain("CLIXML", cleaned);
    }

    [Fact]
    public void Clixml_with_only_progress_records_gives_an_empty_message()
    {
        const string progress =
            "#< CLIXML\r\n<Objs><Obj S=\"progress\"><AV>Preparazione dei moduli</AV></Obj></Objs>";

        Assert.Equal("", PowerShellRunner.CleanError(progress));
    }

    [Fact]
    public async Task Script_errors_surface_as_DiskOperationException()
    {
        var ex = await Assert.ThrowsAsync<DiskOperationException>(
            () => PowerShellRunner.RunAsync("throw 'errore di prova'"));

        Assert.Contains("errore di prova", ex.Message);
    }

    [Fact]
    public async Task Output_is_returned_for_successful_scripts()
    {
        string output = await PowerShellRunner.RunAsync("Write-Output 'ciao àèì'");

        Assert.Equal("ciao àèì", output.Trim());
    }
}
