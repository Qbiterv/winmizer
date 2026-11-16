using System.Text.RegularExpressions;
using Microsoft.Win32;
using Winmizer.Core;
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
        yield return new PowerSettingTweak("power.usbsuspend", "Wyłącz selektywne zawieszanie USB",
            "Zapobiega usypianiu portów USB (mniej zacięć myszy/klawiatury/padów). Na laptopach tylko zasilanie sieciowe (AC).",
            "2a737441-1930-4402-8d77-b2bebba308a3", "48e6b7a6-50f5-4782-a5d4-53bb8f07e226", 0, RiskLevel.Moderate);

        // PCI Express Link State Power Management: subgroup 501a4d13..., setting ee12f906... (0 = Off)
        yield return new PowerSettingTweak("power.pcie", "Wyłącz oszczędzanie energii łącza PCIe",
            "Link State Power Management = Wyłączone - mniejsze opóźnienia GPU/NVMe. Na laptopach tylko AC.",
            "501a4d13-42af-4429-9fd1-a8218c268e20", "ee12f906-d277-404b-b6da-e5fa1a576df5", 0, RiskLevel.Moderate);

        // Minimum processor state: subgroup 54533251..., setting 893dee8e... (AC only)
        yield return new PowerSettingTweak("power.procmin", "Minimalny stan procesora 100% (AC)",
            "Procesor nie obniża taktowania na zasilaniu sieciowym - stabilniejszy FPS, wyższy pobór prądu i temperatura w spoczynku.",
            "54533251-82be-4824-96c1-47b60b740d00", "893dee8e-2bef-41e0-89c6-b55d0929964c", 100, RiskLevel.Moderate, acOnly: true);

        yield return new RegistryTweak("power.faststartup", "Wyłącz szybkie uruchamianie (Fast Startup)",
            "Pełne zamknięcie systemu przy \"Zamknij\" - czysty stan sterowników i mniej problemów po aktualizacjach. Start może być nieco wolniejszy.",
            c, RiskLevel.Moderate, Dw(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 0));

        yield return new HibernateTweak();
    }
}

/// <summary>Duplicates the hidden Ultimate Performance plan as "Winmizer Ultimate" and activates it.</summary>
public sealed class UltimatePlanTweak : TweakBase
{
    public UltimatePlanTweak() : base("power.ultimate", "Plan zasilania: Winmizer Ultimate (Ultimate Performance)",
        "Tworzy i aktywuje plan Ultimate Performance (najniższe opóźnienia). Oryginalny plan jest zapamiętany i można do niego wrócić.",
        TweakCategory.Power, RiskLevel.Moderate)
    {
        Warning = "Laptopy: plan zużywa baterię znacznie szybciej - zalecany tylko przy zasilaniu z sieci.";
    }

    public override bool Recommended => !Hardware.IsLaptop;

    public override bool IsApplied() => PowerCfg.Active().Name.Equals(PowerCfg.UltimateName, StringComparison.OrdinalIgnoreCase);

    public override void Apply()
    {
        var original = PowerCfg.Active();
        if (Ctx.DryRun)
        {
            Ctx.Log.Dry($"Zduplikowałbym plan Ultimate Performance, nazwał go \"{PowerCfg.UltimateName}\" i aktywował (obecny: {original.Name}).");
            return;
        }

        string? guid = PowerCfg.List().FirstOrDefault(p => p.Name.Equals(PowerCfg.UltimateName, StringComparison.OrdinalIgnoreCase)).Guid;
        bool created = false;
        if (string.IsNullOrEmpty(guid))
        {
            var r = Proc.Run("powercfg.exe", new[] { "-duplicatescheme", PowerCfg.UltimateTemplate });
            var m = Regex.Match(r.Out, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
            if (!r.Ok || !m.Success)
                throw new InvalidOperationException("Ten system nie udostępnia planu Ultimate Performance. Użyj planu Wysoka wydajność. " + r.Text);
            guid = m.Value.ToLowerInvariant();
            created = true;
            Proc.Run("powercfg.exe", new[] { "-changename", guid, PowerCfg.UltimateName, "Utworzony przez Winmizer" });
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
    public HighPerformancePlanTweak() : base("power.highperf", "Plan zasilania: Wysoka wydajność",
        "Aktywuje wbudowany plan Wysoka wydajność (lżejsza alternatywa dla Ultimate). Można wrócić do Zrównoważonego.",
        TweakCategory.Power, RiskLevel.Safe)
    {
        Warning = "Laptopy: krótszy czas pracy na baterii.";
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
    public HibernateTweak() : base("power.hibernate", "Wyłącz hibernację (zwalnia miejsce)",
        "powercfg -h off usuwa hiberfil.sys (kilka-kilkanaście GB). Wyłącza też Fast Startup i hibernację.",
        TweakCategory.Power, RiskLevel.Advanced)
    {
        Warning = "Laptopy: utrata możliwości hibernacji.";
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
