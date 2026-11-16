using System.Text.Json;
using Microsoft.Win32;

namespace Winmizer.Core;

/// <summary>One saved original value. Kind: reg, service, task, power, fsutil, dns, hibernate,
/// activescheme, createdscheme, startupreg, startupfile.</summary>
public sealed class BackupEntry
{
    public string Kind { get; set; } = "";
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public bool Existed { get; set; } = true;
    public string? ValueKind { get; set; }
    public string? Value { get; set; }
}

public sealed class BackupData
{
    public int Version { get; set; } = 1;
    public string? RestorePoint { get; set; }
    public Dictionary<string, List<BackupEntry>> Tweaks { get; set; } = new();
}

/// <summary>
/// JSON backup (%ProgramData%\Winmizer\backup.json) holding the ORIGINAL values of every
/// tweak. Only the first original per (kind,key,name) is kept, so re-applying never
/// overwrites the true original.
/// </summary>
public sealed class BackupStore
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
    private BackupData _data = new();
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;
        try
        {
            if (File.Exists(Paths.BackupFile))
                _data = JsonSerializer.Deserialize<BackupData>(File.ReadAllText(Paths.BackupFile)) ?? new BackupData();
        }
        catch (Exception ex)
        {
            Ctx.Log.Warn("Nie można wczytać backup.json: " + ex.Message);
            try { File.Copy(Paths.BackupFile, Paths.BackupFile + ".corrupt", true); } catch { }
            _data = new BackupData();
        }
    }

    public IReadOnlyCollection<string> Ids { get { EnsureLoaded(); return _data.Tweaks.Keys.ToList(); } }
    public bool Has(string id) { EnsureLoaded(); return _data.Tweaks.TryGetValue(id, out var l) && l.Count > 0; }
    public IReadOnlyList<BackupEntry> Entries(string id)
    {
        EnsureLoaded();
        return _data.Tweaks.TryGetValue(id, out var l) ? l : new List<BackupEntry>();
    }

    public string? RestorePointInfo { get { EnsureLoaded(); return _data.RestorePoint; } }
    public void SetRestorePoint(string info) { EnsureLoaded(); if (!Ctx.DryRun) { _data.RestorePoint = info; Save(); } }

    public void Record(string id, BackupEntry entry)
    {
        EnsureLoaded();
        if (Ctx.DryRun) return;
        if (!_data.Tweaks.TryGetValue(id, out var list)) _data.Tweaks[id] = list = new List<BackupEntry>();
        if (list.Any(e => e.Kind == entry.Kind && e.Key == entry.Key && e.Name == entry.Name)) return;
        list.Add(entry);
    }

    /// <summary>Records the current state of a registry value before it is changed.</summary>
    public void RecordReg(string id, string key, string name)
    {
        bool has = Reg.TryGet(key, name, out var v, out var k);
        Record(id, new BackupEntry
        {
            Kind = "reg", Key = key, Name = name, Existed = has,
            ValueKind = has ? k.ToString() : null,
            Value = has ? Reg.Serialize(v, k) : null
        });
    }

    public void Remove(string id)
    {
        EnsureLoaded();
        if (Ctx.DryRun) return;
        if (_data.Tweaks.Remove(id)) Save();
    }

    public void Save()
    {
        EnsureLoaded();
        if (Ctx.DryRun) return;
        try
        {
            Directory.CreateDirectory(Paths.DataDir);
            string tmp = Paths.BackupFile + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(_data, JsonOpts));
            File.Move(tmp, Paths.BackupFile, true);
        }
        catch (Exception ex) { Ctx.Log.Error("Zapis backup.json nie powiódł się: " + ex.Message); }
    }
}
