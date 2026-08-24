using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Performance;

public class DisableVisualBloatTweak : ITweak
{
    public string Id => "perf.snappy_ui_effects";
    public string Name => "Optimize Windows UI Responsiveness & Remove Menu Delay";
    public string Description => "Sets MenuShowDelay to 0ms and minimizes window animation latency for instant snappy navigation.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string DesktopKey = @"Control Panel\Desktop";

    public Task<bool> IsAppliedAsync()
    {
        var delay = SafeRegistry.GetString(RegistryHive.CurrentUser, DesktopKey, "MenuShowDelay");
        return Task.FromResult(delay == "0");
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: MenuShowDelay would be set to 0ms.", isDryRun: true));

        SafeRegistry.SetString(RegistryHive.CurrentUser, DesktopKey, "MenuShowDelay", "0");
        SafeRegistry.SetString(RegistryHive.CurrentUser, DesktopKey, "WaitToKillAppTimeout", "2000");
        SafeRegistry.SetString(RegistryHive.CurrentUser, DesktopKey, "HungAppTimeout", "1000");

        return Task.FromResult(ExecutionResult.Ok("UI delays removed and snappy response enabled."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: MenuShowDelay would be restored to 400ms.", isDryRun: true));

        SafeRegistry.SetString(RegistryHive.CurrentUser, DesktopKey, "MenuShowDelay", "400");
        SafeRegistry.SetString(RegistryHive.CurrentUser, DesktopKey, "WaitToKillAppTimeout", "5000");
        SafeRegistry.SetString(RegistryHive.CurrentUser, DesktopKey, "HungAppTimeout", "5000");

        return Task.FromResult(ExecutionResult.Ok("UI delays restored to Windows defaults."));
    }
}
