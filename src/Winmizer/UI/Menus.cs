using System.Diagnostics;
using Spectre.Console;
using Winmizer.Core;
using Winmizer.Tweaks;
using Winmizer.Tweaks.Debloat;

namespace Winmizer.UI;

public static class Menus
{
    private sealed record Item(string Label, ITweak? Tweak = null, string? Id = null, StartupItem? Startup = null);

    public static void Run()
    {
        while (true)
        {
            Ui.Banner();
            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title("[bold aqua]Menu główne[/]")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Aqua))
                .AddChoices(
                    "1) Szybka optymalizacja (zalecany bezpieczny zestaw)",
                    "2) Własny wybór (według kategorii)",
                    "3) Odśmiecanie aplikacji i autostart",
                    "4) Plany zasilania",
                    "5) Informacje o systemie / status tweaków",
                    "6) Cofnij / przywróć",
                    "7) Wyjście"));

            switch (choice[0])
            {
                case '1': Quick(); break;
                case '2': Custom(); break;
                case '3': DebloatMenu(); break;
                case '4': PowerMenu(); break;
                case '5': StatusScreen(); break;
                case '6': RevertMenu(); break;
                default: return;
            }
            Ui.Pause();
        }
    }

    // ------------------------------------------------------------------ 1) Quick
    private static void Quick()
    {
        var preset = TweakCatalog.QuickPreset().ToList();
        AnsiConsole.Write(new Panel(
            "Szybka optymalizacja stosuje wyłącznie [green]sprawdzone, bezpieczne[/] ustawienia: gaming, prywatność, usługi telemetryczne,\n" +
            "wygląd, dysk i czyszczenie plików tymczasowych. Drukarki, sieć, Windows Update, Defender, Sklep i Xbox pozostają nietknięte.\n" +
            (Hardware.IsLaptop ? "[yellow]Wykryto laptopa - ustawienia zasilania wymagające baterii zostały pominięte.[/]\n" : "") +
            "Wszystko można cofnąć w menu [bold]Cofnij / przywróć[/].")
            .Header("[bold]Szybka optymalizacja[/]").BorderColor(Color.Green).Padding(1, 0));
        var todo = AnsiConsole.Status().Start("Sprawdzanie obecnego stanu...", _ => preset.Where(t => !SafeApplied(t)).ToList());
        if (todo.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]Wszystkie zalecane tweaki są już zastosowane.[/]");
            return;
        }
        Ui.ApplyFlow(todo);
    }

    private static bool SafeApplied(ITweak t) { try { return !t.IsAction && t.IsApplied(); } catch { return false; } }

    // ------------------------------------------------------------------ 2) Custom
    private static void Custom()
    {
        var cats = AnsiConsole.Prompt(new MultiSelectionPrompt<TweakCategory>()
            .Title("Wybierz [aqua]kategorie[/] do przejrzenia:")
            .NotRequired()
            .InstructionsText("[grey](Spacja - zaznacz, Enter - zatwierdź; Odśmiecanie aplikacji ma osobne menu)[/]")
            .UseConverter(c => Labels.Category(c))
            .AddChoices(Enum.GetValues<TweakCategory>().Where(c => c != TweakCategory.Debloat)));
        if (cats.Count == 0) return;

        var tweaks = TweakCatalog.Applicable.Where(t => cats.Contains(t.Category)).ToList();
        Ui.ShowTweakTable(tweaks);

        var prompt = new MultiSelectionPrompt<Item>()
            .Title("Zaznacz tweaki do zastosowania ([green]Bezpieczne[/]/[yellow]Umiarkowane[/] zalecane są domyślnie, [red]Zaawansowane[/] - odznaczone):")
            .NotRequired().PageSize(20).Mode(SelectionMode.Leaf)
            .InstructionsText("[grey](Spacja - zaznacz, Enter - zatwierdź)[/]")
            .UseConverter(i => i.Label);
        var preselect = new List<Item>();
        foreach (var cat in cats)
        {
            var items = tweaks.Where(t => t.Category == cat).Select(t => new Item(Ui.ChoiceLabel(t), t)).ToList();
            prompt.AddChoiceGroup(new Item($"[bold underline]{Ui.Esc(Labels.Category(cat))}[/]"), items);
            preselect.AddRange(items.Where(i => i.Tweak!.Recommended && i.Tweak.Risk != RiskLevel.Advanced));
        }
        foreach (var i in preselect) prompt.Select(i);

        var chosen = AnsiConsole.Prompt(prompt).Where(i => i.Tweak != null).Select(i => i.Tweak!).ToList();
        Ui.ApplyFlow(chosen);
    }

    // ------------------------------------------------------------------ 3) Debloat + startup
    private static void DebloatMenu()
    {
        var c = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[bold]Odśmiecanie[/]")
            .AddChoices("Usuń preinstalowane aplikacje", "Aplikacje autostartu (wyłącz wybrane)", "Wróć"));
        if (c.StartsWith("Usuń")) Debloat();
        else if (c.StartsWith("Aplikacje")) StartupMenu();
    }

    private static void Debloat()
    {
        AnsiConsole.MarkupLine("[grey]Zawsze zachowane: Sklep Microsoft, App Installer/winget, Kalkulator, Zdjęcia, Notatnik, Terminal, Paint, Narzędzie do wycinania, Aparat, tożsamość Xbox i Gaming Services.[/]");
        var installed = AnsiConsole.Status().Start("Wyszukiwanie zainstalowanych aplikacji...",
            _ => TweakCatalog.ByCategory(TweakCategory.Debloat).Where(t => t.IsApplicable && !t.IsApplied()).ToList());
        if (installed.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]Brak aplikacji z listy do usunięcia - system jest już czysty.[/]");
            return;
        }
        var prompt = new MultiSelectionPrompt<Item>()
            .Title("Zaznacz aplikacje do [red]usunięcia[/] (domyślnie tylko bezpieczne, promocyjne):")
            .NotRequired().PageSize(22)
            .InstructionsText("[grey](Spacja - zaznacz, Enter - zatwierdź)[/]")
            .UseConverter(i => i.Label);
        foreach (var t in installed)
        {
            var item = new Item($"{Ui.ChoiceLabel(t)} [grey]- {Ui.Esc(Ui.Truncate(t.Description, 60))}[/]", t);
            prompt.AddChoice(item);
            if (t is AppxTweak { DefaultSelected: true }) prompt.Select(item);
        }
        Ui.ApplyFlow(AnsiConsole.Prompt(prompt).Select(i => i.Tweak!).ToList());
    }

    private static void StartupMenu()
    {
        var items = StartupManager.List();
        if (items.Count == 0) { AnsiConsole.MarkupLine("[yellow]Nie znaleziono wpisów autostartu.[/]"); return; }

        var table = new Table().Border(TableBorder.Rounded).AddColumn("Nazwa").AddColumn("Źródło").AddColumn("Polecenie");
        foreach (var i in items) table.AddRow(Ui.Esc(i.Name), Ui.Esc(i.Source), Ui.Esc(Ui.Truncate(i.Command, 70)));
        AnsiConsole.Write(table);

        var prompt = new MultiSelectionPrompt<Item>()
            .Title("Zaznacz wpisy do [yellow]wyłączenia[/] (można je później przywrócić w menu Cofnij):")
            .NotRequired().PageSize(18).UseConverter(i => i.Label);
        foreach (var i in items) prompt.AddChoice(new Item($"{Ui.Esc(i.Name)} [grey]({Ui.Esc(i.Source)})[/]", Startup: i));
        var chosen = AnsiConsole.Prompt(prompt);
        if (chosen.Count == 0) return;
        if (!Ctx.DryRun && !AnsiConsole.Confirm($"Wyłączyć {chosen.Count} wpisów autostartu?", true)) return;

        foreach (var c in chosen)
        {
            try
            {
                StartupManager.Disable(c.Startup!);
                AnsiConsole.MarkupLine($"[green]Wyłączono:[/] {Ui.Esc(c.Startup!.Name)}");
            }
            catch (Exception ex)
            {
                Ctx.Log.Error($"Autostart {c.Startup!.Name}: {ex}");
                AnsiConsole.MarkupLine($"[red]Błąd:[/] {Ui.Esc(c.Startup!.Name)} - {Ui.Esc(ex.Message)}");
            }
        }
        Ctx.Backup.TrySave();
        foreach (var (_, m) in Ctx.Log.Drain()) AnsiConsole.MarkupLine($"[grey]  {Ui.Esc(m)}[/]");
    }

    // ------------------------------------------------------------------ 4) Power
    private static void PowerMenu()
    {
        var (guid, name) = PowerCfg.Active();
        AnsiConsole.MarkupLine($"Aktywny plan zasilania: [bold aqua]{Ui.Esc(name)}[/]  {(Hardware.IsLaptop ? "[yellow](wykryto laptopa)[/]" : "[grey](komputer stacjonarny)[/]")}");
        if (Hardware.IsLaptop)
            AnsiConsole.MarkupLine("[yellow]Uwaga: plany Ultimate/Wysoka wydajność znacząco skracają czas pracy na baterii.[/]");

        var c = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[bold]Plany zasilania[/]")
            .AddChoices("Winmizer Ultimate (Ultimate Performance)", "Wysoka wydajność", "Zrównoważony (domyślny Windows)",
                        "Dodatkowe ustawienia (USB, PCIe, min. stan CPU, hibernacja, Fast Startup)", "Pokaż dostępne plany", "Wróć"));
        if (c.StartsWith("Winmizer")) Ui.ApplyFlow(new List<ITweak> { TweakCatalog.All.First(t => t.Id == "power.ultimate") });
        else if (c.StartsWith("Wysoka")) Ui.ApplyFlow(new List<ITweak> { TweakCatalog.All.First(t => t.Id == "power.highperf") });
        else if (c.StartsWith("Zrówn"))
        {
            try
            {
                PowerCfg.SetActive(PowerCfg.Balanced);
                AnsiConsole.MarkupLine("[green]Przełączono na plan Zrównoważony.[/]");
            }
            catch (Exception ex) { AnsiConsole.MarkupLine($"[red]Błąd:[/] {Ui.Esc(ex.Message)}"); }
            foreach (var (_, m) in Ctx.Log.Drain()) AnsiConsole.MarkupLine($"[grey]  {Ui.Esc(m)}[/]");
        }
        else if (c.StartsWith("Dodatkowe"))
        {
            var extra = TweakCatalog.ByCategory(TweakCategory.Power)
                .Where(t => t.Id is not ("power.ultimate" or "power.highperf")).ToList();
            Ui.ShowTweakTable(extra);
            var prompt = new MultiSelectionPrompt<Item>().Title("Zaznacz ustawienia:").NotRequired().UseConverter(i => i.Label);
            foreach (var t in extra)
            {
                var item = new Item(Ui.ChoiceLabel(t), t);
                prompt.AddChoice(item);
                if (t.Recommended && t.Risk != RiskLevel.Advanced) prompt.Select(item);
            }
            Ui.ApplyFlow(AnsiConsole.Prompt(prompt).Select(i => i.Tweak!).ToList());
        }
        else if (c.StartsWith("Pokaż"))
        {
            var table = new Table().Border(TableBorder.Rounded).AddColumn("Plan").AddColumn("GUID").AddColumn("Aktywny");
            foreach (var p in PowerCfg.List())
                table.AddRow(Ui.Esc(p.Name), p.Guid, p.Guid == guid ? "[green]TAK[/]" : "");
            AnsiConsole.Write(table);
        }
    }

    // ------------------------------------------------------------------ 5) Status
    public static void StatusScreen()
    {
        AnsiConsole.Status().Spinner(Spinner.Known.Dots).Start("Zbieranie informacji o systemie...", _ =>
        {
            var info = new Table().Border(TableBorder.Rounded).BorderColor(Color.Aqua).HideHeaders();
            info.AddColumn("k").AddColumn("v");
            info.AddRow("[bold]System[/]", Ui.Esc(OsInfo.Description) + (OsInfo.IsSupported ? "" : " [yellow](nieobsługiwana wersja)[/]"));
            info.AddRow("[bold]Zgodność tweaków[/]", $"{TweakCatalog.Applicable.Count()} z {TweakCatalog.All.Count} dostępnych w tym systemie");
            info.AddRow("[bold]Procesor[/]", Ui.Esc(Hardware.Cpu));
            info.AddRow("[bold]Karta graficzna[/]", Ui.Esc(Hardware.Gpus));
            info.AddRow("[bold]Pamięć RAM[/]", Ui.Esc(Hardware.Ram));
            info.AddRow("[bold]Typ urządzenia[/]", Hardware.IsLaptop ? "Laptop (wykryto baterię)" : "Komputer stacjonarny");
            info.AddRow("[bold]Plan zasilania[/]", Ui.Esc(PowerCfg.Active().Name));
            info.AddRow("[bold]Punkt przywracania Winmizer[/]", Ui.Esc(Ctx.Backup.RestorePointInfo ?? "brak"));
            info.AddRow("[bold]Kopia zapasowa[/]", Ui.Esc(Paths.BackupFile) + $" ({Ctx.Backup.Ids.Count} wpisów)");
            AnsiConsole.Write(info);

            var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            table.AddColumn("Kategoria").AddColumn("Tweak").AddColumn("Ryzyko").AddColumn("Zastosowany?");
            int applied = 0, total = 0;
            foreach (var t in TweakCatalog.All)
            {
                if (!t.IsAction && t.IsApplicable) { total++; if (SafeApplied(t)) applied++; }
                table.AddRow(Ui.Esc(Labels.Category(t.Category)), Ui.Esc(t.Name), Ui.RiskMarkup(t.Risk), Ui.StatusMarkup(t));
            }
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[bold]Zastosowane tweaki:[/] [green]{applied}[/] / {total}");
        });
    }

    // ------------------------------------------------------------------ 6) Revert
    private static void RevertMenu()
    {
        var c = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[bold]Cofnij / przywróć[/]")
            .AddChoices("Cofnij wszystko (z backup.json)", "Cofnij wybrane tweaki", "Otwórz Przywracanie systemu (punkt przywracania)", "Wróć"));
        var engine = new TweakEngine();

        if (c.StartsWith("Cofnij wszystko"))
        {
            if (Ctx.Backup.Ids.Count == 0) { AnsiConsole.MarkupLine("[yellow]Brak zapisanych zmian do cofnięcia.[/]"); return; }
            if (!Ctx.DryRun && !AnsiConsole.Confirm($"Przywrócić oryginalne wartości dla {Ctx.Backup.Ids.Count} wpisów?", false)) return;
            RunRevert(() => engine.RevertAll());
        }
        else if (c.StartsWith("Cofnij wybrane"))
        {
            var ids = Ctx.Backup.Ids.ToList();
            if (ids.Count == 0) { AnsiConsole.MarkupLine("[yellow]Brak zapisanych zmian do cofnięcia.[/]"); return; }
            var byId = TweakCatalog.All.ToDictionary(t => t.Id);
            var prompt = new MultiSelectionPrompt<Item>().Title("Zaznacz zmiany do cofnięcia:").NotRequired().PageSize(20).UseConverter(i => i.Label);
            foreach (var id in ids)
            {
                string label = byId.TryGetValue(id, out var t) ? Ui.Esc(t.Name) : Ui.Esc("Autostart: " + id[(id.IndexOf('|') + 1)..]);
                prompt.AddChoice(new Item(label, Id: id));
            }
            var chosen = AnsiConsole.Prompt(prompt).Select(i => i.Id!).ToList();
            if (chosen.Count > 0) RunRevert(() => engine.RevertAll(chosen));
        }
        else if (c.StartsWith("Otwórz"))
        {
            if (Ctx.DryRun) { AnsiConsole.MarkupLine("[yellow](dry-run) Otworzyłbym rstrui.exe[/]"); return; }
            try { Process.Start(new ProcessStartInfo("rstrui.exe") { UseShellExecute = true }); }
            catch (Exception ex) { AnsiConsole.MarkupLine($"[red]Nie można uruchomić rstrui:[/] {Ui.Esc(ex.Message)}"); }
        }
    }

    private static void RunRevert(Func<IReadOnlyList<TweakResult>> action)
    {
        var results = AnsiConsole.Status().Spinner(Spinner.Known.Dots).Start("Przywracanie wartości...", _ => action());
        Ui.Summary(results, "Cofnięto");
        foreach (var (level, m) in Ctx.Log.Drain().Where(l => l.Level is "WARN" or "ERROR" or "DRY"))
            AnsiConsole.MarkupLine($"[grey]  [[{level}]] {Ui.Esc(m)}[/]");
        if (!Ctx.DryRun && results.Any(r => r.Status == ResultStatus.Reverted)) Ui.RebootPrompt();
    }
}

