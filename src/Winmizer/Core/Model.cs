using Winmizer.Core.Localization;

namespace Winmizer.Core;

public enum RiskLevel { Safe, Moderate, Advanced }

public enum TweakCategory
{
    Gaming, Power, Privacy, Services, Network, Visual, Storage, Cleanup, Debloat, OneDrive
}

public enum ResultStatus { Applied, Skipped, Failed, DryRun, Reverted }

/// <summary>What the user has to do for a tweak's change to become visible.</summary>
public enum RestartNeed { None, Explorer, SignOut, Reboot }

/// <summary>A single optimization that can be detected, applied and reverted.</summary>
public interface ITweak
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    TweakCategory Category { get; }
    RiskLevel Risk { get; }
    /// <summary>Part of the "Quick Optimize" preset.</summary>
    bool Recommended { get; }
    bool IsReversible { get; }
    /// <summary>One-shot action (cleanup) rather than a persistent setting.</summary>
    bool IsAction { get; }
    string? Warning { get; }
    /// <summary>Windows generation the tweak is meant for (default: both).</summary>
    OsSupport SupportedOn { get; }
    /// <summary>Minimum Windows build the tweak needs (0 = any).</summary>
    int MinBuild { get; }
    /// <summary>Extra per-OS remark (e.g. partial support on Windows 10).</summary>
    string? OsNote { get; }
    /// <summary>False when the tweak does not exist / behaves differently on this OS; it is then skipped.</summary>
    bool IsApplicable { get; }
    /// <summary>What must be restarted for the change to show (nothing / Explorer / sign-out / reboot).</summary>
    RestartNeed Restart { get; }
    /// <summary>Extra info after Apply (e.g. freed space).</summary>
    string? ResultNote { get; }

    bool IsApplied();
    void Apply();
    void Revert();
}

/// <summary>Thrown by a tweak when it has nothing to do (not an error).</summary>
public sealed class TweakSkippedException : Exception
{
    public TweakSkippedException(string message) : base(message) { }
}

public sealed record TweakResult(ITweak Tweak, ResultStatus Status, string Message);

/// <summary>Localized UI labels for enums.</summary>
public static class Labels
{
    public static string Risk(RiskLevel r) => L.T(r switch
    {
        RiskLevel.Safe => "Safe",
        RiskLevel.Moderate => "Moderate",
        _ => "Advanced"
    });

    public static string RiskColor(RiskLevel r) => r switch
    {
        RiskLevel.Safe => "green",
        RiskLevel.Moderate => "yellow",
        _ => "red"
    };

    public static string Category(TweakCategory c) => L.T(c switch
    {
        TweakCategory.Gaming => "Gaming",
        TweakCategory.Power => "Power",
        TweakCategory.Privacy => "Privacy & telemetry",
        TweakCategory.Services => "Services",
        TweakCategory.Network => "Network",
        TweakCategory.Visual => "Appearance & UI",
        TweakCategory.Storage => "Storage",
        TweakCategory.Cleanup => "Cleanup",
        TweakCategory.Debloat => "App debloat",
        TweakCategory.OneDrive => "OneDrive",
        _ => c.ToString()
    });
}
