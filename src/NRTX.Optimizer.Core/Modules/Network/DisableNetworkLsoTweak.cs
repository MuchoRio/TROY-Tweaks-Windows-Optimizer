using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Network;

public class DisableNetworkLsoTweak : ITweak
{
    public string Id => "network.disable_lso";
    public string Name => "Disable Large Send Offload (LSO) & Hardware Checksum Delay";
    public string Description => "Disables network packet segmentation offloading (LSO), preventing buffer micro-spikes and packet drops in competitive real-time online games.";
    public TweakCategory Category => TweakCategory.Network;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => false;

    private const string TcpipKey = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
    private const string NetAdaptersClassKey = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}";

    public Task<bool> IsAppliedAsync()
    {
        var val = SafeRegistry.GetDword(RegistryHive.LocalMachine, TcpipKey, "DisableTaskOffload");
        return Task.FromResult(val == 1);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Large Send Offload (LSO) would be disabled.", isDryRun: true));

        RegistryBackupEngine.BackupKey(@"HKEY_LOCAL_MACHINE\" + TcpipKey, "tcpip_lso");

        // 1. Disable global task offload
        bool ok = SafeRegistry.SetDword(RegistryHive.LocalMachine, TcpipKey, "DisableTaskOffload", 1);

        // 2. Iterate network adapters to disable *LSOv2IPv4 & *LSOv2IPv6 properties
        int adapterCount = 0;
        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var classKey = baseKey.OpenSubKey(NetAdaptersClassKey, true);
            if (classKey != null)
            {
                foreach (var subName in classKey.GetSubKeyNames())
                {
                    if (int.TryParse(subName, out _))
                    {
                        using var adapterKey = classKey.OpenSubKey(subName, true);
                        if (adapterKey?.GetValue("DriverDesc") != null)
                        {
                            adapterKey.SetValue("*LSOv2IPv4", "0", RegistryValueKind.String);
                            adapterKey.SetValue("*LSOv2IPv6", "0", RegistryValueKind.String);
                            adapterCount++;
                        }
                    }
                }
            }
        }
        catch { }

        return Task.FromResult(ok
            ? ExecutionResult.Ok($"Large Send Offload (LSO) disabled across {adapterCount} network adapters.")
            : ExecutionResult.Fail("Failed to update TCP/IP Task Offload parameters."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Large Send Offload (LSO) would be restored.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, TcpipKey, "DisableTaskOffload", 0);

        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var classKey = baseKey.OpenSubKey(NetAdaptersClassKey, true);
            if (classKey != null)
            {
                foreach (var subName in classKey.GetSubKeyNames())
                {
                    if (int.TryParse(subName, out _))
                    {
                        using var adapterKey = classKey.OpenSubKey(subName, true);
                        if (adapterKey?.GetValue("DriverDesc") != null)
                        {
                            adapterKey.SetValue("*LSOv2IPv4", "1", RegistryValueKind.String);
                            adapterKey.SetValue("*LSOv2IPv6", "1", RegistryValueKind.String);
                        }
                    }
                }
            }
        }
        catch { }

        return Task.FromResult(ExecutionResult.Ok("Large Send Offload (LSO) restored to Windows defaults."));
    }
}
