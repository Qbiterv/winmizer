namespace Winmizer.Core;

public enum RiskLevel { Safe, Moderate, Advanced }

public enum TweakCategory
{
    Gaming, Power, Privacy, Services, Network, Visual, Storage, Cleanup, Debloat, OneDrive
}

public enum ResultStatus { Applied, Skipped, Failed, DryRun, Reverted }

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

/// <summary>Polish UI labels for enums.</summary>
public static class Labels
{
    public static string Risk(RiskLevel r) => r switch
    {
        RiskLevel.Safe => "Bezpieczny",
        RiskLevel.Moderate => "Umiarkowany",
        _ => "Zaawansowany"
    };

    public static string RiskColor(RiskLevel r) => r switch
    {
        RiskLevel.Safe => "green",
        RiskLevel.Moderate => "yellow",
        _ => "red"
    };

    public static string Category(TweakCategory c) => c switch
    {
        TweakCategory.Gaming => "Gaming",
        TweakCategory.Power => "Zasilanie",
        TweakCategory.Privacy => "Prywatność i telemetria",
        TweakCategory.Services => "Usługi",
        TweakCategory.Network => "Sieć",
        TweakCategory.Visual => "Wygląd i interfejs",
        TweakCategory.Storage => "Dysk",
        TweakCategory.Cleanup => "Czyszczenie",
        TweakCategory.Debloat => "Odśmiecanie aplikacji",
        TweakCategory.OneDrive => "OneDrive",
        _ => c.ToString()
    };
}
