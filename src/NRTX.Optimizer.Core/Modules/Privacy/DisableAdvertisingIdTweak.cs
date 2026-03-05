using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Privacy;

public class DisableAdvertisingIdTweak : ITweak
{
    public string Id => "privacy.disable_advertising_id";
    public string Name => "Disable Advertising ID & Targeted Ads Tracking";
    public string Description => "Prevents Windows and apps from using your Advertising ID for tailored experiences and tracking.";
    public TweakCategory Category => TweakCategory.Privacy;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string AdvKey = @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo";
    private const string PolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo";

    public Task<bool> IsAppliedAsync()
    {
        var val1 = SafeRegistry.GetDword(RegistryHive.CurrentUser, AdvKey, "Enabled");
        var val2 = SafeRegistry.GetDword(RegistryHive.LocalMachine, PolicyKey, "DisabledByGroupPolicy");
        return Task.FromResult(val1 == 0 && val2 == 1);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Advertising ID would be disabled.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.CurrentUser, AdvKey, "Enabled", 0);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, PolicyKey, "DisabledByGroupPolicy", 1);

        return Task.FromResult(ExecutionResult.Ok("Advertising ID & Tracking successfully disabled."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Advertising ID would be restored to default.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.CurrentUser, AdvKey, "Enabled", 1);
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, PolicyKey, "DisabledByGroupPolicy");

        return Task.FromResult(ExecutionResult.Ok("Advertising ID restored to default."));
    }
}
