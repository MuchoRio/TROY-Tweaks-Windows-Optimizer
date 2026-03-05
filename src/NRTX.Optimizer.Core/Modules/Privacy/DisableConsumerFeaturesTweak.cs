using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Privacy;

public class DisableConsumerFeaturesTweak : ITweak
{
    public string Id => "privacy.disable_consumer_features";
    public string Name => "Disable Windows Start Menu Sponsored Apps & Cloud Content";
    public string Description => "Prevents Windows from auto-installing sponsored apps, suggested store games, and third-party promotions onto your Start Menu.";
    public TweakCategory Category => TweakCategory.Privacy;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string CloudContentPolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\CloudContent";
    private const string ContentDeliveryUserKey = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";

    public Task<bool> IsAppliedAsync()
    {
        var val1 = SafeRegistry.GetDword(RegistryHive.LocalMachine, CloudContentPolicyKey, "DisableWindowsConsumerFeatures");
        var val2 = SafeRegistry.GetDword(RegistryHive.CurrentUser, ContentDeliveryUserKey, "SilentInstalledAppsEnabled");
        return Task.FromResult(val1 == 1 && val2 == 0);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Windows Consumer Features & Sponsored apps would be disabled.", isDryRun: true));

        RegistryBackupEngine.BackupKey(@"HKEY_LOCAL_MACHINE\" + CloudContentPolicyKey, "cloud_content");

        SafeRegistry.SetDword(RegistryHive.LocalMachine, CloudContentPolicyKey, "DisableWindowsConsumerFeatures", 1);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, CloudContentPolicyKey, "DisableSoftLanding", 1);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, ContentDeliveryUserKey, "SilentInstalledAppsEnabled", 0);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, ContentDeliveryUserKey, "SystemPaneSuggestionsEnabled", 0);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, ContentDeliveryUserKey, "SubscribedContent-338388Enabled", 0);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, ContentDeliveryUserKey, "SubscribedContent-338389Enabled", 0);

        return Task.FromResult(ExecutionResult.Ok("Windows Start Menu sponsored apps & promotions disabled."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Windows Consumer Features would be restored.", isDryRun: true));

        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, CloudContentPolicyKey, "DisableWindowsConsumerFeatures");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, CloudContentPolicyKey, "DisableSoftLanding");
        SafeRegistry.SetDword(RegistryHive.CurrentUser, ContentDeliveryUserKey, "SilentInstalledAppsEnabled", 1);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, ContentDeliveryUserKey, "SystemPaneSuggestionsEnabled", 1);

        return Task.FromResult(ExecutionResult.Ok("Windows Consumer Features restored to defaults."));
    }
}
