using System.Diagnostics;
using Spectre.Console;
using Winmizer.Core;
using Winmizer.Core.Localization;
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
            // Choices are keys; labels are translated by the converter, so the logic never depends on the language.
            var choice = AnsiConsole.Prompt(new SelectionPrompt<char>()
                .Title("[bold aqua]" + L.T("Main menu") + "[/]")
                .PageSize(10)
                .HighlightStyle(new Style(Color.Aqua))
                .UseConverter(k => k switch
                {
                    '1' => L.T("1) Quick optimize (recommended safe set)"),
                    '2' => L.T("2) Custom selection (by category)"),
                    '3' => L.T("3) App debloat and startup apps"),
                    '4' => L.T("4) Power plans"),
                    '5' => L.T("5) System info / tweak status"),
                    '6' => L.T("6) Undo / restore"),
                    '7' => "7) Language / Język",
                    _ => L.T("8) Exit")
                })
                .AddChoices('1', '2', '3', '4', '5', '6', '7', '8'));

            switch (choice)
            {
                case '1': Quick(); break;
                case '2': Custom(); break;
                case '3': DebloatMenu(); break;
                case '4': PowerMenu(); break;
                case '5': StatusScreen(); break;
                case '6': RevertMenu(); break;
                case '7': LanguageMenu(); continue; // the banner redraws in the new language
                default: return;
            }
            Ui.Pause();
        }
    }

    // ------------------------------------------------------------------ 7) Language
    private static void LanguageMenu()
    {
        var lang = AnsiConsole.Prompt(new SelectionPrompt<Lang>()
            .Title("[bold]Language / Język[/]")
            .UseConverter(l => l == Lang.En ? "English" : "Polski")
            .AddChoices(Lang.En, Lang.Pl));
        L.Current = lang;
        Settings.SaveLanguage(lang);
    }

    // ------------------------------------------------------------------ 1) Quick
    private static void Quick()
    {
        var preset = TweakCatalog.QuickPreset().ToList();
        AnsiConsole.Write(new Panel(
            L.T("Quick optimize applies only [green]proven, safe[/] settings: gaming, privacy, telemetry services,") + "\n" +
            L.T("appearance, disk and temp-file cleanup. Printers, network, Windows Update, Defender, the Store and Xbox are left untouched.") + "\n" +
            (Hardware.IsLaptop ? "[yellow]" + L.T("Laptop detected - power settings that need a battery were skipped.") + "[/]\n" : "") +
            L.T("Everything can be undone in the [bold]Undo / restore[/] menu."))
            .Header("[bold]" + L.T("Quick optimize") + "[/]").BorderColor(Color.Green).Padding(1, 0));
        var todo = AnsiConsole.Status().Start(L.T("Checking current state..."), _ => preset.Where(t => !SafeApplied(t)).ToList());
        if (todo.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]" + L.T("All recommended tweaks are already applied.") + "[/]");
            return;
        }
        Ui.ApplyFlow(todo);
    }

    private static bool SafeApplied(ITweak t) { try { return !t.IsAction && t.IsApplied(); } catch { return false; } }

    // ------------------------------------------------------------------ 2) Custom
    private static void Custom()
    {
        var cats = AnsiConsole.Prompt(new MultiSelectionPrompt<TweakCategory>()
            .Title(L.T("Choose [aqua]categories[/] to review:"))
            .NotRequired()
            .InstructionsText("[grey]" + L.T("(Space - select, Enter - confirm; app debloat has its own menu)") + "[/]")
            .UseConverter(c => Labels.Category(c))
            .AddChoices(Enum.GetValues<TweakCategory>().Where(c => c != TweakCategory.Debloat)));
        if (cats.Count == 0) return;

        var tweaks = TweakCatalog.Applicable.Where(t => cats.Contains(t.Category)).ToList();
        Ui.ShowTweakTable(tweaks);

        var prompt = new MultiSelectionPrompt<Item>()
            .Title(L.T("Select tweaks to apply ([green]Safe[/]/[yellow]Moderate[/] are preselected, [red]Advanced[/] are not):"))
            .NotRequired().PageSize(20).Mode(SelectionMode.Leaf)
            .InstructionsText("[grey]" + L.T("(Space - select, Enter - confirm)") + "[/]")
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
        var c = AnsiConsole.Prompt(new SelectionPrompt<char>().Title("[bold]" + L.T("App debloat") + "[/]")
            .UseConverter(k => k switch
            {
                'r' => L.T("Remove preinstalled apps"),
                's' => L.T("Startup apps (disable selected)"),
                _ => L.T("Back")
            })
            .AddChoices('r', 's', 'b'));
        if (c == 'r') Debloat();
        else if (c == 's') StartupMenu();
    }

    private static void Debloat()
    {
        AnsiConsole.MarkupLine("[grey]" + L.T("Always kept: Microsoft Store, App Installer/winget, Calculator, Photos, Notepad, Terminal, Paint, Snipping Tool, Camera, Xbox identity and Gaming Services.") + "[/]");
        var installed = AnsiConsole.Status().Start(L.T("Looking for installed apps..."),
            _ => TweakCatalog.ByCategory(TweakCategory.Debloat).Where(t => t.IsApplicable && !t.IsApplied()).ToList());
        if (installed.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]" + L.T("No apps from the list to remove - the system is already clean.") + "[/]");
            return;
        }
        var prompt = new MultiSelectionPrompt<Item>()
            .Title(L.T("Select apps to [red]remove[/] (by default only the safe, promotional ones):"))
            .NotRequired().PageSize(22)
            .InstructionsText("[grey]" + L.T("(Space - select, Enter - confirm)") + "[/]")
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
        if (items.Count == 0) { AnsiConsole.MarkupLine("[yellow]" + L.T("No startup entries found.") + "[/]"); return; }

        var table = new Table().Border(TableBorder.Rounded).AddColumn(L.T("Name")).AddColumn(L.T("Source")).AddColumn(L.T("Command"));
        foreach (var i in items) table.AddRow(Ui.Esc(i.Name), Ui.Esc(i.Source), Ui.Esc(Ui.Truncate(i.Command, 70)));
        AnsiConsole.Write(table);

        var prompt = new MultiSelectionPrompt<Item>()
            .Title(L.T("Select entries to [yellow]disable[/] (you can restore them later in the Undo menu):"))
            .NotRequired().PageSize(18).UseConverter(i => i.Label);
        foreach (var i in items) prompt.AddChoice(new Item($"{Ui.Esc(i.Name)} [grey]({Ui.Esc(i.Source)})[/]", Startup: i));
        var chosen = AnsiConsole.Prompt(prompt);
        if (chosen.Count == 0) return;
        if (!Ctx.DryRun && !AnsiConsole.Confirm(L.F("Disable {0} startup entries?", chosen.Count), true)) return;

        foreach (var c in chosen)
        {
            try
            {
                StartupManager.Disable(c.Startup!);
                AnsiConsole.MarkupLine($"[green]{L.T("Disabled:")}[/] {Ui.Esc(c.Startup!.Name)}");
            }
            catch (Exception ex)
            {
                Ctx.Log.Error(L.F("Startup entry {0}: {1}", c.Startup!.Name, ex));
                AnsiConsole.MarkupLine($"[red]{L.T("Error:")}[/] {Ui.Esc(c.Startup!.Name)} - {Ui.Esc(ex.Message)}");
            }
        }
        Ctx.Backup.TrySave();
        foreach (var (_, m) in Ctx.Log.Drain()) AnsiConsole.MarkupLine($"[grey]  {Ui.Esc(m)}[/]");
    }

    // ------------------------------------------------------------------ 4) Power
    private static void PowerMenu()
    {
        var (guid, name) = PowerCfg.Active();
        AnsiConsole.MarkupLine($"{L.T("Active power plan:")} [bold aqua]{Ui.Esc(name)}[/]  {(Hardware.IsLaptop ? "[yellow](" + L.T("laptop detected") + ")[/]" : "[grey](" + L.T("desktop PC") + ")[/]")}");
        if (Hardware.IsLaptop)
            AnsiConsole.MarkupLine("[yellow]" + L.T("Note: Ultimate/High performance plans significantly shorten battery life.") + "[/]");

        var c = AnsiConsole.Prompt(new SelectionPrompt<char>().Title("[bold]" + L.T("Power plans") + "[/]")
            .UseConverter(k => k switch
            {
                'u' => L.T("Winmizer Ultimate (Ultimate Performance)"),
                'h' => L.T("High performance"),
                'b' => L.T("Balanced (Windows default)"),
                'e' => L.T("Extra settings (USB, PCIe, min. CPU state, hibernation, Fast Startup)"),
                'l' => L.T("Show available plans"),
                _ => L.T("Back")
            })
            .AddChoices('u', 'h', 'b', 'e', 'l', 'x'));
        if (c == 'u') Ui.ApplyFlow(new List<ITweak> { TweakCatalog.All.First(t => t.Id == "power.ultimate") });
        else if (c == 'h') Ui.ApplyFlow(new List<ITweak> { TweakCatalog.All.First(t => t.Id == "power.highperf") });
        else if (c == 'b')
        {
            try
            {
                PowerCfg.SetActive(PowerCfg.Balanced);
                AnsiConsole.MarkupLine("[green]" + L.T("Switched to the Balanced plan.") + "[/]");
            }
            catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{L.T("Error:")}[/] {Ui.Esc(ex.Message)}"); }
            foreach (var (_, m) in Ctx.Log.Drain()) AnsiConsole.MarkupLine($"[grey]  {Ui.Esc(m)}[/]");
        }
        else if (c == 'e')
        {
            var extra = TweakCatalog.ByCategory(TweakCategory.Power)
                .Where(t => t.Id is not ("power.ultimate" or "power.highperf")).ToList();
            Ui.ShowTweakTable(extra);
            var prompt = new MultiSelectionPrompt<Item>().Title(L.T("Select settings:")).NotRequired().UseConverter(i => i.Label);
            foreach (var t in extra)
            {
                var item = new Item(Ui.ChoiceLabel(t), t);
                prompt.AddChoice(item);
                if (t.Recommended && t.Risk != RiskLevel.Advanced) prompt.Select(item);
            }
            Ui.ApplyFlow(AnsiConsole.Prompt(prompt).Select(i => i.Tweak!).ToList());
        }
        else if (c == 'l')
        {
            var table = new Table().Border(TableBorder.Rounded).AddColumn(L.T("Plan")).AddColumn("GUID").AddColumn(L.T("Active"));
            foreach (var p in PowerCfg.List())
                table.AddRow(Ui.Esc(p.Name), p.Guid, p.Guid == guid ? "[green]" + L.T("YES") + "[/]" : "");
            AnsiConsole.Write(table);
        }
    }

    // ------------------------------------------------------------------ 5) Status
    public static void StatusScreen()
    {
        AnsiConsole.Status().Spinner(Spinner.Known.Dots).Start(L.T("Collecting system information..."), _ =>
        {
            var info = new Table().Border(TableBorder.Rounded).BorderColor(Color.Aqua).HideHeaders();
            info.AddColumn("k").AddColumn("v");
            info.AddRow("[bold]" + L.T("System") + "[/]", Ui.Esc(OsInfo.Description) + (OsInfo.IsSupported ? "" : " [yellow](" + L.T("unsupported version") + ")[/]"));
            info.AddRow("[bold]" + L.T("Tweak compatibility") + "[/]", L.F("{0} of {1} available on this system", TweakCatalog.Applicable.Count(), TweakCatalog.All.Count));
            info.AddRow("[bold]" + L.T("Processor") + "[/]", Ui.Esc(Hardware.Cpu));
            info.AddRow("[bold]" + L.T("Graphics card") + "[/]", Ui.Esc(Hardware.Gpus));
            info.AddRow("[bold]" + L.T("Memory (RAM)") + "[/]", Ui.Esc(Hardware.Ram));
            info.AddRow("[bold]" + L.T("Device type") + "[/]", Hardware.IsLaptop ? L.T("Laptop (battery detected)") : L.T("Desktop PC"));
            info.AddRow("[bold]" + L.T("Power plan") + "[/]", Ui.Esc(PowerCfg.Active().Name));
            info.AddRow("[bold]" + L.T("Winmizer restore point") + "[/]", Ui.Esc(Ctx.Backup.RestorePointInfo ?? L.T("none")));
            info.AddRow("[bold]" + L.T("Backup") + "[/]", Ui.Esc(Paths.BackupFile) + L.F(" ({0} entries)", Ctx.Backup.Ids.Count));
            AnsiConsole.Write(info);

            var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            table.AddColumn(L.T("Category")).AddColumn(L.T("Tweak")).AddColumn(L.T("Risk")).AddColumn(L.T("Applied?"));
            int applied = 0, total = 0;
            foreach (var t in TweakCatalog.All)
            {
                if (!t.IsAction && t.IsApplicable) { total++; if (SafeApplied(t)) applied++; }
                table.AddRow(Ui.Esc(Labels.Category(t.Category)), Ui.Esc(t.Name), Ui.RiskMarkup(t.Risk), Ui.StatusMarkup(t));
            }
            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[bold]{L.T("Applied tweaks:")}[/] [green]{applied}[/] / {total}");
        });
    }

    // ------------------------------------------------------------------ 6) Revert
    private static void RevertMenu()
    {
        var c = AnsiConsole.Prompt(new SelectionPrompt<char>().Title("[bold]" + L.T("Undo / restore") + "[/]")
            .UseConverter(k => k switch
            {
                'a' => L.T("Undo everything (from backup.json)"),
                's' => L.T("Undo selected tweaks"),
                'o' => L.T("Open System Restore (restore point)"),
                _ => L.T("Back")
            })
            .AddChoices('a', 's', 'o', 'b'));
        var engine = new TweakEngine();

        if (c == 'a')
        {
            if (Ctx.Backup.Ids.Count == 0) { AnsiConsole.MarkupLine("[yellow]" + L.T("No saved changes to undo.") + "[/]"); return; }
            if (!Ctx.DryRun && !AnsiConsole.Confirm(L.F("Restore the original values for {0} entries?", Ctx.Backup.Ids.Count), false)) return;
            RunRevert(() => engine.RevertAll());
        }
        else if (c == 's')
        {
            var ids = Ctx.Backup.Ids.ToList();
            if (ids.Count == 0) { AnsiConsole.MarkupLine("[yellow]" + L.T("No saved changes to undo.") + "[/]"); return; }
            var byId = TweakCatalog.All.ToDictionary(t => t.Id);
            var prompt = new MultiSelectionPrompt<Item>().Title(L.T("Select changes to undo:")).NotRequired().PageSize(20).UseConverter(i => i.Label);
            foreach (var id in ids)
            {
                string label = byId.TryGetValue(id, out var t) ? Ui.Esc(t.Name) : Ui.Esc(L.F("Startup: {0}", id[(id.IndexOf('|') + 1)..]));
                prompt.AddChoice(new Item(label, Id: id));
            }
            var chosen = AnsiConsole.Prompt(prompt).Select(i => i.Id!).ToList();
            if (chosen.Count > 0) RunRevert(() => engine.RevertAll(chosen));
        }
        else if (c == 'o')
        {
            if (Ctx.DryRun) { AnsiConsole.MarkupLine("[yellow]" + L.T("(dry-run) Would open rstrui.exe") + "[/]"); return; }
            try { Process.Start(new ProcessStartInfo("rstrui.exe") { UseShellExecute = true }); }
            catch (Exception ex) { AnsiConsole.MarkupLine($"[red]{L.T("Cannot launch rstrui:")}[/] {Ui.Esc(ex.Message)}"); }
        }
    }

    private static void RunRevert(Func<IReadOnlyList<TweakResult>> action)
    {
        var results = AnsiConsole.Status().Spinner(Spinner.Known.Dots).Start(L.T("Restoring values..."), _ => action());
        Ui.Summary(results, Ui.RevertedVerb);
        foreach (var (level, m) in Ctx.Log.Drain().Where(l => l.Level is "WARN" or "ERROR" or "DRY"))
            AnsiConsole.MarkupLine($"[grey]  [[{level}]] {Ui.Esc(m)}[/]");
        if (!Ctx.DryRun && results.Any(r => r.Status == ResultStatus.Reverted)) Ui.RebootPrompt();
    }
}
