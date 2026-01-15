using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Performance;

public class OptimizeMemoryPagingTweak : ITweak
{
    public string Id => "perf.disable_paging_executive";
    public string Name => "Keep Kernel & Drivers Resident in RAM (Disable Paging Executive)";
    public string Description => "Forces Windows Kernel and Device Drivers to stay resident in physical RAM rather than paging to disk, improving kernel responsiveness.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => true;

    private const string MemoryKey = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";

    public Task<bool> IsAppliedAsync()
    {
        var val = SafeRegistry.GetDword(RegistryHive.LocalMachine, MemoryKey, "DisablePagingExecutive");
        return Task.FromResult(val == 1);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: DisablePagingExecutive would be set to 1.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, MemoryKey, "DisablePagingExecutive", 1);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, MemoryKey, "LargeSystemCache", 0);

        return Task.FromResult(ExecutionResult.Ok("Kernel resident memory execution enabled."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: DisablePagingExecutive would be reverted to 0.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, MemoryKey, "DisablePagingExecutive", 0);

        return Task.FromResult(ExecutionResult.Ok("Memory paging settings reverted to Windows defaults."));
    }
}
