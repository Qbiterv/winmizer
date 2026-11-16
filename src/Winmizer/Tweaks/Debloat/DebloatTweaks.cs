using System.Text.RegularExpressions;
using Winmizer.Core;

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

    public static bool IsProtected(string pattern) =>
        Protected.Any(p => Wildcard.IsMatch(pattern, p) || Wildcard.IsMatch(p, pattern));

    private sealed record App(string Pattern, string Name, string Description, bool Default, RiskLevel Risk = RiskLevel.Safe, string? Warning = null);

    private static readonly App[] Apps =
    {
        new("Microsoft.549981C3F5F10", "Cortana", "Stary asystent głosowy Cortana.", true),
        new("Microsoft.BingNews", "Microsoft News (Bing News)", "Aplikacja z wiadomościami.", true),
        new("Microsoft.BingWeather", "Pogoda (Bing Weather)", "Aplikacja pogodowa MSN.", true),
        new("Microsoft.BingFinance", "Finanse (Bing Finance)", "Aplikacja finansowa MSN.", true),
        new("Microsoft.BingSports", "Sport (Bing Sports)", "Aplikacja sportowa MSN.", true),
        new("Microsoft.BingSearch", "Bing Search (dodatek do Start)", "Integracja wyszukiwania Bing.", true),
        new("Microsoft.GetHelp", "Uzyskaj pomoc (Get Help)", "Aplikacja pomocy technicznej.", true),
        new("Microsoft.Getstarted", "Porady (Tips)", "Aplikacja z poradami Windows.", true),
        new("Microsoft.MicrosoftOfficeHub", "Microsoft 365 (Office Hub)", "Reklamowy skrót do pakietu Office.", true),
        new("Microsoft.MicrosoftSolitaireCollection", "Microsoft Solitaire Collection", "Gry karciane z reklamami.", true),
        new("Microsoft.People", "Osoby (People)", "Książka adresowa.", true),
        new("Microsoft.WindowsFeedbackHub", "Centrum opinii (Feedback Hub)", "Wysyłanie opinii do Microsoft.", true),
        new("Microsoft.WindowsMaps", "Mapy Windows", "Aplikacja map.", true),
        new("Clipchamp.Clipchamp", "Clipchamp", "Edytor wideo Microsoft.", true),
        new("Microsoft.Todos", "Microsoft To Do", "Lista zadań.", true),
        new("Microsoft.PowerAutomateDesktop", "Power Automate", "Automatyzacja zadań.", true),
        new("MicrosoftTeams", "Microsoft Teams (konsumencki)", "Wbudowany Teams (Chat).", true),
        new("MSTeams", "Microsoft Teams (nowy)", "Nowa wersja Teams preinstalowana w systemie.", true),
        new("Microsoft.Copilot", "Microsoft Copilot", "Aplikacja Copilot.", true),
        new("Microsoft.Windows.Ai.Copilot.Provider", "Dostawca Windows Copilot", "Komponent Copilot.", true),
        new("Microsoft.SkypeApp", "Skype", "Komunikator Skype.", true),
        new("Microsoft.MixedReality.Portal", "Portal rzeczywistości mieszanej", "Dla gogli VR Windows MR.", true),
        new("*Disney*", "Disney+", "Preinstalowana aplikacja promocyjna.", true),
        new("SpotifyAB.SpotifyMusic", "Spotify", "Preinstalowana aplikacja promocyjna (możesz ją zainstalować ponownie).", true),
        new("*TikTok*", "TikTok", "Preinstalowana aplikacja promocyjna.", true),
        new("*CandyCrush*", "Candy Crush i podobne gry", "Promocyjne gry (Candy Crush Saga/Soda itp.).", true),
        new("*Facebook*", "Facebook", "Preinstalowana aplikacja promocyjna.", true),
        new("*Instagram*", "Instagram", "Preinstalowana aplikacja promocyjna.", true),
        new("*Twitter*", "Twitter / X", "Preinstalowana aplikacja promocyjna.", true),
        new("Microsoft.ZuneMusic", "Odtwarzacz multimediów (Groove/ZuneMusic)", "W Windows 11 to nowy Media Player - usuń tylko jeśli używasz innego odtwarzacza.", false, RiskLevel.Moderate),
        new("Microsoft.ZuneVideo", "Filmy i TV (ZuneVideo)", "Aplikacja do filmów.", false, RiskLevel.Moderate),
        new("Microsoft.YourPhone", "Łączność z telefonem (Phone Link)", "Usuń, jeśli nie łączysz telefonu z Windows.", false, RiskLevel.Moderate),
        new("Microsoft.WindowsAlarms", "Zegar i alarmy", "Alarmy, minutniki, stoper.", false, RiskLevel.Moderate),
        new("Microsoft.XboxGamingOverlay", "Xbox Game Bar (nakładka)", "Nakładka Win+G. Tożsamość Xbox i Gaming Services zostają (potrzebne grom z Game Pass).", false, RiskLevel.Moderate,
            "Gry z Game Pass nadal działają, ale stracisz nakładkę Win+G."),
    };

    public static IEnumerable<ITweak> Create() => Apps.Select(a =>
        (ITweak)new AppxTweak(a.Pattern, a.Name, a.Description, a.Risk, a.Default, a.Warning));

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
            throw new InvalidOperationException($"Pakiet {pattern} jest chroniony - nie można utworzyć tweaku.");
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
        string script =
            $"$p='{p}'; " +
            "Get-AppxPackage -AllUsers -Name $p | ForEach-Object { try { Remove-AppxPackage -Package $_.PackageFullName -AllUsers } catch { Write-Output ('WARN ' + $_.Exception.Message) } }; " +
            "Get-AppxProvisionedPackage -Online | Where-Object { $_.DisplayName -like $p } | ForEach-Object { try { Remove-AppxProvisionedPackage -Online -PackageName $_.PackageName | Out-Null } catch { Write-Output ('WARN ' + $_.Exception.Message) } }";
        var r = PS.Run(script, description: $"Usunąłbym pakiet Appx: {_pattern} (Remove-AppxPackage -AllUsers + Remove-AppxProvisionedPackage)");
        foreach (var line in r.Out.Split('\n').Where(l => l.StartsWith("WARN"))) Ctx.Log.Warn($"{_pattern}: {line.Trim()}");
        if (!r.Ok) throw new InvalidOperationException(r.Text);
        AppxCache.Invalidate();
    }

    public override void Revert()
    {
        var r = PS.Run($"$f=Get-AppxPackage -AllUsers -Name '{PS.Q(_pattern)}'; if(-not $f){{ throw 'brak' }}; " +
                       "$f | ForEach-Object { Add-AppxPackage -DisableDevelopmentMode -Register \"$($_.InstallLocation)\\AppxManifest.xml\" }");
        if (!r.Ok) throw new TweakSkippedException("Usunięta aplikacja wymaga ponownej instalacji ze sklepu Microsoft Store (lub: winget install).");
        AppxCache.Invalidate();
    }
}
