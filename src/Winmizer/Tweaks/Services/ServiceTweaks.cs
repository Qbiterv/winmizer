using Winmizer.Core;
using Winmizer.Core.Localization;

namespace Winmizer.Tweaks.Services;

public static class ServiceTweaks
{
    public static IEnumerable<ITweak> Create()
    {
        // Spooler / PrintNotify and other critical services are deliberately NOT here (see ServiceHelper.Protected).
        yield return new ServiceTweak("DiagTrack", "Telemetry: Connected User Experiences and Telemetry (DiagTrack)",
            "The main service that sends diagnostic data to Microsoft. Nothing in the system depends on it.", 4, RiskLevel.Safe, true);
        yield return new ServiceTweak("dmwappushservice", "Telemetry: WAP Push Message Routing (dmwappushservice)",
            "Helper service for telemetry/MDM. Not needed on a home PC.", 4, RiskLevel.Safe, true);
        yield return new ServiceTweak("Fax", "Fax (Fax)",
            "The FAX service - unrelated to printing. Printers and \"Print to PDF\" work as before.", 4, RiskLevel.Safe, true);
        yield return new ServiceTweak("RetailDemo", "Retail demo mode (RetailDemo)",
            "Only used on store display units.", 4, RiskLevel.Safe, true);
        yield return new ServiceTweak("MapsBroker", "Downloaded maps (MapsBroker) -> manual",
            "Offline map updates; started only when needed.", 3, RiskLevel.Safe, true);
        yield return new ServiceTweak("WerSvc", "Windows Error Reporting (WerSvc) -> manual",
            "Sends crash reports to Microsoft; starts on demand.", 3, RiskLevel.Safe, true);
        yield return new ServiceTweak("RemoteRegistry", "Remote Registry (RemoteRegistry)",
            "Remote registry editing over the network - a security risk, unnecessary at home. (Already disabled by default on Windows 11.)", 4, RiskLevel.Safe, true);
        yield return new ServiceTweak("SysMain", "SysMain (Superfetch) -> disabled",
            "Preloads apps into RAM. Pointless on an SSD/NVMe and causes extra writes; on an HDD it may slow app launches.", 4, RiskLevel.Moderate, false);
        yield return new ServiceTweak("lfsvc", "Geolocation service (lfsvc)",
            "Turns off the location service - apps (Maps, Weather) can't determine your position.", 4, RiskLevel.Moderate, false);
        yield return new ServiceTweak("WSearch", "Windows Search (WSearch) -> manual",
            "File indexing. Manual start = slower search in Explorer/Start, less disk load.", 3, RiskLevel.Advanced, false);
    }
}

/// <summary>Changes a service start type (disabled/manual) and records the original.</summary>
public sealed class ServiceTweak : TweakBase
{
    private readonly string _service;
    private readonly int _target;

    public ServiceTweak(string service, string name, string description, int targetStart, RiskLevel risk, bool recommended)
        : base("services." + service.ToLowerInvariant(), name, description, TweakCategory.Services, risk)
    {
        if (ServiceHelper.IsProtected(service))
            throw new InvalidOperationException(L.F("Service {0} is protected - cannot create a tweak for it.", service));
        _service = service; _target = targetStart; Recommended = recommended;
    }

    public override bool IsApplied()
    {
        var cur = ServiceHelper.Get(_service);
        if (cur == null) return true; // service absent - nothing to do
        return _target == 4 ? cur.Value.Start == 4 : cur.Value.Start is 3 or 4;
    }

    public override void Apply()
    {
        var cur = ServiceHelper.Get(_service) ?? throw new TweakSkippedException(L.T("Service does not exist."));
        Ctx.Backup.Record(Id, new BackupEntry { Kind = "service", Key = _service, Value = $"{cur.Start}|{(cur.Delayed ? 1 : 0)}" });
        Ctx.Backup.Save();
        ServiceHelper.SetStart(_service, _target, false);
        try { ServiceHelper.Stop(_service); } catch (Exception ex) { Ctx.Log.Warn(L.F("Could not stop {0}: {1}", _service, ex.Message)); }
    }
}
