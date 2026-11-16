using Microsoft.Win32;

namespace Winmizer.Core;

public readonly record struct RegSet(string Key, string Name, object? Value, RegistryValueKind Kind = RegistryValueKind.DWord);

/// <summary>Short factories for registry entries.</summary>
public static class R
{
    public static RegSet Dw(string key, string name, uint value) => new(key, name, value, RegistryValueKind.DWord);
    public static RegSet Sz(string key, string name, string value) => new(key, name, value, RegistryValueKind.String);
    /// <summary>Entry meaning "this value must not exist".</summary>
    public static RegSet Rm(string key, string name) => new(key, name, null, RegistryValueKind.String);
}

/// <summary>Registry helpers (64-bit view, "HKLM\..." / "HKCU\..." paths). Writes honour dry-run.</summary>
public static class Reg
{
    private static RegistryKey Root(string hive) => hive.ToUpperInvariant() switch
    {
        "HKLM" => RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64),
        "HKCU" => RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64),
        _ => throw new ArgumentException("Unsupported hive: " + hive)
    };

    private static (string Hive, string Sub) Split(string path)
    {
        int i = path.IndexOf('\\');
        return i < 0 ? (path, "") : (path[..i], path[(i + 1)..]);
    }

    public static bool TryGet(string key, string name, out object? value, out RegistryValueKind kind)
    {
        value = null; kind = RegistryValueKind.Unknown;
        try
        {
            var (hive, sub) = Split(key);
            using var root = Root(hive);
            using var rk = root.OpenSubKey(sub, false);
            if (rk == null) return false;
            var v = rk.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            if (v == null) return false;
            value = v; kind = rk.GetValueKind(name);
            return true;
        }
        catch { return false; }
    }

    public static bool KeyExists(string key)
    {
        try
        {
            var (hive, sub) = Split(key);
            using var root = Root(hive);
            using var rk = root.OpenSubKey(sub, false);
            return rk != null;
        }
        catch { return false; }
    }

    public static string[] SubKeys(string key)
    {
        try
        {
            var (hive, sub) = Split(key);
            using var root = Root(hive);
            using var rk = root.OpenSubKey(sub, false);
            return rk?.GetSubKeyNames() ?? Array.Empty<string>();
        }
        catch { return Array.Empty<string>(); }
    }

    public static string[] ValueNames(string key)
    {
        try
        {
            var (hive, sub) = Split(key);
            using var root = Root(hive);
            using var rk = root.OpenSubKey(sub, false);
            return rk?.GetValueNames() ?? Array.Empty<string>();
        }
        catch { return Array.Empty<string>(); }
    }

    public static void Set(string key, string name, object value, RegistryValueKind kind)
    {
        if (Ctx.DryRun)
        {
            Ctx.Log.Dry($"Ustawiłbym rejestr: {key} -> {name} = {Serialize(value, kind)}");
            return;
        }
        var (hive, sub) = Split(key);
        using var root = Root(hive);
        using var rk = root.CreateSubKey(sub, true) ?? throw new InvalidOperationException("Nie można otworzyć klucza " + key);
        object toWrite = value;
        if (kind == RegistryValueKind.DWord)
            toWrite = value is uint u ? unchecked((int)u) : Convert.ToInt32(value);
        rk.SetValue(name, toWrite, kind);
        Ctx.Log.Info($"REG SET {key} -> {name} = {Serialize(value, kind)}");
    }

    public static void Delete(string key, string name)
    {
        if (Ctx.DryRun)
        {
            Ctx.Log.Dry($"Usunąłbym wartość rejestru: {key} -> {name}");
            return;
        }
        var (hive, sub) = Split(key);
        using var root = Root(hive);
        using var rk = root.OpenSubKey(sub, true);
        if (rk == null) return;
        rk.DeleteValue(name, false);
        Ctx.Log.Info($"REG DEL {key} -> {name}");
    }

    private static uint ToUInt(object? o) => o switch
    {
        uint u => u,
        int i => unchecked((uint)i),
        long l => unchecked((uint)l),
        _ => uint.MaxValue - 12345
    };

    public static bool Equal(object? actual, object? desired, RegistryValueKind desiredKind, RegistryValueKind actualKind)
    {
        if (desiredKind == RegistryValueKind.DWord)
            return actualKind == RegistryValueKind.DWord && ToUInt(actual) == ToUInt(desired);
        if (desiredKind is RegistryValueKind.String or RegistryValueKind.ExpandString)
            return string.Equals(actual?.ToString(), desired?.ToString(), StringComparison.OrdinalIgnoreCase);
        if (desiredKind == RegistryValueKind.Binary)
            return actual is byte[] a && desired is byte[] d && a.SequenceEqual(d);
        return Equals(actual, desired);
    }

    public static string Serialize(object? v, RegistryValueKind kind) => kind switch
    {
        RegistryValueKind.DWord => ToUInt(v).ToString(),
        RegistryValueKind.QWord => Convert.ToInt64(v).ToString(),
        RegistryValueKind.Binary => Convert.ToBase64String((byte[])v!),
        RegistryValueKind.MultiString => string.Join("\0", (string[])v!),
        _ => v?.ToString() ?? ""
    };

    public static object Deserialize(string s, RegistryValueKind kind) => kind switch
    {
        RegistryValueKind.DWord => uint.Parse(s),
        RegistryValueKind.QWord => long.Parse(s),
        RegistryValueKind.Binary => Convert.FromBase64String(s),
        RegistryValueKind.MultiString => s.Split('\0'),
        _ => s
    };
}
