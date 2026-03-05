using System.Diagnostics;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Native;

namespace NRTX.Optimizer.Core.Modules.Performance;

public class MemoryTrimTweak : ITweak
{
    public string Id => "perf.memory_trim";
    public string Name => "Flush & Trim System Process Working Set (Instant RAM Free)";
    public string Description => "Uses native Psapi EmptyWorkingSet to flush idle unreferenced memory pages from all active processes back to the standby pool.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    public Task<bool> IsAppliedAsync()
    {
        // Memory trim is an on-demand optimization action
        return Task.FromResult(false);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: System working set would be trimmed across all accessible processes.", isDryRun: true));

        return Task.Run(() =>
        {
            int successCount = 0;
            int totalProcesses = 0;
            var currentPid = Process.GetCurrentProcess().Id;

            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    totalProcesses++;
                    if (proc.Id != currentPid && proc.Id != 0 && proc.Id != 4)
                    {
                        if (NativeMethods.EmptyWorkingSet(proc.Handle))
                        {
                            successCount++;
                        }
                    }
                }
                catch { }
                finally
                {
                    proc.Dispose();
                }
            }

            // Also trim own process
            try
            {
                NativeMethods.EmptyWorkingSet(Process.GetCurrentProcess().Handle);
            }
            catch { }

            return ExecutionResult.Ok($"RAM working set trimmed successfully across {successCount} active processes.");
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        return Task.FromResult(ExecutionResult.Ok("Memory trim is a transient optimization; no rollback required."));
    }
}
