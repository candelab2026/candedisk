using System.Net.Http;
using System.Text.Json;

namespace CandeDisk;

/// <summary>Cerca l'ultima release su GitHub. Non scarica né installa nulla: apre solo la pagina della release.</summary>
public static class UpdateChecker
{
    private const string LatestApi = "https://api.github.com/repos/candelab2026/candedisk/releases/latest";
    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var c = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
        c.DefaultRequestHeaders.UserAgent.ParseAdd("CandeDisk-update-check");
        c.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return c;
    }

    /// <summary>Versione nuova e pagina della release; null se non c'è un aggiornamento. Lancia se la richiesta fallisce.</summary>
    public static async Task<(string Version, string Url)?> CheckAsync()
    {
        using var response = await Http.GetAsync(LatestApi);
        response.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string tag = doc.RootElement.GetProperty("tag_name").GetString() ?? "";
        string url = doc.RootElement.GetProperty("html_url").GetString() ?? AppInfo.RepositoryUrl + "/releases";
        if (!IsNewer(tag, AppInfo.Version)) return null;
        // Solo indirizzi della pagina del progetto: nient'altro viene mai aperto.
        if (!url.StartsWith(AppInfo.RepositoryUrl + "/", StringComparison.Ordinal)) url = AppInfo.RepositoryUrl + "/releases";
        return (tag.TrimStart('v', 'V'), url);
    }

    public static bool IsNewer(string latestTag, string current)
    {
        return Version.TryParse(latestTag.TrimStart('v', 'V'), out var latest)
            && Version.TryParse(current, out var cur)
            && latest > cur;
    }
}
