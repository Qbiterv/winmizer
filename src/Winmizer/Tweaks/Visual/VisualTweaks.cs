using Winmizer.Core;
using static Winmizer.Core.R;

namespace Winmizer.Tweaks.Visual;

public static class VisualTweaks
{
    private const string Adv = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";

    public static IEnumerable<ITweak> Create()
    {
        var c = TweakCategory.Visual;

        yield return new RegistryTweak("visual.effects", "Efekty wizualne pod wydajność (z zachowaniem czcionek i miniatur)",
            "Wyłącza animacje okien i paska zadań oraz cienie/zaznaczenia przezroczyste. Wygładzanie czcionek (ClearType) i miniatury pozostają włączone.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 3),
            Sz(@"HKCU\Control Panel\Desktop\WindowMetrics", "MinAnimate", "0"),
            Dw(Adv, "TaskbarAnimations", 0),
            Dw(Adv, "ListviewAlphaSelect", 0),
            Dw(Adv, "ListviewShadow", 0)) { Recommended = true };

        yield return new RegistryTweak("visual.transparency", "Wyłącz efekty przezroczystości",
            "Wyłącza akryl/przezroczystość w Start i oknach - odrobinę mniej obciążony GPU.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0));

        yield return new RegistryTweak("visual.startupdelay", "Usuń opóźnienie uruchamiania aplikacji autostartu",
            "Serialize\\StartupDelayInMSec=0 - programy autostartu startują od razu po zalogowaniu.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "StartupDelayInMSec", 0)) { Recommended = true };

        yield return new RegistryTweak("visual.menudelay", "Menu bez opóźnienia (MenuShowDelay=0)",
            "Menu rozwijane pojawiają się natychmiast.",
            c, RiskLevel.Safe, Sz(@"HKCU\Control Panel\Desktop", "MenuShowDelay", "0")) { Recommended = true };

        yield return new RegistryTweak("visual.bgapps", "Zablokuj aplikacje działające w tle",
            "Zasada LetAppsRunInBackground=2 - aplikacje Sklepu nie pracują w tle (oszczędza RAM/CPU). Mogą przestać przychodzić powiadomienia z takich aplikacji.",
            c, RiskLevel.Moderate,
            Dw(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsRunInBackground", 2));
    }
}
