using System.Text.RegularExpressions;
using Microsoft.Win32;
using Winmizer.Core;
using Winmizer.Core.Localization;
using static Winmizer.Core.R;

namespace Winmizer.Tweaks.Power;

public static class PowerTweaks
{
    public static IEnumerable<ITweak> Create()
    {
        var c = TweakCategory.Power;
        yield return new UltimatePlanTweak();
        yield return new HighPerformancePlanTweak();

        // USB selective suspend: subgroup 2a737441..., setting 48e6b7a6...
        yield return new PowerSettingTweak("power.usbsuspend", "Disable USB selective suspend",
            "Stops USB ports from going to sleep (fewer mouse/keyboard/gamepad stutters). On laptops, AC power only.",
            "2a737441-1930-4402-8d77-b2bebba308a3", "48e6b7a6-50f5-4782-a5d4-53bb8f07e226", 0, RiskLevel.Moderate);

        // PCI Express Link State Power Management: subgroup 501a4d13..., setting ee12f906... (0 = Off)
        yield return new PowerSettingTweak("power.pcie", "Disable PCIe link power saving",
            "Link State Power Management = Off - lower GPU/NVMe latency. On laptops, AC only.",
            "501a4d13-42af-4429-9fd1-a8218c268e20", "ee12f906-d277-404b-b6da-e5fa1a576df5", 0, RiskLevel.Moderate);

        // Minimum processor state: subgroup 54533251..., setting 893dee8e... (AC only)
        yield return new PowerSettingTweak("power.procmin", "Minimum processor state 100% (AC)",
            "The CPU never drops its clock speed on AC power - steadier FPS, higher idle power draw and temperature.",
            "54533251-82be-4824-96c1-47b60b740d00", "893dee8e-2bef-41e0-89c6-b55d0929964c", 100, RiskLevel.Moderate, acOnly: true);

        yield return new RegistryTweak("power.faststartup", "Disable Fast Startup",
            "Full shutdown on \"Shut down\" - clean driver state and fewer problems after updates. Boot may be slightly slower.",
            c, RiskLevel.Moderate, Dw(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 0));

        yield return new HibernateTweak();
    }
}

/// <summary>Duplicates the hidden Ultimate Performance plan as "Winmizer Ultimate" and activates it.</summary>
public sealed class UltimatePlanTweak : TweakBase
{
    public UltimatePlanTweak() : base("power.ultimate", "Power plan: Winmizer Ultimate (Ultimate Performance)",
        "Creates and activates the Ultimate Performance plan (lowest latency). Your original plan is remembered, so you can switch back.",
        TweakCategory.Power, RiskLevel.Moderate)
    {
        Warning = "Laptops: this plan drains the battery much faster - recommended on AC power only.";
    }

    public override bool Recommended => !Hardware.IsLaptop;

    public override bool IsApplied() => PowerCfg.Active().Name.Equals(PowerCfg.UltimateName, StringComparison.OrdinalIgnoreCase);

    public override void Apply()
    {
        var original = PowerCfg.Active();
        if (Ctx.DryRun)
        {
            Ctx.Log.Dry(L.F("Would duplicate the Ultimate Performance plan, name it \"{0}\" and activate it (current: {1}).", PowerCfg.UltimateName, original.Name));
            return;
        }

        string? guid = PowerCfg.List().FirstOrDefault(p => p.Name.Equals(PowerCfg.UltimateName, StringComparison.OrdinalIgnoreCase)).Guid;
        bool created = false;
        if (string.IsNullOrEmpty(guid))
        {
            var r = Proc.Run("powercfg.exe", new[] { "-duplicatescheme", PowerCfg.UltimateTemplate });
            var m = Regex.Match(r.Out, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
            if (!r.Ok || !m.Success)
                throw new InvalidOperationException(L.T("This system does not offer the Ultimate Performance plan. Use the High performance plan instead.") + " " + r.Text);
            guid = m.Value.ToLowerInvariant();
            created = true;
            Proc.Run("powercfg.exe", new[] { "-changename", guid, PowerCfg.UltimateName, "Created by Winmizer" });
        }

        // Order matters: revert undoes in reverse (activate original first, then delete created plan).
        if (created) Ctx.Backup.Record(Id, new BackupEntry { Kind = "createdscheme", Key = guid });
        Ctx.Backup.Record(Id, new BackupEntry { Kind = "activescheme", Key = original.Guid, Value = original.Name });
        Ctx.Backup.Save();
        PowerCfg.SetActive(guid);
    }
}

public sealed class HighPerformancePlanTweak : TweakBase
{
    public HighPerformancePlanTweak() : base("power.highperf", "Power plan: High performance",
        "Activates the built-in High performance plan (a lighter alternative to Ultimate). You can switch back to Balanced.",
        TweakCategory.Power, RiskLevel.Safe)
    {
        Warning = "Laptops: shorter battery life.";
    }

    public override bool IsApplied() => PowerCfg.Active().Guid == PowerCfg.HighPerformance;

    public override void Apply()
    {
        var original = PowerCfg.Active();
        Ctx.Backup.Record(Id, new BackupEntry { Kind = "activescheme", Key = original.Guid, Value = original.Name });
        Ctx.Backup.Save();
        PowerCfg.SetActive(PowerCfg.HighPerformance);
    }
}

/// <summary>Sets one powercfg value on the active scheme (AC always, DC only on desktops).</summary>
public sealed class PowerSettingTweak : TweakBase
{
    private readonly string _sub, _setting;
    private readonly uint _value;
    private readonly bool _acOnly;

    public PowerSettingTweak(string id, string name, string description, string sub, string setting, uint value,
        RiskLevel risk, bool acOnly = false)
        : base(id, name, description, TweakCategory.Power, risk)
    {
        _sub = sub; _setting = setting; _value = value; _acOnly = acOnly;
    }

    public override bool Recommended => !Hardware.IsLaptop;

    private bool ApplyDc => !_acOnly && !Hardware.IsLaptop; // keep laptop battery settings sane

    public override bool IsApplied()
    {
        string scheme = PowerCfg.Active().Guid;
        if (string.IsNullOrEmpty(scheme)) return false;
        if (PowerCfg.GetIndex(scheme, _sub, _setting, true) != _value) return false;
        return !ApplyDc || PowerCfg.GetIndex(scheme, _sub, _setting, false) == _value;
    }

    public override void Apply()
    {
        string scheme = PowerCfg.Active().Guid;
        string key = $"{scheme}|{_sub}|{_setting}";
        void Do(bool ac)
        {
            var old = PowerCfg.GetIndex(scheme, _sub, _setting, ac);
            if (old.HasValue)
                Ctx.Backup.Record(Id, new BackupEntry { Kind = "power", Key = key, Name = ac ? "ac" : "dc", Value = old.Value.ToString() });
            Ctx.Backup.Save();
            PowerCfg.SetIndex(scheme, _sub, _setting, ac, _value);
        }
        Do(true);
        if (ApplyDc) Do(false);
    }
}

public sealed class HibernateTweak : TweakBase
{
    public HibernateTweak() : base("power.hibernate", "Disable hibernation (frees disk space)",
        "powercfg -h off deletes hiberfil.sys (several to a dozen or so GB). Also disables Fast Startup and hibernation.",
        TweakCategory.Power, RiskLevel.Advanced)
    {
        Warning = "Laptops: you lose the ability to hibernate.";
    }

    public override bool IsApplied() =>
        Reg.TryGet(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", out var v, out _) && Convert.ToInt32(v) == 0;

    public override void Apply()
    {
        Ctx.Backup.Record(Id, new BackupEntry { Kind = "hibernate", Key = "hibernate", Value = "on" });
        Ctx.Backup.Save();
        var r = Proc.Run("powercfg.exe", new[] { "-h", "off" });
        if (!r.Ok) throw new InvalidOperationException(r.Text);
    }
}
