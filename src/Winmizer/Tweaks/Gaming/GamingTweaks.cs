using Winmizer.Core;
using static Winmizer.Core.R;

namespace Winmizer.Tweaks.Gaming;

public static class GamingTweaks
{
    private const string GameBar = @"HKCU\Software\Microsoft\GameBar";
    private const string GameConfig = @"HKCU\System\GameConfigStore";
    private const string Mmcss = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
    private const string DxKey = @"HKCU\Software\Microsoft\DirectX\UserGpuPreferences";

    public static IEnumerable<ITweak> Create()
    {
        var c = TweakCategory.Gaming;

        yield return new RegistryTweak("gaming.gamemode", "Enable Game Mode",
            "Windows prioritizes the game and limits background tasks (Windows Update won't install drivers while you play).",
            c, RiskLevel.Safe,
            Dw(GameBar, "AllowAutoGameMode", 1), Dw(GameBar, "AutoGameModeEnabled", 1)) { Recommended = true };

        yield return new RegistryTweak("gaming.hags", "Hardware-accelerated GPU scheduling (HAGS)",
            "Sets HwSchMode=2. Can reduce latency and CPU load; needs a supported GPU and driver. Requires a restart.",
            c, RiskLevel.Moderate, Dw(@"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2))
        {
            Warning = "On older GPUs/drivers it can cause artifacts - use Revert if you run into problems.",
            MinBuild = 19041, // Windows 10 2004+
            OsNote = "Needs Windows 10 2004+ (build 19041), a WDDM 2.7+ GPU and a recent driver; otherwise the setting is ignored."
        };

        yield return new RegistryTweak("gaming.gamedvr", "Disable Game DVR / background recording",
            "Turns off Xbox Game Bar background recording (GameDVR_Enabled=0, AllowGameDVR=0). Game Bar itself and the Xbox services stay.",
            c, RiskLevel.Safe,
            Dw(GameConfig, "GameDVR_Enabled", 0),
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0),
            Dw(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", 0)) { Recommended = true };

        yield return new RegistryTweak("gaming.fso", "Disable fullscreen optimizations globally",
            "Sets the global FSE behavior (GameDVR_FSEBehaviorMode=2). Some games run smoother in true fullscreen, others worse (alt-tab).",
            c, RiskLevel.Advanced,
            Dw(GameConfig, "GameDVR_FSEBehaviorMode", 2),
            Dw(GameConfig, "GameDVR_HonorUserFSEBehaviorMode", 1),
            Dw(GameConfig, "GameDVR_FSEBehavior", 2),
            Dw(GameConfig, "GameDVR_DXGIHonorFSEWindowsCompatible", 1));

        yield return new RegistryTweak("gaming.mmcss", "MMCSS: system responsiveness and network",
            "SystemResponsiveness=10 (not 0 - leaves headroom for audio/system) and NetworkThrottlingIndex=0xFFFFFFFF (disables multimedia network throttling).",
            c, RiskLevel.Safe,
            Dw(Mmcss, "SystemResponsiveness", 10), Dw(Mmcss, "NetworkThrottlingIndex", 0xffffffff)) { Recommended = true };

        yield return new RegistryTweak("gaming.mmcss.games", "MMCSS: \"Games\" task profile",
            "GPU Priority=8, Priority=6, Scheduling Category=High, SFIO Priority=High for game processes.",
            c, RiskLevel.Safe,
            Dw(Mmcss + @"\Tasks\Games", "GPU Priority", 8),
            Dw(Mmcss + @"\Tasks\Games", "Priority", 6),
            Sz(Mmcss + @"\Tasks\Games", "Scheduling Category", "High"),
            Sz(Mmcss + @"\Tasks\Games", "SFIO Priority", "High")) { Recommended = true };

        yield return new RegistryTweak("gaming.prioritysep", "Win32PrioritySeparation = 0x26",
            "Gives the foreground application (the game) a larger share of CPU time compared to background processes.",
            c, RiskLevel.Moderate, Dw(@"HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 0x26));

        yield return new RegistryTweak("gaming.mouse", "Disable mouse acceleration",
            "Turns off \"Enhance pointer precision\" (MouseSpeed/Threshold = 0) - constant, predictable sensitivity in FPS games.",
            c, RiskLevel.Safe,
            Sz(@"HKCU\Control Panel\Mouse", "MouseSpeed", "0"),
            Sz(@"HKCU\Control Panel\Mouse", "MouseThreshold1", "0"),
            Sz(@"HKCU\Control Panel\Mouse", "MouseThreshold2", "0")) { Recommended = true };

        yield return new RegistryTweak("gaming.stickykeys", "Disable Sticky/Filter/Toggle Keys prompts",
            "Turns off the shortcuts that open accessibility dialogs (5x Shift etc.), which can minimize a game.",
            c, RiskLevel.Safe,
            Sz(@"HKCU\Control Panel\Accessibility\StickyKeys", "Flags", "506"),
            Sz(@"HKCU\Control Panel\Accessibility\ToggleKeys", "Flags", "58"),
            Sz(@"HKCU\Control Panel\Accessibility\Keyboard Response", "Flags", "122")) { Recommended = true };

        yield return new RegistryTweak("gaming.vrr", "Windowed-game optimizations + VRR",
            "Enables SwapEffectUpgradeEnable and VRROptimizeEnable (DirectX) - lower latency in windowed games and variable refresh rate.",
            c, RiskLevel.Safe,
            () => new[] { Sz(DxKey, "DirectXUserGlobalSettings", MergeDx()) })
        {
            Recommended = true,
            MinBuild = 18362, // Windows 10 1903+ (flags do not exist earlier)
            OsNote = OsInfo.IsWin11 ? null : "Windows 10 has only partial support (VRR since 1903, windowed-game optimizations are limited)."
        };
    }

    /// <summary>Merges our flags into the existing DirectXUserGlobalSettings string (keeps other settings).</summary>
    private static string MergeDx()
    {
        Reg.TryGet(DxKey, "DirectXUserGlobalSettings", out var v, out _);
        var d = new Dictionary<string, string>();
        foreach (var p in (v as string ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = p.Split('=', 2);
            if (kv.Length == 2) d[kv[0]] = kv[1];
        }
        d["VRROptimizeEnable"] = "1";
        d["SwapEffectUpgradeEnable"] = "1";
        return string.Join(";", d.Select(kv => $"{kv.Key}={kv.Value}")) + ";";
    }
}
