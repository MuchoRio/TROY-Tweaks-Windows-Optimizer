using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Performance;

public class Win32PrioritySeparationTweak : ITweak
{
    public string Id => "perf.win32_priority_separation";
    public string Name => "Optimize CPU Thread Quantum & Priority Separation (0x26 Gaming Low-Latency)";
    public string Description => "Configures Win32PrioritySeparation to 0x26 (38 decimal), dedicating shorter variable CPU time slices with maximum 3:1 foreground priority boost for ultra-responsive gaming & UI.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => false;

    private const string PriorityControlKey = @"SYSTEM\CurrentControlSet\Control\PriorityControl";
    private const int OptimizedQuantum = 38; // 0x26
    private const int DefaultQuantum = 2;

    public Task<bool> IsAppliedAsync()
    {
        var val = SafeRegistry.GetDword(RegistryHive.LocalMachine, PriorityControlKey, "Win32PrioritySeparation");
        return Task.FromResult(val == OptimizedQuantum);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Win32PrioritySeparation would be set to 0x26 (38).", isDryRun: true));

        RegistryBackupEngine.BackupKey(@"HKEY_LOCAL_MACHINE\" + PriorityControlKey, "priority_control");
        bool ok = SafeRegistry.SetDword(RegistryHive.LocalMachine, PriorityControlKey, "Win32PrioritySeparation", OptimizedQuantum);

        return Task.FromResult(ok
            ? ExecutionResult.Ok("CPU Win32 Priority Separation set to 0x26 (Max Foreground Responsiveness).")
            : ExecutionResult.Fail("Failed to write Win32PrioritySeparation registry key."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Win32PrioritySeparation would be restored to default (2).", isDryRun: true));

        bool ok = SafeRegistry.SetDword(RegistryHive.LocalMachine, PriorityControlKey, "Win32PrioritySeparation", DefaultQuantum);

        return Task.FromResult(ok
            ? ExecutionResult.Ok("Win32PrioritySeparation restored to Windows default (2).")
            : ExecutionResult.Fail("Failed to restore Win32PrioritySeparation."));
    }
}
