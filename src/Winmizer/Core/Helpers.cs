using System.Text.RegularExpressions;

namespace Winmizer.Core;

/// <summary>Windows service helpers based on the registry and sc.exe.</summary>
public static class ServiceHelper
{
    // Services that Winmizer must never touch (printing, updates, security, network, audio...).
    private static readonly HashSet<string> Protected = new(StringComparer.OrdinalIgnoreCase)
    {
        "Spooler", "PrintNotify", "PrintWorkflowUserSvc", "wuauserv", "UsoSvc", "BITS", "WaaSMedicSvc", "TrustedInstaller",
        "WinDefend", "WdNisSvc", "Sense", "SecurityHealthService", "wscsvc", "mpssvc", "BFE", "Dhcp", "Dnscache",
        "WlanSvc", "bthserv", "BthAvctpSvc", "NlaSvc", "netprofm", "Audiosrv", "AudioEndpointBuilder", "PlugPlay",
        "InstallService", "ClipSVC", "AppXSvc", "XblAuthManager", "XblGameSave", "GamingServices", "GamingServicesNet",
        "XboxGipSvc", "XboxNetApiSvc", "WbioSrvc", "NgcSvc", "NgcCtnrSvc", "RpcSs", "DcomLaunch", "EventLog",
        "LanmanWorkstation", "LanmanServer", "TabletInputService", "UserManager", "ProfSvc", "StateRepository"
    };

    public static bool IsProtected(string name) => Protected.Contains(name);

    public static (int Start, bool Delayed)? Get(string name)
    {
        string key = @"HKLM\SYSTEM\CurrentControlSet\Services\" + name;
        if (!Reg.TryGet(key, "Start", out var v, out _)) return null;
        bool delayed = Reg.TryGet(key, "DelayedAutostart", out var d, out _) && Convert.ToInt32(d) == 1;
        return (Convert.ToInt32(v), delayed);
    }

    public static void SetStart(string name, int start, bool delayed)
    {
        if (IsProtected(name)) throw new InvalidOperationException($"Usługa {name} jest chroniona i nie może być zmieniana.");
        string mode = start switch
        {
            2 => delayed ? "delayed-auto" : "auto",
            3 => "demand",
            4 => "disabled",
            _ => throw new InvalidOperationException("Nieobsługiwany typ startu: " + start)
        };
        var r = Proc.Run("sc.exe", new[] { "config", name, "start=", mode },
            description: $"Ustawiłbym usługę {name} na tryb: {mode}");
        if (!r.Ok) throw new InvalidOperationException($"sc config {name} nie powiodło się: {r.Text}");
    }

    public static bool IsRunning(string name)
    {
        var r = Proc.Run("sc.exe", new[] { "query", name }, mutating: false);
        var m = Regex.Match(r.Out, @"STATE\s*:\s*(\d+)");
        return m.Success && m.Groups[1].Value == "4";
    }

    public static void Stop(string name)
    {
        if (IsProtected(name))
            throw new InvalidOperationException($"Usługa {name} jest chroniona.");
        if (!Ctx.DryRun && !IsRunning(name)) return;
        Proc.Run("net.exe", new[] { "stop", name, "/y" }, timeoutMs: 60_000, description: $"Zatrzymałbym usługę {name}");
    }

    public static void StartNow(string name) =>
        Proc.Run("net.exe", new[] { "start", name }, timeoutMs: 60_000, description: $"Uruchomiłbym usługę {name}");
}

/// <summary>powercfg wrapper.</summary>
public static class PowerCfg
{
    public const string HighPerformance = "8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
    public const string Balanced = "381b4222-f694-41f0-9685-ff5bb260df2e";
    public const string UltimateTemplate = "e9a42b02-d5df-448d-aa00-03f14749eb61";
    public const string UltimateName = "Winmizer Ultimate";

    public static (string Guid, string Name) Active()
    {
        var r = Proc.Run("powercfg.exe", new[] { "/getactivescheme" }, mutating: false);
        var g = Regex.Match(r.Out, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
        var n = Regex.Match(r.Out, @"\(([^()]*)\)\s*$", RegexOptions.Multiline);
        return (g.Success ? g.Value.ToLowerInvariant() : "", n.Success ? n.Groups[1].Value.Trim() : "?");
    }

    public static List<(string Guid, string Name)> List()
    {
        var r = Proc.Run("powercfg.exe", new[] { "/list" }, mutating: false);
        return Regex.Matches(r.Out, @"([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\s+\(([^)]*)\)")
            .Select(m => (m.Groups[1].Value.ToLowerInvariant(), m.Groups[2].Value.Trim())).ToList();
    }

    /// <summary>Reads the AC or DC index of a setting (locale independent: last two hex numbers).</summary>
    public static uint? GetIndex(string scheme, string sub, string setting, bool ac)
    {
        var r = Proc.Run("powercfg.exe", new[] { "/query", scheme, sub, setting }, mutating: false);
        var hex = Regex.Matches(r.Out, @"0x[0-9a-fA-F]{8}").Select(m => m.Value).ToList();
        if (hex.Count < 2) return null;
        string pick = ac ? hex[^2] : hex[^1];
        return Convert.ToUInt32(pick, 16);
    }

    public static void SetIndex(string scheme, string sub, string setting, bool ac, uint value)
    {
        var r = Proc.Run("powercfg.exe", new[] { ac ? "/setacvalueindex" : "/setdcvalueindex", scheme, sub, setting, value.ToString() },
            description: $"Ustawiłbym powercfg {(ac ? "AC" : "DC")} {sub}/{setting} = {value}");
        if (!r.Ok) throw new InvalidOperationException("powercfg: " + r.Text);
        // Re-apply only if this scheme is the active one (never switch plans as a side effect of a revert).
        if (Ctx.DryRun || Active().Guid.Equals(scheme, StringComparison.OrdinalIgnoreCase)) SetActive(scheme);
    }

    public static void SetActive(string scheme)
    {
        var r = Proc.Run("powercfg.exe", new[] { "/setactive", scheme }, description: $"Aktywowałbym plan zasilania {scheme}");
        if (!r.Ok) throw new InvalidOperationException("powercfg /setactive: " + r.Text);
    }

    public static void Delete(string guid) =>
        Proc.Run("powercfg.exe", new[] { "/delete", guid }, description: $"Usunąłbym plan zasilania {guid}");
}

/// <summary>DNS helpers via PowerShell.</summary>
public static class DnsHelper
{
    public static readonly string[] Cloudflare = { "1.1.1.1", "1.0.0.1" };

    /// <summary>Returns (alias, comma separated IPv4 DNS) for every connected physical adapter.</summary>
    public static List<(string Alias, string Dns)> Adapters()
    {
        // DNS is read from the adapter's registry NameServer value: it is empty when the DNS comes from DHCP
        // (so revert resets to automatic instead of freezing the DHCP-provided addresses as static ones).
        var r = PS.Query("Get-NetAdapter -Physical | Where-Object Status -eq 'Up' | ForEach-Object { " +
                         "$ns=(Get-ItemProperty -Path ('HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\' + $_.InterfaceGuid) -ErrorAction SilentlyContinue).NameServer; " +
                         "$d=(@($ns -split '[,\\s]+' | Where-Object { $_ }) -join ','); " +
                         "\"$($_.Name)|$d\" }");
        var list = new List<(string, string)>();
        if (!r.Ok) return list;
        foreach (var line in r.Out.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var t = line.Trim();
            int i = t.LastIndexOf('|');
            if (i > 0) list.Add((t[..i], t[(i + 1)..]));
        }
        return list;
    }

    public static void Set(string alias, string commaSeparated)
    {
        string script = string.IsNullOrWhiteSpace(commaSeparated)
            ? $"Set-DnsClientServerAddress -InterfaceAlias '{PS.Q(alias)}' -ResetServerAddresses"
            : $"Set-DnsClientServerAddress -InterfaceAlias '{PS.Q(alias)}' -ServerAddresses ({string.Join(",", commaSeparated.Split(',').Select(a => "'" + PS.Q(a) + "'"))})";
        var r = PS.Run(script, description: $"Ustawiłbym DNS na karcie '{alias}': {(commaSeparated == "" ? "automatyczny" : commaSeparated)}");
        if (!r.Ok) throw new InvalidOperationException("DNS: " + r.Text);
    }
}

/// <summary>Hardware / OS information via WMI.</summary>
public static class Hardware
{
    private static bool? _laptop;
    public static bool IsLaptop => _laptop ??= Query("SELECT * FROM Win32_Battery", "Name").Count > 0;

    private static List<string> Query(string wql, string prop)
    {
        var res = new List<string>();
        try
        {
            using var s = new System.Management.ManagementObjectSearcher(wql);
            foreach (System.Management.ManagementBaseObject o in s.Get())
                res.Add(o[prop]?.ToString()?.Trim() ?? "");
        }
        catch { }
        return res;
    }

    public static string Cpu => string.Join(", ", Query("SELECT Name FROM Win32_Processor", "Name"));
    public static string Gpus => string.Join(", ", Query("SELECT Name FROM Win32_VideoController", "Name"));
    public static string Os => string.Join("", Query("SELECT Caption FROM Win32_OperatingSystem", "Caption")) +
                               " (build " + string.Join("", Query("SELECT BuildNumber FROM Win32_OperatingSystem", "BuildNumber")) + ")";
    public static string Ram
    {
        get
        {
            var v = Query("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem", "TotalPhysicalMemory").FirstOrDefault();
            return double.TryParse(v, out var b) ? $"{b / 1024 / 1024 / 1024:0.#} GB" : "?";
        }
    }
}
