namespace Winmizer.Core;

/// <summary>Base class: metadata plus a generic backup-based Revert.</summary>
public abstract class TweakBase : ITweak
{
    protected TweakBase(string id, string name, string description, TweakCategory category, RiskLevel risk)
    {
        Id = id; Name = name; Description = description; Category = category; Risk = risk;
    }

    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public TweakCategory Category { get; }
    public RiskLevel Risk { get; }
    public virtual bool Recommended { get; init; }
    public string? Warning { get; init; }
    public OsSupport SupportedOn { get; init; } = OsSupport.Both;
    public int MinBuild { get; init; }
    public string? OsNote { get; init; }
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
    public override void Revert() => throw new TweakSkippedException("Akcji czyszczenia nie można cofnąć.");
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

    public override bool IsApplied()
    {
        var list = _entries().ToList();
        if (list.Count == 0) return true;
        foreach (var e in list)
        {
            bool has = Reg.TryGet(e.Key, e.Name, out var v, out var k);
            if (e.Value is null) { if (has) return false; }
            else if (!has || !Reg.Equal(v, e.Value, e.Kind, k)) return false;
        }
        return true;
    }

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
    }
}
