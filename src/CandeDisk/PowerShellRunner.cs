using System.Diagnostics;
using System.IO;
using System.Text;

namespace CandeDisk;

public sealed class DiskOperationException : Exception
{
    public DiskOperationException(string message) : base(message) { }
}

/// <summary>Esegue script PowerShell (modulo Storage di Windows) e restituisce l'output.</summary>
public static class PowerShellRunner
{
    public static async Task<string> RunAsync(string script)
    {
        // -EncodedCommand evita ogni problema di quoting; gli errori diventano exit code 1 + stderr.
        string wrapped =
            "$ErrorActionPreference='Stop'; $ProgressPreference='SilentlyContinue'; [Console]::OutputEncoding=[Text.Encoding]::UTF8; " +
            "try { " + script + " } catch { [Console]::Error.WriteLine($_.Exception.Message); exit 1 }";
        string encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(wrapped));

        var psi = new ProcessStartInfo("powershell.exe",
            $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encoded}")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        return await RunProcessAsync(psi);
    }

    /// <summary>Esegue uno script diskpart (usato solo come ripiego per le partizioni protette).</summary>
    public static async Task<string> RunDiskpartAsync(string commands)
    {
        string file = Path.Combine(Path.GetTempPath(), $"candedisk_{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(file, commands);
        try
        {
            var psi = new ProcessStartInfo("diskpart.exe", $"/s \"{file}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            return await RunProcessAsync(psi);
        }
        finally
        {
            try { File.Delete(file); } catch { /* file temporaneo, non critico */ }
        }
    }

    private static async Task<string> RunProcessAsync(ProcessStartInfo psi)
    {
        using var process = Process.Start(psi) ?? throw new DiskOperationException("Impossibile avviare il processo.");
        Task<string> stdout = process.StandardOutput.ReadToEndAsync();
        Task<string> stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        string output = await stdout;
        string error = CleanError((await stderr).Trim());
        if (process.ExitCode != 0)
            throw new DiskOperationException(string.IsNullOrEmpty(error) ? output.Trim() : error);
        return output;
    }

    /// <summary>PowerShell con stderr rediretto emette XML (CLIXML): estrae solo i messaggi di errore.</summary>
    internal static string CleanError(string error)
    {
        if (!error.StartsWith("#< CLIXML")) return error;

        var messages = System.Text.RegularExpressions.Regex
            .Matches(error, "<S S=\"Error\">(.*?)</S>", System.Text.RegularExpressions.RegexOptions.Singleline)
            .Select(m => m.Groups[1].Value.Replace("_x000D_", "").Replace("_x000A_", "\n").Trim())
            .Where(s => s.Length > 0);
        return string.Join("\n", messages);
    }
}
