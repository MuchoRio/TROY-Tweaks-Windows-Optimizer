using System.Text.Json;
using Spectre.Console;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Engine;
using NRTX.Optimizer.Core.Localization;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Native;
using NRTX.Optimizer.Core.Profiles;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Cli.UI;

public static class ConsoleUi
{
    public static void ShowBanner()
    {
        AnsiConsole.Clear();

        AnsiConsole.Write(
            new FigletText("TROY TWEAKS")
                .Centered()
                .Color(Color.Cyan1));

        var rule = new Rule("[bold mediumpurple2]TROY Tweaks Windows Optimizer Community Suite v5.4.1[/]")
        {
            Style = Style.Parse("mediumpurple2")
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    public static void ShowSystemDashboard(SystemSpecs specs)
    {
        var adminBadge = specs.IsAdmin
            ? "[bold green][ELEVATED] (ADMINISTRATOR)[/]"
            : "[bold red][RESTRICTED] (STANDARD USER - ELEVATION RECOMMENDED)[/]";

        var memColor = specs.MemoryLoadPercent > 80 ? "red" : (specs.MemoryLoadPercent > 60 ? "yellow" : "green");

        var sysTable = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey35);
        sysTable.AddColumn("[bold cyan]System Parameter[/]");
        sysTable.AddColumn("[bold white]Value[/]");
        sysTable.AddRow("[grey]Operating System[/]", $"[bold white]{specs.OsName}[/] ([grey]{specs.OsBuild}[/])");
        sysTable.AddRow("[grey]Processor (CPU)[/]", $"[bold white]{specs.CpuName}[/]");
        sysTable.AddRow("[grey]Graphics (GPU)[/]", $"[bold white]{specs.GpuName}[/]");
        sysTable.AddRow("[grey]System Memory (RAM)[/]", $"[{memColor}]{specs.AvailableRamGb} GB free[/] / [white]{specs.TotalRamGb} GB[/] ([{memColor}]{specs.MemoryLoadPercent}% Load[/])");
        sysTable.AddRow("[grey]Power Scheme[/]", $"[bold gold1]{specs.ActivePowerPlan}[/]");
        sysTable.AddRow("[grey]Privilege Token[/]", adminBadge);

        AnsiConsole.Write(new Panel(sysTable)
            .Header("[bold cyan] Live System Status [/]")
            .BorderColor(Color.MediumPurple2));
        AnsiConsole.WriteLine();
    }

    public static void ShowDetailedMemoryTable(DetailedMemoryStats stats)
    {
        var memTable = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
        memTable.AddColumn("[bold cyan]NT Kernel Memory Region[/]");
        memTable.AddColumn("[bold white]Allocated / Available Size[/]");

        memTable.AddRow("Physical RAM In-Use", $"[yellow]{stats.PhysicalInUseGb} GB[/] / {stats.PhysicalTotalGb} GB ({stats.PhysicalUsagePercent}%)");
        memTable.AddRow("Physical RAM Available", $"[green]{stats.PhysicalAvailableGb} GB[/]");
        memTable.AddRow("Pagefile (Commit Charge)", $"{stats.PagefileInUseGb} GB / {stats.PagefileTotalGb} GB ({stats.PagefileUsagePercent}%)");
        memTable.AddRow("System Cache Size", $"[cyan]{stats.SystemCacheMb} MB[/]");
        memTable.AddRow("Kernel Paged Pool", $"{stats.KernelPagedMb} MB");
        memTable.AddRow("Kernel Non-Paged Pool", $"{stats.KernelNonpagedMb} MB");
        memTable.AddRow("System Commit (Total / Limit)", $"{stats.CommitTotalGb} GB / {stats.CommitLimitGb} GB");
        memTable.AddRow("Active Process & Handle Count", $"{stats.ProcessesCount} Processes · {stats.HandlesCount} Handles · {stats.ThreadsCount} Threads");

        AnsiConsole.Write(new Panel(memTable)
            .Header("[bold green] NT Kernel Memory Telemetry [/]")
            .BorderColor(Color.Green));
        AnsiConsole.WriteLine();
    }

    public static void ShowHealthReport(SystemHealthReport report)
    {
        var scoreColor = report.HealthScore >= 80 ? "green" : (report.HealthScore >= 60 ? "yellow" : "red");
        AnsiConsole.MarkupLine($"[bold]System Optimization Score:[/] [{scoreColor} bold]{report.HealthScore} / 100[/] ({report.AppliedCount}/{report.TotalCount} Tweaks Active)");
        AnsiConsole.WriteLine();

        var table = new Table().Border(TableBorder.Simple);
        table.AddColumn(LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "[bold]Kategori[/]" : "[bold]Category[/]");
        table.AddColumn(LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "[bold]Nama Modul Tweak[/]" : "[bold]Tweak Name[/]");
        table.AddColumn(LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "[bold]Tingkat Resiko[/]" : "[bold]Risk[/]");
        table.AddColumn(LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "[bold]Status[/]" : "[bold]Status[/]");

        foreach (var status in report.Statuses)
        {
            var info = LocalizationManager.GetTweakInfo(status.Tweak.Id);
            var isIndo = LocalizationManager.CurrentLanguage == AppLanguage.Indonesian;

            var statusBadge = status.IsApplied
                ? (isIndo ? "[green]AKTIF[/]" : "[green]ACTIVE[/]")
                : (isIndo ? "[grey]BAWAAN[/]" : "[grey]DEFAULT[/]");

            var riskBadge = status.Tweak.Risk switch
            {
                RiskLevel.Safe => isIndo ? "[green]Aman[/]" : "[green]Safe[/]",
                RiskLevel.Recommended => isIndo ? "[cyan]Rekomendasi[/]" : "[cyan]Recommended[/]",
                RiskLevel.Advanced => isIndo ? "[yellow]Lanjutan[/]" : "[yellow]Advanced[/]",
                _ => "[grey]Unknown[/]"
            };

            table.AddRow(
                $"[mediumpurple2]{LocalizationManager.GetCategoryName(status.Tweak.Category)}[/]",
                info.Name,
                riskBadge,
                statusBadge
            );
        }

        AnsiConsole.Write(table);
    }

    public static void ExportReportToJson(SystemHealthReport report, string outputPath)
    {
        var dto = new
        {
            Timestamp = DateTime.UtcNow,
            report.HealthScore,
            report.AppliedCount,
            report.TotalCount,
            Specs = report.Specs,
            Statuses = report.Statuses.Select(s => new
            {
                s.Tweak.Id,
                s.Tweak.Name,
                Category = s.Tweak.Category.ToString(),
                Risk = s.Tweak.Risk.ToString(),
                s.IsApplied
            })
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(outputPath, json);
    }

    public static void ExportReportToMarkdown(SystemHealthReport report, string outputPath)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# TROY Tweaks Windows Optimizer Diagnostic Report");
        sb.AppendLine();
        sb.AppendLine($"- **Generated At**: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine($"- **System Optimization Score**: **{report.HealthScore} / 100** ({report.AppliedCount}/{report.TotalCount} Tweaks Active)");
        sb.AppendLine($"- **OS**: {report.Specs.OsName} ({report.Specs.OsBuild})");
        sb.AppendLine($"- **CPU**: {report.Specs.CpuName}");
        sb.AppendLine($"- **GPU**: {report.Specs.GpuName}");
        sb.AppendLine($"- **RAM**: {report.Specs.AvailableRamGb} GB free / {report.Specs.TotalRamGb} GB ({report.Specs.MemoryLoadPercent}% Load)");
        sb.AppendLine();
        sb.AppendLine("## Tweak Status Matrix");
        sb.AppendLine();
        sb.AppendLine("| Category | Tweak ID | Tweak Name | Risk | Status |");
        sb.AppendLine("| :--- | :--- | :--- | :--- | :--- |");

        foreach (var st in report.Statuses)
        {
            var statusStr = st.IsApplied ? "Active" : "Default";
            sb.AppendLine($"| {st.Tweak.Category} | `{st.Tweak.Id}` | {st.Tweak.Name} | {st.Tweak.Risk} | {statusStr} |");
        }

        File.WriteAllText(outputPath, sb.ToString());
    }
}
