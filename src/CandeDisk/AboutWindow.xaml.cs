using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace CandeDisk;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        VersionText.Text = Loc.F("AboutVersion", AppInfo.Version);
    }

    private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        AppInfo.OpenUrl(e.Uri.AbsoluteUri);
        e.Handled = true;
    }
}

public static class AppInfo
{
    public const string RepositoryUrl = "https://github.com/candelab2026/candedisk";

    public static string Version
    {
        get
        {
            var attr = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            // Rimuove il suffisso "+commit" che il compilatore aggiunge.
            return attr?.InformationalVersion.Split('+')[0] ?? "1.0.0";
        }
    }

    public static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Logger.Write("Cannot open " + url + ": " + ex.Message);
        }
    }
}
