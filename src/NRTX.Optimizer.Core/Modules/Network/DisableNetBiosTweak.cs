using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Modules.Network;

public class DisableNetBiosTweak : ITweak
{
    public string Id => "network.disable_netbios";
    public string Name => "Disable NetBIOS over TCP/IP (Reduce LAN Broadcasts)";
    public string Description => "Disables legacy NetBIOS name query broadcast traffic across network adapters, improving security and reducing chatter.";
    public TweakCategory Category => TweakCategory.Network;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string NetbtKey = @"SYSTEM\CurrentControlSet\Services\NetBT\Parameters\Interfaces";

    public Task<bool> IsAppliedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(NetbtKey);
                if (key == null) return false;

                foreach (var sub in key.GetSubKeyNames())
                {
                    using var ifKey = key.OpenSubKey(sub);
                    var val = ifKey?.GetValue("NetbiosOptions");
                    if (val is int intVal && intVal == 2) return true;
                }
            }
            catch { }
            return false;
        });
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: NetBIOS would be set to disabled (2) on all interfaces.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(NetbtKey, true);
                if (key == null) return ExecutionResult.Fail("Failed to open NetBT interfaces key.");

                int count = 0;
                foreach (var sub in key.GetSubKeyNames())
                {
                    using var ifKey = key.OpenSubKey(sub, true);
                    if (ifKey != null)
                    {
                        ifKey.SetValue("NetbiosOptions", 2, RegistryValueKind.DWord);
                        count++;
                    }
                }

                return ExecutionResult.Ok($"NetBIOS disabled across {count} network interfaces.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Failed to disable NetBIOS.", ex);
            }
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: NetBIOS would be restored to DHCP Default (0).", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using var key = baseKey.OpenSubKey(NetbtKey, true);
                if (key != null)
                {
                    foreach (var sub in key.GetSubKeyNames())
                    {
                        using var ifKey = key.OpenSubKey(sub, true);
                        ifKey?.SetValue("NetbiosOptions", 0, RegistryValueKind.DWord);
                    }
                }

                return ExecutionResult.Ok("NetBIOS restored to DHCP default.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Failed to restore NetBIOS.", ex);
            }
        });
    }
}
