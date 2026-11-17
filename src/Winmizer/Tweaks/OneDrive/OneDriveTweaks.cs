using Winmizer.Core;
using Winmizer.Core.Localization;
using static Winmizer.Core.R;

namespace Winmizer.Tweaks.OneDrive;

public static class OneDriveTweaks
{
    private const string NavClsid = @"HKLM\SOFTWARE\Classes\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}";
    private const string NavClsid32 = @"HKLM\SOFTWARE\Classes\Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}";

    public static IEnumerable<ITweak> Create()
    {
        var c = TweakCategory.OneDrive;

        yield return new RegistryTweak("onedrive.policy", "OneDrive: disable sync via policy (DisableFileSyncNGSC)",
            "A policy that blocks OneDrive file sync from running. Local files are left untouched.",
            c, RiskLevel.Moderate,
            Dw(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\OneDrive", "DisableFileSyncNGSC", 1))
        { Warning = "Cloud sync will stop. Online-only files are not deleted." };

        yield return new RegistryTweak("onedrive.navpane", "OneDrive: remove from the File Explorer navigation pane",
            "Hides the OneDrive icon in the left pane of File Explorer.",
            c, RiskLevel.Safe,
            Dw(NavClsid, "System.IsPinnedToNameSpaceTree", 0),
            Dw(NavClsid32, "System.IsPinnedToNameSpaceTree", 0));

        yield return new RegistryTweak("onedrive.startup", "OneDrive: remove from startup",
            "Removes the Run entries that launch OneDrive at sign-in (the backup lets you restore them).",
            c, RiskLevel.Safe,
            Rm(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Run", "OneDrive"),
            Rm(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Run", "OneDriveSetup"));

        yield return new OneDriveUninstallTweak();
    }
}

public sealed class OneDriveUninstallTweak : TweakBase
{
    public OneDriveUninstallTweak() : base("onedrive.uninstall", "OneDrive: full uninstall",
        "Closes the OneDrive process and runs the official uninstaller (OneDriveSetup.exe /uninstall, winget as a fallback).",
        TweakCategory.OneDrive, RiskLevel.Moderate)
    {
        Warning = "Local sync will stop working. Files that exist ONLY in the cloud are not deleted, but become unavailable from this PC until you reinstall. Make sure your files are downloaded locally first.";
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
        Proc.Run("taskkill.exe", new[] { "/f", "/im", "OneDrive.exe" }, description: L.T("Would close the OneDrive.exe process"));
        Ctx.Backup.Record(Id, new BackupEntry { Kind = "onedrive", Key = "installed", Value = "1" });
        Ctx.Backup.Save();

        var setup = Setup();
        if (setup != null)
            Proc.Run(setup, new[] { "/uninstall" }, timeoutMs: 180_000, description: L.F("Would run {0} /uninstall", setup));
        else Ctx.Log.Warn(L.T("OneDriveSetup.exe not found - trying winget."));

        if (Ctx.DryRun) return;
        if (!IsApplied())
        {
            var r = Proc.Run("winget.exe", new[] { "uninstall", "-e", "--id", "Microsoft.OneDrive", "--silent", "--accept-source-agreements" },
                timeoutMs: 300_000);
            if (!r.Ok && !IsApplied()) throw new InvalidOperationException(L.F("OneDrive uninstall failed: {0}", r.Text));
        }
    }

    public override void Revert()
    {
        var setup = Setup();
        if (setup != null)
        {
            var r = Proc.Run(setup, Array.Empty<string>(), timeoutMs: 300_000, description: L.F("Would run {0} (reinstall OneDrive)", setup));
            if (r.Ok) { Ctx.Backup.Remove(Id); return; }
        }
        var w = Proc.Run("winget.exe", new[] { "install", "-e", "--id", "Microsoft.OneDrive", "--silent", "--accept-package-agreements", "--accept-source-agreements" },
            timeoutMs: 600_000, description: L.T("Would run winget install Microsoft.OneDrive"));
        if (!w.Ok) throw new InvalidOperationException(L.T("Could not reinstall OneDrive. Install it manually from the Microsoft website."));
        Ctx.Backup.Remove(Id);
    }
}
