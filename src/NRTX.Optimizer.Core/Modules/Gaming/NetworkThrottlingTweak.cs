using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class NetworkThrottlingTweak : ITweak
{
    public string Id => "gaming.disable_network_throttling";
    public string Name => "Disable Windows Network Packet Throttling Index";
    public string Description => "Disables Windows default network packet rate-limiting mechanism during gaming and heavy network loads.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => false;

    private const string ProfileKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";

    public Task<bool> IsAppliedAsync()
    {
        var val = SafeRegistry.GetDword(RegistryHive.LocalMachine, ProfileKey, "NetworkThrottlingIndex");
        return Task.FromResult(val == -1 || val == unchecked((int)0xffffffff));
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: NetworkThrottlingIndex would be set to 0xffffffff (Disabled).", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, ProfileKey, "NetworkThrottlingIndex", unchecked((int)0xffffffff));

        return Task.FromResult(ExecutionResult.Ok("Network Throttling successfully disabled."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: NetworkThrottlingIndex would be restored to default 10.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, ProfileKey, "NetworkThrottlingIndex", 10);

        return Task.FromResult(ExecutionResult.Ok("Network Throttling restored to default (10)."));
    }
}
