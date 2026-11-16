using Winmizer.Core;

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

        yield return new FolderCleanTweak("cleanup.temp", "Wyczyść pliki tymczasowe (%TEMP% i Windows\\Temp)",
            "Usuwa pliki tymczasowe użytkownika i systemu. Pliki używane przez działające programy są pomijane. Prefetch NIE jest czyszczony.",
            RiskLevel.Safe, new[] { Path.GetTempPath(), Path.Combine(win, "Temp") }) { Recommended = true };

        yield return new FolderCleanTweak("cleanup.shader", "Wyczyść pamięć podręczną shaderów (DirectX/NVIDIA/AMD)",
            "Usuwa D3DSCache, NVIDIA DXCache/GLCache i AMD DxCache. Gry skompilują shadery od nowa - pierwsze uruchomienie może się lekko dławić.",
            RiskLevel.Moderate, new[]
            {
                Path.Combine(local, "D3DSCache"), Path.Combine(local, "NVIDIA", "DXCache"), Path.Combine(local, "NVIDIA", "GLCache"),
                Path.Combine(local, "AMD", "DxCache"), Path.Combine(local, "AMD", "DxcCache"), Path.Combine(local, "AMD", "GLCache")
            });

        yield return new WindowsUpdateCacheTweak();
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

    /// <summary>Deletes the contents of a folder (keeps the folder). Locked files are skipped.</summary>
    public static void Clean(string path)
    {
        if (!Directory.Exists(path)) return;
        var di = new DirectoryInfo(path);
        foreach (var f in di.EnumerateFiles())
            try { f.Delete(); } catch { }
        foreach (var d in di.EnumerateDirectories())
            try { d.Delete(true); } catch { }
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
            Ctx.Log.Dry($"Usunąłbym do {CleanupTweaks.FormatSize(before)} z: {string.Join("; ", _folders)}");
            ResultNote = "do zwolnienia ~" + CleanupTweaks.FormatSize(before);
            return;
        }
        foreach (var f in _folders) FolderCleaner.Clean(f);
        long after = _folders.Sum(FolderCleaner.Size);
        FreedBytes = Math.Max(0, before - after);
        ResultNote = "zwolniono " + CleanupTweaks.FormatSize(FreedBytes);
        Ctx.Log.Info($"{Id}: {ResultNote}");
    }
}

public sealed class WindowsUpdateCacheTweak : ActionTweak
{
    public WindowsUpdateCacheTweak() : base("cleanup.wucache", "Wyczyść pamięć podręczną Windows Update (SoftwareDistribution\\Download)",
        "Zatrzymuje usługę wuauserv, usuwa pobrane pliki aktualizacji i uruchamia usługę ponownie. Aktualizacje zostaną pobrane ponownie w razie potrzeby.",
        TweakCategory.Cleanup, RiskLevel.Moderate)
    { }

    public override void Apply()
    {
        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
        long before = FolderCleaner.Size(dir);
        if (Ctx.DryRun)
        {
            Ctx.Log.Dry($"Zatrzymałbym wuauserv, usunął {CleanupTweaks.FormatSize(before)} z {dir} i uruchomił wuauserv ponownie.");
            ResultNote = "do zwolnienia ~" + CleanupTweaks.FormatSize(before);
            return;
        }
        bool wasRunning = ServiceHelper.IsRunning("wuauserv");
        try
        {
            ServiceHelper.Stop("wuauserv");
            FolderCleaner.Clean(dir);
        }
        finally
        {
            if (wasRunning) ServiceHelper.StartNow("wuauserv"); // always restore the update service
        }
        FreedBytes = Math.Max(0, before - FolderCleaner.Size(dir));
        ResultNote = "zwolniono " + CleanupTweaks.FormatSize(FreedBytes);
    }
}

public sealed class RecycleBinTweak : ActionTweak
{
    public RecycleBinTweak() : base("cleanup.recyclebin", "Opróżnij Kosz",
        "Trwale usuwa wszystko z Kosza.", TweakCategory.Cleanup, RiskLevel.Moderate)
    {
        Warning = "Operacja nieodwracalna - sprawdź zawartość Kosza.";
    }

    public override void Apply()
    {
        var r = PS.Run("Clear-RecycleBin -Force -ErrorAction SilentlyContinue", description: "Opróżniłbym Kosz");
        if (!r.Ok) throw new InvalidOperationException(r.Text);
    }
}

public sealed class DismCleanupTweak : ActionTweak
{
    public DismCleanupTweak() : base("cleanup.dism", "DISM: czyszczenie składników (StartComponentCleanup)",
        "DISM /Online /Cleanup-Image /StartComponentCleanup - usuwa stare wersje składników systemu. Bezpieczne, ale trwa kilka-kilkanaście minut.",
        TweakCategory.Cleanup, RiskLevel.Advanced)
    { }

    public override void Apply()
    {
        var r = Proc.Run("dism.exe", new[] { "/Online", "/Cleanup-Image", "/StartComponentCleanup" },
            timeoutMs: 40 * 60_000, description: "Uruchomiłbym DISM /Online /Cleanup-Image /StartComponentCleanup");
        if (!r.Ok) throw new InvalidOperationException("DISM: kod " + r.ExitCode);
    }
}
