using Winmizer.Core;
using static Winmizer.Core.R;

namespace Winmizer.Tweaks.Visual;

public static class VisualTweaks
{
    private const string Adv = @"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";

    public static IEnumerable<ITweak> Create()
    {
        var c = TweakCategory.Visual;

        yield return new RegistryTweak("visual.effects", "Visual effects tuned for performance (keeps fonts and thumbnails)",
            "Turns off window and taskbar animations plus transparent shadows/selection. Font smoothing (ClearType) and thumbnails stay on.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 3),
            Sz(@"HKCU\Control Panel\Desktop\WindowMetrics", "MinAnimate", "0"),
            Dw(Adv, "TaskbarAnimations", 0),
            Dw(Adv, "ListviewAlphaSelect", 0),
            Dw(Adv, "ListviewShadow", 0)) { Recommended = true };

        yield return new RegistryTweak("visual.transparency", "Disable transparency effects",
            "Turns off acrylic/transparency in Start and windows - slightly less GPU load.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0));

        yield return new RegistryTweak("visual.startupdelay", "Remove the startup-app launch delay",
            "Serialize\\StartupDelayInMSec=0 - startup programs launch right after sign-in.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "StartupDelayInMSec", 0)) { Recommended = true };

        yield return new RegistryTweak("visual.menudelay", "No menu delay (MenuShowDelay=0)",
            "Drop-down menus appear instantly.",
            c, RiskLevel.Safe, Sz(@"HKCU\Control Panel\Desktop", "MenuShowDelay", "0")) { Recommended = true };

        yield return new RegistryTweak("visual.bgapps", "Block background apps",
            "Policy LetAppsRunInBackground=2 - Store apps don't run in the background (saves RAM/CPU). Notifications from such apps may stop arriving.",
            c, RiskLevel.Moderate,
            Dw(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\AppPrivacy", "LetAppsRunInBackground", 2));

        // ---- Windows 11 only ----
        yield return new RegistryTweak("visual.widgets", "Windows 11: disable Widgets (policy)",
            "Policy AllowNewsAndInterests=0 turns off the Widgets panel (weather, news, ads) and the Widgets.exe background process.",
            c, RiskLevel.Safe,
            Dw(@"HKLM\SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0))
        { SupportedOn = OsSupport.Win11 };

        // ---- Windows 10 only ----
        yield return new RegistryTweak("visual.peoplebar", "Windows 10: hide the People bar",
            "Removes the People icon from the taskbar (PeopleBand=0, HidePeopleBar=1). This element doesn't exist on Windows 11.",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\People", "PeopleBand", 0),
            Dw(@"HKCU\Software\Policies\Microsoft\Windows\Explorer", "HidePeopleBar", 1))
        { Recommended = true, SupportedOn = OsSupport.Win10 };

        yield return new RegistryTweak("visual.meetnow", "Windows 10: hide the Meet Now icon",
            "Hides the Meet Now (Skype) button in the taskbar tray (HideSCAMeetNow=1).",
            c, RiskLevel.Safe,
            Dw(@"HKCU\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "HideSCAMeetNow", 1))
        { Recommended = true, SupportedOn = OsSupport.Win10 };
    }
}
