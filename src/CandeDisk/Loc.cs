using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Markup;

namespace CandeDisk;

public sealed record Language(string Code, string Name, string Culture);

/// <summary>
/// Traduzioni dell'interfaccia. Ogni lingua è un file <c>Languages/xx.lang</c> incorporato nell'eseguibile
/// (righe <c>Chiave = testo</c>, <c>\n</c> per andare a capo): aggiungere una lingua non richiede codice.
/// L'inglese è la lingua di riserva per le chiavi mancanti.
/// </summary>
public static class Loc
{
    public const string Fallback = "en";

    private static readonly Dictionary<string, Dictionary<string, string>> Tables = new();
    private static readonly List<Language> LanguageList = new();

    static Loc()
    {
        var assembly = Assembly.GetExecutingAssembly();
        foreach (string resource in assembly.GetManifestResourceNames()
                     .Where(n => n.StartsWith("lang.") && n.EndsWith(".lang")).OrderBy(n => n))
        {
            string code = resource["lang.".Length..^".lang".Length];
            using Stream stream = assembly.GetManifestResourceStream(resource)!;
            using var reader = new StreamReader(stream);
            var table = Parse(reader.ReadToEnd());
            Tables[code] = table;
            LanguageList.Add(new Language(code, table.GetValueOrDefault("@Name", code), table.GetValueOrDefault("@Culture", code)));
        }
        Current = Fallback;
    }

    public static IReadOnlyList<Language> Languages => LanguageList;

    public static string Current { get; private set; }

    internal static IReadOnlyDictionary<string, string> Table(string code) => Tables[code];

    internal static Dictionary<string, string> Parse(string text)
    {
        var table = new Dictionary<string, string>();
        foreach (string raw in text.Replace("\r\n", "\n").Split('\n'))
        {
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            int eq = line.IndexOf('=');
            if (eq <= 0) continue;
            table[line[..eq].Trim()] = line[(eq + 1)..].Trim().Replace("\\n", "\n");
        }
        return table;
    }

    // ---- scelta della lingua ----------------------------------------------

    private static string SettingsFile => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CandeDisk", "settings.ini");

    /// <summary>Lingua all'avvio: quella richiesta da riga di comando, poi quella salvata, poi quella di Windows.</summary>
    public static void Init(string? requested)
    {
        string? code = Normalize(requested) ?? Normalize(ReadSaved()) ?? Normalize(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        Use(code ?? Fallback);
    }

    /// <summary>Cambia lingua e la ricorda per i prossimi avvii.</summary>
    public static void Set(string code)
    {
        Use(code);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsFile)!);
            File.WriteAllText(SettingsFile, "language=" + Current + Environment.NewLine);
        }
        catch
        {
            // Preferenza non salvata: l'app resta comunque nella lingua scelta per questa sessione.
        }
    }

    private static void Use(string code)
    {
        Current = Tables.ContainsKey(code) ? code : Fallback;
        // Anche i numeri (es. "1,5 GB") seguono la lingua scelta.
        try
        {
            var culture = new CultureInfo(LanguageList.FirstOrDefault(l => l.Code == Current)?.Culture ?? "en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.CurrentCulture = culture;
        }
        catch (CultureNotFoundException)
        {
        }
    }

    private static string? Normalize(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        string two = code.Trim().ToLowerInvariant();
        if (two.Length > 2) two = two[..2];
        return Tables.ContainsKey(two) ? two : null;
    }

    private static string? ReadSaved()
    {
        try
        {
            if (!File.Exists(SettingsFile)) return null;
            foreach (string line in File.ReadAllLines(SettingsFile))
                if (line.StartsWith("language=")) return line["language=".Length..];
        }
        catch
        {
            // File illeggibile: si usa la lingua di Windows.
        }
        return null;
    }

    // ---- testi ------------------------------------------------------------

    public static string T(string key)
    {
        if (Tables[Current].TryGetValue(key, out string? value)) return value;
        if (Tables[Fallback].TryGetValue(key, out value)) return value;
        return key;
    }

    public static string F(string key, params object[] args) => string.Format(CultureInfo.CurrentCulture, T(key), args);

    /// <summary>Testo della guida nella lingua corrente (docs/guide/xx.md, incorporato), o in inglese.</summary>
    public static string Guide()
    {
        var assembly = Assembly.GetExecutingAssembly();
        foreach (string code in new[] { Current, Fallback })
        {
            using Stream? stream = assembly.GetManifestResourceStream($"guide.{code}.md");
            if (stream == null) continue;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
        return "# " + T("HelpMissingTitle") + "\n" + T("HelpMissingText");
    }

    internal static bool HasGuide(string code) =>
        Assembly.GetExecutingAssembly().GetManifestResourceNames().Contains($"guide.{code}.md");

    private static readonly Regex AccessKey = new(@"\(_.\)", RegexOptions.Compiled);

    /// <summary>Toglie le sottolineature dei tasti di accesso ("_File", "文件(_F)") dove non servono.</summary>
    public static string Plain(string text) => AccessKey.Replace(text, "").Replace("_", "");
}

/// <summary>Uso in XAML: <c>Text="{local:T Chiave}"</c>, oppure <c>{local:T Chiave, Plain=True}</c> senza tasti di accesso.</summary>
[MarkupExtensionReturnType(typeof(string))]
public sealed class TExtension : MarkupExtension
{
    public TExtension(string key) => Key = key;

    public string Key { get; set; }

    public bool Plain { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        string text = Loc.T(Key);
        return Plain ? Loc.Plain(text) : text;
    }
}
