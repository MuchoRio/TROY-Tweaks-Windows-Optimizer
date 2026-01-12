using Spectre.Console;
using NRTX.Optimizer.Cli.UI;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Engine;
using NRTX.Optimizer.Core.Localization;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Modules.Performance;
using NRTX.Optimizer.Core.Native;
using NRTX.Optimizer.Core.Profiles;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Cli;

public static class Program
{
    private static readonly TweakRegistry Registry = new();
    private static readonly ExecutionEngine Engine = new();
    private static readonly SystemDiagnosticEngine Diagnostics = new(Registry);
    private static readonly CancellationTokenSource AppCts = new();

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            AppCts.Cancel();
            AnsiConsole.MarkupLine("\n[bold yellow]⚠️ Cancellation signal received. Stopping gracefully...[/]");
        };

        // Check for --lang parameter across arguments
        int langIdx = Array.IndexOf(args, "--lang");
        if (langIdx < 0) langIdx = Array.IndexOf(args, "-l");
        if (langIdx >= 0 && langIdx + 1 < args.Length)
        {
            var l = args[langIdx + 1].ToLowerInvariant();
            if (l == "id" || l == "id-id" || l == "indonesia")
            {
                LocalizationManager.SetLanguage(AppLanguage.Indonesian);
            }
            else
            {
                LocalizationManager.SetLanguage(AppLanguage.English);
            }
        }

        // 1. Handle Headless CLI arguments
        if (args.Length > 0 && !args[0].StartsWith("--lang") && !args[0].StartsWith("-l"))
        {
            return await HandleHeadlessArgsAsync(args);
        }

        // 2. Interactive TUI Menu Loop
        while (true)
        {
            ConsoleUi.ShowBanner();
            var specs = await SystemSpecs.CollectAsync();
            ConsoleUi.ShowSystemDashboard(specs);

            var isIndo = LocalizationManager.CurrentLanguage == AppLanguage.Indonesian;

            if (!specs.IsAdmin)
            {
                var tipMsg = isIndo
                    ? "[yellow]💡 Tips: Jalankan ulang sebagai Administrator untuk memodifikasi kunci registri & layanan Windows terlindungi.[/]"
                    : "[yellow]💡 Tip: Relaunch as Administrator to modify system-protected registry keys and services.[/]";
                AnsiConsole.MarkupLine(tipMsg);
                AnsiConsole.WriteLine();
            }

            var promptTitle = isIndo ? "[bold mediumpurple2]Pilih aksi yang diinginkan:[/]" : "[bold mediumpurple2]Select an action:[/]";

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(promptTitle)
                    .PageSize(14)
                    .AddChoices([
                        isIndo ? "🎯 Terapkan Profil Optimasi 1-Klik" : "🎯 Apply 1-Click Optimization Profile",
                        isIndo ? "🧩 Jelajahi & Pilih Modul Tweak per Kategori" : "🧩 Explore & Toggle Tweaks by Category",
                        isIndo ? "🧠 Inspeksi Detail Telemetri Memori Kernel NT" : "🧠 Inspect Detailed NT Kernel Memory Telemetry",
                        isIndo ? "🚀 Bersihkan RAM & File Temp Instan" : "🚀 Instant RAM Trim & Temp Clean",
                        isIndo ? "📊 Jalankan Scan Diagnostik & Skor Kesehatan Sistem" : "📊 Run Full Diagnostic Scan & Health Report",
                        isIndo ? "📄 Ekspor Laporan Diagnostik (JSON / Markdown)" : "📄 Export Diagnostic Report (JSON / Markdown)",
                        isIndo ? "💾 Buat Titik Pemulihan (Windows Restore Point)" : "💾 Create Windows System Restore Point",
                        isIndo ? "📦 Kelola Snapshot State Sistem & Cadangan" : "📦 Manage System Snapshots & Backups",
                        isIndo ? "⏮️  Kembalikan / Rollback Perubahan ke Bawaan" : "⏮️  Rollback / Undo Changes",
                        isIndo ? "🌐 Ganti Bahasa / Switch Language (EN / ID)" : "🌐 Switch Language / Ganti Bahasa (EN / ID)",
                        isIndo ? "🛡️  Jalankan Ulang sebagai Administrator (UAC)" : "🛡️  Relaunch as Administrator (UAC)",
                        isIndo ? "🚪 Keluar" : "🚪 Exit"
                    ]));

            if (choice.StartsWith("🚪"))
            {
                AnsiConsole.MarkupLine("[bold cyan]🌸 Thank you for using NRTX Optimizer. Stay optimal! ✨[/]");
                break;
            }

            if (choice.StartsWith("🌐"))
            {
                var newLang = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select Language / Pilih Bahasa:")
                        .AddChoices(["🇺🇸 English (en-US)", "🇮🇩 Bahasa Indonesia (id-ID)"]));

                if (newLang.Contains("Indonesia"))
                {
                    LocalizationManager.SetLanguage(AppLanguage.Indonesian);
                }
                else
                {
                    LocalizationManager.SetLanguage(AppLanguage.English);
                }
                continue;
            }

            if (choice.StartsWith("🛡️"))
            {
                if (PrivilegeGuard.RelaunchAsAdmin())
                {
                    return 0;
                }
                AnsiConsole.MarkupLine("[red]Failed to trigger UAC elevation.[/]");
                Thread.Sleep(1500);
                continue;
            }

            if (choice.StartsWith("🎯"))
            {
                await MenuApplyProfileAsync();
            }
            else if (choice.StartsWith("🧩"))
            {
                await MenuExploreTweaksAsync();
            }
            else if (choice.StartsWith("🧠"))
            {
                var stats = MemReductEngine.GetStats();
                ConsoleUi.ShowDetailedMemoryTable(stats);
            }
            else if (choice.StartsWith("🚀"))
            {
                await MenuQuickCleanAsync();
            }
            else if (choice.StartsWith("📊"))
            {
                await MenuRunDiagnosticsAsync();
            }
            else if (choice.StartsWith("📄"))
            {
                await MenuExportReportAsync();
            }
            else if (choice.StartsWith("💾"))
            {
                await MenuCreateRestorePointAsync();
            }
            else if (choice.StartsWith("📦"))
            {
                MenuManageSnapshots();
            }
            else if (choice.StartsWith("⏮️"))
            {
                await MenuRollbackAsync();
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(isIndo ? "[grey]Tekan tombol apapun untuk kembali ke menu utama...[/]" : "[grey]Press any key to return to main menu...[/]");
            Console.ReadKey(true);
        }

        return 0;
    }

    private static async Task MenuApplyProfileAsync()
    {
        var isIndo = LocalizationManager.CurrentLanguage == AppLanguage.Indonesian;

        var profileChoice = AnsiConsole.Prompt(
            new SelectionPrompt<IProfile>()
                .Title(isIndo ? "[bold cyan]Pilih Profil Optimasi:[/]" : "[bold cyan]Choose an Optimization Profile:[/]")
                .UseConverter(p =>
                {
                    var (name, desc) = LocalizationManager.GetProfileInfo(p.Id);
                    return $"{p.Icon} {name} [grey]({p.TargetTweakIds.Count} tweaks)[/]\n   [grey]{desc}[/]";
                })
                .AddChoices(ProfileManager.AllProfiles));

        var targetTweaks = profileChoice.TargetTweakIds
            .Select(id => Registry.GetById(id))
            .Where(t => t != null)
            .Cast<ITweak>()
            .ToList();

        var (profName, _) = LocalizationManager.GetProfileInfo(profileChoice.Id);
        AnsiConsole.MarkupLine($"[bold]{(isIndo ? "Menerapkan Profil:" : "Applying Profile:")}[/] [cyan]{profName}[/]");

        var dryRun = AnsiConsole.Confirm(isIndo ? "Jalankan sebagai simulasi ([bold yellow]Dry-Run[/]) dulu?" : "Run as simulation ([bold yellow]Dry-Run[/]) first?", defaultValue: false);
        var createRestore = !dryRun && AnsiConsole.Confirm(isIndo ? "Buat [bold green]System Restore Point[/] sebelum menerapkan?" : "Create a [bold green]System Restore Point[/] before applying?", defaultValue: true);

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(isIndo ? "Menerapkan optimasi..." : "Applying optimization profile...", async ctx =>
            {
                Engine.OnLog += msg => AnsiConsole.MarkupLine(msg);
                await Engine.ApplyTweaksAsync(targetTweaks, createRestorePoint: createRestore, dryRun: dryRun, cancellationToken: AppCts.Token);
            });

        AnsiConsole.MarkupLine(isIndo ? "[bold green]✨ Proses penerapan profil berhasil selesai![/]" : "[bold green]✨ Profile application process finished![/]");
    }

    private static async Task MenuExploreTweaksAsync()
    {
        var isIndo = LocalizationManager.CurrentLanguage == AppLanguage.Indonesian;
        var categories = Enum.GetValues<TweakCategory>();

        var catChoice = AnsiConsole.Prompt(
            new SelectionPrompt<TweakCategory>()
                .Title(isIndo ? "[bold cyan]Pilih Kategori Tweak untuk Diinspeksi & Diterapkan:[/]" : "[bold cyan]Select Tweak Category to Inspect & Apply:[/]")
                .UseConverter(c => LocalizationManager.GetCategoryName(c))
                .AddChoices(categories));

        var categoryTweaks = Registry.GetByCategory(catChoice).ToList();
        if (categoryTweaks.Count == 0)
        {
            AnsiConsole.MarkupLine(isIndo ? "[yellow]Tidak ada tweak pada kategori ini.[/]" : "[yellow]No tweaks found in this category.[/]");
            return;
        }

        var tweakStatusMap = new Dictionary<ITweak, bool>();
        await AnsiConsole.Status().StartAsync(isIndo ? "Memeriksa status aktif..." : "Checking active status...", async ctx =>
        {
            foreach (var tw in categoryTweaks)
            {
                tweakStatusMap[tw] = await tw.IsAppliedAsync();
            }
        });

        var catDisplay = LocalizationManager.GetCategoryName(catChoice);
        var multiPrompt = new MultiSelectionPrompt<ITweak>()
            .Title($"[bold mediumpurple2]{catDisplay} Catalog ({categoryTweaks.Count} Modules)[/]")
            .PageSize(15)
            .UseConverter(t =>
            {
                var info = LocalizationManager.GetTweakInfo(t.Id);
                var state = tweakStatusMap.TryGetValue(t, out var applied) && applied
                    ? (isIndo ? "[green]✔ AKTIF[/]" : "[green]✔ ACTIVE[/]")
                    : (isIndo ? "[grey]⚪ BAWAAN[/]" : "[grey]⚪ DEFAULT[/]");
                var risk = LocalizationManager.GetRiskName(t.Risk);
                return $"{state} [bold white]{info.Name}[/] [grey]({risk})[/]\n   [grey]{info.Description}[/]\n   [dim cyan]💡 {info.Purpose}[/]";
            })
            .InstructionsText(isIndo ? "[grey](Tekan [blue]<spasi>[/] untuk memilih/uncheck, [green]<enter>[/] untuk eksekusi)[/]" : "[grey](Press [blue]<space>[/] to toggle a tweak, [green]<enter>[/] to execute selected)[/]");

        foreach (var tw in categoryTweaks)
        {
            var item = multiPrompt.AddChoice(tw);
            if (tweakStatusMap.TryGetValue(tw, out var applied) && applied)
            {
                item.Select();
            }
        }

        var selectedTweaks = AnsiConsole.Prompt(multiPrompt);
        if (selectedTweaks.Count == 0)
        {
            AnsiConsole.MarkupLine(isIndo ? "[yellow]Tidak ada tweak yang dipilih.[/]" : "[yellow]No tweaks selected.[/]");
            return;
        }

        var confirm = AnsiConsole.Confirm(isIndo ? $"Terapkan {selectedTweaks.Count} tweak terpilih sekarang?" : $"Apply {selectedTweaks.Count} selected tweaks now?", defaultValue: true);
        if (!confirm) return;

        await AnsiConsole.Status().StartAsync(isIndo ? "Menerapkan tweak terpilih..." : "Applying selected tweaks...", async ctx =>
        {
            Engine.OnLog += msg => AnsiConsole.MarkupLine(msg);
            await Engine.ApplyTweaksAsync(selectedTweaks, createRestorePoint: false, dryRun: false, cancellationToken: AppCts.Token);
        });
    }

    private static async Task MenuQuickCleanAsync()
    {
        var isIndo = LocalizationManager.CurrentLanguage == AppLanguage.Indonesian;
        AnsiConsole.MarkupLine(isIndo ? "[bold cyan]🚀 Menjalankan Trim Memori Instan & Pembersihan Cache...[/]" : "[bold cyan]🚀 Running Instant Memory Trim & Cache Purge...[/]");

        var trimTweak = new MemoryTrimTweak();
        var tempTweak = new Core.Modules.Maintenance.CleanTempFilesTweak();
        var dnsTweak = new Core.Modules.Network.FlushDnsTweak();

        var res1 = await trimTweak.ApplyAsync();
        AnsiConsole.MarkupLine($"[green]✅ {res1.Message}[/]");

        var res2 = await tempTweak.ApplyAsync();
        AnsiConsole.MarkupLine($"[green]✅ {res2.Message}[/]");

        var res3 = await dnsTweak.ApplyAsync();
        AnsiConsole.MarkupLine($"[green]✅ {res3.Message}[/]");
    }

    private static async Task MenuRunDiagnosticsAsync()
    {
        var isIndo = LocalizationManager.CurrentLanguage == AppLanguage.Indonesian;
        await AnsiConsole.Status().StartAsync(isIndo ? "Memindai status sistem dan tweak aktif..." : "Scanning system status and active tweaks...", async ctx =>
        {
            var report = await Diagnostics.ScanAsync();
            ConsoleUi.ShowHealthReport(report);
        });
    }

    private static async Task MenuExportReportAsync()
    {
        var format = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select Export Format / Pilih Format Ekspor:")
                .AddChoices(["JSON (.json)", "Markdown (.md)"]));

        var report = await Diagnostics.ScanAsync();
        var isJson = format.StartsWith("JSON");
        var ext = isJson ? "json" : "md";
        var fileName = $"troy_diagnostic_{DateTime.Now:yyyyMMdd_HHmmss}.{ext}";
        var outPath = Path.Combine(Environment.CurrentDirectory, fileName);

        if (isJson)
        {
            ConsoleUi.ExportReportToJson(report, outPath);
        }
        else
        {
            ConsoleUi.ExportReportToMarkdown(report, outPath);
        }

        AnsiConsole.MarkupLine($"[bold green]✅ Report exported successfully to:[/] [cyan]{outPath}[/]");
    }

    private static async Task MenuCreateRestorePointAsync()
    {
        if (!PrivilegeGuard.IsAdministrator())
        {
            AnsiConsole.MarkupLine("[bold red]❌ Administrator elevation is required to create a System Restore Point.[/]");
            return;
        }

        await AnsiConsole.Status().StartAsync("Creating Windows System Restore Point...", async ctx =>
        {
            bool ok = await RestorePointManager.CreateRestorePointAsync("Manual Snapshot via NRTX Optimizer CLI");
            if (ok)
            {
                AnsiConsole.MarkupLine("[bold green]✅ System Restore Point created successfully![/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[bold red]❌ Failed to create restore point. Ensure System Protection is enabled on C: drive.[/]");
            }
        });
    }

    private static void MenuManageSnapshots()
    {
        var snapshots = SnapshotManager.ListSnapshots();
        if (snapshots.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No state snapshots found in local storage.[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("[bold]Snapshot ID[/]");
        table.AddColumn("[bold]Created At (UTC)[/]");
        table.AddColumn("[bold]States Stored[/]");

        foreach (var s in snapshots)
        {
            table.AddRow(s.Id, s.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), s.RegistryStates.Count.ToString());
        }

        AnsiConsole.Write(table);
    }

    private static async Task MenuRollbackAsync()
    {
        var isIndo = LocalizationManager.CurrentLanguage == AppLanguage.Indonesian;
        var confirm = AnsiConsole.Confirm(isIndo ? "[bold yellow]Apakah Anda yakin ingin mengembalikan seluruh tweak ke pengaturan awal Windows?[/]" : "[bold yellow]Are you sure you want to rollback all tweaks to default?[/]", defaultValue: false);
        if (!confirm) return;

        await AnsiConsole.Status().StartAsync(isIndo ? "Mengembalikan seluruh optimasi..." : "Rolling back all optimizations...", async ctx =>
        {
            Engine.OnLog += msg => AnsiConsole.MarkupLine(msg);
            await Engine.RollbackTweaksAsync(Registry.AllTweaks, dryRun: false, cancellationToken: AppCts.Token);
        });

        AnsiConsole.MarkupLine("[bold green]✅ Rollback process completed.[/]");
    }

    private static async Task<int> HandleHeadlessArgsAsync(string[] args)
    {
        var cmd = args[0].ToLowerInvariant();
        bool dryRun = args.Contains("--dry-run");

        if (cmd == "scan" || cmd == "--scan")
        {
            var report = await Diagnostics.ScanAsync();
            ConsoleUi.ShowHealthReport(report);
            return 0;
        }

        if (cmd == "mem-stats")
        {
            var stats = MemReductEngine.GetStats();
            ConsoleUi.ShowDetailedMemoryTable(stats);
            return 0;
        }

        if (cmd == "export" || cmd == "--export")
        {
            var report = await Diagnostics.ScanAsync();
            string format = "json";
            string output = $"troy_diagnostic_{DateTime.Now:yyyyMMdd_HHmmss}.json";

            int fIdx = Array.IndexOf(args, "--format");
            if (fIdx >= 0 && fIdx + 1 < args.Length) format = args[fIdx + 1].ToLowerInvariant();

            int oIdx = Array.IndexOf(args, "--output");
            if (oIdx >= 0 && oIdx + 1 < args.Length) output = args[oIdx + 1];

            if (format == "md" || format == "markdown")
            {
                ConsoleUi.ExportReportToMarkdown(report, output);
            }
            else
            {
                ConsoleUi.ExportReportToJson(report, output);
            }

            Console.WriteLine($"Report exported to {output}");
            return 0;
        }

        if (cmd == "trim-ram")
        {
            var trim = new MemoryTrimTweak();
            var res = await trim.ApplyAsync(dryRun);
            Console.WriteLine(res.Message);
            return res.Success ? 0 : 1;
        }

        if (cmd == "apply" || cmd == "--apply")
        {
            string profileName = "gaming";
            int profIdx = Array.IndexOf(args, "--profile");
            if (profIdx >= 0 && profIdx + 1 < args.Length)
            {
                profileName = args[profIdx + 1].ToLowerInvariant();
            }

            var profile = ProfileManager.AllProfiles.FirstOrDefault(p => p.Id.Contains(profileName, StringComparison.OrdinalIgnoreCase))
                          ?? new GamingProfile();

            var tweaks = profile.TargetTweakIds
                .Select(id => Registry.GetById(id))
                .Where(t => t != null)
                .Cast<ITweak>()
                .ToList();

            var (name, _) = LocalizationManager.GetProfileInfo(profile.Id);
            Console.WriteLine($"Applying profile '{name}' ({tweaks.Count} tweaks, dryRun={dryRun})...");
            var results = await Engine.ApplyTweaksAsync(tweaks, createRestorePoint: !dryRun, dryRun: dryRun, cancellationToken: AppCts.Token);
            return results.All(r => r.Success) ? 0 : 1;
        }

        if (cmd == "rollback" || cmd == "--rollback")
        {
            Console.WriteLine($"Rolling back all tweaks (dryRun={dryRun})...");
            var results = await Engine.RollbackTweaksAsync(Registry.AllTweaks, dryRun: dryRun, cancellationToken: AppCts.Token);
            return results.All(r => r.Success) ? 0 : 1;
        }

        Console.WriteLine("Usage: NRTX.Optimizer.Cli [scan | mem-stats | export [--format json|md] [--output <path>] | apply --profile <esports|gaming|dev|privacy|safe> | rollback | trim-ram] [--dry-run] [--lang <en|id>]");
        return 0;
    }
}
