using Winmizer.Core;
using Winmizer.Core.Localization;

namespace Winmizer.Tweaks.Cleanup;

public static class CleanupTweaks
{
    public static string FormatSize(long bytes)
    {
        string[] u = { "B", "KB", "MB", "GB", "TB" };
        double v = bytes; int i = 0;
        while (v >= 1024 && i < u.Length - 1) { v /= 1024; i++; }
        return $"{v:0.##} {u[i]}";
    }

    public static IEnumerable<ITweak> Create()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var win = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        yield return new FolderCleanTweak("cleanup.temp", "Clean temporary files (%TEMP% and Windows\\Temp)",
            "Deletes user and system temporary files. Files in use by running programs are skipped. Prefetch is NOT cleaned.",
            RiskLevel.Safe, new[] { Path.GetTempPath(), Path.Combine(win, "Temp") }) { Recommended = true };

        yield return new FolderCleanTweak("cleanup.shader", "Clear shader caches (DirectX/NVIDIA/AMD)",
            "Deletes D3DSCache, NVIDIA DXCache/GLCache and AMD DxCache. Games recompile shaders from scratch - the first launch may stutter a little.",
            RiskLevel.Moderate, new[]
            {
                Path.Combine(local, "D3DSCache"), Path.Combine(local, "NVIDIA", "DXCache"), Path.Combine(local, "NVIDIA", "GLCache"),
                Path.Combine(local, "AMD", "DxCache"), Path.Combine(local, "AMD", "DxcCache"), Path.Combine(local, "AMD", "GLCache")
            });
        yield return new RecycleBinTweak();
        yield return new DismCleanupTweak();
    }
}

internal static class FolderCleaner
{
    public static long Size(string path)
    {
        long total = 0;
        try
        {
            if (!Directory.Exists(path)) return 0;
            foreach (var f in new DirectoryInfo(path).EnumerateFiles("*", SearchOption.AllDirectories))
                try { total += f.Length; } catch { }
        }
        catch { }
        return total;
    }

    /// <summary>True if the folder is a drive root or a well-known system/profile folder that must never be emptied.</summary>
    public static bool IsDangerous(string path)
    {
        try
        {
            string full = Path.GetFullPath(path).TrimEnd('\\', '/');
            if (full.Length <= 3 || Path.GetPathRoot(full)!.TrimEnd('\\', '/').Equals(full, StringComparison.OrdinalIgnoreCase)) return true;
            var bad = new[]
            {
                Environment.SpecialFolder.Windows, Environment.SpecialFolder.System, Environment.SpecialFolder.UserProfile,
                Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolder.ProgramFilesX86,
                Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolder.Desktop,
                Environment.SpecialFolder.MyDocuments
            };
            return bad.Select(Environment.GetFolderPath).Where(p => p.Length > 0)
                .Any(p => p.TrimEnd('\\', '/').Equals(full, StringComparison.OrdinalIgnoreCase));
        }
        catch { return true; }
    }

    /// <summary>Deletes the contents of a folder (keeps the folder). Locked files are skipped;
    /// junctions/symlinks are never followed.</summary>
    public static void Clean(string path)
    {
        if (!Directory.Exists(path)) return;
        if (IsDangerous(path))
        {
            Ctx.Log.Warn(L.F("Skipped dangerous cleanup folder: {0}", path));
            return;
        }
        var di = new DirectoryInfo(path);
        foreach (var f in di.EnumerateFiles())
            try { f.Delete(); } catch { }
        foreach (var d in di.EnumerateDirectories())
        {
            try
            {
                if (d.Attributes.HasFlag(FileAttributes.ReparsePoint)) d.Delete(false); // remove the link only
                else d.Delete(true);
            }
            catch { }
        }
    }
}
public sealed class FolderCleanTweak : ActionTweak
{
    private readonly string[] _folders;

    public FolderCleanTweak(string id, string name, string description, RiskLevel risk, string[] folders)
        : base(id, name, description, TweakCategory.Cleanup, risk) => _folders = folders;

    public override void Apply()
    {
        long before = _folders.Sum(FolderCleaner.Size);
        if (Ctx.DryRun)
        {
            Ctx.Log.Dry(L.F("Would delete up to {0} from: {1}", CleanupTweaks.FormatSize(before), string.Join("; ", _folders)));
            ResultNote = L.F("~{0} to free", CleanupTweaks.FormatSize(before));
            return;
        }
        foreach (var f in _folders) FolderCleaner.Clean(f);
        long after = _folders.Sum(FolderCleaner.Size);
        FreedBytes = Math.Max(0, before - after);
        ResultNote = L.F("freed {0}", CleanupTweaks.FormatSize(FreedBytes));
        Ctx.Log.Info($"{Id}: {ResultNote}");
    }
}

public sealed class RecycleBinTweak : ActionTweak
{
    public RecycleBinTweak() : base("cleanup.recyclebin", "Empty the Recycle Bin",
        "Permanently deletes everything in the Recycle Bin.", TweakCategory.Cleanup, RiskLevel.Moderate)
    {
        Warning = "This cannot be undone - check the Recycle Bin contents first.";
    }

    public override void Apply()
    {
        var r = PS.Run("Clear-RecycleBin -Force -ErrorAction SilentlyContinue", description: L.T("Would empty the Recycle Bin"));
        if (!r.Ok) throw new InvalidOperationException(r.Text);
    }
}

public sealed class DismCleanupTweak : ActionTweak
{
    public DismCleanupTweak() : base("cleanup.dism", "DISM: component cleanup (StartComponentCleanup)",
        "DISM /Online /Cleanup-Image /StartComponentCleanup - removes old versions of system components. Safe, but takes several to a dozen or so minutes.",
        TweakCategory.Cleanup, RiskLevel.Advanced)
    { }

    public override void Apply()
    {
        var r = Proc.Run("dism.exe", new[] { "/Online", "/Cleanup-Image", "/StartComponentCleanup" },
            timeoutMs: 40 * 60_000, description: L.T("Would run DISM /Online /Cleanup-Image /StartComponentCleanup"));
        if (!r.Ok) throw new InvalidOperationException(L.F("DISM: exit code {0}", r.ExitCode));
    }
}
