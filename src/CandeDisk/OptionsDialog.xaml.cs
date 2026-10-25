using System.Windows;

namespace CandeDisk;

/// <summary>Finestra unica per formatta / crea / ridimensiona: mostra solo i campi richiesti.</summary>
public partial class OptionsDialog : Window
{
    private readonly bool _askSize;
    private readonly bool _askFs;
    private readonly bool _askLabel;
    private readonly ulong _minMb;
    private readonly ulong _maxMb;

    public ulong SizeMb { get; private set; }
    public string FileSystem { get; private set; } = "";
    public string VolumeLabel { get; private set; } = "";
    public bool QuickFormat => QuickBox.IsChecked == true;

    /// <summary>Etichetta proposta all'apertura della finestra.</summary>
    public string InitialLabel
    {
        set => LabelBox.Text = value;
    }

    public OptionsDialog(
        string title,
        string message,
        bool askSize = false, ulong minMb = 0, ulong maxMb = 0, ulong defaultMb = 0,
        bool askFileSystem = false, bool allowNoFormat = false,
        bool askLabel = false, bool askQuick = false)
    {
        InitializeComponent();
        QuickBox.Visibility = askQuick ? Visibility.Visible : Visibility.Collapsed;
        Title = title;
        MessageText.Text = message;

        _askSize = askSize;
        _askFs = askFileSystem;
        _askLabel = askLabel;
        _minMb = minMb;
        _maxMb = maxMb;

        SizePanel.Visibility = askSize ? Visibility.Visible : Visibility.Collapsed;
        FsPanel.Visibility = askFileSystem ? Visibility.Visible : Visibility.Collapsed;
        LabelPanel.Visibility = askLabel ? Visibility.Visible : Visibility.Collapsed;

        if (askSize)
        {
            SizeLabel.Text = Loc.F("DlgSizeLabel", minMb, maxMb);
            SizeBox.Text = defaultMb.ToString();
        }
        if (askFileSystem)
        {
            foreach (var fs in DiskService.FileSystems) FsBox.Items.Add(fs);
            if (allowNoFormat) FsBox.Items.Add(Loc.T("DlgNoFormat"));
            FsBox.SelectedIndex = 0;
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (_askSize)
        {
            if (!ulong.TryParse(SizeBox.Text.Trim(), out ulong mb) || mb < _minMb || mb > _maxMb)
            {
                MessageBox.Show(this, Loc.F("DlgBadSize", _minMb, _maxMb),
                    Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SizeMb = mb;
        }

        if (_askFs)
        {
            // L'ultima voce, se presente, è "non formattare": le altre sono i file system in lista chiusa.
            FileSystem = FsBox.SelectedIndex < DiskService.FileSystems.Length ? DiskService.FileSystems[FsBox.SelectedIndex] : "";
        }

        if (_askLabel)
        {
            string label = LabelBox.Text.Trim();
            if (!Safety.IsValidLabel(label))
            {
                MessageBox.Show(this, Loc.T("DlgBadLabel"),
                    Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            VolumeLabel = label;
        }

        DialogResult = true;
    }
}
