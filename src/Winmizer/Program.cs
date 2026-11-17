using System.Text;
using Spectre.Console;
using Winmizer.Core;
using Winmizer.Core.Localization;
using Winmizer.Tweaks;
using Winmizer.UI;

namespace Winmizer;

public static class Program
{
    private static string Help()
    {
        (string Cmd, string Desc)[] rows =
        {
            ("Winmizer.exe", "interactive menu (requires administrator)"),
            ("Winmizer.exe --list", "list all tweaks and their detected status (no system changes)"),
            ("Winmizer.exe --dry-run", "menu in preview mode - nothing is changed"),
            ("Winmizer.exe --dry-run --quick", "preview the Quick optimize set incl. app removal (non-interactive)"),
            ("Winmizer.exe --dry-run --apply id1,id2|all", "preview selected tweaks"),
            ("Winmizer.exe --quick --yes", "apply the Quick optimize set without prompts (also restarts Explorer if needed)"),
            ("Winmizer.exe --apply id1,id2 --yes", "apply selected tweaks without prompts"),
            ("Winmizer.exe --revert-all --yes", "undo everything from backup.json"),
            ("Winmizer.exe --lang en|pl", "UI language for this run (English is the default; the menu can save a choice)"),
            ("Winmizer.exe --help", ""),
        };
        int w = rows.Max(r => r.Cmd.Length) + 3;
        var sb = new StringBuilder();
        sb.AppendLine(L.T("Winmizer - Windows 10/11 gaming optimizer"));
        sb.AppendLine();
        sb.AppendLine(L.T("Usage:"));
        foreach (var (cmd, desc) in rows)
            sb.AppendLine("  " + (desc.Length == 0 ? cmd : cmd.PadRight(w) + L.T(desc)));
        return sb.ToString().TrimEnd();
    }

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

        // Language: --lang, then WINMIZER_LANG, then settings.json, otherwise English.
        string? langArg = Value("--lang");
        if (Has("--lang") && !L.TryParse(langArg, out _))
        {
            Console.Error.WriteLine("Invalid or missing --lang value (use en or pl) / Nieprawidłowa lub brakująca wartość --lang (użyj en lub pl).");
            return 2;
        }
        L.Init(langArg);

        try
        {
            if (Has("--help") || Has("-h") || Has("/?")) { Console.WriteLine(Help()); return 0; }
            if (Has("--list")) return Cli.List();
            if (Has("--list-md")) return Cli.ListMarkdown();
            if (Has("--lang-check")) return Cli.LangCheck();

            bool quick = Has("--quick"), revertAll = Has("--revert-all");
            string? apply = Value("--apply");
            bool batch = quick || apply != null || revertAll;

            if (batch)
            {
                if (!Ctx.DryRun && !Has("--yes"))
                {
                    AnsiConsole.MarkupLine("[red]" + L.T("Non-interactive mode requires --dry-run (preview) or --yes (confirm changes).") + "[/]");
                    return 2;
                }
                if (!Ctx.DryRun && !RequireAdmin()) return 1;
                return Cli.Batch(quick, apply, revertAll);
            }

            // Interactive mode: elevation is required for real changes.
            if (!Ctx.DryRun && !RequireAdmin()) return 1;
            Menus.Run();
            AnsiConsole.MarkupLine("[aqua]" + L.T("See you! Remember to restart after the changes.") + "[/]");
            return 0;
        }
        catch (Exception ex)
        {
            Ctx.Log.Error(L.F("Unhandled exception: {0}", ex));
            AnsiConsole.MarkupLine($"[red]{L.T("Unexpected error:")}[/] {Markup.Escape(ex.Message)}");
            return 99;
        }
    }

    private static bool RequireAdmin()
    {
        if (Admin.IsElevated()) return true;

        // Offer UAC elevation by relaunching with the same arguments in a new window.
        try
        {
            string? exe = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exe) && !Path.GetFileNameWithoutExtension(exe).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            {
                var psi = new System.Diagnostics.ProcessStartInfo(exe) { UseShellExecute = true, Verb = "runas" };
                foreach (var a in Environment.GetCommandLineArgs().Skip(1)) psi.ArgumentList.Add(a);
                System.Diagnostics.Process.Start(psi);
                AnsiConsole.MarkupLine("[grey]" + L.T("Launched a new window with administrator rights.") + "[/]");
                return false;
            }
        }
        catch { /* UAC declined - fall through to the instructions below */ }

        AnsiConsole.Write(new Panel(
            L.T("Winmizer must run with [bold]administrator rights[/] to change system settings.") + "\n\n" +
            L.T("Close this window and start the program again: [bold]right-click -> Run as administrator[/].") + "\n" +
            L.T("Just want to see what would change? Run with [bold]--dry-run[/] or [bold]--list[/]."))
            .Header("[yellow]" + L.T("Administrator rights missing") + "[/]").BorderColor(Color.Yellow).Padding(1, 1));
        return false;
    }
}

internal static class Cli
{
    public static int List()
    {
        Ui.Banner();
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
        table.AddColumn("ID").AddColumn(L.T("Category")).AddColumn(L.T("Tweak")).AddColumn(L.T("Risk")).AddColumn("Quick").AddColumn(L.T("Status"));
        int applied = 0;
        foreach (var t in TweakCatalog.All)
        {
            string status = Ui.StatusMarkup(t);
            if (t.IsApplicable && !t.IsAction && SafeApplied(t)) applied++;
            table.AddRow(t.Id, Markup.Escape(Labels.Category(t.Category)), Markup.Escape(t.Name), Ui.RiskMarkup(t.Risk),
                t.Recommended && t.Risk != RiskLevel.Advanced ? "[green]" + L.T("yes") + "[/]" : "", status);
        }
        AnsiConsole.Write(table);
        int total = TweakCatalog.All.Count(t => !t.IsAction);
        int na = TweakCatalog.All.Count(t => !t.IsApplicable);
        AnsiConsole.MarkupLine("\n" + L.F("Total tweaks: [bold]{0}[/] (settings: {1}, actions: {2}); applied: [green]{3}[/]; unavailable on this system: [grey]{4}[/]",
            TweakCatalog.All.Count, total, TweakCatalog.All.Count - total, applied, na));
        AnsiConsole.MarkupLine(L.F("Laptop: {0}; power plan: {1}", Hardware.IsLaptop ? L.T("yes") : L.T("no"), Markup.Escape(PowerCfg.Active().Name)));
        return 0;
    }

    private static bool SafeApplied(ITweak t) { try { return t.IsApplied(); } catch { return false; } }

    /// <summary>Prints the tweak reference as Markdown (used to generate the README tables; honours --lang).</summary>
    public static int ListMarkdown()
    {
        var sb = new StringBuilder();
        foreach (var cat in Enum.GetValues<TweakCategory>())
        {
            var list = TweakCatalog.ByCategory(cat).ToList();
            sb.AppendLine($"### {Labels.Category(cat)} ({list.Count})\n");
            sb.AppendLine($"| ID | {L.T("Tweak")} | {L.T("Risk")} | Quick | {L.T("Description")} |");
            sb.AppendLine("|----|-------|--------|:-----:|------|");
            foreach (var t in list)
            {
                string d = t.Description.Replace("|", "/") + (t.Warning != null ? $" **{L.T("Warning:")}** " + t.Warning.Replace("|", "/") : "");
                if (t.SupportedOn != OsSupport.Both) d += $" *[{L.F("{0} only", Ui.OsOnlyLabel(t.SupportedOn))}]*";
                if (t.OsNote != null) d += " *[" + t.OsNote.Replace("|", "/") + "]*";
                sb.AppendLine($"| `{t.Id}` | {t.Name.Replace("|", "/")} | {Labels.Risk(t.Risk)} | {(t.Recommended && t.Risk != RiskLevel.Advanced ? L.T("yes") : "")} | {d} |");
            }
            sb.AppendLine();
        }
        Console.Out.Write(sb.ToString());
        return 0;
    }

    /// <summary>Developer check: lists tweak texts that have no Polish translation.</summary>
    public static int LangCheck()
    {
        L.Current = Lang.En;
        var missing = new SortedSet<string>(StringComparer.Ordinal);
        void Check(string? s) { if (!string.IsNullOrEmpty(s) && !PolishStrings.Map.ContainsKey(s)) missing.Add(s); }
        foreach (var t in TweakCatalog.All) { Check(t.Name); Check(t.Description); Check(t.Warning); Check(t.OsNote); }
        foreach (var r in Enum.GetValues<RiskLevel>()) Check(Labels.Risk(r));
        foreach (var c in Enum.GetValues<TweakCategory>()) Check(Labels.Category(c));
        foreach (var m in missing) Console.WriteLine("MISSING: " + m);
        Console.WriteLine($"{missing.Count} missing translation(s).");
        return missing.Count == 0 ? 0 : 1;
    }

    public static int Batch(bool quick, string? apply, bool revertAll)
    {
        Ui.Banner();
        if (revertAll)
        {
            var res = new TweakEngine().RevertAll();
            if (res.Count == 0) AnsiConsole.MarkupLine("[yellow]" + L.T("No saved changes to undo.") + "[/]");
            else Ui.Summary(res, Ui.RevertedVerb);
            return res.Any(r => r.Status == ResultStatus.Failed) ? 3 : 0;
        }

        List<ITweak> tweaks;
        if (quick) tweaks = TweakCatalog.QuickPreset().ToList();
        else if (string.Equals(apply, "all", StringComparison.OrdinalIgnoreCase)) tweaks = TweakCatalog.Applicable.ToList();
        else
        {
            var ids = apply!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            tweaks = TweakCatalog.All.Where(t => ids.Contains(t.Id, StringComparer.OrdinalIgnoreCase)).ToList();
            var unknown = ids.Where(i => !TweakCatalog.All.Any(t => t.Id.Equals(i, StringComparison.OrdinalIgnoreCase))).ToList();
            if (unknown.Count > 0)
            {
                AnsiConsole.MarkupLine($"[red]{L.T("Unknown ID:")}[/] {Markup.Escape(string.Join(", ", unknown))} {L.T("(see --list)")}");
                return 2;
            }
        }
        Ui.ApplyFlow(tweaks, interactive: false);
        return 0;
    }
}
