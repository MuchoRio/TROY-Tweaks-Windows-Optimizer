using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Network;

public class TcpNoDelayTweak : ITweak
{
    public string Id => "network.tcp_nodelay_ack";
    public string Name => "Disable Nagle's Algorithm & TCP Delayed ACKs (Lowest Latency Ping)";
    public string Description => "Configures TCPNoDelay=1 and TcpAckFrequency=1 on network adapters to eliminate artificial packet delay buffers.";
    public TweakCategory Category => TweakCategory.Network;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => false;

    private const string InterfacesKey = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";

    public Task<bool> IsAppliedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using var interfaces = baseKey.OpenSubKey(InterfacesKey);
                if (interfaces == null) return false;

                foreach (var subName in interfaces.GetSubKeyNames())
                {
                    using var ifKey = interfaces.OpenSubKey(subName);
                    if (ifKey?.GetValue("DhcpIPAddress") != null || ifKey?.GetValue("IPAddress") != null)
                    {
                        var nodelay = ifKey.GetValue("TCPNoDelay");
                        var ack = ifKey.GetValue("TcpAckFrequency");
                        if (nodelay is int nd && nd == 1 && ack is int ac && ac == 1)
                        {
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        });
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: TCPNoDelay and TcpAckFrequency would be applied to active interfaces.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using var interfaces = baseKey.OpenSubKey(InterfacesKey, true);
                if (interfaces == null) return ExecutionResult.Fail("Failed to open network interfaces registry key.");

                int count = 0;
                foreach (var subName in interfaces.GetSubKeyNames())
                {
                    using var ifKey = interfaces.OpenSubKey(subName, true);
                    if (ifKey != null && (ifKey.GetValue("DhcpIPAddress") != null || ifKey.GetValue("IPAddress") != null))
                    {
                        ifKey.SetValue("TCPNoDelay", 1, RegistryValueKind.DWord);
                        ifKey.SetValue("TcpAckFrequency", 1, RegistryValueKind.DWord);
                        ifKey.SetValue("TcpDelAckTicks", 0, RegistryValueKind.DWord);
                        count++;
                    }
                }

                return ExecutionResult.Ok($"TCP latency optimization applied to {count} network interfaces.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Failed to apply TCP latency tweaks.", ex);
            }
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: TCP latency settings would be removed.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using var interfaces = baseKey.OpenSubKey(InterfacesKey, true);
                if (interfaces != null)
                {
                    foreach (var subName in interfaces.GetSubKeyNames())
                    {
                        using var ifKey = interfaces.OpenSubKey(subName, true);
                        if (ifKey != null)
                        {
                            ifKey.DeleteValue("TCPNoDelay", false);
                            ifKey.DeleteValue("TcpAckFrequency", false);
                            ifKey.DeleteValue("TcpDelAckTicks", false);
                        }
                    }
                }

                return ExecutionResult.Ok("TCP latency settings restored to defaults.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Failed to restore TCP latency settings.", ex);
            }
        });
    }
}
