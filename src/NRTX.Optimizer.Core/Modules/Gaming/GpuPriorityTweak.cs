using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class GpuPriorityTweak : ITweak
{
    public string Id => "gaming.gpu_mmcss_priority";
    public string Name => "Optimize MMCSS GPU Priority for Games (DirectX/Vulkan Scheduling)";
    public string Description => "Configures Multimedia Class Scheduler Service (MMCSS) to assign highest GPU & CPU scheduling priority (GPU Priority 8, Priority 6) to gaming workloads.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => false;

    private const string GamesKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games";

    public Task<bool> IsAppliedAsync()
    {
        var gpu = SafeRegistry.GetDword(RegistryHive.LocalMachine, GamesKey, "GPU Priority");
        var prio = SafeRegistry.GetDword(RegistryHive.LocalMachine, GamesKey, "Priority");
        var sched = SafeRegistry.GetString(RegistryHive.LocalMachine, GamesKey, "Scheduling Category");
        return Task.FromResult(gpu == 8 && prio == 6 && sched == "High");
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: GPU Priority would be set to 8 and Scheduling Category to High.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, GamesKey, "GPU Priority", 8);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, GamesKey, "Priority", 6);
        SafeRegistry.SetString(RegistryHive.LocalMachine, GamesKey, "Scheduling Category", "High");
        SafeRegistry.SetString(RegistryHive.LocalMachine, GamesKey, "SFIO Priority", "High");

        return Task.FromResult(ExecutionResult.Ok("MMCSS Game GPU Priority boosted to High."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: GPU Priority would be restored to default.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, GamesKey, "GPU Priority", 8);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, GamesKey, "Priority", 2);
        SafeRegistry.SetString(RegistryHive.LocalMachine, GamesKey, "Scheduling Category", "Medium");
        SafeRegistry.SetString(RegistryHive.LocalMachine, GamesKey, "SFIO Priority", "Normal");

        return Task.FromResult(ExecutionResult.Ok("MMCSS Game Priority restored to default."));
    }
}
