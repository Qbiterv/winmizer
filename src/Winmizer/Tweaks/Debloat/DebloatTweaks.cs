using System.Text.RegularExpressions;
using Winmizer.Core;
using Winmizer.Core.Localization;

namespace Winmizer.Tweaks.Debloat;

/// <summary>Cached list of installed / provisioned Appx packages (one PowerShell call).</summary>
public static class AppxCache
{
    private static List<string>? _names;

    public static void Invalidate() => _names = null;

    public static List<string> Names()
    {
        if (_names != null) return _names;
        var r = PS.Query(
            "$n=@(); " +
            "try { $n += Get-AppxPackage -AllUsers | Select-Object -ExpandProperty Name } catch { try { $n += Get-AppxPackage | Select-Object -ExpandProperty Name } catch {} }; " +
            "try { $n += Get-AppxProvisionedPackage -Online | Select-Object -ExpandProperty DisplayName } catch {}; " +
            "$n | Sort-Object -Unique", 120_000);
        _names = r.Out.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        return _names;
    }

    public static bool IsPresent(string pattern) => Names().Any(n => Wildcard.IsMatch(pattern, n));
}

internal static class Wildcard
{
    public static bool IsMatch(string pattern, string text) =>
        Regex.IsMatch(text, "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
}

public static class DebloatTweaks
{
    // Apps that must never be removed (Store, winget, essentials, Xbox identity / gaming services, Defender UI, runtimes).
    private static readonly string[] Protected =
    {
        "Microsoft.WindowsStore", "Microsoft.StorePurchaseApp", "Microsoft.DesktopAppInstaller", "Microsoft.WindowsCalculator",
        "Microsoft.Windows.Photos", "Microsoft.WindowsNotepad", "Microsoft.WindowsTerminal", "Microsoft.Paint",
        "Microsoft.ScreenSketch", "Microsoft.WindowsCamera", "Microsoft.XboxIdentityProvider", "Microsoft.GamingServices",
        "Microsoft.GamingApp", "Microsoft.Xbox.TCUI", "Microsoft.XboxSpeechToTextOverlay", "Microsoft.XboxGameOverlay",
        "Microsoft.SecHealthUI", "Microsoft.WindowsSoundRecorder", "Microsoft.MicrosoftStickyNotes",
        "Microsoft.PrintDialog", "Microsoft.Print3D.PrintQueue", "Microsoft.VCLibs.140.00", "Microsoft.NET.Native.Runtime.2.2",
        "Microsoft.UI.Xaml.2.8", "Microsoft.WindowsAppRuntime.1.5", "Microsoft.HEVCVideoExtension", "Microsoft.WebpImageExtension",
        "Microsoft.HEIFImageExtension", "Microsoft.VP9VideoExtensions", "Microsoft.MicrosoftEdge.Stable",
        "Microsoft.Windows.ShellExperienceHost", "Microsoft.Windows.StartMenuExperienceHost", "MicrosoftWindows.Client.CBS",
        "Microsoft.AAD.BrokerPlugin", "Microsoft.LockApp", "Microsoft.Windows.CloudExperienceHost", "Microsoft.CredDialogHost",
        "windows.immersivecontrolpanel", "Microsoft.AsyncTextService", "Microsoft.Win32WebViewHost", "Microsoft.BioEnrollment",
    };

    public static IReadOnlyList<string> ProtectedNames => Protected;

    public static bool IsProtected(string pattern) =>
        Protected.Any(p => Wildcard.IsMatch(pattern, p) || Wildcard.IsMatch(p, pattern));

    private sealed record App(string Pattern, string Name, string Description, bool Default, RiskLevel Risk = RiskLevel.Safe, string? Warning = null, OsSupport Os = OsSupport.Both);

    private static readonly App[] Apps =
    {
        new("Microsoft.549981C3F5F10", "Cortana", "The old Cortana voice assistant. On Windows 10 this removes only the app; the system component is limited by a Privacy policy.", true),
        new("Microsoft.BingNews", "Microsoft News (Bing News)", "News app.", true),
        new("Microsoft.BingWeather", "Weather (Bing Weather)", "MSN weather app.", true),
        new("Microsoft.BingFinance", "Finance (Bing Finance)", "MSN finance app.", true),
        new("Microsoft.BingSports", "Sports (Bing Sports)", "MSN sports app.", true),
        new("Microsoft.BingSearch", "Bing Search (Start add-on)", "Bing search integration.", true, Os: OsSupport.Win11),
        new("Microsoft.GetHelp", "Get Help", "Support app.", true),
        new("Microsoft.Getstarted", "Tips", "Windows tips app.", true),
        new("Microsoft.MicrosoftOfficeHub", "Microsoft 365 (Office Hub)", "Advertising shortcut to the Office suite.", true),
        new("Microsoft.MicrosoftSolitaireCollection", "Microsoft Solitaire Collection", "Card games with ads.", true),
        new("Microsoft.People", "People", "Address book.", true),
        new("Microsoft.WindowsFeedbackHub", "Feedback Hub", "Sends feedback to Microsoft.", true),
        new("Microsoft.WindowsMaps", "Windows Maps", "Maps app.", true),
        new("Clipchamp.Clipchamp", "Clipchamp", "Microsoft video editor.", true, Os: OsSupport.Win11),
        new("Microsoft.Todos", "Microsoft To Do", "To-do list.", true),
        new("Microsoft.PowerAutomateDesktop", "Power Automate", "Task automation.", true),
        new("MicrosoftTeams", "Microsoft Teams (consumer)", "Built-in Teams (Chat).", true, Os: OsSupport.Win11),
        new("MSTeams", "Microsoft Teams (new)", "The new Teams version preinstalled with the system.", true, Os: OsSupport.Win11),
        new("Microsoft.Copilot", "Microsoft Copilot", "The Copilot app.", true, Os: OsSupport.Win11),
        new("Microsoft.Windows.Ai.Copilot.Provider", "Windows Copilot provider", "Copilot component.", true, Os: OsSupport.Win11),
        new("Microsoft.SkypeApp", "Skype", "Skype messenger.", true),
        new("Microsoft.MixedReality.Portal", "Mixed Reality Portal", "For Windows MR VR headsets.", true),
        new("Microsoft.Microsoft3DViewer", "3D Viewer", "Windows 10: 3D model viewer.", true, Os: OsSupport.Win10),
        new("Microsoft.MSPaint", "Paint 3D", "Windows 10: Paint 3D (not the classic Paint - that one stays).", true, Os: OsSupport.Win10),
        new("Microsoft.Print3D", "Print 3D", "Windows 10: 3D printing app.", true, Os: OsSupport.Win10),
        new("*Disney*", "Disney+", "Preinstalled promotional app.", true),
        new("SpotifyAB.SpotifyMusic", "Spotify", "Preinstalled promotional app (you can reinstall it).", true),
        new("*TikTok*", "TikTok", "Preinstalled promotional app.", true),
        new("*CandyCrush*", "Candy Crush and similar games", "Promotional games (Candy Crush Saga/Soda etc.).", true),
        new("*Facebook*", "Facebook", "Preinstalled promotional app.", true),
        new("*Instagram*", "Instagram", "Preinstalled promotional app.", true),
        new("*Twitter*", "Twitter / X", "Preinstalled promotional app.", true),
        new("Microsoft.ZuneMusic", "Media Player (Groove/ZuneMusic)", "On Windows 11 this is the new Media Player - remove it only if you use another player.", false, RiskLevel.Moderate),
        new("Microsoft.ZuneVideo", "Movies & TV (ZuneVideo)", "Movie app.", false, RiskLevel.Moderate),
        new("Microsoft.YourPhone", "Phone Link", "Remove it if you don't connect your phone to Windows.", false, RiskLevel.Moderate),
        new("Microsoft.WindowsAlarms", "Clock & Alarms", "Alarms, timers, stopwatch.", false, RiskLevel.Moderate),
        new("Microsoft.XboxGamingOverlay", "Xbox Game Bar (overlay)", "The Win+G overlay. Xbox identity and Gaming Services stay (needed by Game Pass games).", false, RiskLevel.Moderate,
            "Game Pass games keep working, but you lose the Win+G overlay."),
    };

    public static IEnumerable<ITweak> Create() => Apps.Select(a =>
        (ITweak)new AppxTweak(a.Pattern, a.Name, a.Description, a.Risk, a.Default, a.Warning) { SupportedOn = a.Os });

    public static bool AnyInstalled(IEnumerable<ITweak> t) => t.Any(x => !x.IsApplied());
}

/// <summary>Removes an Appx package for all users and de-provisions it for new users.</summary>
public sealed class AppxTweak : TweakBase
{
    private readonly string _pattern;

    public AppxTweak(string pattern, string name, string description, RiskLevel risk, bool recommended, string? warning)
        : base("debloat." + Regex.Replace(pattern, "[^A-Za-z0-9.]", "").ToLowerInvariant(), name, description, TweakCategory.Debloat, risk)
    {
        if (DebloatTweaks.IsProtected(pattern))
            throw new InvalidOperationException(L.F("Package {0} is protected - cannot create a tweak for it.", pattern));
        _pattern = pattern;
        DefaultSelected = recommended;
        Warning = warning;
    }

    public override bool IsReversible => false;
    /// <summary>Default selection in the debloat list.</summary>
    public bool DefaultSelected { get; }

    public override bool IsApplied() => !AppxCache.IsPresent(_pattern);

    public override void Apply()
    {
        string p = PS.Q(_pattern);
        string prot = string.Join(",", DebloatTweaks.ProtectedNames.Select(n => "'" + PS.Q(n) + "'"));
        string script =
            $"$p='{p}'; $prot=@({prot}); " +
            "Get-AppxPackage -AllUsers -Name $p | Where-Object { $n=$_.Name; -not ($prot | Where-Object { $n -like $_ }) } | ForEach-Object { try { Remove-AppxPackage -Package $_.PackageFullName -AllUsers } catch { Write-Output ('WARN ' + $_.Exception.Message) } }; " +
            "Get-AppxProvisionedPackage -Online | Where-Object { $n=$_.DisplayName; $n -like $p -and -not ($prot | Where-Object { $n -like $_ }) } | ForEach-Object { try { Remove-AppxProvisionedPackage -Online -PackageName $_.PackageName | Out-Null } catch { Write-Output ('WARN ' + $_.Exception.Message) } }";
        var r = PS.Run(script, description: L.F("Would remove Appx package: {0} (Remove-AppxPackage -AllUsers + Remove-AppxProvisionedPackage)", _pattern));
        foreach (var line in r.Out.Split('\n').Where(l => l.StartsWith("WARN"))) Ctx.Log.Warn($"{_pattern}: {line.Trim()}");
        if (!r.Ok) throw new InvalidOperationException(r.Text);
        AppxCache.Invalidate();
    }

    public override void Revert()
    {
        var r = PS.Run($"$f=Get-AppxPackage -AllUsers -Name '{PS.Q(_pattern)}'; if(-not $f){{ throw 'missing' }}; " +
                       "$f | ForEach-Object { Add-AppxPackage -DisableDevelopmentMode -Register \"$($_.InstallLocation)\\AppxManifest.xml\" }");
        if (!r.Ok) throw new TweakSkippedException(L.T("The removed app must be reinstalled from the Microsoft Store (or with winget install)."));
        AppxCache.Invalidate();
    }
}
