using System.Reflection;

namespace Winmizer.Core;

/// <summary>Application version info, taken from the assembly (set by the csproj / build.ps1 -Version).</summary>
public static class AppInfo
{
    /// <summary>Version without the "+commit" build metadata, e.g. "1.0.1".</summary>
    public static string Version { get; } = ReadVersion();

    private static string ReadVersion()
    {
        var asm = typeof(AppInfo).Assembly;
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(info))
        {
            int plus = info.IndexOf('+');
            return plus >= 0 ? info[..plus] : info;
        }
        return asm.GetName().Version?.ToString(3) ?? "0.0.0";
    }
}
