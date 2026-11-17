using System.Globalization;
using System.Text.Json;

namespace Winmizer.Core.Localization;

public enum Lang { En, Pl }

/// <summary>
/// Minimal localization layer. English is the source language: every user-facing string is written in
/// English in the code and acts as its own key. Polish translations live in <see cref="PolishStrings"/>;
/// a missing translation falls back to the English text, so the UI never shows an empty string.
/// </summary>
public static class L
{
    public static Lang Current { get; set; } = Lang.En;

    /// <summary>Translates a literal (or a composite-format string) into the current language.</summary>
    public static string T(string en) =>
        Current == Lang.Pl && PolishStrings.Map.TryGetValue(en, out var pl) ? pl : en;

    /// <summary>Translates a composite-format string and formats it (culture independent).</summary>
    public static string F(string en, params object?[] args) =>
        string.Format(CultureInfo.InvariantCulture, T(en), args);

    public static string Code(Lang l) => l == Lang.Pl ? "pl" : "en";

    public static bool TryParse(string? s, out Lang lang)
    {
        switch (s?.Trim().ToLowerInvariant())
        {
            case "en": case "eng": case "english": lang = Lang.En; return true;
            case "pl": case "pol": case "polski": case "polish": lang = Lang.Pl; return true;
            default: lang = Lang.En; return false;
        }
    }

    /// <summary>Picks the startup language: --lang flag, then WINMIZER_LANG, then settings.json, then English.</summary>
    public static void Init(string? cliValue)
    {
        Current = Lang.En;
        if (TryParse(cliValue, out var fromCli)) { Current = fromCli; return; }
        if (TryParse(Environment.GetEnvironmentVariable("WINMIZER_LANG"), out var fromEnv)) { Current = fromEnv; return; }
        if (Settings.Load().Language is { } saved && TryParse(saved, out var fromFile)) Current = fromFile;
    }
}

/// <summary>User settings stored in %ProgramData%\Winmizer\settings.json (language only, for now).</summary>
public sealed class Settings
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

    public string? Language { get; set; }

    /// <summary>Never throws: a missing or corrupt file simply yields defaults.</summary>
    public static Settings Load()
    {
        try
        {
            if (File.Exists(Paths.SettingsFile))
                return JsonSerializer.Deserialize<Settings>(File.ReadAllText(Paths.SettingsFile)) ?? new Settings();
        }
        catch { /* corrupt or unreadable - ignore */ }
        return new Settings();
    }

    /// <summary>Persists the language choice. Dry-run never writes; failures are ignored.</summary>
    public static void SaveLanguage(Lang lang)
    {
        if (Ctx.DryRun) return;
        try
        {
            var s = Load();
            s.Language = L.Code(lang);
            Directory.CreateDirectory(Paths.DataDir);
            string tmp = Paths.SettingsFile + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(s, JsonOpts));
            File.Move(tmp, Paths.SettingsFile, true);
        }
        catch { /* the choice still applies for this session */ }
    }
}
