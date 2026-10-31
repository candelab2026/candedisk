using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace CandeDisk;

/// <summary>Impostazioni semplici chiave=valore in %LOCALAPPDATA%\CandeDisk\settings.ini (le altre chiavi si conservano).</summary>
public static class SettingsStore
{
    private static string File_ => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CandeDisk", "settings.ini");

    public static string? Get(string key)
    {
        try
        {
            if (!File.Exists(File_)) return null;
            foreach (string line in File.ReadAllLines(File_))
                if (line.StartsWith(key + "=")) return line[(key.Length + 1)..];
        }
        catch
        {
            // File illeggibile: come se la chiave non ci fosse.
        }
        return null;
    }

    public static void Set(string key, string value)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(File_)!);
            var lines = File.Exists(File_) ? File.ReadAllLines(File_).Where(l => !l.StartsWith(key + "=")).ToList() : new List<string>();
            lines.Add(key + "=" + value);
            File.WriteAllLines(File_, lines);
        }
        catch
        {
            // Preferenza non salvata: vale solo per questa sessione.
        }
    }
}

/// <summary>
/// Conteggio d'uso anonimo: al massimo UNA richiesta al giorno ("app_open") a candelab.net/stats con nome dell'app,
/// versione, "windows" e lingua dell'interfaccia. Nessun identificativo, nessun dato su dischi, partizioni o file.
/// Si disattiva dal menu File.
/// </summary>
public static class UsageStats
{
    private const string Endpoint = "https://candelab.net/stats/v1/event";
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };

    public static bool Enabled
    {
        get => SettingsStore.Get("stats") != "off";
        set => SettingsStore.Set("stats", value ? "on" : "off");
    }

    /// <summary>Invia il conteggio del giorno in background; ogni errore viene ignorato.</summary>
    public static void PingDaily()
    {
        if (!Enabled || AppOptions.Demo || AppOptions.ScreenshotPath != null) return;
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        if (SettingsStore.Get("stats_last") == today) return;

        _ = Task.Run(async () =>
        {
            try
            {
                string json = JsonSerializer.Serialize(new
                {
                    app = "candedisk",
                    @event = "app_open",
                    platform = "windows",
                    version = AppInfo.Version,
                    lang = Loc.Current,
                });
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var response = await Http.PostAsync(Endpoint, content);
                if ((int)response.StatusCode == 204) SettingsStore.Set("stats_last", today);
            }
            catch
            {
                // Le statistiche non devono mai disturbare l'app.
            }
        });
    }
}
