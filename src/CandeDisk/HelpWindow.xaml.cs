using System.IO;
using System.Reflection;
using System.Windows;

namespace CandeDisk;

/// <summary>Mostra docs/GUIDA.md, incorporato nell'eseguibile: la guida è una sola, per app e GitHub.</summary>
public partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();
        Viewer.Document = MarkdownView.Build(LoadGuide());
    }

    private static string LoadGuide()
    {
        using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GUIDA.md");
        if (stream == null) return "# Guida non disponibile\nIl file della guida non è incluso in questa versione.";
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
