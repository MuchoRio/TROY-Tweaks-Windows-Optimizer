using System.Collections.Concurrent;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Engine;

public class ExecutionEngine
{
    public event Action<string, double>? OnProgress;
    public event Action<string>? OnLog;

    private readonly object _eventLock = new();

    private void ReportProgress(string msg, double progress)
    {
        lock (_eventLock)
        {
            OnProgress?.Invoke(msg, progress);
        }
    }

    private void ReportLog(string msg)
    {
        lock (_eventLock)
        {
            OnLog?.Invoke(msg);
        }
    }

    public async Task<List<ExecutionResult>> ApplyTweaksAsync(
        IEnumerable<ITweak> tweaks,
        bool createRestorePoint = true,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        var tweakList = tweaks.ToList();
        if (tweakList.Count == 0)
        {
            ReportLog("No tweaks selected for execution.");
            return [];
        }

        // 1. Pre-flight Safety Gate (Restore Point - Strictly Sequential)
        if (createRestorePoint && !dryRun && PrivilegeGuard.IsAdministrator() && !cancellationToken.IsCancellationRequested)
        {
            ReportLog("🛡️ Safety Gate: Creating Windows System Restore Point...");
            ReportProgress("Creating System Restore Point...", 0.05);

            bool rpCreated = await RestorePointManager.CreateRestorePointAsync("NRTX Optimizer Pre-Execution Snapshot");
            if (rpCreated)
            {
                ReportLog("✅ System Restore Point successfully created.");
            }
            else
            {
                ReportLog("⚠️ Warning: System Restore Point creation skipped or not supported on this volume.");
            }
        }

        if (cancellationToken.IsCancellationRequested)
        {
            ReportLog("🛑 Execution cancelled by user.");
            ReportProgress("Cancelled", 1.0);
            return [];
        }

        // 2. Resource-Aware Partitioning: Fast Registry vs Throttled Commands/Services
        var heavyTweaks = new List<ITweak>();
        var fastTweaks = new List<ITweak>();

        foreach (var t in tweakList)
        {
            if (IsHeavyTweak(t)) heavyTweaks.Add(t);
            else fastTweaks.Add(t);
        }

        int total = tweakList.Count;
        int completedCount = 0;
        var results = new ConcurrentBag<ExecutionResult>();

        // 3. Execute Fast Registry Tweaks in Parallel
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(4, Environment.ProcessorCount),
            CancellationToken = cancellationToken
        };

        var fastTask = Parallel.ForEachAsync(fastTweaks, parallelOptions, async (tweak, ct) =>
        {
            if (ct.IsCancellationRequested) return;

            try
            {
                var res = await tweak.ApplyAsync(dryRun);
                results.Add(res);

                int done = Interlocked.Increment(ref completedCount);
                double progress = (double)done / total;

                if (res.Success)
                {
                    ReportLog($"  ✅ [{done}/{total}] {res.Message}");
                    AuditLogger.Log(AuditLogLevel.Success, "ExecutionEngine", $"Applied tweak [{tweak.Id}] - {res.Message}");
                }
                else
                {
                    ReportLog($"  ❌ [{done}/{total}] {res.Message} {(res.Exception != null ? "(" + res.Exception.Message + ")" : "")}");
                    AuditLogger.Log(AuditLogLevel.Warn, "ExecutionEngine", $"Failed applying [{tweak.Id}] - {res.Message}");
                }

                ReportProgress($"Applied: {tweak.Name}", progress);
            }
            catch (Exception ex)
            {
                int done = Interlocked.Increment(ref completedCount);
                var failRes = ExecutionResult.Fail($"Unhandled error in {tweak.Id}", ex);
                results.Add(failRes);
                ReportLog($"  ❌ [{done}/{total}] Exception in {tweak.Name}: {ex.Message}");
                AuditLogger.Log(AuditLogLevel.Error, "ExecutionEngine", $"Exception applying [{tweak.Id}]: {ex.Message}");
            }
        });

        // 4. Execute Heavy Subprocess / Service Tweaks with Bounded Throttle (Semaphore 2)
        using var throttle = new SemaphoreSlim(2, 2);
        var heavyTasks = heavyTweaks.Select(async tweak =>
        {
            if (cancellationToken.IsCancellationRequested) return;

            try
            {
                await throttle.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                if (cancellationToken.IsCancellationRequested) return;

                var res = await tweak.ApplyAsync(dryRun);
                results.Add(res);

                int done = Interlocked.Increment(ref completedCount);
                double progress = (double)done / total;

                if (res.Success)
                {
                    ReportLog($"  ✅ [{done}/{total}] {res.Message}");
                    AuditLogger.Log(AuditLogLevel.Success, "ExecutionEngine", $"Applied heavy tweak [{tweak.Id}] - {res.Message}");
                }
                else
                {
                    ReportLog($"  ❌ [{done}/{total}] {res.Message} {(res.Exception != null ? "(" + res.Exception.Message + ")" : "")}");
                    AuditLogger.Log(AuditLogLevel.Warn, "ExecutionEngine", $"Failed applying heavy [{tweak.Id}] - {res.Message}");
                }

                ReportProgress($"Applied: {tweak.Name}", progress);
            }
            catch (Exception ex)
            {
                int done = Interlocked.Increment(ref completedCount);
                var failRes = ExecutionResult.Fail($"Unhandled error in {tweak.Id}", ex);
                results.Add(failRes);
                ReportLog($"  ❌ [{done}/{total}] Exception in {tweak.Name}: {ex.Message}");
                AuditLogger.Log(AuditLogLevel.Error, "ExecutionEngine", $"Exception applying [{tweak.Id}]: {ex.Message}");
            }
            finally
            {
                throttle.Release();
            }
        }).ToArray();

        try
        {
            await Task.WhenAll(fastTask, Task.WhenAll(heavyTasks));
        }
        catch (OperationCanceledException)
        {
            ReportLog("🛑 Execution cancelled by user.");
        }

        ReportProgress(cancellationToken.IsCancellationRequested ? "Cancelled" : "Completed", 1.0);
        return results.ToList();
    }

    public async Task<List<ExecutionResult>> RollbackTweaksAsync(
        IEnumerable<ITweak> tweaks,
        bool dryRun = false,
        CancellationToken cancellationToken = default)
    {
        var tweakList = tweaks.ToList();
        if (tweakList.Count == 0) return [];

        var heavyTweaks = new List<ITweak>();
        var fastTweaks = new List<ITweak>();

        foreach (var t in tweakList)
        {
            if (IsHeavyTweak(t)) heavyTweaks.Add(t);
            else fastTweaks.Add(t);
        }

        int total = tweakList.Count;
        int completedCount = 0;
        var results = new ConcurrentBag<ExecutionResult>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(4, Environment.ProcessorCount),
            CancellationToken = cancellationToken
        };

        var fastTask = Parallel.ForEachAsync(fastTweaks, parallelOptions, async (tweak, ct) =>
        {
            if (ct.IsCancellationRequested) return;

            try
            {
                var res = await tweak.RollbackAsync(dryRun);
                results.Add(res);

                int done = Interlocked.Increment(ref completedCount);
                double progress = (double)done / total;

                if (res.Success)
                {
                    ReportLog($"  ✅ [{done}/{total}] {res.Message}");
                    AuditLogger.Log(AuditLogLevel.Info, "ExecutionEngine", $"Rolled back [{tweak.Id}] - {res.Message}");
                }
                else
                {
                    ReportLog($"  ❌ [{done}/{total}] {res.Message}");
                    AuditLogger.Log(AuditLogLevel.Warn, "ExecutionEngine", $"Rollback warning [{tweak.Id}]: {res.Message}");
                }

                ReportProgress($"Reverted: {tweak.Name}", progress);
            }
            catch (Exception ex)
            {
                int done = Interlocked.Increment(ref completedCount);
                var failRes = ExecutionResult.Fail($"Rollback failed for {tweak.Id}", ex);
                results.Add(failRes);
                ReportLog($"  ❌ [{done}/{total}] Exception in {tweak.Name}: {ex.Message}");
                AuditLogger.Log(AuditLogLevel.Error, "ExecutionEngine", $"Rollback exception for [{tweak.Id}]: {ex.Message}");
            }
        });

        using var throttle = new SemaphoreSlim(2, 2);
        var heavyTasks = heavyTweaks.Select(async tweak =>
        {
            if (cancellationToken.IsCancellationRequested) return;

            try
            {
                await throttle.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                if (cancellationToken.IsCancellationRequested) return;

                var res = await tweak.RollbackAsync(dryRun);
                results.Add(res);

                int done = Interlocked.Increment(ref completedCount);
                double progress = (double)done / total;

                if (res.Success)
                {
                    ReportLog($"  ✅ [{done}/{total}] {res.Message}");
                    AuditLogger.Log(AuditLogLevel.Info, "ExecutionEngine", $"Rolled back [{tweak.Id}] - {res.Message}");
                }
                else
                {
                    ReportLog($"  ❌ [{done}/{total}] {res.Message}");
                    AuditLogger.Log(AuditLogLevel.Warn, "ExecutionEngine", $"Rollback warning [{tweak.Id}]: {res.Message}");
                }

                ReportProgress($"Reverted: {tweak.Name}", progress);
            }
            catch (Exception ex)
            {
                int done = Interlocked.Increment(ref completedCount);
                var failRes = ExecutionResult.Fail($"Rollback failed for {tweak.Id}", ex);
                results.Add(failRes);
                ReportLog($"  ❌ [{done}/{total}] Exception in {tweak.Name}: {ex.Message}");
                AuditLogger.Log(AuditLogLevel.Error, "ExecutionEngine", $"Rollback exception for [{tweak.Id}]: {ex.Message}");
            }
            finally
            {
                throttle.Release();
            }
        }).ToArray();

        try
        {
            await Task.WhenAll(fastTask, Task.WhenAll(heavyTasks));
        }
        catch (OperationCanceledException)
        {
            ReportLog("🛑 Rollback cancelled by user.");
        }

        ReportProgress(cancellationToken.IsCancellationRequested ? "Rollback Cancelled" : "Rollback Completed", 1.0);
        return results.ToList();
    }

    private static bool IsHeavyTweak(ITweak tweak)
    {
        var id = tweak.Id.ToLowerInvariant();
        return tweak.Category == TweakCategory.Services ||
               tweak.Category == TweakCategory.Debloater ||
               id.Contains("hyperv") ||
               id.Contains("power_plan") ||
               id.Contains("hibernation") ||
               id.Contains("autotuning") ||
               id.Contains("congestion") ||
               id.Contains("flush_dns") ||
               id.Contains("update_cache");
    }
}
