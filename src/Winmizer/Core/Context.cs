using System.Security.Principal;
using System.Text;

namespace Winmizer.Core;

public static class Paths
{
    public static string DataDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Winmizer");
    public static string LogDir => Path.Combine(DataDir, "logs");
    public static string BackupFile => Path.Combine(DataDir, "backup.json");
    public static string DisabledStartupDir => Path.Combine(DataDir, "DisabledStartup");
}

/// <summary>Global run context (dry-run flag, logger, backup store).</summary>
public static class Ctx
{
    public static bool DryRun { get; set; }
    public static Logger Log { get; } = new();
    public static BackupStore Backup { get; } = new();
}

public static class Admin
{
    public static bool IsElevated()
    {
        try
        {
            using var id = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch { return false; }
    }
}

/// <summary>
/// File logger (%ProgramData%\Winmizer\logs). Warnings, errors and dry-run lines are also
/// buffered so the UI can print them under the tweak that produced them.
/// </summary>
public sealed class Logger
{
    private readonly object _lock = new();
    private string? _file;
    private readonly List<(string Level, string Message)> _pending = new();

    public string? FilePath => _file;

    public void Info(string m) => Write("INFO", m, false);
    public void Warn(string m) => Write("WARN", m, true);
    public void Error(string m) => Write("ERROR", m, true);
    public void Dry(string m) => Write("DRY", m, true);

    public List<(string Level, string Message)> Drain()
    {
        lock (_lock)
        {
            var copy = _pending.ToList();
            _pending.Clear();
            return copy;
        }
    }

    private void Write(string level, string message, bool surface)
    {
        lock (_lock)
        {
            if (surface) _pending.Add((level, message));
            if (Ctx.DryRun) return; // dry-run never touches the disk
            try
            {
                if (_file == null)
                {
                    Directory.CreateDirectory(Paths.LogDir);
                    _file = Path.Combine(Paths.LogDir, $"winmizer-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                }
                File.AppendAllText(_file,
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}",
                    Encoding.UTF8);
            }
            catch { /* logging must never crash the app */ }
        }
    }
}
