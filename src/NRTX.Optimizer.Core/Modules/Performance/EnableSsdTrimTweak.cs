using System.Diagnostics;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Modules.Performance;

public class EnableSsdTrimTweak : ITweak
{
    public string Id => "perf.enable_ssd_trim";
    public string Name => "Enable Native SSD & NVMe TRIM Garbage Collection (DisableDeleteNotify 0)";
    public string Description => "Ensures NTFS/ReFS TRIM command pass-through is active on all solid-state drives, preventing SSD degradation and maintaining write speeds.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    public Task<bool> IsAppliedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "fsutil.exe",
                    Arguments = "behavior query DisableDeleteNotify",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                var outText = proc?.StandardOutput.ReadToEnd() ?? "";
                proc?.WaitForExit(3000);

                // DisableDeleteNotify = 0 means TRIM is enabled
                return outText.Contains("DisableDeleteNotify = 0") || outText.Contains("NTFS DisableDeleteNotify = 0");
            }
            catch
            {
                return false;
            }
        });
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: fsutil behavior set DisableDeleteNotify 0 would be executed.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "fsutil.exe",
                    Arguments = "behavior set DisableDeleteNotify 0",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(5000);

                return proc?.ExitCode == 0
                    ? ExecutionResult.Ok("SSD/NVMe TRIM garbage collection enabled successfully.")
                    : ExecutionResult.Fail("Failed to execute fsutil to enable SSD TRIM. Ensure Administrator privileges.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Exception while configuring SSD TRIM.", ex);
            }
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: SSD TRIM would remain enabled.", isDryRun: true));

        return Task.FromResult(ExecutionResult.Ok("SSD TRIM is a critical drive health feature; kept enabled for safety."));
    }
}
