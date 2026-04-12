using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Network;

public class FastRouteAntiJitterPacingTweak : ITweak
{
    public string Id => "network.fastroute_anti_jitter_pacing";
    public string Name => "TROY FastRoute: Anti-Jitter UDP Packet Pacing & Bufferbloat Fix";
    public string Description => "Optimizes UDP/TCP network socket buffers, expands user port limits to 65,534, minimizes TIME_WAIT delay to 30s, and enables NonSack RTT resiliency to eliminate ping spikes and packet jitter.";
    public TweakCategory Category => TweakCategory.Network;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string TcpipParamKey = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
    private const string PacerParamKey = @"SYSTEM\CurrentControlSet\Services\Pacer\Parameters";

    public Task<bool> IsAppliedAsync()
    {
        var maxUserPort = SafeRegistry.GetDword(RegistryHive.LocalMachine, TcpipParamKey, "MaxUserPort");
        var tcpTimedWait = SafeRegistry.GetDword(RegistryHive.LocalMachine, TcpipParamKey, "TcpTimedWaitDelay");
        var nonSack = SafeRegistry.GetDword(RegistryHive.LocalMachine, PacerParamKey, "NonSackRttResiliency");

        bool applied = maxUserPort == 65534 && tcpTimedWait == 30 && nonSack == 1;
        return Task.FromResult(applied);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Anti-jitter packet pacing parameters would be configured.", isDryRun: true));

        // 1. Expand ephemeral port range & reduce connection reuse delay
        SafeRegistry.SetDword(RegistryHive.LocalMachine, TcpipParamKey, "MaxUserPort", 65534);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, TcpipParamKey, "TcpTimedWaitDelay", 30);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, TcpipParamKey, "DefaultReceiveWindow", 65535);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, TcpipParamKey, "DefaultSendWindow", 65535);

        // 2. Enable Non-SACK RTT Resiliency in pacer.sys for fast packet recovery
        SafeRegistry.SetDword(RegistryHive.LocalMachine, PacerParamKey, "NonSackRttResiliency", 1);
        SafeRegistry.SetDword(RegistryHive.LocalMachine, PacerParamKey, "EnableDynamicPacing", 1);

        return Task.FromResult(ExecutionResult.Ok("FastRoute Anti-Jitter Packet Pacing applied."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Packet pacing would be restored to defaults.", isDryRun: true));

        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, TcpipParamKey, "MaxUserPort");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, TcpipParamKey, "TcpTimedWaitDelay");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, TcpipParamKey, "DefaultReceiveWindow");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, TcpipParamKey, "DefaultSendWindow");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, PacerParamKey, "NonSackRttResiliency");
        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, PacerParamKey, "EnableDynamicPacing");

        return Task.FromResult(ExecutionResult.Ok("FastRoute Packet Pacing restored to defaults."));
    }
}
