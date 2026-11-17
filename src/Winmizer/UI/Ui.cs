using Spectre.Console;
using Winmizer.Core;
using Winmizer.Core.Localization;
using Winmizer.Tweaks;
using Winmizer.Tweaks.Cleanup;

namespace Winmizer.UI;

/// <summary>Shared Spectre.Console rendering helpers (all user-facing text goes through <see cref="L"/>).</summary>
public static class Ui
{
    public static void Banner()
    {
        if (AnsiConsole.Profile.Capabilities.Interactive) AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Winmizer").Color(Color.Aqua));
        AnsiConsole.MarkupLine("[grey]" + L.T("Windows 10/11 gaming optimizer - safe, reversible, with a restore point") + "[/]");
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
            return t.IsApplied() ? "[green]" + L.T("YES - applied") + "[/]" : "[grey]" + L.T("no") + "[/]";
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
            table.AddRow(Esc(Labels.Category(r.Tweak.Category)), Esc(r.Tweak.Name), st, Esc(r.Message));
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

        if (interactive && !Ctx.DryRun &&
            !AnsiConsole.Confirm(L.F("Apply [bold]{0}[/] change(s)? A system restore point will be created first.", tweaks.Count), false))
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

        if (interactive && !Ctx.DryRun && results.Any(r => r.Status == ResultStatus.Applied))
            RebootPrompt();
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
