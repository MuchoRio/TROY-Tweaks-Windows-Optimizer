using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Privacy;

public class DisableActivityHistoryTweak : ITweak
{
    public string Id => "privacy.disable_activity_history";
    public string Name => "Disable Windows Activity History & Timeline Tracking";
    public string Description => "Stops Windows from tracking app history and syncing user activities to Microsoft cloud.";
    public TweakCategory Category => TweakCategory.Privacy;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string PolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\System";

    public Task<bool> IsAppliedAsync()
    {
        var pub = SafeRegistry.GetDword(RegistryHive.LocalMachine, PolicyKey, "PublishUserActivities");
        var up = SafeRegistry.GetDword(RegistryHive.LocalMachine, PolicyKey, "UploadUserActivities");
        var enable = SafeRegistry.GetDword(RegistryHive.LocalMachine, PolicyKey, "EnableActivityFeed");
        return Task.FromResult(pub == 0 && up == 0 && enable == 0);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Activity History would be disabled.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, PolicyKey, "PublishUserActivities", 0);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, PolicyKey, "UploadUserActivities", 0);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, PolicyKey, "EnableActivityFeed", 0);

        return Task.FromResult(ExecutionResult.Ok("Activity History and Timeline sync disabled."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Activity History would be restored to default.", isDryRun: true));

        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, PolicyKey, "PublishUserActivities");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, PolicyKey, "UploadUserActivities");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, PolicyKey, "EnableActivityFeed");

        return Task.FromResult(ExecutionResult.Ok("Activity History restored to default."));
    }
}
