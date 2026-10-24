using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CandeDisk;

public partial class MainWindow : Window
{
    private const ulong MiB = DiskService.MiB;

    private readonly bool _isAdmin =
        new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    private List<DiskInfo> _disks = new();
    private bool _busy;

    private DiskInfo? SelectedDisk => DiskList.SelectedItem as DiskInfo;
    private PartitionInfo? SelectedRow => PartitionGrid.SelectedItem as PartitionInfo;

    public MainWindow()
    {
        InitializeComponent();
        AdminBanner.Visibility = _isAdmin ? Visibility.Collapsed : Visibility.Visible;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await RefreshAsync();

    // ---- caricamento e visualizzazione -----------------------------------

    private async Task RefreshAsync()
    {
        int? keepDisk = SelectedDisk?.Number;
        SetBusy(true, "Lettura dei dischi...");
        try
        {
            _disks = await DiskService.ListAsync();
            DiskList.ItemsSource = _disks;
            DiskList.SelectedItem = _disks.FirstOrDefault(d => d.Number == keepDisk) ?? _disks.FirstOrDefault();
            StatusText.Text = $"{_disks.Count} dischi trovati";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Errore nella lettura dei dischi";
            MessageBox.Show(this, ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void DiskList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var disk = SelectedDisk;
        PartitionGrid.ItemsSource = disk?.Rows;
        DiskHeader.Text = disk == null ? "" : $"{disk.Title} - {disk.FriendlyName}";
        DrawBar(disk);
        UpdateButtons();
    }

    private void PartitionGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        DrawBar(SelectedDisk);
        UpdateButtons();
    }

    /// <summary>Barra proporzionale con una casella per partizione / spazio libero.</summary>
    private void DrawBar(DiskInfo? disk)
    {
        BarGrid.Children.Clear();
        BarGrid.ColumnDefinitions.Clear();
        if (disk == null || disk.Rows.Count == 0) return;

        ulong total = disk.Rows.Sum(r => (decimal)r.Size) > 0
            ? (ulong)disk.Rows.Sum(r => (decimal)r.Size)
            : 1;

        for (int i = 0; i < disk.Rows.Count; i++)
        {
            var row = disk.Rows[i];
            // Minimo visivo del 4%: le partizioni piccole (EFI, MSR) restano cliccabili.
            double share = Math.Max((double)row.Size / total, 0.04);
            BarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(share, GridUnitType.Star) });

            bool selected = ReferenceEquals(row, SelectedRow);
            var border = new Border
            {
                Background = BarColor(row),
                BorderBrush = selected ? Brushes.Black : Brushes.White,
                BorderThickness = new Thickness(selected ? 3 : 1),
                Margin = new Thickness(0),
                Cursor = Cursors.Hand,
                ToolTip = row.BarLabel.Replace("\n", " · "),
                Child = new TextBlock
                {
                    Text = row.BarLabel,
                    Margin = new Thickness(4),
                    FontSize = 11,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Foreground = row.IsFree ? Brushes.DimGray : Brushes.White,
                },
            };
            border.MouseLeftButtonDown += (_, _) => PartitionGrid.SelectedItem = row;
            Grid.SetColumn(border, i);
            BarGrid.Children.Add(border);
        }
    }

    private static Brush BarColor(PartitionInfo row)
    {
        if (row.IsFree) return new SolidColorBrush(Color.FromRgb(0xE4, 0xE4, 0xE4));
        if (row.IsBoot) return new SolidColorBrush(Color.FromRgb(0x1F, 0x6F, 0xD1));
        if (row.IsSpecial) return new SolidColorBrush(Color.FromRgb(0xC9, 0x8A, 0x10));
        return new SolidColorBrush(Color.FromRgb(0x4F, 0x9D, 0x6B));
    }

    private void SetBusy(bool busy, string? message = null)
    {
        _busy = busy;
        if (message != null) StatusText.Text = message;
        Cursor = busy ? Cursors.Wait : null;
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        var disk = SelectedDisk;
        var row = SelectedRow;
        bool canAct = _isAdmin && !_busy && disk != null;

        RefreshButton.IsEnabled = !_busy;
        ElevateButton.IsEnabled = !_busy;
        InitButton.IsEnabled = canAct && disk!.IsRaw;
        CreateButton.IsEnabled = canAct && row is { IsFree: true } && !disk!.IsRaw;
        FormatButton.IsEnabled = canAct && row is { IsFree: false };
        ResizeButton.IsEnabled = canAct && row is { IsFree: false };
        DeleteButton.IsEnabled = canAct && row is { IsFree: false };
    }

    // ---- azioni -----------------------------------------------------------

    private async void Refresh_Click(object sender, RoutedEventArgs e) => await RefreshAsync();

    private void Elevate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(Environment.ProcessPath!) { UseShellExecute = true, Verb = "runas" });
            Application.Current.Shutdown();
        }
        catch (Win32Exception)
        {
            // L'utente ha annullato la richiesta UAC: si resta in sola lettura.
        }
    }

    private async void Init_Click(object sender, RoutedEventArgs e)
    {
        var disk = SelectedDisk;
        if (disk == null || !disk.IsRaw) return;
        if (!Confirm($"Inizializzare il Disco {disk.Number} ({SizeFormat.Bytes(disk.Size)}) con tabella GPT?")) return;
        await RunAsync($"Inizializzazione del disco {disk.Number}...", () => DiskService.InitializeAsync(disk.Number));
    }

    private async void Create_Click(object sender, RoutedEventArgs e)
    {
        var disk = SelectedDisk;
        var gap = SelectedRow;
        if (disk == null || gap is not { IsFree: true }) return;
        if (disk.IsOffline || disk.IsReadOnly)
        {
            Warn("Il disco è offline o in sola lettura.");
            return;
        }

        ulong alignedStart = (gap.Offset + MiB - 1) / MiB * MiB;
        ulong maxMb = gap.End > alignedStart ? (gap.End - alignedStart) / MiB : 0;
        if (maxMb < 1)
        {
            Warn("Lo spazio non allocato è troppo piccolo.");
            return;
        }

        var dlg = new OptionsDialog("Crea partizione",
            $"Nuova partizione nello spazio non allocato di {gap.SizeText} del Disco {disk.Number}.",
            askSize: true, minMb: 1, maxMb: maxMb, defaultMb: maxMb,
            askFileSystem: true, allowNoFormat: true, askLabel: true)
        { Owner = this };
        if (dlg.ShowDialog() != true) return;

        ulong sizeBytes = dlg.SizeMb * MiB;
        string fs = dlg.FileSystem.Length > 0 ? dlg.FileSystem : "nessuna formattazione";
        if (!Confirm($"Creare una partizione da {SizeFormat.Bytes(sizeBytes)} sul Disco {disk.Number} ({fs})?")) return;

        await RunAsync("Creazione della partizione...",
            () => DiskService.CreateAsync(disk.Number, gap.Offset, sizeBytes, dlg.FileSystem, dlg.VolumeLabel));
    }

    private async void Format_Click(object sender, RoutedEventArgs e)
    {
        var disk = SelectedDisk;
        var part = SelectedRow;
        if (disk == null || part == null) return;
        if (!CheckAllowed(disk, part, PartitionAction.Format)) return;

        var dlg = new OptionsDialog("Formatta partizione",
            $"Formatta la partizione {part.PartitionNumber} del Disco {disk.Number} ({part.SizeText}).",
            askFileSystem: true, askLabel: true)
        { Owner = this, InitialLabel = part.Label };
        if (dlg.ShowDialog() != true) return;

        if (!ConfirmDestructive(disk, part,
                $"Formattare la partizione {Describe(part)} del Disco {disk.Number} come {dlg.FileSystem}?\n" +
                "TUTTI I DATI presenti verranno cancellati.")) return;

        await RunAsync("Formattazione in corso...",
            () => DiskService.FormatAsync(disk.Number, part.PartitionNumber, dlg.FileSystem, dlg.VolumeLabel));
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        var disk = SelectedDisk;
        var part = SelectedRow;
        if (disk == null || part == null) return;
        if (!CheckAllowed(disk, part, PartitionAction.Delete)) return;

        if (!ConfirmDestructive(disk, part,
                $"Eliminare la partizione {Describe(part)} del Disco {disk.Number}?\n" +
                "TUTTI I DATI presenti verranno cancellati.")) return;

        await RunAsync("Eliminazione in corso...",
            () => DiskService.DeleteAsync(disk.Number, part.PartitionNumber));
    }

    private async void Resize_Click(object sender, RoutedEventArgs e)
    {
        var disk = SelectedDisk;
        var part = SelectedRow;
        if (disk == null || part == null) return;
        if (!CheckAllowed(disk, part, PartitionAction.Resize)) return;

        SupportedSize range;
        SetBusy(true, "Calcolo delle dimensioni supportate...");
        try
        {
            range = await DiskService.GetSupportedSizeAsync(disk.Number, part.PartitionNumber);
        }
        catch (Exception ex)
        {
            Warn("Impossibile ridimensionare questa partizione:\n" + ex.Message);
            return;
        }
        finally
        {
            SetBusy(false, "Pronto");
        }

        ulong minMb = (range.Min + MiB - 1) / MiB;
        ulong maxMb = range.Max / MiB;
        ulong currentMb = part.Size / MiB;
        if (minMb >= maxMb)
        {
            Warn("Questa partizione non può essere né ridotta né estesa (nessuno spazio disponibile).");
            return;
        }

        var dlg = new OptionsDialog("Ridimensiona partizione",
            $"Partizione {Describe(part)} del Disco {disk.Number}: attuale {part.SizeText}.\n" +
            "Per estendere serve spazio non allocato subito dopo la partizione.",
            askSize: true, minMb: minMb, maxMb: maxMb, defaultMb: Math.Clamp(currentMb, minMb, maxMb))
        { Owner = this };
        if (dlg.ShowDialog() != true) return;

        ulong newBytes = dlg.SizeMb * MiB;
        if (newBytes == part.Size) return;
        bool shrinking = newBytes < part.Size;

        string msg = $"{(shrinking ? "Ridurre" : "Estendere")} la partizione {Describe(part)} del Disco {disk.Number} " +
                     $"da {part.SizeText} a {SizeFormat.Bytes(newBytes)}?";
        if (shrinking) msg += "\nFai prima un backup dei dati importanti.";
        if (!Confirm(msg)) return;

        await RunAsync("Ridimensionamento in corso...",
            () => DiskService.ResizeAsync(disk.Number, part.PartitionNumber, newBytes));
    }

    // ---- supporto ---------------------------------------------------------

    private async Task RunAsync(string busyMessage, Func<Task> operation)
    {
        SetBusy(true, busyMessage);
        try
        {
            await operation();
            StatusText.Text = "Operazione completata";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Operazione non riuscita";
            MessageBox.Show(this, ex.Message, "Operazione non riuscita", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
        }
        await RefreshAsync();
    }

    private bool CheckAllowed(DiskInfo disk, PartitionInfo part, PartitionAction action)
    {
        string? reason = Safety.BlockReason(disk, part, action);
        if (reason == null) return true;
        Warn("Operazione bloccata per sicurezza:\n" + reason);
        return false;
    }

    private static string Describe(PartitionInfo p) =>
        p.Letter != "" ? $"{p.Letter} (#{p.PartitionNumber})" : $"#{p.PartitionNumber}";

    private bool Confirm(string message) =>
        MessageBox.Show(this, message, "Conferma", MessageBoxButton.YesNo, MessageBoxImage.Question,
            MessageBoxResult.No) == MessageBoxResult.Yes;

    /// <summary>Doppia conferma per le operazioni che cancellano dati; terza se la partizione è speciale.</summary>
    private bool ConfirmDestructive(DiskInfo disk, PartitionInfo part, string message)
    {
        if (MessageBox.Show(this, message, "Conferma operazione distruttiva", MessageBoxButton.YesNo,
                MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
            return false;

        string? warning = Safety.Warning(disk, part);
        if (warning == null) return true;

        return MessageBox.Show(this, warning + "\n\nVuoi davvero continuare?", "Attenzione",
            MessageBoxButton.YesNo, MessageBoxImage.Stop, MessageBoxResult.No) == MessageBoxResult.Yes;
    }

    private void Warn(string message) =>
        MessageBox.Show(this, message, "CandeDisk", MessageBoxButton.OK, MessageBoxImage.Warning);
}
