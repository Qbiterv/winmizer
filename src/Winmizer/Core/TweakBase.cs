using Winmizer.Core.Localization;

namespace Winmizer.Core;

/// <summary>
/// Base class: metadata plus a generic backup-based Revert. Name, description, warning and OS note are
/// written in English and translated on read, so the language can change at runtime.
/// </summary>
public abstract class TweakBase : ITweak
{
    private readonly string _name, _description;
    private readonly string? _warning, _osNote;

    protected TweakBase(string id, string name, string description, TweakCategory category, RiskLevel risk)
    {
        Id = id; _name = name; _description = description; Category = category; Risk = risk;
    }

    public string Id { get; }
    public string Name => L.T(_name);
    public string Description => L.T(_description);
    public TweakCategory Category { get; }
    public RiskLevel Risk { get; }
    public virtual bool Recommended { get; init; }
    public string? Warning { get => _warning is null ? null : L.T(_warning); init => _warning = value; }
    public OsSupport SupportedOn { get; init; } = OsSupport.Both;
    public int MinBuild { get; init; }
    public string? OsNote { get => _osNote is null ? null : L.T(_osNote); init => _osNote = value; }
    public RestartNeed Restart { get; init; }
    public bool IsApplicable => OsInfo.Matches(SupportedOn, MinBuild);
    public virtual bool IsReversible => true;
    public virtual bool IsAction => false;
    public string? ResultNote { get; protected set; }

    public abstract bool IsApplied();
    public abstract void Apply();
    public virtual void Revert() => Restorer.RestoreTweak(Id);
}

/// <summary>One-shot action (cleanup). Never "applied", cannot be reverted.</summary>
public abstract class ActionTweak : TweakBase
{
    protected ActionTweak(string id, string name, string description, TweakCategory category, RiskLevel risk)
        : base(id, name, description, category, risk) { }

    public long FreedBytes { get; protected set; }
    public override bool IsReversible => false;
    public override bool IsAction => true;
    public override bool IsApplied() => false;
    public override void Revert() => throw new TweakSkippedException(L.T("Cleanup actions cannot be undone."));
}

/// <summary>Sets/removes registry values; originals are backed up automatically.</summary>
public sealed class RegistryTweak : TweakBase
{
    private readonly Func<IEnumerable<RegSet>> _entries;

    public RegistryTweak(string id, string name, string description, TweakCategory category, RiskLevel risk,
        params RegSet[] entries)
        : this(id, name, description, category, risk, () => entries) { }

    public RegistryTweak(string id, string name, string description, TweakCategory category, RiskLevel risk,
        Func<IEnumerable<RegSet>> entries)
        : base(id, name, description, category, risk) => _entries = entries;

    /// <summary>Appx packages (wildcards allowed) that must also be gone for the tweak to count as applied.</summary>
    public string[] AppxPatterns { get; init; } = Array.Empty<string>();

    private bool RegistryApplied()
    {
        var list = _entries().ToList();
        foreach (var e in list)
        {
            bool has = Reg.TryGet(e.Key, e.Name, out var v, out var k);
            if (e.Value is null) { if (has) return false; }
            else if (!has || !Reg.Equal(v, e.Value, e.Kind, k)) return false;
        }
        return true;
    }

    public override bool IsApplied() =>
        RegistryApplied() && !AppxPatterns.Any(Winmizer.Tweaks.Debloat.AppxCache.IsPresent);

    public override void Apply()
    {
        var list = _entries().ToList();
        foreach (var e in list) Ctx.Backup.RecordReg(Id, e.Key, e.Name);
        Ctx.Backup.Save(); // originals hit the disk before the first write
        foreach (var e in list)
        {
            if (e.Value is null) Reg.Delete(e.Key, e.Name);
            else Reg.Set(e.Key, e.Name, e.Value, e.Kind);
        }
        if (AppxPatterns.Length > 0) Winmizer.Tweaks.Debloat.AppxRemover.RemoveAll(AppxPatterns);
    }
}
