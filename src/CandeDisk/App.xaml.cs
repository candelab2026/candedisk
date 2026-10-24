using System.Windows;
using System.Windows.Threading;

namespace CandeDisk;

/// <summary>Opzioni da riga di comando (usate per demo e screenshot della documentazione).</summary>
public static class AppOptions
{
    /// <summary>Dati di esempio al posto dei dischi reali; nessuna operazione viene eseguita.</summary>
    public static bool Demo { get; private set; }

    /// <summary>Se valorizzato, salva uno screenshot della finestra in questo file e chiude l'app.</summary>
    public static string? ScreenshotPath { get; private set; }

    /// <summary>Con --screenshot: "help" o "about" per catturare quella finestra invece della principale.</summary>
    public static string? Window { get; private set; }

    public static int? SelectDisk { get; private set; }
    public static int? SelectRow { get; private set; }

    public static void Parse(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--demo": Demo = true; break;
                case "--screenshot" when i + 1 < args.Length: ScreenshotPath = args[++i]; break;
                case "--window" when i + 1 < args.Length: Window = args[++i]; break;
                case "--disk" when i + 1 < args.Length && int.TryParse(args[i + 1], out int d): SelectDisk = d; i++; break;
                case "--row" when i + 1 < args.Length && int.TryParse(args[i + 1], out int r): SelectRow = r; i++; break;
            }
        }
    }
}

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppOptions.Parse(e.Args);
        base.OnStartup(e);
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Logger.Write("ERRORE NON GESTITO: " + e.Exception);
        MessageBox.Show(
            "Si è verificato un errore imprevisto:\n\n" + e.Exception.Message +
            "\n\nI dettagli sono nel file di log (menu Aiuto > Apri cartella dei log).",
            "CandeDisk", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
