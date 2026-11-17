using Microsoft.Win32;
using Winmizer.Core.Localization;

namespace Winmizer.Core;

public sealed record StartupItem(string Name, string Command, string Source, bool IsRegistry,
    string Location /* full run-key path or file path */, RegistryValueKind Kind = RegistryValueKind.String, object? RawValue = null);

/// <summary>
/// Lists startup entries and disables them reversibly: registry values are moved to
/// HKxx\SOFTWARE\Winmizer\DisabledStartup\..., shortcut files to %ProgramData%\Winmizer\DisabledStartup.
/// </summary>
public static class StartupManager
{
    private static readonly (string Key, string Label)[] RunKeys =
    {
        (@"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKLM Run"),
        (@"HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", "HKLM Run (32-bit)"),
        (@"HKCU\Software\Microsoft\Windows\CurrentVersion\Run", "HKCU Run"),
    };

    private static string DisabledKey(string runKey)
    {
        string hive = runKey[..runKey.IndexOf('\\')];
        return hive + @"\SOFTWARE\Winmizer\DisabledStartup\" + runKey.Replace('\\', '_');
    }

    public static string BackupId(StartupItem i) => "startup:" + i.Location + "|" + i.Name;

    public static List<StartupItem> List()
    {
        var items = new List<StartupItem>();
        foreach (var (key, label) in RunKeys)
            foreach (var name in Reg.ValueNames(key))
                if (Reg.TryGet(key, name, out var v, out var k))
                    items.Add(new StartupItem(name, v?.ToString() ?? "", label, true, key, k, v));

        foreach (var (dir, label) in new[]
        {
            (Environment.GetFolderPath(Environment.SpecialFolder.Startup), L.T("Startup folder (current user)")),
            (Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), L.T("Startup folder (all users)"))
        })
        {
            try
            {
                if (!Directory.Exists(dir)) continue;
                foreach (var f in Directory.GetFiles(dir))
                    if (!Path.GetFileName(f).Equals("desktop.ini", StringComparison.OrdinalIgnoreCase))
                        items.Add(new StartupItem(Path.GetFileName(f), f, label, false, f));
            }
            catch { }
        }
        return items;
    }

    public static void Disable(StartupItem item)
    {
        string id = BackupId(item);
        if (item.IsRegistry)
        {
            Ctx.Backup.Record(id, new BackupEntry
            {
                Kind = "startupreg", Key = item.Location, Name = item.Name, Existed = true,
                ValueKind = item.Kind.ToString(), Value = Reg.Serialize(item.RawValue, item.Kind)
            });
            Ctx.Backup.Save();
            Reg.Set(DisabledKey(item.Location), item.Name, item.RawValue!, item.Kind);
            Reg.Delete(item.Location, item.Name);
        }
        else
        {
            string target = Path.Combine(Paths.DisabledStartupDir, Guid.NewGuid().ToString("N").Substring(0, 8) + "_" + item.Name);
            Ctx.Backup.Record(id, new BackupEntry { Kind = "startupfile", Key = item.Location, Name = item.Name, Value = target });
            Ctx.Backup.Save();
            if (Ctx.DryRun) { Ctx.Log.Dry(L.F("Would move {0} -> {1}", item.Location, target)); return; }
            Directory.CreateDirectory(Paths.DisabledStartupDir);
            File.Move(item.Location, target);
            Ctx.Log.Info(L.F("Moved {0} -> {1}", item.Location, target));
        }
    }

    public static void Restore(BackupEntry e)
    {
        if (e.Kind == "startupreg")
        {
            var kind = Enum.Parse<RegistryValueKind>(e.ValueKind ?? "String");
            Reg.Set(e.Key, e.Name, Reg.Deserialize(e.Value ?? "", kind), kind);
            Reg.Delete(DisabledKey(e.Key), e.Name);
        }
        else if (e.Kind == "startupfile")
        {
            if (Ctx.DryRun) { Ctx.Log.Dry(L.F("Would restore {0} -> {1}", e.Value, e.Key)); return; }
            if (File.Exists(e.Value)) File.Move(e.Value!, e.Key, false);
            else Ctx.Log.Warn(L.F("File to restore not found: {0}", e.Value));
        }
    }

    /// <summary>Backup ids of startup entries disabled by Winmizer.</summary>
    public static List<string> DisabledIds() =>
        Ctx.Backup.Ids.Where(i => i.StartsWith("startup:", StringComparison.Ordinal)).ToList();
}
