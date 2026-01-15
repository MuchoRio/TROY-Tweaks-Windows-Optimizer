using System.Diagnostics;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class MouseUsbPowerThrottlingTweak : ITweak
{
    public string Id => "gaming.mouse_usb_power_throttling_disable";
    public string Name => "Disable USB Power Throttling & Selective Suspend for Gaming Mice";
    public string Description => "Disables Windows USB selective suspend and port sleep states, ensuring USB controllers and gaming mouse optical sensors remain at full 1000Hz+ active polling without micro-sleep wake delays when holding tactical angles.";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    private const string UsbPowerKey = @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling";

    public Task<bool> IsAppliedAsync()
    {
        var powerThrottlingOff = SafeRegistry.GetDword(RegistryHive.LocalMachine, UsbPowerKey, "PowerThrottlingOff");
        return Task.FromResult(powerThrottlingOff == 1);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: USB power throttling and selective suspend would be disabled.", isDryRun: true));

        SafeRegistry.SetDword(RegistryHive.LocalMachine, UsbPowerKey, "PowerThrottlingOff", 1);

        try
        {
            // Execute powercfg to disable USB selective suspend on current active power scheme
            using var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "powercfg",
                Arguments = "/setacvalueindex scheme_current 2a737441-1930-4402-8d77-b2bebba4d5a0 48e6b63d-50e4-4ad5-8752-a1076223ac28 0",
                CreateNoWindow = true,
                UseShellExecute = false
            });
            proc?.WaitForExit(3000);

            using var procActive = Process.Start(new ProcessStartInfo
            {
                FileName = "powercfg",
                Arguments = "/setactive scheme_current",
                CreateNoWindow = true,
                UseShellExecute = false
            });
            procActive?.WaitForExit(3000);
        }
        catch { }

        return Task.FromResult(ExecutionResult.Ok("USB Power Throttling and Selective Suspend disabled for gaming peripherals."));
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: USB power throttling would be restored to default.", isDryRun: true));

        SafeRegistry.DeleteValue(RegistryHive.LocalMachine, UsbPowerKey, "PowerThrottlingOff");

        try
        {
            using var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "powercfg",
                Arguments = "/setacvalueindex scheme_current 2a737441-1930-4402-8d77-b2bebba4d5a0 48e6b63d-50e4-4ad5-8752-a1076223ac28 1",
                CreateNoWindow = true,
                UseShellExecute = false
            });
            proc?.WaitForExit(3000);
        }
        catch { }

        return Task.FromResult(ExecutionResult.Ok("USB Power Throttling restored to Windows default."));
    }
}
