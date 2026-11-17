using Spectre.Console;
using Winmizer.Core;
using Winmizer.Core.Localization;
using Winmizer.Tweaks;
using Winmizer.Tweaks.Cleanup;
using Winmizer.Tweaks.Debloat;

namespace Winmizer.UI;

/// <summary>Shared Spectre.Console rendering helpers (all user-facing text goes through <see cref="L"/>).</summary>
public static class Ui
{
    public static void Banner()
    {
        if (AnsiConsole.Profile.Capabilities.Interactive) AnsiConsole.Clear();
        try { Console.Title = $"Winmizer v{AppInfo.Version}"; } catch { /* no console window */ }
        AnsiConsole.Write(new FigletText("Winmizer").Color(Color.Aqua));
        AnsiConsole.MarkupLine($"[mediumpurple1]v{Esc(AppInfo.Version)}[/]  [grey]" + L.T("Windows 10/11 gaming optimizer - safe, reversible, with a restore point") + "[/]");
        AnsiConsole.MarkupLine($"[grey]{L.T("System:")}[/] [aqua]{Esc(OsInfo.Description)}[/]");
        if (OsInfo.UnsupportedWarning is { } w)
            AnsiConsole.MarkupLine($"[black on yellow] {L.T("WARNING")} [/] [yellow]{Esc(w)}[/]");
        if (Ctx.DryRun)
            AnsiConsole.MarkupLine($"[black on yellow] {L.T("DRY-RUN MODE")} [/] [yellow]{L.T("Nothing will be changed - preview of actions only.")}[/]");
        AnsiConsole.WriteLine();
    }

    public static string Esc(string s) => Markup.Escape(s);

    public static string RiskMarkup(RiskLevel r) => $"[{Labels.RiskColor(r)}]{Labels.Risk(r)}[/]";

    public static string StatusMarkup(ITweak t)
    {
        try
        {
            if (!t.IsApplicable) return "[grey]" + L.T("N/A on this system") + "[/]";
            if (t.IsAction) return "[grey]" + L.T("one-time action") + "[/]";
            return t.IsApplied() ? "[green]" + L.T("YES - applied") + "[/]" : "[grey]" + L.T("Not applied") + "[/]";
        }
        catch { return "[red]" + L.T("detection error") + "[/]"; }
    }

    /// <summary>Localized "Windows 10" / "Windows 11" for the OS-only notes.</summary>
    public static string OsOnlyLabel(OsSupport s) => s == OsSupport.Win11 ? "Windows 11" : "Windows 10";

    /// <summary>Table with risk level and description of every tweak (shown before applying).</summary>
    public static void ShowTweakTable(IEnumerable<ITweak> tweaks)
    {
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
        table.AddColumn(L.T("Category"));
        table.AddColumn(L.T("Tweak"));
        table.AddColumn(L.T("Risk"));
        table.AddColumn(L.T("Description"));
        foreach (var t in tweaks)
        {
            string desc = Esc(t.Description);
            if (t.Warning != null) desc += $"\n[red]{L.T("Warning:")}[/] {Esc(t.Warning)}";
            if (t.OsNote != null) desc += $"\n[grey]{Esc(OsInfo.Name)}:[/] {Esc(t.OsNote)}";
            if (t.SupportedOn != OsSupport.Both) desc += $"\n[grey]{L.F("Applies to: {0} only", OsOnlyLabel(t.SupportedOn))}[/]";
            table.AddRow(Esc(Labels.Category(t.Category)), $"[bold]{Esc(t.Name)}[/]", RiskMarkup(t.Risk), desc);
        }
        AnsiConsole.Write(table);
    }

    public static string ChoiceLabel(ITweak t)
    {
        string label = $"[{Labels.RiskColor(t.Risk)}]{Labels.Risk(t.Risk)}[/] {Esc(t.Name)}";
        if (t.Warning != null) label += " [red]![/]";
        return label;
    }

    public static void Pause()
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive) return;
        AnsiConsole.MarkupLine("\n[grey]" + L.T("Press any key to return to the menu...") + "[/]");
        Console.ReadKey(true);
    }

    public static void Summary(IReadOnlyList<TweakResult> results, string verb)
    {
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
        table.AddColumn(L.T("Category"));
        table.AddColumn(L.T("Tweak"));
        table.AddColumn(L.T("Result"));
        table.AddColumn(L.T("Details"));
        foreach (var r in results)
        {
            string st = r.Status switch
            {
                ResultStatus.Applied => "[green]" + L.T("applied") + "[/]",
                ResultStatus.Reverted => "[green]" + L.T("reverted") + "[/]",
                ResultStatus.DryRun => "[yellow]dry-run[/]",
                ResultStatus.Skipped => "[grey]" + L.T("skipped") + "[/]",
                _ => "[red]" + L.T("ERROR") + "[/]"
            };
            string details = r.Message;
            if (r.Status is ResultStatus.Applied or ResultStatus.DryRun && r.Tweak.Restart != RestartNeed.None)
                details = (details.Length > 0 ? details + " " : "") + "(" + RestartLabel(r.Tweak.Restart) + ")";
            table.AddRow(Esc(Labels.Category(r.Tweak.Category)), Esc(r.Tweak.Name), st, Esc(details));
        }
        AnsiConsole.Write(table);

        int ok = results.Count(r => r.Status is ResultStatus.Applied or ResultStatus.Reverted or ResultStatus.DryRun);
        int skipped = results.Count(r => r.Status == ResultStatus.Skipped);
        int failed = results.Count(r => r.Status == ResultStatus.Failed);
        AnsiConsole.MarkupLine($"\n[bold]{L.T("Summary:")}[/] [green]{verb}: {ok}[/]  [grey]{L.T("Skipped")}: {skipped}[/]  [red]{L.T("Errors")}: {failed}[/]");

        long freed = results.Select(r => r.Tweak).OfType<ActionTweak>().Sum(a => a.FreedBytes);
        if (freed > 0) AnsiConsole.MarkupLine($"[bold]{L.T("Space freed:")}[/] [aqua]{CleanupTweaks.FormatSize(freed)}[/]");
        if (Ctx.Log.FilePath != null) AnsiConsole.MarkupLine($"[grey]{L.T("Log:")} {Esc(Ctx.Log.FilePath)}[/]");
    }

    /// <summary>Verb for the summary line ("Applied" / "Reverted" / dry-run variant).</summary>
    public static string AppliedVerb => L.T("Applied");
    public static string RevertedVerb => L.T("Reverted");

    /// <summary>The whole apply pipeline: preview, confirmation, restore point, execution, summary, reboot prompt.</summary>
    public static void ApplyFlow(List<ITweak> tweaks, bool interactive = true)
    {
        if (tweaks.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]" + L.T("Nothing selected.") + "[/]");
            return;
        }

        ShowTweakTable(tweaks);
        if (tweaks.Any(t => t.Risk == RiskLevel.Advanced))
            AnsiConsole.MarkupLine("[red]" + L.T("You selected advanced tweaks - read their descriptions above carefully.") + "[/]");

        int removing = ShowAppRemovalList(tweaks);
        string confirmText = removing > 0
            ? L.F("Apply [bold]{0}[/] change(s), including removing [bold]{1}[/] app(s)? A system restore point will be created first.", tweaks.Count, removing)
            : L.F("Apply [bold]{0}[/] change(s)? A system restore point will be created first.", tweaks.Count);

        if (interactive && !Ctx.DryRun && !AnsiConsole.Confirm(confirmText, false))
        {
            AnsiConsole.MarkupLine("[grey]" + L.T("Cancelled.") + "[/]");
            return;
        }

        // Safety first: restore point before anything is changed.
        (bool Ok, string Message) rp = (true, "");
        AnsiConsole.Status().Spinner(Spinner.Known.Dots).Start(L.T("Creating system restore point..."), _ => rp = RestorePoint.Create());
        if (rp.Ok) AnsiConsole.MarkupLine(Ctx.DryRun ? "[yellow]" + L.T("Restore point: skipped (dry-run).") + "[/]" : "[green]" + L.T("Restore point created.") + "[/]");
        else
        {
            AnsiConsole.MarkupLine($"[red]{L.T("Failed to create a restore point:")}[/] {Esc(rp.Message)}");
            if (!interactive || !AnsiConsole.Confirm(L.T("Continue anyway? (The values are still backed up to backup.json)"), false))
            {
                AnsiConsole.MarkupLine("[grey]" + L.T("Aborted - nothing was changed.") + "[/]");
                return;
            }
        }

        var results = new List<TweakResult>();
        var engine = new TweakEngine();
        var logLines = new Dictionary<string, List<(string Level, string Message)>>();
        AnsiConsole.Progress()
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn(), new SpinnerColumn())
            .Start(ctx =>
            {
                var task = ctx.AddTask(L.T("Start"), maxValue: tweaks.Count);
                engine.Apply(tweaks,
                    onStart: t => task.Description = Esc(Truncate(t.Name, 60)),
                    onDone: r => { results.Add(r); logLines[r.Tweak.Id] = Ctx.Log.Drain(); task.Increment(1); });
                task.Description = L.T("Done");
            });

        Summary(results, Ctx.DryRun ? L.T("To apply (dry-run)") : AppliedVerb);

        var interesting = logLines.Values.SelectMany(x => x).Where(l => l.Level is "WARN" or "ERROR" or "DRY").ToList();
        if (interesting.Count > 0)
        {
            AnsiConsole.MarkupLine($"\n[bold]{L.T("Action details:")}[/]");
            foreach (var (level, msg) in interesting)
            {
                string color = level switch { "ERROR" => "red", "WARN" => "yellow", _ => "grey" };
                AnsiConsole.MarkupLine($"[{color}]  [[{level}]] {Esc(msg)}[/]");
            }
        }

        RestartFlow(results, interactive);
    }

    /// <summary>Localized description of what has to be restarted.</summary>
    public static string RestartLabel(RestartNeed n) => n switch
    {
        RestartNeed.Explorer => L.T("needs Explorer restart"),
        RestartNeed.SignOut => L.T("needs sign-out"),
        RestartNeed.Reboot => L.T("needs reboot"),
        _ => L.T("no restart needed")
    };

    /// <summary>
    /// Lists the apps the selected tweaks would uninstall (with their current state) so the user can
    /// confirm once. Returns how many of them are actually installed. Protected packages never appear.
    /// </summary>
    private static int ShowAppRemovalList(IEnumerable<ITweak> tweaks)
    {
        var rows = new List<(string Name, string Pattern)>();
        foreach (var t in tweaks)
        {
            if (t is AppxTweak a) rows.Add((t.Name, a.Pattern));
            else if (t is RegistryTweak { AppxPatterns.Length: > 0 } r)
                foreach (var p in r.AppxPatterns) rows.Add((t.Name, p));
        }
        rows = rows.DistinctBy(x => x.Pattern, StringComparer.OrdinalIgnoreCase).ToList();
        if (rows.Count == 0) return 0;

        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Red);
        table.AddColumn(L.T("Apps to be removed")).AddColumn(L.T("Package")).AddColumn(L.T("State"));
        int installed = 0;
        AnsiConsole.Status().Start(L.T("Looking for installed apps..."), _ =>
        {
            foreach (var (name, pattern) in rows)
            {
                bool present = false;
                try { present = AppxCache.IsPresent(pattern); } catch { }
                if (present) installed++;
                table.AddRow(Esc(name), Esc(pattern),
                    present ? "[red]" + L.T("installed - will be removed") + "[/]" : "[grey]" + L.T("not installed - skipped") + "[/]");
            }
        });
        AnsiConsole.WriteLine();
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[grey]" + L.T("Always kept: Microsoft Store, App Installer/winget, Calculator, Photos, Notepad, Terminal, Paint, Snipping Tool, Camera, Xbox identity and Gaming Services.") + "[/]");
        return installed;
    }

    /// <summary>Tells the user what is needed for the changes to show; offers / performs the Explorer restart.</summary>
    private static void RestartFlow(IReadOnlyList<TweakResult> results, bool interactive)
    {
        var done = results.Where(r => r.Status is ResultStatus.Applied or ResultStatus.DryRun).Select(r => r.Tweak).ToList();
        var byNeed = done.Where(t => t.Restart != RestartNeed.None).GroupBy(t => t.Restart).ToDictionary(g => g.Key, g => g.ToList());
        if (byNeed.Count == 0)
        {
            if (done.Count > 0) AnsiConsole.MarkupLine("\n[green]" + L.T("No restart needed - the changes are already in effect.") + "[/]");
            return;
        }

        AnsiConsole.MarkupLine($"\n[bold]{L.T("For the changes to show:")}[/]");
        string Names(RestartNeed n) => Esc(string.Join(", ", byNeed[n].Select(t => Truncate(t.Name, 50))));
        if (byNeed.ContainsKey(RestartNeed.Reboot))
            AnsiConsole.MarkupLine("  [yellow]" + L.T("Restart the computer:") + "[/] " + Names(RestartNeed.Reboot));
        if (byNeed.ContainsKey(RestartNeed.SignOut))
            AnsiConsole.MarkupLine("  [yellow]" + L.T("Sign out and back in:") + "[/] " + Names(RestartNeed.SignOut));
        bool explorer = byNeed.ContainsKey(RestartNeed.Explorer);
        if (explorer)
            AnsiConsole.MarkupLine("  [yellow]" + L.T("Restart Explorer (taskbar / Start / shell):") + "[/] " + Names(RestartNeed.Explorer));

        if (explorer)
        {
            bool go = Ctx.DryRun || !interactive || AnsiConsole.Confirm(L.T("Restart Explorer now? (y/N)"), false);
            if (go)
            {
                var (ok, msg) = ExplorerHelper.Restart();
                foreach (var (_, m) in Ctx.Log.Drain().Where(l => l.Level == "DRY")) AnsiConsole.MarkupLine($"[grey]  {Esc(m)}[/]");
                if (!Ctx.DryRun)
                    AnsiConsole.MarkupLine(ok ? "[green]" + L.T("Explorer restarted.") + "[/]" : $"[red]{L.T("Cannot restart Explorer:")}[/] {Esc(msg)}");
            }
            else AnsiConsole.MarkupLine("[grey]" + L.T("Explorer not restarted - sign out or restart it later for the changes to show.") + "[/]");
        }

        if (byNeed.ContainsKey(RestartNeed.Reboot) && interactive && !Ctx.DryRun) RebootPrompt();
    }

    public static void RebootPrompt()
    {
        AnsiConsole.MarkupLine("\n[yellow]" + L.T("A restart is recommended for all changes to take effect.") + "[/]");
        if (AnsiConsole.Confirm(L.T("Restart now?"), false))
        {
            Proc.Run("shutdown.exe", new[] { "/r", "/t", "15", "/c", L.T("Winmizer: restart after optimization") });
            AnsiConsole.MarkupLine("[grey]" + L.T("Restarting in 15 seconds. To cancel: shutdown /a") + "[/]");
        }
        else AnsiConsole.MarkupLine("[grey]" + L.T("Remember to restart later.") + "[/]");
    }

    public static string Truncate(string s, int n) => s.Length <= n ? s : s[..(n - 1)] + "…";
}
