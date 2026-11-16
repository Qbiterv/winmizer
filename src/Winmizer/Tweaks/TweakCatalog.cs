using Winmizer.Core;
using Winmizer.Tweaks.Cleanup;
using Winmizer.Tweaks.Debloat;
using Winmizer.Tweaks.Gaming;
using Winmizer.Tweaks.Network;
using Winmizer.Tweaks.OneDrive;
using Winmizer.Tweaks.Power;
using Winmizer.Tweaks.Privacy;
using Winmizer.Tweaks.Services;
using Winmizer.Tweaks.Storage;
using Winmizer.Tweaks.Visual;

namespace Winmizer.Tweaks;

/// <summary>Single source of truth listing every tweak (order = apply order).</summary>
public static class TweakCatalog
{
    private static List<ITweak>? _all;

    public static IReadOnlyList<ITweak> All => _all ??= Build();

    private static List<ITweak> Build() => new[]
    {
        PowerTweaks.Create(),      // plan first: later power settings apply to the active plan
        GamingTweaks.Create(),
        PrivacyTweaks.Create(),
        ServiceTweaks.Create(),
        NetworkTweaks.Create(),
        VisualTweaks.Create(),
        StorageTweaks.Create(),
        OneDriveTweaks.Create(),
        DebloatTweaks.Create(),
        CleanupTweaks.Create(),    // cleanup last
    }.SelectMany(x => x).ToList();

    public static IEnumerable<ITweak> ByCategory(TweakCategory c) => All.Where(t => t.Category == c);

    /// <summary>Quick Optimize preset: recommended, non-advanced tweaks.</summary>
    public static IEnumerable<ITweak> QuickPreset() => All.Where(t => t.Recommended && t.Risk != RiskLevel.Advanced);
}
