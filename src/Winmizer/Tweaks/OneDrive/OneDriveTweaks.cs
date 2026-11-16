using Winmizer.Core;
using static Winmizer.Core.R;

namespace Winmizer.Tweaks.OneDrive;

public static class OneDriveTweaks
{
    private const string NavClsid = @"HKLM\SOFTWARE\Classes\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}";
    private const string NavClsid32 = @"HKLM\SOFTWARE\Classes\Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}";

    public static IEnumerable<ITweak> Create()
    {
        var c = TweakCategory.OneDrive;

        yield return new RegistryTweak("onedrive.policy", "OneDrive: wyłącz synchronizację zasadą (DisableFileSyncNGSC)",
            "Zasada blokująca uruchamianie synchronizacji plików OneDrive. Pliki lokalne pozostają nietknięte.",
            c, RiskLevel.Moderate,
            Dw(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\OneDrive", "DisableFileSyncNGSC", 1))
        { Warning = "Synchronizacja z chmurą zostanie zatrzymana. Pliki dostępne tylko online nie są usuwane." };

        yield return new RegistryTweak("onedrive.navpane", "OneDrive: usuń z panelu nawigacji Eksploratora",
            "Ukrywa ikonę OneDrive w lewym panelu Eksploratora plików.",
            c, RiskLevel.Safe,
            Dw(NavClsid, "System.IsPinnedToNameSpaceTree", 0),
            Dw(NavClsid32, "System.IsPinnedToNameSpaceTree", 0));

        yield return new RegistryTweak("onedrive.startup", "OneDrive: usuń z autostartu",
            "Usuwa wpisy Run uruchamiające OneDrive przy logowaniu (kopia zapasowa pozwala je przywrócić).",
            c, RiskLevel.Safe,
            Rm(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Run", "OneDrive"),
            Rm(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Run", "OneDriveSetup"));

        yield return new OneDriveUninstallTweak();
    }
}

public sealed class OneDriveUninstallTweak : TweakBase
{
    public OneDriveUninstallTweak() : base("onedrive.uninstall", "OneDrive: pełna deinstalacja",
        "Zamyka proces OneDrive i uruchamia oficjalny deinstalator (OneDriveSetup.exe /uninstall, awaryjnie winget).",
        TweakCategory.OneDrive, RiskLevel.Moderate)
    {
        Warning = "Lokalna synchronizacja przestanie działać. Pliki istniejące WYŁĄCZNIE w chmurze nie są kasowane, ale stają się niedostępne z tego komputera do czasu ponownej instalacji. Najpierw upewnij się, że pliki są pobrane lokalnie.";
    }

    private static IEnumerable<string> Candidates => new[]
    {
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "OneDrive", "OneDrive.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft OneDrive", "OneDrive.exe"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft OneDrive", "OneDrive.exe"),
    };

    private static string? Setup()
    {
        var win = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        return new[] { Path.Combine(win, "SysWOW64", "OneDriveSetup.exe"), Path.Combine(win, "System32", "OneDriveSetup.exe") }
            .FirstOrDefault(File.Exists);
    }

    public override bool IsReversible => true;
    public override bool IsApplied() => !Candidates.Any(File.Exists);

    public override void Apply()
    {
        Proc.Run("taskkill.exe", new[] { "/f", "/im", "OneDrive.exe" }, description: "Zamknąłbym proces OneDrive.exe");
        Ctx.Backup.Record(Id, new BackupEntry { Kind = "onedrive", Key = "installed", Value = "1" });
        Ctx.Backup.Save();

        var setup = Setup();
        if (setup != null)
            Proc.Run(setup, new[] { "/uninstall" }, timeoutMs: 180_000, description: $"Uruchomiłbym {setup} /uninstall");
        else Ctx.Log.Warn("Nie znaleziono OneDriveSetup.exe - próbuję winget.");

        if (Ctx.DryRun) return;
        if (!IsApplied())
        {
            var r = Proc.Run("winget.exe", new[] { "uninstall", "-e", "--id", "Microsoft.OneDrive", "--silent", "--accept-source-agreements" },
                timeoutMs: 300_000);
            if (!r.Ok && !IsApplied()) throw new InvalidOperationException("Deinstalacja OneDrive nie powiodła się: " + r.Text);
        }
    }

    public override void Revert()
    {
        var setup = Setup();
        if (setup != null)
        {
            var r = Proc.Run(setup, Array.Empty<string>(), timeoutMs: 300_000, description: $"Uruchomiłbym {setup} (ponowna instalacja OneDrive)");
            if (r.Ok) { Ctx.Backup.Remove(Id); return; }
        }
        var w = Proc.Run("winget.exe", new[] { "install", "-e", "--id", "Microsoft.OneDrive", "--silent", "--accept-package-agreements", "--accept-source-agreements" },
            timeoutMs: 600_000, description: "Uruchomiłbym winget install Microsoft.OneDrive");
        if (!w.Ok) throw new InvalidOperationException("Nie udało się zainstalować OneDrive ponownie. Zainstaluj ręcznie ze strony Microsoft.");
        Ctx.Backup.Remove(Id);
    }
}
