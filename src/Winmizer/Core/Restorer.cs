using Microsoft.Win32;
using Winmizer.Core.Localization;

namespace Winmizer.Core;

/// <summary>Restores saved originals from the backup store (generic Revert implementation).</summary>
public static class Restorer
{
    public static void RestoreTweak(string id)
    {
        var entries = Ctx.Backup.Entries(id).ToList();
        if (entries.Count == 0) throw new TweakSkippedException(L.T("No backup - nothing to restore."));

        var errors = new List<string>();
        foreach (var e in Enumerable.Reverse(entries)) // undo in reverse order
        {
            try { RestoreEntry(e); }
            catch (Exception ex) { errors.Add($"{e.Kind}:{e.Key}: {ex.Message}"); Ctx.Log.Error($"Revert {id} {e.Kind} {e.Key}: {ex.Message}"); }
        }
        if (errors.Count > 0) throw new InvalidOperationException(string.Join("; ", errors));
        Ctx.Backup.Remove(id);
    }

    private static void RestoreEntry(BackupEntry e)
    {
        switch (e.Kind)
        {
            case "reg":
                if (!e.Existed) Reg.Delete(e.Key, e.Name);
                else
                {
                    var kind = Enum.Parse<RegistryValueKind>(e.ValueKind ?? "String");
                    Reg.Set(e.Key, e.Name, Reg.Deserialize(e.Value ?? "", kind), kind);
                }
                break;

            case "service":
                {
                    var parts = (e.Value ?? "3|0").Split('|');
                    int start = int.Parse(parts[0]);
                    ServiceHelper.SetStart(e.Key, start, parts.Length > 1 && parts[1] == "1");
                    if (start == 2) ServiceHelper.StartNow(e.Key);
                    break;
                }

            case "task":
                if (e.Value != "Disabled")
                {
                    var r = PS.Run($"Enable-ScheduledTask -TaskPath '{PS.Q(e.Key)}' -TaskName '{PS.Q(e.Name)}' | Out-Null",
                        description: L.F("Would enable task {0}{1}", e.Key, e.Name));
                    if (!r.Ok) throw new InvalidOperationException(r.Text);
                }
                break;

            case "power":
                {
                    var p = e.Key.Split('|'); // scheme|sub|setting
                    PowerCfg.SetIndex(p[0], p[1], p[2], e.Name == "ac", uint.Parse(e.Value ?? "0"));
                    break;
                }

            case "fsutil":
                {
                    var r = Proc.Run("fsutil.exe", new[] { "behavior", "set", e.Key, e.Value ?? "0" },
                        description: L.F("Would restore fsutil {0} = {1}", e.Key, e.Value));
                    if (!r.Ok) throw new InvalidOperationException(r.Text);
                    break;
                }

            case "dns":
                DnsHelper.Set(e.Key, e.Value ?? "");
                break;

            case "hibernate":
                Proc.Run("powercfg.exe", new[] { "-h", e.Value ?? "on" }, description: L.F("Would run powercfg -h {0}", e.Value));
                break;

            case "activescheme":
                PowerCfg.SetActive(e.Key);
                break;

            case "createdscheme":
                if (PowerCfg.Active().Guid.Equals(e.Key, StringComparison.OrdinalIgnoreCase))
                    Ctx.Log.Warn(L.T("The Winmizer Ultimate plan is still active - not deleting it."));
                else PowerCfg.Delete(e.Key);
                break;

            case "startupreg":
            case "startupfile":
                StartupManager.Restore(e);
                break;

            default:
                Ctx.Log.Warn(L.F("Unknown backup entry kind: {0}", e.Kind));
                break;
        }
    }
}
