using Winmizer.Core;
using Winmizer.Core.Localization;
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

        yield return new RegistryTweak("privacy.telemetry", "Telemetry: basic level",
            "AllowTelemetry=1 (Basic/Required). Value 0 only works on Enterprise, so it isn't used. Turn off the DiagTrack services in the Services category.",
            c, RiskLevel.Safe, Dw(PolWin + @"\DataCollection", "AllowTelemetry", 1)) { Recommended = true };

        yield return new RegistryTweak("privacy.adid", "Disable advertising ID",
            "Apps can no longer use the Advertising ID to profile you for ads.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0),
            Dw(PolWin + @"\AdvertisingInfo", "DisabledByGroupPolicy", 1)) { Recommended = true };

        yield return new RegistryTweak("privacy.tailored", "Disable tailored experiences",
            "Microsoft doesn't use diagnostic data to personalize tips and ads.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0)) { Recommended = true };

        yield return new RegistryTweak("privacy.activity", "Disable activity history",
            "Stops collecting and uploading activity history (Timeline).",
            c, RiskLevel.Safe,
            Dw(PolWin + @"\System", "EnableActivityFeed", 0),
            Dw(PolWin + @"\System", "PublishUserActivities", 0),
            Dw(PolWin + @"\System", "UploadUserActivities", 0)) { Recommended = true };

        yield return new RegistryTweak("privacy.consumer", "Disable consumer features and suggested apps",
            "DisableWindowsConsumerFeatures=1 - Windows no longer installs promoted apps automatically (Candy Crush etc.).",
            c, RiskLevel.Safe,
            Dw(PolWin + @"\CloudContent", "DisableWindowsConsumerFeatures", 1),
            Dw(Cdm, "SilentInstalledAppsEnabled", 0),
            Dw(Cdm, "PreInstalledAppsEnabled", 0),
            Dw(Cdm, "OemPreInstalledAppsEnabled", 0),
            Dw(Cdm, "ContentDeliveryAllowed", 0)) { Recommended = true };

        yield return new RegistryTweak("privacy.startads", "Disable ads and suggestions in the Start menu",
            "Removes suggested apps/ads in Start and Settings, plus recommended items.",
            c, RiskLevel.Safe,
            () => StartAdsEntries()) { Recommended = true, Restart = RestartNeed.Explorer };

        yield return new RegistryTweak("privacy.lockscreen", "Disable tips and ads on the lock screen",
            "A lock screen without fun facts, Spotlight ads and tips.",
            c, RiskLevel.Safe,
            Dw(Cdm, "RotatingLockScreenOverlayEnabled", 0),
            Dw(Cdm, "SubscribedContent-338387Enabled", 0),
            Dw(Cdm, "SoftLandingEnabled", 0)) { Recommended = true };

        yield return new TelemetryTasksTweak();

        yield return new RegistryTweak("privacy.cortana", "Disable and remove Cortana (policy + app)",
            "Turns Cortana off via the AllowCortana=0 policy and uninstalls the Cortana app (Microsoft.549981C3F5F10). Windows Search keeps working normally. Counts as applied only when the app is gone too.",
            c, RiskLevel.Safe, Dw(PolWin + @"\Windows Search", "AllowCortana", 0))
        { Recommended = true, AppxPatterns = new[] { "Microsoft.549981C3F5F10" } };

        yield return new RegistryTweak("privacy.copilot", "Disable and remove Windows Copilot (policy, taskbar button, app)",
            "TurnOffWindowsCopilot=1 (user and machine policy), ShowCopilotButton=0 (taskbar button) and removal of the Copilot app packages. Restart Explorer for the taskbar button to disappear.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1),
            Dw(PolWin + @"\WindowsCopilot", "TurnOffWindowsCopilot", 1),
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 0))
        {
            Recommended = true, SupportedOn = OsSupport.Win11, Restart = RestartNeed.Explorer,
            AppxPatterns = new[] { "*Copilot*" } // none of the protected packages contain "Copilot"; the remover filters them anyway
        };

        // ---- Windows 10 equivalents (shown only on Windows 10) ----
        yield return new RegistryTweak("privacy.cortana.win10", "Windows 10: restrict Cortana (lock screen, consent)",
            "On Windows 10 Cortana is a separate component: this blocks it on the lock screen (AllowCortanaAboveLock=0) and withdraws user consent (CortanaConsent=0). Complements the AllowCortana=0 policy.",
            c, RiskLevel.Safe,
            Dw(PolWin + @"\Windows Search", "AllowCortanaAboveLock", 0),
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 0))
        { Recommended = true, SupportedOn = OsSupport.Win10 };

        yield return new RegistryTweak("privacy.newsfeed", "Windows 10: disable \"News and interests\" on the taskbar",
            "The EnableFeeds=0 policy removes the weather/news widget from the taskbar (MSN ads and tracking, RAM used by Edge WebView).",
            c, RiskLevel.Safe,
            Dw(PolWin + @"\Windows Feeds", "EnableFeeds", 0))
        { Recommended = true, SupportedOn = OsSupport.Win10, MinBuild = 19041, Restart = RestartNeed.Explorer };

        yield return new RegistryTweak("privacy.websearch", "Disable web search (Bing) in Start",
            "Search in Start shows local results only (BingSearchEnabled=0, DisableSearchBoxSuggestions=1).",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0),
            Dw(@"HKCU\Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1)) { Recommended = true, Restart = RestartNeed.Explorer };
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

    public TelemetryTasksTweak() : base("privacy.tasks", "Disable telemetry tasks",
        "Disables the scheduled tasks Compatibility Appraiser, ProgramDataUpdater, Consolidator, UsbCeip and DmClient. Windows Update is not affected.",
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
                description: L.F("Would disable task {0}{1}", t.Path, t.Name));
            if (!r.Ok) Ctx.Log.Warn(L.F("Task {0}: {1}", t.Name, r.Text));
        }
    }
}
