using Spectre.Console;
using Winmizer.Core;
using Winmizer.Tweaks;
using Winmizer.Tweaks.Cleanup;

namespace Winmizer.UI;

/// <summary>Shared Spectre.Console rendering helpers (all user-facing text is Polish).</summary>
public static class Ui
{
    public static void Banner()
    {
        if (AnsiConsole.Profile.Capabilities.Interactive) AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Winmizer").Color(Color.Aqua));
        AnsiConsole.MarkupLine("[grey]Optymalizator Windows 10/11 dla graczy - bezpieczny, odwracalny, z punktem przywracania[/]");
        AnsiConsole.MarkupLine($"[grey]System:[/] [aqua]{Esc(OsInfo.Description)}[/]");
        if (OsInfo.UnsupportedWarning is { } w)
            AnsiConsole.MarkupLine($"[black on yellow] UWAGA [/] [yellow]{Esc(w)}[/]");
        if (Ctx.DryRun)
            AnsiConsole.MarkupLine("[black on yellow] TRYB DRY-RUN [/] [yellow]Nic nie zostanie zmienione - tylko podgląd działań.[/]");
        AnsiConsole.WriteLine();
    }

    public static string Esc(string s) => Markup.Escape(s);

    public static string RiskMarkup(RiskLevel r) => $"[{Labels.RiskColor(r)}]{Labels.Risk(r)}[/]";

    public static string StatusMarkup(ITweak t)
    {
        try
        {
            if (!t.IsApplicable) return "[grey]N/D w tym systemie[/]";
            if (t.IsAction) return "[grey]akcja jednorazowa[/]";
            return t.IsApplied() ? "[green]TAK - zastosowane[/]" : "[grey]nie[/]";
        }
        catch { return "[red]błąd detekcji[/]"; }
    }

    /// <summary>Table with risk level and description of every tweak (shown before applying).</summary>
    public static void ShowTweakTable(IEnumerable<ITweak> tweaks)
    {
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
        table.AddColumn("Kategoria");
        table.AddColumn("Tweak");
        table.AddColumn("Ryzyko");
        table.AddColumn("Opis");
        foreach (var t in tweaks)
        {
            string desc = Esc(t.Description);
            if (t.Warning != null) desc += $"\n[red]Uwaga:[/] {Esc(t.Warning)}";
            if (t.OsNote != null) desc += $"\n[grey]{Esc(OsInfo.Name)}:[/] {Esc(t.OsNote)}";
            if (t.SupportedOn != OsSupport.Both) desc += $"\n[grey]Dotyczy: tylko {(t.SupportedOn == OsSupport.Win11 ? "Windows 11" : "Windows 10")}[/]";
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
        AnsiConsole.MarkupLine("\n[grey]Naciśnij dowolny klawisz, aby wrócić do menu...[/]");
        Console.ReadKey(true);
    }

    public static void Summary(IReadOnlyList<TweakResult> results, string verb = "Zastosowano")
    {
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
        table.AddColumn("Kategoria");
        table.AddColumn("Tweak");
        table.AddColumn("Wynik");
        table.AddColumn("Szczegóły");
        foreach (var r in results)
        {
            string st = r.Status switch
            {
                ResultStatus.Applied => "[green]zastosowano[/]",
                ResultStatus.Reverted => "[green]cofnięto[/]",
                ResultStatus.DryRun => "[yellow]dry-run[/]",
                ResultStatus.Skipped => "[grey]pominięto[/]",
                _ => "[red]BŁĄD[/]"
            };
            table.AddRow(Esc(Labels.Category(r.Tweak.Category)), Esc(r.Tweak.Name), st, Esc(r.Message));
        }
        AnsiConsole.Write(table);

        int ok = results.Count(r => r.Status is ResultStatus.Applied or ResultStatus.Reverted or ResultStatus.DryRun);
        int skipped = results.Count(r => r.Status == ResultStatus.Skipped);
        int failed = results.Count(r => r.Status == ResultStatus.Failed);
        AnsiConsole.MarkupLine($"\n[bold]Podsumowanie:[/] [green]{verb}: {ok}[/]  [grey]Pominięto: {skipped}[/]  [red]Błędy: {failed}[/]");

        long freed = results.Select(r => r.Tweak).OfType<ActionTweak>().Sum(a => a.FreedBytes);
        if (freed > 0) AnsiConsole.MarkupLine($"[bold]Zwolnione miejsce:[/] [aqua]{CleanupTweaks.FormatSize(freed)}[/]");
        if (Ctx.Log.FilePath != null) AnsiConsole.MarkupLine($"[grey]Dziennik: {Esc(Ctx.Log.FilePath)}[/]");
    }

    /// <summary>The whole apply pipeline: preview, confirmation, restore point, execution, summary, reboot prompt.</summary>
    public static void ApplyFlow(List<ITweak> tweaks, bool interactive = true)
    {
        if (tweaks.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Nic nie wybrano.[/]");
            return;
        }

        ShowTweakTable(tweaks);
        if (tweaks.Any(t => t.Risk == RiskLevel.Advanced))
            AnsiConsole.MarkupLine("[red]Wybrano tweaki zaawansowane - przeczytaj uważnie ich opisy powyżej.[/]");

        if (interactive && !Ctx.DryRun &&
            !AnsiConsole.Confirm($"Zastosować [bold]{tweaks.Count}[/] zmian(y)? Najpierw zostanie utworzony punkt przywracania systemu.", false))
        {
            AnsiConsole.MarkupLine("[grey]Anulowano.[/]");
            return;
        }

        // Safety first: restore point before anything is changed.
        (bool Ok, string Message) rp = (true, "");
        AnsiConsole.Status().Spinner(Spinner.Known.Dots).Start("Tworzenie punktu przywracania systemu...", _ => rp = RestorePoint.Create());
        if (rp.Ok) AnsiConsole.MarkupLine(Ctx.DryRun ? "[yellow]Punkt przywracania: (dry-run) pominięto.[/]" : "[green]Punkt przywracania utworzony.[/]");
        else
        {
            AnsiConsole.MarkupLine($"[red]Nie udało się utworzyć punktu przywracania:[/] {Esc(rp.Message)}");
            if (!interactive || !AnsiConsole.Confirm("Kontynuować mimo to? (Backup wartości w backup.json i tak powstanie)", false))
            {
                AnsiConsole.MarkupLine("[grey]Przerwano - nic nie zmieniono.[/]");
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
                var task = ctx.AddTask("Start", maxValue: tweaks.Count);
                engine.Apply(tweaks,
                    onStart: t => task.Description = Esc(Truncate(t.Name, 60)),
                    onDone: r => { results.Add(r); logLines[r.Tweak.Id] = Ctx.Log.Drain(); task.Increment(1); });
                task.Description = "Gotowe";
            });

        Summary(results, Ctx.DryRun ? "Do zastosowania (dry-run)" : "Zastosowano");

        var interesting = logLines.Values.SelectMany(x => x).Where(l => l.Level is "WARN" or "ERROR" or "DRY").ToList();
        if (interesting.Count > 0)
        {
            AnsiConsole.MarkupLine("\n[bold]Szczegóły działań:[/]");
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
        AnsiConsole.MarkupLine("\n[yellow]Zalecany jest restart komputera, aby wszystkie zmiany zadziałały.[/]");
        if (AnsiConsole.Confirm("Uruchomić ponownie teraz?", false))
        {
            Proc.Run("shutdown.exe", new[] { "/r", "/t", "15", "/c", "Winmizer: restart po optymalizacji" });
            AnsiConsole.MarkupLine("[grey]Restart za 15 sekund. Aby anulować: shutdown /a[/]");
        }
        else AnsiConsole.MarkupLine("[grey]Pamiętaj o ponownym uruchomieniu później.[/]");
    }

    public static string Truncate(string s, int n) => s.Length <= n ? s : s[..(n - 1)] + "…";
}

