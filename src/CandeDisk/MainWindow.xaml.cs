using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CandeDisk;

public partial class MainWindow : Window
{
    private const ulong MiB = DiskService.MiB;

    private readonly bool _isAdmin =
        AppOptions.Demo ||
        new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    private List<DiskInfo> _disks = new();
    private bool _busy;

    private DiskInfo? SelectedDisk => DiskList.SelectedItem as DiskInfo;
    private PartitionInfo? SelectedRow => PartitionGrid.SelectedItem as PartitionInfo;

    public MainWindow()
    {
        InitializeComponent();
        AdminBanner.Visibility = _isAdmin ? Visibility.Collapsed : Visibility.Visible;
        ModeText.Text = AppOptions.Demo ? Loc.T("ModeDemo")
                      : _isAdmin ? Loc.T("ModeAdmin")
                      : Loc.T("ModeReadOnly");
        Title = Loc.F("WindowTitle", AppInfo.Version);
        BuildLanguageMenu();
        Logger.Write($"Starting CandeDisk {AppInfo.Version} (administrator: {_isAdmin}, demo: {AppOptions.Demo}, language: {Loc.Current})");
    }

    private void BuildLanguageMenu()
    {
        foreach (var language in Loc.Languages)
        {
            var item = new MenuItem { Header = language.Name, Tag = language.Code, IsChecked = language.Code == Loc.Current };
            item.Click += Language_Click;
            MenuLanguage.Items.Add(item);
        }
    }

    /// <summary>Cambia lingua subito: riapre la finestra nella stessa posizione, senza riavviare né richiedere di nuovo l'UAC.</summary>
    private void Language_Click(object sender, RoutedEventArgs e)
    {
        string code = (string)((MenuItem)sender).Tag;
        if (code == Loc.Current || _busy) return;

        Loc.Set(code);
        var next = new MainWindow
        {
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = Left, Top = Top, Width = Width, Height = Height, WindowState = WindowState,
        };
        next.Show();
        Close();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await RefreshAsync();
        if (AppOptions.ScreenshotPath != null) await TakeScreenshotAndExitAsync(AppOptions.ScreenshotPath);
    }

    // ---- caricamento e visualizzazione -----------------------------------

    private async Task RefreshAsync()
    {
        int? keepDisk = SelectedDisk?.Number ?? AppOptions.SelectDisk;
        int? keepPartition = SelectedRow is { IsFree: false } p ? p.PartitionNumber : null;
        SetBusy(true, Loc.T("StatusReading"));
        try
        {
            _disks = AppOptions.Demo ? DemoData.Disks() : await DiskService.ListAsync();
            DiskList.ItemsSource = _disks;
            DiskList.SelectedItem = _disks.FirstOrDefault(d => d.Number == keepDisk) ?? _disks.FirstOrDefault();

            // Dopo un'operazione la riga selezionata resta la stessa, se esiste ancora.
            if (SelectedDisk != null && keepPartition != null)
                PartitionGrid.SelectedItem = SelectedDisk.Rows.FirstOrDefault(r => !r.IsFree && r.PartitionNumber == keepPartition);

            StatusText.Text = Loc.F("StatusDisksFound", _disks.Count);
        }
        catch (Exception ex)
        {
            Logger.Write("Error reading disks: " + ex.Message);
            StatusText.Text = Loc.T("StatusReadError");
            MessageBox.Show(this, ex.Message, Loc.T("ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
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

        double total = disk.Rows.Sum(r => (double)r.Size);
        if (total <= 0) total = 1;

        for (int i = 0; i < disk.Rows.Count; i++)
        {
            var row = disk.Rows[i];
            // Minimo visivo del 4%: le partizioni piccole (EFI, MSR) restano cliccabili.
            double share = Math.Max(row.Size / total, 0.04);
            BarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(share, GridUnitType.Star) });

            bool selected = ReferenceEquals(row, SelectedRow);
            var border = new Border
            {
                Background = BarColor(row),
                BorderBrush = selected ? Brushes.Black : Brushes.White,
                BorderThickness = new Thickness(selected ? 3 : 1),
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

        bool init = canAct && disk!.IsRaw;
        bool create = canAct && row is { IsFree: true } && !disk!.IsRaw;
        bool onPartition = canAct && row is { IsFree: false };

        RefreshButton.IsEnabled = MenuRefresh.IsEnabled = !_busy;
        ElevateButton.IsEnabled = MenuElevate.IsEnabled = !_busy && !_isAdmin;
        InitButton.IsEnabled = MenuInit.IsEnabled = init;
        CreateButton.IsEnabled = MenuCreate.IsEnabled = create;
        FormatButton.IsEnabled = MenuFormat.IsEnabled = onPartition;
        ResizeButton.IsEnabled = MenuResize.IsEnabled = onPartition;
        DeleteButton.IsEnabled = MenuDelete.IsEnabled = onPartition;
    }

    // ---- scorciatoie ------------------------------------------------------

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        var shift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

        if (e.Key == Key.F5 && RefreshButton.IsEnabled) Refresh_Click(this, e);
        else if (e.Key == Key.F1) Help_Click(this, e);
        else if (ctrl && !shift && e.Key == Key.N && CreateButton.IsEnabled) Create_Click(this, e);
        else if (ctrl && shift && e.Key == Key.F && FormatButton.IsEnabled) Format_Click(this, e);
        else if (ctrl && !shift && e.Key == Key.R && ResizeButton.IsEnabled) Resize_Click(this, e);
        // Canc agisce solo se il focus è sulla tabella delle partizioni, per evitare gesti accidentali.
        else if (e.Key == Key.Delete && PartitionGrid.IsKeyboardFocusWithin && DeleteButton.IsEnabled) Delete_Click(this, e);
        else return;

        e.Handled = true;
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

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    private void Help_Click(object sender, RoutedEventArgs e) => new HelpWindow { Owner = this }.Show();

    private void About_Click(object sender, RoutedEventArgs e) => new AboutWindow { Owner = this }.ShowDialog();

    private void Website_Click(object sender, RoutedEventArgs e) => AppInfo.OpenUrl(AppInfo.RepositoryUrl);

    private void OpenLogs_Click(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory(Logger.Folder);
        AppInfo.OpenUrl(Logger.Folder);
    }

    private void CopySummary_Click(object sender, RoutedEventArgs e)
    {
        var sb = new StringBuilder();
        foreach (var disk in _disks)
        {
            sb.AppendLine($"{disk.Title} - {disk.Subtitle}");
            foreach (var r in disk.Rows)
            {
                string name = r.IsFree ? Loc.T("Unallocated") : $"#{r.PartitionNumber} {r.Letter} {r.Label}".Trim();
                sb.AppendLine($"  {name,-28} {r.FileSystem,-8} {r.SizeText,10}  {r.FlagsText}".TrimEnd());
            }
            sb.AppendLine();
        }
        Clipboard.SetText(sb.ToString().TrimEnd());
        StatusText.Text = Loc.T("StatusSummaryCopied");
    }

    private async void Init_Click(object sender, RoutedEventArgs e)
    {
        var disk = SelectedDisk;
        if (disk == null || !disk.IsRaw || BlockDemo()) return;
        if (!Confirm(Loc.F("ConfirmInit", disk.Number, disk.FriendlyName, SizeFormat.Bytes(disk.Size)))) return;
        await RunAsync(Loc.F("StatusInitializing", disk.Number),
            $"INITIALIZE disk {disk.Number} ({disk.FriendlyName}) as GPT",
            () => DiskService.InitializeAsync(disk.Number));
    }

    private async void Create_Click(object sender, RoutedEventArgs e)
    {
        var disk = SelectedDisk;
        var gap = SelectedRow;
        if (disk == null || gap is not { IsFree: true } || BlockDemo()) return;
        if (disk.IsOffline || disk.IsReadOnly)
        {
            Warn(Loc.T("MsgDiskOfflineOrRO"));
            return;
        }

        ulong alignedStart = (gap.Offset + MiB - 1) / MiB * MiB;
        ulong maxMb = gap.End > alignedStart ? (gap.End - alignedStart) / MiB : 0;
        if (maxMb < 1)
        {
            Warn(Loc.T("MsgGapTooSmall"));
            return;
        }

        var dlg = new OptionsDialog(Loc.T("DlgCreateTitle"),
            Loc.F("DlgCreateMsg", gap.SizeText, disk.Number, disk.FriendlyName),
            askSize: true, minMb: 1, maxMb: maxMb, defaultMb: maxMb,
            askFileSystem: true, allowNoFormat: true, askLabel: true)
        { Owner = this };
        if (dlg.ShowDialog() != true) return;

        ulong sizeBytes = dlg.SizeMb * MiB;
        string fs = dlg.FileSystem.Length > 0 ? dlg.FileSystem : Loc.T("NoFormat");
        if (!Confirm(Loc.F("ConfirmCreate", SizeFormat.Bytes(sizeBytes), disk.Number, disk.FriendlyName, fs))) return;

        await RunAsync(Loc.T("StatusCreating"),
            $"CREATE partition {SizeFormat.Bytes(sizeBytes)} on disk {disk.Number} ({disk.FriendlyName}), file system: {(dlg.FileSystem.Length > 0 ? dlg.FileSystem : "none")}",
            () => DiskService.CreateAsync(disk.Number, gap.Offset, sizeBytes, gap.End, dlg.FileSystem, dlg.VolumeLabel));
    }

    private async void Format_Click(object sender, RoutedEventArgs e)
    {
        var disk = SelectedDisk;
        var part = SelectedRow;
        if (disk == null || part == null || BlockDemo()) return;
        if (!CheckAllowed(disk, part, PartitionAction.Format)) return;

        var dlg = new OptionsDialog(Loc.T("DlgFormatTitle"),
            Loc.F("DlgFormatMsg", part.PartitionNumber, disk.Number, disk.FriendlyName, part.SizeText),
            askFileSystem: true, askLabel: true, askQuick: true)
        { Owner = this, InitialLabel = part.Label };
        if (dlg.ShowDialog() != true) return;

        if (!ConfirmDestructive(disk, part,
                Loc.F("ConfirmFormat", Loc.T(dlg.QuickFormat ? "FormatQuick" : "FormatFull"),
                    Describe(part), disk.Number, disk.FriendlyName, dlg.FileSystem))) return;

        string mode = dlg.QuickFormat ? "quick" : "full";
        await RunAsync(Loc.T(dlg.QuickFormat ? "StatusFormatting" : "StatusFormattingFull"),
            $"FORMAT ({mode}) partition {Describe(part)} of disk {disk.Number} ({disk.FriendlyName}) as {dlg.FileSystem}",
            () => DiskService.FormatAsync(disk.Number, part.PartitionNumber, dlg.FileSystem, dlg.VolumeLabel, dlg.QuickFormat));
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        var disk = SelectedDisk;
        var part = SelectedRow;
        if (disk == null || part == null || BlockDemo()) return;
        if (!CheckAllowed(disk, part, PartitionAction.Delete)) return;

        if (!ConfirmDestructive(disk, part,
                Loc.F("ConfirmDelete", Describe(part), disk.Number, disk.FriendlyName))) return;

        await RunAsync(Loc.T("StatusDeleting"),
            $"DELETE partition {Describe(part)} of disk {disk.Number} ({disk.FriendlyName})",
            () => DiskService.DeleteAsync(disk.Number, part.PartitionNumber, part.IsSpecial));
    }

    private async void Resize_Click(object sender, RoutedEventArgs e)
    {
        var disk = SelectedDisk;
        var part = SelectedRow;
        if (disk == null || part == null || BlockDemo()) return;
        if (!CheckAllowed(disk, part, PartitionAction.Resize)) return;

        SupportedSize range;
        SetBusy(true, Loc.T("StatusCalcSizes"));
        try
        {
            range = await DiskService.GetSupportedSizeAsync(disk.Number, part.PartitionNumber);
        }
        catch (Exception ex)
        {
            Logger.Write("Supported sizes not available: " + ex.Message);
            Warn(Loc.F("MsgResizeUnavailable", ex.Message));
            return;
        }
        finally
        {
            SetBusy(false, Loc.T("StatusReady"));
        }

        ulong minMb = (range.Min + MiB - 1) / MiB;
        ulong maxMb = range.Max / MiB;
        ulong currentMb = part.Size / MiB;
        if (minMb >= maxMb)
        {
            Warn(Loc.T("MsgNoResize"));
            return;
        }

        var dlg = new OptionsDialog(Loc.T("DlgResizeTitle"),
            Loc.F("DlgResizeMsg", Describe(part), disk.Number, part.SizeText),
            askSize: true, minMb: minMb, maxMb: maxMb, defaultMb: Math.Clamp(currentMb, minMb, maxMb))
        { Owner = this };
        if (dlg.ShowDialog() != true) return;

        ulong newBytes = dlg.SizeMb * MiB;
        if (newBytes == part.Size) return;
        bool shrinking = newBytes < part.Size;

        if (!Confirm(Loc.F(shrinking ? "ConfirmShrink" : "ConfirmExtend",
                Describe(part), disk.Number, disk.FriendlyName, part.SizeText, SizeFormat.Bytes(newBytes)))) return;

        await RunAsync(Loc.T("StatusResizing"),
            $"RESIZE partition {Describe(part)} of disk {disk.Number} ({disk.FriendlyName}): {part.SizeText} -> {SizeFormat.Bytes(newBytes)}",
            () => DiskService.ResizeAsync(disk.Number, part.PartitionNumber, newBytes));
    }

    // ---- supporto ---------------------------------------------------------

    private async Task RunAsync(string busyMessage, string logLine, Func<Task> operation)
    {
        Logger.Write("START   " + logLine);
        SetBusy(true, busyMessage);
        try
        {
            await operation();
            Logger.Write("OK      " + logLine);
            StatusText.Text = Loc.T("StatusDone");
        }
        catch (Exception ex)
        {
            Logger.Write("ERROR   " + logLine + " -> " + ex.Message);
            StatusText.Text = Loc.T("StatusFailed");
            MessageBox.Show(this, ex.Message, Loc.T("StatusFailed"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
        }
        await RefreshAsync();
    }

    private bool BlockDemo()
    {
        if (!AppOptions.Demo) return false;
        Warn(Loc.T("MsgDemo"));
        return true;
    }

    private bool CheckAllowed(DiskInfo disk, PartitionInfo part, PartitionAction action)
    {
        string? reason = Safety.BlockReason(disk, part, action);
        if (reason == null) return true;
        Warn(Loc.F("MsgBlocked", reason));
        return false;
    }

    private static string Describe(PartitionInfo p) =>
        p.Letter != "" ? $"{p.Letter} (#{p.PartitionNumber})" : $"#{p.PartitionNumber}";

    private bool Confirm(string message) =>
        MessageBox.Show(this, message, Loc.T("ConfirmTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question,
            MessageBoxResult.No) == MessageBoxResult.Yes;

    /// <summary>Doppia conferma per le operazioni che cancellano dati; terza se la partizione è speciale.</summary>
    private bool ConfirmDestructive(DiskInfo disk, PartitionInfo part, string message)
    {
        if (MessageBox.Show(this, message, Loc.T("ConfirmDestructiveTitle"), MessageBoxButton.YesNo,
                MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes)
            return false;

        string? warning = Safety.Warning(disk, part);
        if (warning == null) return true;

        return MessageBox.Show(this, Loc.F("ConfirmContinue", warning), Loc.T("AttentionTitle"),
            MessageBoxButton.YesNo, MessageBoxImage.Stop, MessageBoxResult.No) == MessageBoxResult.Yes;
    }

    private void Warn(string message) =>
        MessageBox.Show(this, message, "CandeDisk", MessageBoxButton.OK, MessageBoxImage.Warning);

    // ---- screenshot per la documentazione ---------------------------------

    private async Task TakeScreenshotAndExitAsync(string path)
    {
        if (AppOptions.Window is "help" or "about")
        {
            Window window = AppOptions.Window == "help" ? new HelpWindow() : new AboutWindow();
            window.Owner = this;
            window.Show();
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
            SavePng((FrameworkElement)window.Content, path);
            Application.Current.Shutdown();
            return;
        }

        if (AppOptions.SelectDisk != null)
            DiskList.SelectedItem = _disks.FirstOrDefault(d => d.Number == AppOptions.SelectDisk);
        if (AppOptions.SelectRow is int index && SelectedDisk != null && index < SelectedDisk.Rows.Count)
            PartitionGrid.SelectedItem = SelectedDisk.Rows[index];

        // Lascia finire layout e rendering prima di catturare.
        await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
        UpdateLayout();

        SavePng((FrameworkElement)Content, path);
        Application.Current.Shutdown();
    }

    private static void SavePng(FrameworkElement content, string path)
    {
        content.UpdateLayout();
        const double scale = 2;
        var bitmap = new RenderTargetBitmap(
            (int)(content.ActualWidth * scale), (int)(content.ActualHeight * scale),
            96 * scale, 96 * scale, PixelFormats.Pbgra32);
        bitmap.Render(content);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}
