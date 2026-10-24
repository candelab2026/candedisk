using System.IO;

namespace CandeDisk;

/// <summary>Registro delle operazioni: utile per ricostruire cosa è stato fatto a un disco.</summary>
public static class Logger
{
    private static readonly object Gate = new();

    public static string Folder { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CandeDisk", "logs");

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(Folder);
                string file = Path.Combine(Folder, $"candedisk-{DateTime.Now:yyyyMMdd}.log");
                File.AppendAllText(file, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Il log non deve mai far fallire un'operazione.
        }
    }
}
