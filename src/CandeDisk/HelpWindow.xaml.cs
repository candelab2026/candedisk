using System.Windows;

namespace CandeDisk;

/// <summary>Mostra la guida nella lingua corrente (docs/guide/xx.md, incorporata nell'eseguibile): una sola fonte per app e GitHub.</summary>
public partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();
        Viewer.Document = MarkdownView.Build(Loc.Guide());
    }
}
