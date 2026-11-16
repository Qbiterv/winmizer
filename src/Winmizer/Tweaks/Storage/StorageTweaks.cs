using System.Text.RegularExpressions;
using Winmizer.Core;

namespace Winmizer.Tweaks.Storage;

public static class StorageTweaks
{
    public static IEnumerable<ITweak> Create()
    {
        yield return new FsutilTweak("storage.trim", "Upewnij się, że TRIM jest włączony",
            "fsutil behavior set DisableDeleteNotify 0 - system informuje SSD o zwolnionych blokach (wydajność i żywotność).",
            "DisableDeleteNotify", "0", accept: new[] { 0 }) { Recommended = true };

        yield return new FsutilTweak("storage.lastaccess", "Wyłącz znacznik ostatniego dostępu NTFS",
            "fsutil behavior set disablelastaccess 1 - mniej zapisów na dysk przy każdym odczycie pliku.",
            "disablelastaccess", "1", accept: new[] { 1, 3 }) { Recommended = true };
    }
}

/// <summary>Reads and sets an NTFS behavior flag with fsutil; remembers the original.</summary>
public sealed class FsutilTweak : TweakBase
{
    private readonly string _flag, _target;
    private readonly int[] _accept;

    public FsutilTweak(string id, string name, string description, string flag, string target, int[] accept)
        : base(id, name, description, TweakCategory.Storage, RiskLevel.Safe)
    {
        _flag = flag; _target = target; _accept = accept;
    }

    private int? Current()
    {
        var r = Proc.Run("fsutil.exe", new[] { "behavior", "query", _flag }, mutating: false);
        // Output looks like "NTFS DisableDeleteNotify = 0 (Disabled)" / "DisableLastAccess = 2 (...)"; first number after '='.
        var m = Regex.Match(r.Out, @"=\s*(\d+)");
        return m.Success ? int.Parse(m.Groups[1].Value) : null;
    }

    public override bool IsApplied() => Current() is int v && _accept.Contains(v);

    public override void Apply()
    {
        var cur = Current();
        if (cur.HasValue) Ctx.Backup.Record(Id, new BackupEntry { Kind = "fsutil", Key = _flag, Value = cur.Value.ToString() });
        Ctx.Backup.Save();
        var r = Proc.Run("fsutil.exe", new[] { "behavior", "set", _flag, _target },
            description: $"fsutil behavior set {_flag} {_target}");
        if (!r.Ok) throw new InvalidOperationException(r.Text);
    }
}
