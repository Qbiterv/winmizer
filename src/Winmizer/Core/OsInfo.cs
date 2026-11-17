using Microsoft.Win32;
using Winmizer.Core.Localization;

namespace Winmizer.Core;

/// <summary>Which Windows generation a tweak applies to.</summary>
public enum OsSupport { Both = 0, Win10 = 1, Win11 = 2 }

/// <summary>Detected Windows version (build >= 22000 means Windows 11).</summary>
public static class OsInfo
{
    public const int Win11MinBuild = 22000;
    /// <summary>Windows 10 1809 - the oldest release Winmizer is written for.</summary>
    public const int MinSupportedBuild = 17763;

    private const string CurVer = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

    private static readonly Lazy<Detected> Info = new(Detect);

    public static int Build => Info.Value.Build;
    public static bool IsWin11 => Build >= Win11MinBuild;
    public static bool IsWin10 => Build > 0 && Build < Win11MinBuild;
    public static bool IsSupported => Build >= MinSupportedBuild;
    public static string Edition => Info.Value.Edition;
    public static string DisplayVersion => Info.Value.DisplayVersion;
    public static string Name => IsWin11 ? "Windows 11" : "Windows 10";

    /// <summary>E.g. "Windows 11 Pro 23H2 (build 22631.4890)".</summary>
    public static string Description
    {
        get
        {
            var d = Info.Value;
            string ver = string.IsNullOrEmpty(d.DisplayVersion) ? "" : " " + d.DisplayVersion;
            string ed = string.IsNullOrEmpty(d.Edition) ? "" : " " + d.Edition;
            string ubr = d.Ubr > 0 ? "." + d.Ubr : "";
            return $"{Name}{ed}{ver} (build {d.Build}{ubr})";
        }
    }

    public static string? UnsupportedWarning => IsSupported ? null :
        L.F("Detected {0}. Winmizer supports Windows 10 1809 (build {1}) and newer - " +
            "on older versions some tweaks may not work. Use at your own risk.", Description, MinSupportedBuild);

    public static bool Matches(OsSupport s, int minBuild = 0) =>
        Build >= minBuild && s switch
        {
            OsSupport.Win11 => IsWin11,
            OsSupport.Win10 => IsWin10,
            _ => true
        };

    /// <summary>Short reason shown when a tweak does not apply to this system.</summary>
    public static string NotApplicableReason(OsSupport s, int minBuild = 0)
    {
        if (s == OsSupport.Win11 && !IsWin11) return L.T("N/A on this system (Windows 11 only)");
        if (s == OsSupport.Win10 && !IsWin10) return L.T("N/A on this system (Windows 10 only)");
        if (Build < minBuild) return L.F("N/A on this system (needs build {0}+, you have {1})", minBuild, Build);
        return "";
    }

    private sealed record Detected(int Build, int Ubr, string Edition, string DisplayVersion);

    private static Detected Detect()
    {
        int build = Environment.OSVersion.Version.Build; // real version in .NET (RtlGetVersion)
        int ubr = 0;
        string edition = "", display = "";
        try
        {
            using var k = Registry.LocalMachine.OpenSubKey(CurVer);
            if (k != null)
            {
                if (build <= 0 && int.TryParse(k.GetValue("CurrentBuildNumber") as string, out var b)) build = b;
                if (k.GetValue("UBR") is int u) ubr = u;
                display = k.GetValue("DisplayVersion") as string ?? k.GetValue("ReleaseId") as string ?? "";
                // EditionID is reliable; ProductName still says "Windows 10" on Windows 11.
                edition = EditionName(k.GetValue("EditionID") as string);
            }
        }
        catch { /* registry unavailable - fall back to what we have */ }
        return new Detected(build, ubr, edition, display);
    }

    private static string EditionName(string? id) => id switch
    {
        null or "" => "",
        "Core" => "Home",
        "CoreN" => "Home N",
        "CoreSingleLanguage" => "Home Single Language",
        "Professional" => "Pro",
        "ProfessionalN" => "Pro N",
        "ProfessionalWorkstation" => "Pro for Workstations",
        "ProfessionalEducation" => "Pro Education",
        "Education" => "Education",
        "Enterprise" => "Enterprise",
        "EnterpriseS" => "Enterprise LTSC",
        "IoTEnterprise" => "IoT Enterprise",
        _ => id
    };
}
