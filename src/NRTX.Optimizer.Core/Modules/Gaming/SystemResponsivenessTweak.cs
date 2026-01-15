using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class SystemResponsivenessTweak : ITweak
{
    public string Id => "gaming.system_responsiveness";
    public string Name => "Maximize Multimedia & Game CPU Priority (SystemResponsiveness 0%)";
    public string Description => "Removes the default 20% CPU reservation for background services, dedicating 100% of CPU cycles to the active foreground game/app.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => false;

    private const string ProfileKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";

    public Task<bool> IsAppliedAsync()
    {
        var val = SafeRegistry.GetDword(RegistryHive.LocalMachine, ProfileKey, "SystemResponsiveness");
        return Task.FromResult(val == 0);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: SystemResponsiveness would be set to 0.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, ProfileKey, "SystemResponsiveness", 0);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, ProfileKey, "NoLazyMode", 1);

        return Task.FromResult(ExecutionResult.Ok("System Responsiveness set to 100% foreground priority."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: SystemResponsiveness would be restored to 20.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, ProfileKey, "SystemResponsiveness", 20);
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, ProfileKey, "NoLazyMode");

        return Task.FromResult(ExecutionResult.Ok("System Responsiveness restored to default (20%)."));
    }
}
