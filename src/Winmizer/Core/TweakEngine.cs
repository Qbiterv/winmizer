using Microsoft.Win32;
using Winmizer.Core.Localization;
using Winmizer.Tweaks;

namespace Winmizer.Core;

public static class RestorePoint
{
    private const string SrKey = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore";
    private const string FreqName = "SystemRestorePointCreationFrequency";

    /// <summary>Creates a System Restore point (enables protection on C: if needed).</summary>
    public static (bool Ok, string Message) Create()
    {
        if (Ctx.DryRun)
        {
            Ctx.Log.Dry(L.T("Would create a system restore point (Checkpoint-Computer)."));
            return (true, "dry-run");
        }

        // Windows allows one restore point per 24h by default; lift that limit temporarily.
        bool had = Reg.TryGet(SrKey, FreqName, out var old, out var oldKind);
        try
        {
            Reg.Set(SrKey, FreqName, 0u, RegistryValueKind.DWord);
            var r = PS.Run(
                "Enable-ComputerRestore -Drive 'C:\\'; " +
                "Checkpoint-Computer -Description 'Winmizer - before optimization' -RestorePointType MODIFY_SETTINGS",
                timeoutMs: 300_000);
            if (!r.Ok) return (false, r.Text);
            Ctx.Backup.SetRestorePoint(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Ctx.Log.Info(L.T("System restore point created."));
            return (true, "OK");
        }
        catch (Exception ex) { return (false, ex.Message); }
        finally
        {
            try
            {
                if (had) Reg.Set(SrKey, FreqName, old!, oldKind);
                else Reg.Delete(SrKey, FreqName);
            }
            catch { }
        }
    }
}

public sealed class TweakEngine
{
    public IReadOnlyList<TweakResult> Apply(IEnumerable<ITweak> selected, Action<ITweak>? onStart = null,
        Action<TweakResult>? onDone = null)
    {
        var order = TweakCatalog.All.Select((t, i) => (t.Id, i)).ToDictionary(x => x.Id, x => x.i);
        var list = selected.OrderBy(t => order.TryGetValue(t.Id, out var i) ? i : int.MaxValue).ToList();
        var results = new List<TweakResult>();

        foreach (var t in list)
        {
            onStart?.Invoke(t);
            TweakResult res;
            try
            {
                if (!t.IsApplicable)
                    res = new TweakResult(t, ResultStatus.Skipped, OsInfo.NotApplicableReason(t.SupportedOn, t.MinBuild));
                else if (!t.IsAction && t.IsApplied())
                    res = new TweakResult(t, ResultStatus.Skipped, L.T("Already applied"));
                else
                {
                    Ctx.Log.Info($"APPLY {t.Id}");
                    t.Apply();
                    res = new TweakResult(t, Ctx.DryRun ? ResultStatus.DryRun : ResultStatus.Applied, t.ResultNote ?? "");
                }
            }
            catch (TweakSkippedException ex) { res = new TweakResult(t, ResultStatus.Skipped, ex.Message); }
            catch (Exception ex)
            {
                Ctx.Log.Error($"{t.Id}: {ex}");
                res = new TweakResult(t, ResultStatus.Failed, ex.Message);
            }
            Ctx.Backup.TrySave();
            results.Add(res);
            onDone?.Invoke(res);
        }
        return results;
    }

    public IReadOnlyList<TweakResult> Revert(IEnumerable<ITweak> selected, Action<ITweak>? onStart = null,
        Action<TweakResult>? onDone = null)
    {
        var results = new List<TweakResult>();
        foreach (var t in selected)
        {
            onStart?.Invoke(t);
            TweakResult res;
            try
            {
                if (!t.IsApplicable && !Ctx.Backup.Ids.Contains(t.Id))
                {
                    // Nothing was ever applied here, so there is nothing to restore.
                    res = new TweakResult(t, ResultStatus.Skipped, OsInfo.NotApplicableReason(t.SupportedOn, t.MinBuild));
                }
                else
                {
                    Ctx.Log.Info($"REVERT {t.Id}");
                    t.Revert();
                    res = new TweakResult(t, Ctx.DryRun ? ResultStatus.DryRun : ResultStatus.Reverted, "");
                }
            }
            catch (TweakSkippedException ex) { res = new TweakResult(t, ResultStatus.Skipped, ex.Message); }
            catch (Exception ex)
            {
                Ctx.Log.Error($"Revert {t.Id}: {ex}");
                res = new TweakResult(t, ResultStatus.Failed, ex.Message);
            }
            Ctx.Backup.TrySave();
            results.Add(res);
            onDone?.Invoke(res);
        }
        return results;
    }

    /// <summary>Reverts everything recorded in backup.json (tweaks and disabled startup entries).</summary>
    public IReadOnlyList<TweakResult> RevertAll(IEnumerable<string>? only = null, Action<string>? onStart = null,
        Action<TweakResult>? onDone = null)
    {
        var byId = TweakCatalog.All.ToDictionary(t => t.Id);
        var results = new List<TweakResult>();
        foreach (var id in (only ?? Ctx.Backup.Ids).ToList())
        {
            onStart?.Invoke(id);
            ITweak tweak = byId.TryGetValue(id, out var t) ? t : new BackupOnlyItem(id);
            try
            {
                if (t != null) t.Revert(); else Restorer.RestoreTweak(id);
                results.Add(new TweakResult(tweak, Ctx.DryRun ? ResultStatus.DryRun : ResultStatus.Reverted, ""));
            }
            catch (TweakSkippedException ex) { results.Add(new TweakResult(tweak, ResultStatus.Skipped, ex.Message)); }
            catch (Exception ex)
            {
                Ctx.Log.Error($"Revert {id}: {ex}");
                results.Add(new TweakResult(tweak, ResultStatus.Failed, ex.Message));
            }
            Ctx.Backup.TrySave();
            onDone?.Invoke(results[^1]);
        }
        return results;
    }

    /// <summary>Placeholder for backup entries without a catalog tweak (e.g. disabled startup apps).</summary>
    private sealed class BackupOnlyItem : TweakBase
    {
        public BackupOnlyItem(string id) : base(id, id.StartsWith("startup:") ? L.F("Startup: {0}", id[(id.IndexOf('|') + 1)..]) : id,
            "", TweakCategory.Services, RiskLevel.Safe) { }
        public override bool IsApplied() => true;
        public override void Apply() { }
    }
}
