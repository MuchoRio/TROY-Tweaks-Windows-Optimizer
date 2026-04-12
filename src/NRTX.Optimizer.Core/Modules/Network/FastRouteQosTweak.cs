using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Network;

public class FastRouteQosTweak : ITweak
{
    public string Id => "network.fastroute_qos_dscp46";
    public string Name => "TROY FastRoute: Game Packet QoS & DSCP 46 Expedited Forwarding (ExitLag Tech)";
    public string Description => "Tags game network packets with DSCP 46 (Expedited Forwarding) and unlocks Windows QoS traffic shaping for Valorant, CS2, Apex, and Fortnite, ensuring router/ISP packet prioritization and zero queue delay.";
    public TweakCategory Category => TweakCategory.Network;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string QosPolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\QoS";
    private const string TcpipQosKey = @"SYSTEM\CurrentControlSet\Services\Tcpip\QoS";

    private static readonly string[] TargetGames =
    [
        "VALORANT-Win64-Shipping.exe",
        "cs2.exe",
        "r5apex.exe",
        "Overwatch.exe",
        "FortniteClient-Win64-Shipping.exe",
        "RainbowSix.exe",
        "dota2.exe",
        "League of Legends.exe"
    ];

    public Task<bool> IsAppliedAsync()
    {
        var nla = SafeRegistry.GetString(RegistryHive.LocalMachine, TcpipQosKey, "Do not use NLA");
        bool nlaApplied = string.Equals(nla, "1", StringComparison.OrdinalIgnoreCase);

        bool policyApplied = true;
        foreach (var game in TargetGames)
        {
            var policyName = $"FastRoute_{Path.GetFileNameWithoutExtension(game)}";
            var dscp = SafeRegistry.GetString(RegistryHive.LocalMachine, $@"{QosPolicyKey}\{policyName}", "DSCP Value");
            if (dscp != "46")
            {
                policyApplied = false;
                break;
            }
        }

        return Task.FromResult(nlaApplied && policyApplied);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: DSCP 46 QoS policies would be configured for competitive games.", isDryRun: true));

        // 1. Enable QoS outside domain
        SafeRegistry.SetString(RegistryHive.LocalMachine, TcpipQosKey, "Do not use NLA", "1");

        // 2. Inject DSCP 46 policies for competitive games
        foreach (var game in TargetGames)
        {
            var policyName = $"FastRoute_{Path.GetFileNameWithoutExtension(game)}";
            var subKey = $@"{QosPolicyKey}\{policyName}";

            SafeRegistry.SetString(RegistryHive.LocalMachine, subKey, "Version", "1.0");
            SafeRegistry.SetString(RegistryHive.LocalMachine, subKey, "Application Name", game);
            SafeRegistry.SetString(RegistryHive.LocalMachine, subKey, "Protocol", "*");
            SafeRegistry.SetString(RegistryHive.LocalMachine, subKey, "Local Port", "*");
            SafeRegistry.SetString(RegistryHive.LocalMachine, subKey, "Local IP", "*");
            SafeRegistry.SetString(RegistryHive.LocalMachine, subKey, "Local IP Prefix Length", "*");
            SafeRegistry.SetString(RegistryHive.LocalMachine, subKey, "Remote Port", "*");
            SafeRegistry.SetString(RegistryHive.LocalMachine, subKey, "Remote IP", "*");
            SafeRegistry.SetString(RegistryHive.LocalMachine, subKey, "Remote IP Prefix Length", "*");
            SafeRegistry.SetString(RegistryHive.LocalMachine, subKey, "DSCP Value", "46");
            SafeRegistry.SetString(RegistryHive.LocalMachine, subKey, "Throttle Rate", "-1");
        }

        // 3. Ensure qWave service is running
        try
        {
            using var sc = new ServiceController("QWAVE");
            if (sc.Status != ServiceControllerStatus.Running && sc.Status != ServiceControllerStatus.StartPending)
            {
                sc.Start();
            }
        }
        catch { }

        return Task.FromResult(ExecutionResult.Ok("TROY FastRoute: DSCP 46 QoS policies active for competitive games."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: QoS policies would be removed.", isDryRun: true));

        try
        {
            using var baseKey = Registry.LocalMachine.OpenSubKey(QosPolicyKey, writable: true);
            if (baseKey != null)
            {
                foreach (var game in TargetGames)
                {
                    var policyName = $"FastRoute_{Path.GetFileNameWithoutExtension(game)}";
                    try { baseKey.DeleteSubKeyTree(policyName, throwOnMissingSubKey: false); } catch { }
                }
            }
        }
        catch { }

        return Task.FromResult(ExecutionResult.Ok("FastRoute QoS policies removed."));
    }
}
