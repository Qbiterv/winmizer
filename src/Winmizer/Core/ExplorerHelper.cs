using System.Diagnostics;
using Winmizer.Core.Localization;

namespace Winmizer.Core;

/// <summary>Restarts Windows Explorer so taskbar / Start / shell changes become visible.</summary>
public static class ExplorerHelper
{
    /// <summary>Kills explorer.exe and makes sure it runs again. Dry-run only logs.</summary>
    public static (bool Ok, string Message) Restart()
    {
        if (Ctx.DryRun)
        {
            Ctx.Log.Dry(L.T("Would restart Explorer (kill explorer.exe and start it again)."));
            return (true, "dry-run");
        }

        var kill = Proc.Run("taskkill.exe", new[] { "/F", "/IM", "explorer.exe" }, timeoutMs: 30_000);
        // taskkill exits with 128 when no explorer.exe was running; that is fine, we start one below.
        if (!kill.Ok && kill.ExitCode != 128)
            Ctx.Log.Warn(L.F("taskkill explorer.exe failed: {0}", kill.Text));

        // Windows normally relaunches the shell on its own; give it a moment before starting it ourselves.
        for (int i = 0; i < 20; i++)
        {
            Thread.Sleep(250);
            if (Process.GetProcessesByName("explorer").Length > 0) return Done();
        }
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = true });
            Thread.Sleep(1000);
            return Done();
        }
        catch (Exception ex)
        {
            Ctx.Log.Error(L.F("Cannot start explorer.exe: {0}", ex.Message));
            return (false, ex.Message);
        }

        (bool, string) Done()
        {
            Ctx.Log.Info("Explorer restarted");
            return (true, "OK");
        }
    }
}
