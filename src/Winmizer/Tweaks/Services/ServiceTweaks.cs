using Winmizer.Core;

namespace Winmizer.Tweaks.Services;

public static class ServiceTweaks
{
    public static IEnumerable<ITweak> Create()
    {
        // Spooler / PrintNotify and other critical services are deliberately NOT here (see ServiceHelper.Protected).
        yield return new ServiceTweak("DiagTrack", "Telemetria: Connected User Experiences and Telemetry (DiagTrack)",
            "Główna usługa wysyłająca dane diagnostyczne do Microsoft. Nic w systemie od niej nie zależy.", 4, RiskLevel.Safe, true);
        yield return new ServiceTweak("dmwappushservice", "Telemetria: WAP Push Message Routing (dmwappushservice)",
            "Pomocnicza usługa telemetrii/MDM. Niepotrzebna na komputerze domowym.", 4, RiskLevel.Safe, true);
        yield return new ServiceTweak("Fax", "Faks (Fax)",
            "Usługa FAKSU - nie ma nic wspólnego z drukowaniem. Drukarki i \"Drukuj do PDF\" działają bez zmian.", 4, RiskLevel.Safe, true);
        yield return new ServiceTweak("RetailDemo", "Tryb demonstracyjny sklepu (RetailDemo)",
            "Używany tylko w egzemplarzach pokazowych w sklepach.", 4, RiskLevel.Safe, true);
        yield return new ServiceTweak("MapsBroker", "Pobrane mapy (MapsBroker) -> ręczny",
            "Aktualizacje map offline; uruchamiany tylko gdy potrzebny.", 3, RiskLevel.Safe, true);
        yield return new ServiceTweak("WerSvc", "Raportowanie błędów Windows (WerSvc) -> ręczny",
            "Wysyłanie raportów awarii do Microsoft; uruchamia się na żądanie.", 3, RiskLevel.Safe, true);
        yield return new ServiceTweak("RemoteRegistry", "Zdalny rejestr (RemoteRegistry)",
            "Zdalna edycja rejestru z sieci - ryzyko bezpieczeństwa, niepotrzebna w domu. (W Windows 11 domyślnie już wyłączona.)", 4, RiskLevel.Safe, true);
        yield return new ServiceTweak("SysMain", "SysMain (Superfetch) -> wyłączona",
            "Wstępnie ładuje aplikacje do RAM. Na dysku SSD/NVMe zbędna i generuje zapisy; na HDD może pogorszyć start aplikacji.", 4, RiskLevel.Moderate, false);
        yield return new ServiceTweak("lfsvc", "Usługa lokalizacji geograficznej (lfsvc)",
            "Wyłącza usługę lokalizacji - aplikacje (Mapy, Pogoda) nie ustalą położenia.", 4, RiskLevel.Moderate, false);
        yield return new ServiceTweak("WSearch", "Windows Search (WSearch) -> ręczny",
            "Indeksowanie plików. Ręczny start = wolniejsze wyszukiwanie w Eksploratorze/Start, mniejsze obciążenie dysku.", 3, RiskLevel.Advanced, false);
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
            throw new InvalidOperationException($"Usługa {service} jest chroniona - nie można utworzyć tweaku.");
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
        var cur = ServiceHelper.Get(_service) ?? throw new TweakSkippedException("Usługa nie istnieje.");
        Ctx.Backup.Record(Id, new BackupEntry { Kind = "service", Key = _service, Value = $"{cur.Start}|{(cur.Delayed ? 1 : 0)}" });
        Ctx.Backup.Save();
        ServiceHelper.SetStart(_service, _target, false);
        try { ServiceHelper.Stop(_service); } catch (Exception ex) { Ctx.Log.Warn($"Nie zatrzymano {_service}: {ex.Message}"); }
    }
}
