using System.Diagnostics;
using System.Text;

namespace Winmizer.Core;

public sealed record ProcResult(int ExitCode, string Out, string Err)
{
    public bool Ok => ExitCode == 0;
    public string Text => (Out + Environment.NewLine + Err).Trim();
}

/// <summary>External process runner. Mutating calls are only logged in dry-run mode.</summary>
public static class Proc
{
    public static ProcResult Run(string file, IEnumerable<string> args, bool mutating = true,
        int timeoutMs = 120_000, bool utf8 = false, string? description = null)
    {
        var argList = args.ToList();
        if (mutating && Ctx.DryRun)
        {
            Ctx.Log.Dry(description ?? $"Uruchomiłbym: {file} {string.Join(' ', argList)}");
            return new ProcResult(0, "", "");
        }

        var psi = new ProcessStartInfo(file)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
        };
        var enc = utf8 ? Encoding.UTF8 : OemEncoding();
        psi.StandardOutputEncoding = enc;
        psi.StandardErrorEncoding = enc;
        foreach (var a in argList) psi.ArgumentList.Add(a);

        try
        {
            using var p = Process.Start(psi) ?? throw new InvalidOperationException("Nie udało się uruchomić " + file);
            p.StandardInput.Close();
            var stdout = new StringBuilder();
            var stderr = new StringBuilder();
            p.OutputDataReceived += (_, e) => { if (e.Data != null) lock (stdout) stdout.AppendLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) lock (stderr) stderr.AppendLine(e.Data); };
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            if (!p.WaitForExit(timeoutMs))
            {
                try { p.Kill(true); } catch { }
                Ctx.Log.Warn($"Przekroczono limit czasu: {file}");
                return new ProcResult(-1, stdout.ToString(), "timeout");
            }
            p.WaitForExit(); // flush async readers
            var res = new ProcResult(p.ExitCode, stdout.ToString(), stderr.ToString());
            if (mutating) Ctx.Log.Info($"EXEC {file} {string.Join(' ', argList)} -> {res.ExitCode}");
            return res;
        }
        catch (Exception ex)
        {
            return new ProcResult(-1, "", ex.Message);
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern uint GetOEMCP();

    /// <summary>Console tools (powercfg, sc, fsutil) write in the OEM code page.</summary>
    private static Encoding OemEncoding()
    {
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding((int)GetOEMCP());
        }
        catch { return Encoding.UTF8; }
    }

    public static string System32(string exe) =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), exe);
}

/// <summary>Runs PowerShell 5.1 scripts via -EncodedCommand (no quoting problems).</summary>
public static class PS
{
    public static ProcResult Run(string script, bool mutating = true, int timeoutMs = 180_000, string? description = null)
    {
        if (mutating && Ctx.DryRun)
        {
            Ctx.Log.Dry(description ?? "Uruchomiłbym PowerShell: " + script.Split('\n')[0].Trim());
            return new ProcResult(0, "", "");
        }
        string full = "$ErrorActionPreference='Stop'; $ProgressPreference='SilentlyContinue'; " +
                      "try { [Console]::OutputEncoding=[Text.Encoding]::UTF8 } catch {} ; " + script;
        string enc = Convert.ToBase64String(Encoding.Unicode.GetBytes(full));
        var res = Proc.Run("powershell.exe",
            new[] { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-EncodedCommand", enc },
            mutating: false, timeoutMs: timeoutMs, utf8: true);
        if (mutating) Ctx.Log.Info($"PS [{res.ExitCode}] {script.Split('\n')[0].Trim()}");
        return res;
    }

    /// <summary>Read-only query; always executed, even in dry-run.</summary>
    public static ProcResult Query(string script, int timeoutMs = 60_000) => Run(script, false, timeoutMs);

    /// <summary>Escapes a string for use inside a single-quoted PowerShell literal.</summary>
    public static string Q(string s) => s.Replace("'", "''");
}

