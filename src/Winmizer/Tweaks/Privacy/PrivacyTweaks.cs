using Winmizer.Core;
using static Winmizer.Core.R;

namespace Winmizer.Tweaks.Privacy;

public static class PrivacyTweaks
{
    private const string Cdm = @"HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
    private const string PolWin = @"HKLM\SOFTWARE\Policies\Microsoft\Windows";

    private static IEnumerable<RegSet> StartAdsEntries()
    {
        yield return Dw(Cdm, "SystemPaneSuggestionsEnabled", 0);
        yield return Dw(Cdm, "SubscribedContent-338388Enabled", 0);
        yield return Dw(Cdm, "SubscribedContent-338389Enabled", 0);
        yield return Dw(Cdm, "SubscribedContent-353694Enabled", 0);
        yield return Dw(Cdm, "SubscribedContent-353696Enabled", 0);
        yield return Dw(Cdm, "SubscribedContent-310093Enabled", 0);
        yield return Dw(Cdm, "SubscribedContent-338393Enabled", 0);
        // Windows 11-only Start menu keys: not written on Windows 10.
        if (OsInfo.IsWin11)
        {
            yield return Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", 0);
            yield return Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", 0);
        }
    }

    public static IEnumerable<ITweak> Create()
    {
        var c = TweakCategory.Privacy;

        yield return new RegistryTweak("privacy.telemetry", "Telemetria: poziom podstawowy",
            "AllowTelemetry=1 (Podstawowe/Wymagane). Wartość 0 działa tylko w Enterprise, dlatego jej nie używamy. Usługi DiagTrack wyłączasz w kategorii Usługi.",
            c, RiskLevel.Safe, Dw(PolWin + @"\DataCollection", "AllowTelemetry", 1)) { Recommended = true };

        yield return new RegistryTweak("privacy.adid", "Wyłącz identyfikator reklamowy",
            "Aplikacje nie mogą używać Advertising ID do profilowania reklam.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0),
            Dw(PolWin + @"\AdvertisingInfo", "DisabledByGroupPolicy", 1)) { Recommended = true };

        yield return new RegistryTweak("privacy.tailored", "Wyłącz spersonalizowane środowiska",
            "Microsoft nie używa danych diagnostycznych do dopasowywania porad i reklam.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0)) { Recommended = true };

        yield return new RegistryTweak("privacy.activity", "Wyłącz historię aktywności",
            "Wyłącza zbieranie i wysyłanie historii aktywności (Oś czasu).",
            c, RiskLevel.Safe,
            Dw(PolWin + @"\System", "EnableActivityFeed", 0),
            Dw(PolWin + @"\System", "PublishUserActivities", 0),
            Dw(PolWin + @"\System", "UploadUserActivities", 0)) { Recommended = true };

        yield return new RegistryTweak("privacy.consumer", "Wyłącz funkcje konsumenckie i sugerowane aplikacje",
            "DisableWindowsConsumerFeatures=1 - Windows nie instaluje automatycznie promowanych aplikacji (Candy Crush itp.).",
            c, RiskLevel.Safe,
            Dw(PolWin + @"\CloudContent", "DisableWindowsConsumerFeatures", 1),
            Dw(Cdm, "SilentInstalledAppsEnabled", 0),
            Dw(Cdm, "PreInstalledAppsEnabled", 0),
            Dw(Cdm, "OemPreInstalledAppsEnabled", 0),
            Dw(Cdm, "ContentDeliveryAllowed", 0)) { Recommended = true };

        yield return new RegistryTweak("privacy.startads", "Wyłącz reklamy i sugestie w Menu Start",
            "Usuwa sugerowane aplikacje/reklamy w Start i Ustawieniach oraz polecane elementy.",
            c, RiskLevel.Safe,
            () => StartAdsEntries()) { Recommended = true };

        yield return new RegistryTweak("privacy.lockscreen", "Wyłącz wskazówki i reklamy na ekranie blokady",
            "Ekran blokady bez \"ciekawostek\", Spotlight-reklam i wskazówek.",
            c, RiskLevel.Safe,
            Dw(Cdm, "RotatingLockScreenOverlayEnabled", 0),
            Dw(Cdm, "SubscribedContent-338387Enabled", 0),
            Dw(Cdm, "SoftLandingEnabled", 0)) { Recommended = true };

        yield return new TelemetryTasksTweak();

        yield return new RegistryTweak("privacy.cortana", "Wyłącz Cortanę (zasada AllowCortana=0)",
            "Wyłącza Cortanę przez zasadę systemową. Bezpieczne - Wyszukiwanie Windows działa normalnie.",
            c, RiskLevel.Safe, Dw(PolWin + @"\Windows Search", "AllowCortana", 0)) { Recommended = true };

        yield return new RegistryTweak("privacy.copilot", "Wyłącz Windows Copilot (zasada)",
            "TurnOffWindowsCopilot=1 - ukrywa i wyłącza Copilot w systemie.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1),
            Dw(PolWin + @"\WindowsCopilot", "TurnOffWindowsCopilot", 1)) { Recommended = true, SupportedOn = OsSupport.Win11 };

        // ---- Windows 10 equivalents (shown only on Windows 10) ----
        yield return new RegistryTweak("privacy.cortana.win10", "Windows 10: ogranicz Cortanę (ekran blokady, zgoda)",
            "Cortana na Windows 10 to osobny komponent: blokuje ją na ekranie blokady (AllowCortanaAboveLock=0) i cofa zgodę użytkownika (CortanaConsent=0). Uzupełnia zasadę AllowCortana=0.",
            c, RiskLevel.Safe,
            Dw(PolWin + @"\Windows Search", "AllowCortanaAboveLock", 0),
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 0))
        { Recommended = true, SupportedOn = OsSupport.Win10 };

        yield return new RegistryTweak("privacy.newsfeed", "Windows 10: wyłącz \"Wiadomości i zainteresowania\" na pasku zadań",
            "Zasada EnableFeeds=0 usuwa widżet pogody/wiadomości z paska zadań (reklamy i śledzenie MSN, zużycie RAM przez Edge WebView).",
            c, RiskLevel.Safe,
            Dw(PolWin + @"\Windows Feeds", "EnableFeeds", 0))
        { Recommended = true, SupportedOn = OsSupport.Win10, MinBuild = 19041 };

        yield return new RegistryTweak("privacy.websearch", "Wyłącz wyszukiwanie internetowe (Bing) w Start",
            "Wyszukiwanie w Start pokazuje tylko wyniki lokalne (BingSearchEnabled=0, DisableSearchBoxSuggestions=1).",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0),
            Dw(@"HKCU\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1)) { Recommended = true };
    }
}

/// <summary>Disables telemetry scheduled tasks; stores each task's original state.</summary>
public sealed class TelemetryTasksTweak : TweakBase
{
    private static readonly (string Path, string Name)[] Tasks =
    {
        (@"\Microsoft\Windows\Application Experience\", "Microsoft Compatibility Appraiser"),
        (@"\Microsoft\Windows\Application Experience\", "ProgramDataUpdater"),
        (@"\Microsoft\Windows\Customer Experience Improvement Program\", "Consolidator"),
        (@"\Microsoft\Windows\Customer Experience Improvement Program\", "UsbCeip"),
        (@"\Microsoft\Windows\Feedback\Siuf\", "DmClient"),
        (@"\Microsoft\Windows\Feedback\Siuf\", "DmClientOnScenarioDownload"),
    };

    public TelemetryTasksTweak() : base("privacy.tasks", "Wyłącz zadania telemetryczne",
        "Wyłącza harmonogramowane zadania: Compatibility Appraiser, ProgramDataUpdater, Consolidator, UsbCeip, DmClient. Windows Update nie jest naruszony.",
        TweakCategory.Privacy, RiskLevel.Safe)
    { }

    public override bool Recommended => true;

    private static string ListScript =>
        string.Join("; ", Tasks.Select(t =>
            $"$t=Get-ScheduledTask -TaskPath '{PS.Q(t.Path)}' -TaskName '{PS.Q(t.Name)}' -ErrorAction SilentlyContinue; " +
            $"if($t){{ '{PS.Q(t.Path)}|{PS.Q(t.Name)}|' + $t.State }}"));

    private static List<(string Path, string Name, string State)> Query()
    {
        var r = PS.Query(ListScript);
        var res = new List<(string, string, string)>();
        foreach (var line in r.Out.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var p = line.Trim().Split('|');
            if (p.Length == 3) res.Add((p[0], p[1], p[2]));
        }
        return res;
    }

    public override bool IsApplied() => Query().All(t => t.State == "Disabled");

    public override void Apply()
    {
        foreach (var t in Query().Where(t => t.State != "Disabled"))
        {
            Ctx.Backup.Record(Id, new BackupEntry { Kind = "task", Key = t.Path, Name = t.Name, Value = t.State });
            Ctx.Backup.Save();
            var r = PS.Run($"Disable-ScheduledTask -TaskPath '{PS.Q(t.Path)}' -TaskName '{PS.Q(t.Name)}' | Out-Null",
                description: $"Wyłączyłbym zadanie {t.Path}{t.Name}");
            if (!r.Ok) Ctx.Log.Warn($"Zadanie {t.Name}: {r.Text}");
        }
    }
}
