using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class EnableHagsTweak : ITweak
{
    public string Id => "gaming.enable_hags";
    public string Name => "Enable Hardware-Accelerated GPU Scheduling (HAGS Low-Latency)";
    public string Description => "Enables HAGS (HwSchMode 2), allowing modern GPUs (RTX / RX / Intel Arc) to directly manage their VRAM scheduling for reduced input lag.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => true;

    private const string GraphicsKey = @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers";

    public Task<bool> IsAppliedAsync()
    {
        var val = SafeRegistry.GetDword(RegistryHive.LocalMachine, GraphicsKey, "HwSchMode");
        return Task.FromResult(val == 2);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Hardware-Accelerated GPU Scheduling (HAGS) would be enabled (HwSchMode=2).", isDryRun: true));

        RegistryBackupEngine.BackupKey(@"HKEY_LOCAL_MACHINE\" + GraphicsKey, "graphics_drivers");
        bool ok = SafeRegistry.SetDword(RegistryHive.LocalMachine, GraphicsKey, "HwSchMode", 2);

        return Task.FromResult(ok
            ? ExecutionResult.Ok("Hardware-Accelerated GPU Scheduling (HAGS) enabled. Restart required.")
            : ExecutionResult.Fail("Failed to write HwSchMode registry key."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: HAGS would be disabled (HwSchMode=1).", isDryRun: true));

        bool ok = SafeRegistry.SetDword(RegistryHive.LocalMachine, GraphicsKey, "HwSchMode", 1);

        return Task.FromResult(ok
            ? ExecutionResult.Ok("HAGS reverted to disabled (HwSchMode=1).")
            : ExecutionResult.Fail("Failed to restore HwSchMode."));
    }
}
