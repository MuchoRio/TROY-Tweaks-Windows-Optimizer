using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Privacy;

public class DisableCortanaTweak : ITweak
{
    public string Id => "privacy.disable_cortana";
    public string Name => "Disable Cortana Background Process & Web Search";
    public string Description => "Disables Cortana and prevents Windows Search from querying Bing web servers for local files.";
    public TweakCategory Category => TweakCategory.Privacy;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string PolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search";
    private const string SearchUserKey = @"Software\Microsoft\Windows\CurrentVersion\Search";

    public Task<bool> IsAppliedAsync()
    {
        var val1 = SafeRegistry.GetDword(RegistryHive.LocalMachine, PolicyKey, "AllowCortana");
        var val2 = SafeRegistry.GetDword(RegistryHive.CurrentUser, SearchUserKey, "BingSearchEnabled");
        return Task.FromResult(val1 == 0 && val2 == 0);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Cortana and Bing Search integration would be disabled.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, PolicyKey, "AllowCortana", 0);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, PolicyKey, "DisableWebSearch", 1);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, SearchUserKey, "BingSearchEnabled", 0);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, SearchUserKey, "CortanaConsent", 0);

        return Task.FromResult(ExecutionResult.Ok("Cortana & Bing Web Search integration successfully disabled."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Cortana & Search integration would be restored.", isDryRun: true));

        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, PolicyKey, "AllowCortana");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, PolicyKey, "DisableWebSearch");
        SafeRegistry.SetDword(RegistryHive.CurrentUser, SearchUserKey, "BingSearchEnabled", 1);
        SafeRegistry.SetDword(RegistryHive.CurrentUser, SearchUserKey, "CortanaConsent", 1);

        return Task.FromResult(ExecutionResult.Ok("Cortana & Search settings restored."));
    }
}
