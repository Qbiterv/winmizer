using System.Text;
using Spectre.Console;
using Winmizer.Core;
using Winmizer.Tweaks;
using Winmizer.UI;

namespace Winmizer;

public static class Program
{
    private const string Help = """
        Winmizer - optymalizator Windows 11 dla graczy

        Użycie:
          Winmizer.exe                     interaktywne menu (wymaga administratora)
          Winmizer.exe --list              wypisz wszystkie tweaki i ich wykryty status (bez zmian w systemie)
          Winmizer.exe --dry-run           menu w trybie podglądu - nic nie jest zmieniane
          Winmizer.exe --dry-run --quick   podgląd zestawu Szybka optymalizacja (bez interakcji)
          Winmizer.exe --dry-run --apply id1,id2|all   podgląd wybranych tweaków
          Winmizer.exe --quick --yes       zastosuj zestaw Szybka optymalizacja bez pytań
          Winmizer.exe --apply id1,id2 --yes           zastosuj wybrane tweaki bez pytań
          Winmizer.exe --revert-all --yes  cofnij wszystko z backup.json
          Winmizer.exe --help
        """;

    public static int Main(string[] args)
    {
        try { Console.OutputEncoding = Encoding.UTF8; } catch { }
        if (Console.IsOutputRedirected) AnsiConsole.Profile.Width = 220;

        bool Has(string f) => args.Contains(f, StringComparer.OrdinalIgnoreCase);
        string? Value(string f)
        {
            int i = Array.FindIndex(args, a => a.Equals(f, StringComparison.OrdinalIgnoreCase));
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }

        Ctx.DryRun = Has("--dry-run");

        try
        {
            if (Has("--help") || Has("-h") || Has("/?")) { Console.WriteLine(Help); return 0; }
            if (Has("--list")) return Cli.List();
            if (Has("--list-md")) return Cli.ListMarkdown();

            bool quick = Has("--quick"), revertAll = Has("--revert-all");
            string? apply = Value("--apply");
            bool batch = quick || apply != null || revertAll;

            if (batch)
            {
                if (!Ctx.DryRun && !Has("--yes"))
                {
                    AnsiConsole.MarkupLine("[red]Tryb nieinteraktywny wymaga --dry-run (podgląd) albo --yes (potwierdzenie zmian).[/]");
                    return 2;
                }
                if (!Ctx.DryRun && !RequireAdmin()) return 1;
                return Cli.Batch(quick, apply, revertAll);
            }

            // Interactive mode: elevation is required for real changes.
            if (!Ctx.DryRun && !RequireAdmin()) return 1;
            Menus.Run();
            AnsiConsole.MarkupLine("[aqua]Do zobaczenia! Pamiętaj o restarcie po zmianach.[/]");
            return 0;
        }
        catch (Exception ex)
        {
            Ctx.Log.Error("Nieobsłużony wyjątek: " + ex);
            AnsiConsole.MarkupLine($"[red]Nieoczekiwany błąd:[/] {Markup.Escape(ex.Message)}");
            return 99;
        }
    }

    private static bool RequireAdmin()
    {
        if (Admin.IsElevated()) return true;
        AnsiConsole.Write(new Panel(
            "Winmizer musi działać z [bold]uprawnieniami administratora[/], aby zmieniać ustawienia systemu.\n\n" +
            "Zamknij to okno i uruchom program ponownie: [bold]prawy przycisk myszy -> Uruchom jako administrator[/].\n" +
            "Chcesz tylko zobaczyć, co zostałoby zmienione? Uruchom z parametrem [bold]--dry-run[/] albo [bold]--list[/].")
            .Header("[yellow]Brak uprawnień administratora[/]").BorderColor(Color.Yellow).Padding(1, 1));
        return false;
    }
}

internal static class Cli
{
    public static int List()
    {
        Ui.Banner();
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
        table.AddColumn("ID").AddColumn("Kategoria").AddColumn("Tweak").AddColumn("Ryzyko").AddColumn("Quick").AddColumn("Status");
        int applied = 0;
        foreach (var t in TweakCatalog.All)
        {
            string status = Ui.StatusMarkup(t);
            if (!t.IsAction && SafeApplied(t)) applied++;
            table.AddRow(t.Id, Markup.Escape(Labels.Category(t.Category)), Markup.Escape(t.Name), Ui.RiskMarkup(t.Risk),
                t.Recommended && t.Risk != RiskLevel.Advanced ? "[green]tak[/]" : "", status);
        }
        AnsiConsole.Write(table);
        int total = TweakCatalog.All.Count(t => !t.IsAction);
        AnsiConsole.MarkupLine($"\nŁącznie tweaków: [bold]{TweakCatalog.All.Count}[/] (ustawienia: {total}, akcje: {TweakCatalog.All.Count - total}); zastosowane: [green]{applied}[/]");
        AnsiConsole.MarkupLine($"Laptop: {(Hardware.IsLaptop ? "tak" : "nie")}; plan zasilania: {Markup.Escape(PowerCfg.Active().Name)}");
        return 0;
    }

    private static bool SafeApplied(ITweak t) { try { return t.IsApplied(); } catch { return false; } }

    /// <summary>Prints the tweak reference as Markdown (used to generate the README tables).</summary>
    public static int ListMarkdown()
    {
        var sb = new StringBuilder();
        foreach (var cat in Enum.GetValues<TweakCategory>())
        {
            var list = TweakCatalog.ByCategory(cat).ToList();
            sb.AppendLine($"### {Labels.Category(cat)} ({list.Count})\n");
            sb.AppendLine("| ID | Tweak | Ryzyko | Quick | Opis |");
            sb.AppendLine("|----|-------|--------|:-----:|------|");
            foreach (var t in list)
            {
                string d = t.Description.Replace("|", "/") + (t.Warning != null ? " **Uwaga:** " + t.Warning.Replace("|", "/") : "");
                sb.AppendLine($"| `{t.Id}` | {t.Name.Replace("|", "/")} | {Labels.Risk(t.Risk)} | {(t.Recommended && t.Risk != RiskLevel.Advanced ? "tak" : "")} | {d} |");
            }
            sb.AppendLine();
        }
        Console.Out.Write(sb.ToString());
        return 0;
    }

    public static int Batch(bool quick, string? apply, bool revertAll)
    {
        Ui.Banner();
        if (revertAll)
        {
            var res = new TweakEngine().RevertAll();
            if (res.Count == 0) AnsiConsole.MarkupLine("[yellow]Brak zapisanych zmian do cofnięcia.[/]");
            else Ui.Summary(res, "Cofnięto");
            return res.Any(r => r.Status == ResultStatus.Failed) ? 3 : 0;
        }

        List<ITweak> tweaks;
        if (quick) tweaks = TweakCatalog.QuickPreset().ToList();
        else if (string.Equals(apply, "all", StringComparison.OrdinalIgnoreCase)) tweaks = TweakCatalog.All.ToList();
        else
        {
            var ids = apply!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            tweaks = TweakCatalog.All.Where(t => ids.Contains(t.Id, StringComparer.OrdinalIgnoreCase)).ToList();
            var unknown = ids.Where(i => !TweakCatalog.All.Any(t => t.Id.Equals(i, StringComparison.OrdinalIgnoreCase))).ToList();
            if (unknown.Count > 0)
            {
                AnsiConsole.MarkupLine($"[red]Nieznane ID:[/] {Markup.Escape(string.Join(", ", unknown))} (zobacz --list)");
                return 2;
            }
        }
        Ui.ApplyFlow(tweaks, interactive: false);
        return 0;
    }
}

