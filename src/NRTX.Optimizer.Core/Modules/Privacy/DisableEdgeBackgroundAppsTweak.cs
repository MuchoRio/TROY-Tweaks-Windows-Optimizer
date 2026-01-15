using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Privacy;

public class DisableEdgeBackgroundAppsTweak : ITweak
{
    public string Id => "privacy.disable_edge_background_bloat";
    public string Name => "Disable Microsoft Edge Background Preloading & Startup Boost";
    public string Description => "Stops Microsoft Edge from running background processes on boot and keeps it closed when all tabs are exited, freeing 300-800MB RAM.";
    public TweakCategory Category => TweakCategory.Privacy;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string EdgePolicyKey = @"SOFTWARE\Policies\Microsoft\Edge";

    public Task<bool> IsAppliedAsync()
    {
        var bg = SafeRegistry.GetDword(RegistryHive.LocalMachine, EdgePolicyKey, "BackgroundModeEnabled");
        var boost = SafeRegistry.GetDword(RegistryHive.LocalMachine, EdgePolicyKey, "StartupBoostEnabled");
        return Task.FromResult(bg == 0 && boost == 0);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Edge background mode and startup boost would be disabled.", isDryRun: true));

        RegistryBackupEngine.BackupKey(@"HKEY_LOCAL_MACHINE\" + EdgePolicyKey, "edge_policy");

        SafeRegistry.SetDword(RegistryHive.LocalMachine, EdgePolicyKey, "BackgroundModeEnabled", 0);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, EdgePolicyKey, "StartupBoostEnabled", 0);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, EdgePolicyKey, "HubsSidebarEnabled", 0);

        return Task.FromResult(ExecutionResult.Ok("Microsoft Edge background apps and startup boost disabled."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Microsoft Edge settings would be restored.", isDryRun: true));

        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, EdgePolicyKey, "BackgroundModeEnabled");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, EdgePolicyKey, "StartupBoostEnabled");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, EdgePolicyKey, "HubsSidebarEnabled");

        return Task.FromResult(ExecutionResult.Ok("Microsoft Edge settings restored to defaults."));
    }
}
