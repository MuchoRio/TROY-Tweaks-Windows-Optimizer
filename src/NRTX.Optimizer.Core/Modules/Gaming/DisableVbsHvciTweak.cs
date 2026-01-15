using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class DisableVbsHvciTweak : ITweak
{
    public string Id => "gaming.disable_vbs_hvci";
    public string Name => "Disable Virtualization-Based Security (VBS) & Memory Integrity (HVCI)";
    public string Description => "Disables VBS and Hypervisor-Enforced Code Integrity (HVCI) in the Windows Kernel, eliminating CPU virtualization overhead in games.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Advanced;
    public bool RequiresRestart => true;

    private const string DeviceGuardKey = @"SYSTEM\CurrentControlSet\Control\DeviceGuard";
    private const string HvciKey = @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity";

    public Task<bool> IsAppliedAsync()
    {
        var vbs = SafeRegistry.GetDword(RegistryHive.LocalMachine, DeviceGuardKey, "EnableVirtualizationBasedSecurity");
        var hvci = SafeRegistry.GetDword(RegistryHive.LocalMachine, HvciKey, "Enabled");
        return Task.FromResult(vbs == 0 && hvci == 0);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: VBS and HVCI would be disabled via Registry.", isDryRun: true));

        RegistryBackupEngine.BackupKey(@"HKEY_LOCAL_MACHINE\" + DeviceGuardKey, "vbs_backup");

        bool ok1 = SafeRegistry.SetDword(RegistryHive.LocalMachine, DeviceGuardKey, "EnableVirtualizationBasedSecurity", 0);
        bool ok2 = SafeRegistry.SetDword(RegistryHive.LocalMachine, DeviceGuardKey, "RequirePlatformSecurityFeatures", 0);
        bool ok3 = SafeRegistry.SetDword(RegistryHive.LocalMachine, HvciKey, "Enabled", 0);

        return Task.FromResult(ok1 && ok2 && ok3
            ? ExecutionResult.Ok("VBS and Memory Integrity (HVCI) disabled. Restart required to apply changes.")
            : ExecutionResult.Fail("Failed to update VBS registry keys. Administrator privileges required."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: VBS and HVCI would be restored.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, DeviceGuardKey, "EnableVirtualizationBasedSecurity", 1);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, HvciKey, "Enabled", 1);

        return Task.FromResult(ExecutionResult.Ok("VBS & Memory Integrity restored to default enabled. Restart required."));
    }
}
