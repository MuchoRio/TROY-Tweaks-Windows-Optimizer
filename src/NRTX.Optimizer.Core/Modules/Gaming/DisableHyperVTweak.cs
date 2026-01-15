using System.Diagnostics;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Modules.Gaming;

public class DisableHyperVTweak : ITweak
{
    public string Id => "gaming.disable_hyperv_hypervisor";
    public string Name => "Disable Hyper-V Hypervisor Launch (Bare-Metal CPU Gaming Mode)";
    public string Description => "Sets 'hypervisorlaunchtype off' via BCDEdit, removing the Type-1 hypervisor layer for maximum raw CPU gaming performance and lowest DPC latency. (Note: Turn back ON if using WSL2/Docker).";
    public TweakCategory Category => TweakCategory.Gaming;
    public RiskLevel Risk => RiskLevel.Advanced;
    public bool RequiresRestart => true;

    public Task<bool> IsAppliedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "bcdedit.exe",
                    Arguments = "/enum {current}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                var outText = proc?.StandardOutput.ReadToEnd()?.ToLowerInvariant() ?? "";
                proc?.WaitForExit(3000);

                return outText.Contains("hypervisorlaunchtype") && outText.Contains("off");
            }
            catch
            {
                return false;
            }
        });
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: bcdedit hypervisorlaunchtype would be set to 'off'.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "bcdedit.exe",
                    Arguments = "/set hypervisorlaunchtype off",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(5000);

                return proc?.ExitCode == 0
                    ? ExecutionResult.Ok("Hyper-V Hypervisor launch disabled. Restart computer to enter bare-metal CPU mode.")
                    : ExecutionResult.Fail("Failed to update BCD settings. Ensure Administrator privileges.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Exception while updating BCD Hyper-V settings.", ex);
            }
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: bcdedit hypervisorlaunchtype would be restored to 'auto'.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "bcdedit.exe",
                    Arguments = "/set hypervisorlaunchtype auto",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(5000);

                return proc?.ExitCode == 0
                    ? ExecutionResult.Ok("Hyper-V Hypervisor restored to Auto (WSL2/Docker enabled). Restart required.")
                    : ExecutionResult.Fail("Failed to restore BCD Hyper-V settings.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Exception while restoring BCD settings.", ex);
            }
        });
    }
}
