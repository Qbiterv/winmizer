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

        yield return new RegistryTweak("gaming.gamemode", "Tryb gry (Game Mode) włączony",
            "Windows priorytetyzuje grę i ogranicza zadania w tle (Windows Update nie instaluje sterowników w trakcie gry).",
            c, RiskLevel.Safe,
            Dw(GameBar, "AllowAutoGameMode", 1), Dw(GameBar, "AutoGameModeEnabled", 1)) { Recommended = true };

        yield return new RegistryTweak("gaming.hags", "Planowanie GPU przyspieszane sprzętowo (HAGS)",
            "Ustawia HwSchMode=2. Może zmniejszyć opóźnienia i obciążenie CPU; wymaga wspieranej karty i sterownika. Wymaga restartu.",
            c, RiskLevel.Moderate, Dw(@"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2))
        {
            Warning = "Na starszych GPU/sterownikach może powodować artefakty - w razie problemów użyj Cofnij.",
            MinBuild = 19041, // Windows 10 2004+
            OsNote = "Wymaga Windows 10 2004+ (build 19041) oraz karty z WDDM 2.7+ i nowego sterownika; bez tego ustawienie jest ignorowane."
        };

        yield return new RegistryTweak("gaming.gamedvr", "Wyłącz Game DVR / nagrywanie w tle",
            "Wyłącza nagrywanie w tle Xbox Game Bar (GameDVR_Enabled=0, AllowGameDVR=0). Sam Game Bar i usługi Xbox pozostają.",
            c, RiskLevel.Safe,
            Dw(GameConfig, "GameDVR_Enabled", 0),
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0),
            Dw(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", 0)) { Recommended = true };

        yield return new RegistryTweak("gaming.fso", "Wyłącz optymalizacje pełnoekranowe globalnie",
            "Ustawia globalnie zachowanie FSE (GameDVR_FSEBehaviorMode=2). Część gier działa płynniej w prawdziwym trybie pełnoekranowym, inne gorzej (alt-tab).",
            c, RiskLevel.Advanced,
            Dw(GameConfig, "GameDVR_FSEBehaviorMode", 2),
            Dw(GameConfig, "GameDVR_HonorUserFSEBehaviorMode", 1),
            Dw(GameConfig, "GameDVR_FSEBehavior", 2),
            Dw(GameConfig, "GameDVR_DXGIHonorFSEWindowsCompatible", 1));

        yield return new RegistryTweak("gaming.mmcss", "MMCSS: responsywność systemu i sieć",
            "SystemResponsiveness=10 (nie 0 - zostawia zapas dla audio/systemu) oraz NetworkThrottlingIndex=0xFFFFFFFF (wyłącza dławienie sieci multimediów).",
            c, RiskLevel.Safe,
            Dw(Mmcss, "SystemResponsiveness", 10), Dw(Mmcss, "NetworkThrottlingIndex", 0xffffffff)) { Recommended = true };

        yield return new RegistryTweak("gaming.mmcss.games", "MMCSS: profil zadań \"Games\"",
            "Priorytet GPU=8, Priority=6, Scheduling Category=High, SFIO Priority=High dla procesów gier.",
            c, RiskLevel.Safe,
            Dw(Mmcss + @"\Tasks\Games", "GPU Priority", 8),
            Dw(Mmcss + @"\Tasks\Games", "Priority", 6),
            Sz(Mmcss + @"\Tasks\Games", "Scheduling Category", "High"),
            Sz(Mmcss + @"\Tasks\Games", "SFIO Priority", "High")) { Recommended = true };

        yield return new RegistryTweak("gaming.prioritysep", "Win32PrioritySeparation = 0x26",
            "Zwiększa udział czasu CPU dla aplikacji pierwszoplanowej (gry) względem procesów w tle.",
            c, RiskLevel.Moderate, Dw(@"HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 0x26));

        yield return new RegistryTweak("gaming.mouse", "Wyłącz przyspieszenie myszy",
            "Wyłącza \"Zwiększ precyzję wskaźnika\" (MouseSpeed/Threshold = 0) - stała, przewidywalna czułość w grach FPS.",
            c, RiskLevel.Safe,
            Sz(@"HKCU\Control Panel\Mouse", "MouseSpeed", "0"),
            Sz(@"HKCU\Control Panel\Mouse", "MouseThreshold1", "0"),
            Sz(@"HKCU\Control Panel\Mouse", "MouseThreshold2", "0")) { Recommended = true };

        yield return new RegistryTweak("gaming.stickykeys", "Wyłącz okna Sticky/Filter/Toggle Keys",
            "Wyłącza skróty uruchamiające okna dialogowe (5x Shift itd.), które potrafią zminimalizować grę.",
            c, RiskLevel.Safe,
            Sz(@"HKCU\Control Panel\Accessibility\StickyKeys", "Flags", "506"),
            Sz(@"HKCU\Control Panel\Accessibility\ToggleKeys", "Flags", "58"),
            Sz(@"HKCU\Control Panel\Accessibility\Keyboard Response", "Flags", "122")) { Recommended = true };

        yield return new RegistryTweak("gaming.vrr", "Optymalizacje gier w oknie + VRR",
            "Włącza SwapEffectUpgradeEnable i VRROptimizeEnable (DirectX) - mniejsze opóźnienie w grach okienkowych i zmienne odświeżanie.",
            c, RiskLevel.Safe,
            () => new[] { Sz(DxKey, "DirectXUserGlobalSettings", MergeDx()) })
        {
            Recommended = true,
            MinBuild = 18362, // Windows 10 1903+ (flags do not exist earlier)
            OsNote = OsInfo.IsWin11 ? null : "Windows 10 ma tylko częściowe wsparcie (VRR od 1903, optymalizacje okienkowe działają w ograniczonym zakresie)."
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
