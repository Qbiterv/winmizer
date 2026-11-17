using System.Text.RegularExpressions;
using Winmizer.Core;
using Winmizer.Core.Localization;

namespace Winmizer.Tweaks.Debloat;

/// <summary>Cached list of installed / provisioned Appx packages (one PowerShell call).</summary>
public static class AppxCache
{
    private static List<string>? _names;

    public static void Invalidate() => _names = null;

    /// <summary>
    /// Names of packages that are really present: installed for at least one user (staged-only entries
    /// don't count) or provisioned for new users. Protected packages are never listed.
    /// </summary>
    public static List<string> Names()
    {
        if (_names != null) return _names;
        var r = PS.Query(
            "$n=@(); " +
            "try { $n += Get-AppxPackage -AllUsers | Where-Object { $i=@($_.PackageUserInformation); $i.Count -eq 0 -or @($i | Where-Object { \"$($_.InstallState)\" -eq 'Installed' }).Count -gt 0 } | Select-Object -ExpandProperty Name } catch { try { $n += Get-AppxPackage | Select-Object -ExpandProperty Name } catch {} }; " +
            "try { $n += Get-AppxProvisionedPackage -Online | Select-Object -ExpandProperty DisplayName } catch {}; " +
            "$n | Sort-Object -Unique", 120_000);
        _names = r.Out.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => s.Length > 0)
            .Where(n => !DebloatTweaks.IsProtectedName(n)).ToList();
        return _names;
    }

    public static bool IsPresent(string pattern) => Names().Any(n => Wildcard.IsMatch(pattern, n));
}

internal static class Wildcard
{
    public static bool IsMatch(string pattern, string text) =>
        Regex.IsMatch(text, "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
}

/// <summary>
/// Removes Appx packages for all users and from the provisioned image, then re-queries to verify.
/// If anything is still present the call throws, so the tweak is reported as FAILED (not success).
/// </summary>
public static class AppxRemover
{
    public static void RemoveAll(IEnumerable<string> patterns)
    {
        var failures = new List<string>();
        foreach (var pattern in patterns)
        {
            try { Remove(pattern); }
            catch (Exception ex) { failures.Add(ex.Message); }
        }
        AppxCache.Invalidate();
        if (failures.Count > 0) throw new InvalidOperationException(string.Join(" || ", failures));
    }

    public static void Remove(string pattern)
    {
        if (DebloatTweaks.IsProtected(pattern))
            throw new InvalidOperationException(L.F("Package {0} is protected - cannot create a tweak for it.", pattern));

        string p = PS.Q(pattern);
        string prot = string.Join(",", DebloatTweaks.ProtectedNames.Select(n => "'" + PS.Q(n) + "'"));
        string script =
            $"$p='{p}'; $prot=@({prot}); $out=@(); " +
            "function Keep($n){ -not ($prot | Where-Object { $n -like $_ }) }; " +
            "function Live($x){ $i=@($x.PackageUserInformation); $i.Count -eq 0 -or @($i | Where-Object { \"$($_.InstallState)\" -eq 'Installed' }).Count -gt 0 }; " +
            // 1) installed packages (any user)
            "foreach($x in @(Get-AppxPackage -AllUsers -Name $p | Where-Object { Keep $_.Name })){ " +
            "try { Remove-AppxPackage -Package $x.PackageFullName -AllUsers -ErrorAction Stop } " +
            "catch { $e1=$_.Exception.Message; try { Remove-AppxPackage -Package $x.PackageFullName -ErrorAction Stop } " +
            "catch { $out += ('ERR Remove-AppxPackage ' + $x.PackageFullName + ': ' + $e1 + ' | ' + $_.Exception.Message) } } }; " +
            // 2) provisioned (de-provision for new users) - also covers packages that exist only as provisioned
            "foreach($x in @(Get-AppxProvisionedPackage -Online | Where-Object { $_.DisplayName -like $p -and (Keep $_.DisplayName) })){ " +
            "try { Remove-AppxProvisionedPackage -Online -PackageName $x.PackageName -ErrorAction Stop | Out-Null } " +
            "catch { $out += ('ERR Remove-AppxProvisionedPackage ' + $x.PackageName + ': ' + $_.Exception.Message) } }; " +
            // 3) verify: re-query both sources
            "foreach($x in @(Get-AppxPackage -AllUsers -Name $p | Where-Object { (Keep $_.Name) -and (Live $_) })){ " +
            "$u=(@($x.PackageUserInformation | ForEach-Object { '' + $_.UserSecurityId.UserName + '=' + $_.InstallState }) -join ','); " +
            "$out += ('LEFT installed ' + $x.PackageFullName + ' [' + $u + ']') }; " +
            "foreach($x in @(Get-AppxProvisionedPackage -Online | Where-Object { $_.DisplayName -like $p -and (Keep $_.DisplayName) })){ " +
            "$out += ('LEFT provisioned ' + $x.PackageName) }; " +
            "$out";

        var r = PS.Run(script, description: L.F("Would remove Appx package: {0} (Remove-AppxPackage -AllUsers + Remove-AppxProvisionedPackage)", pattern));
        var lines = r.Out.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        foreach (var l in lines.Where(l => l.StartsWith("ERR"))) Ctx.Log.Warn($"{pattern}: {l}");

        var left = lines.Where(l => l.StartsWith("LEFT")).ToList();
        if (!r.Ok || left.Count > 0)
        {
            string detail = string.Join("; ", lines.Where(l => l.StartsWith("ERR") || l.StartsWith("LEFT")));
            if (!r.Ok && r.Err.Trim().Length > 0) detail = (detail.Length > 0 ? detail + "; " : "") + r.Err.Trim();
            if (detail.Length == 0) detail = r.Text;
            string msg = L.F("Package {0} is still present after removal: {1}", pattern, detail);
            Ctx.Log.Error(msg);
            throw new InvalidOperationException(msg);
        }
    }
}
