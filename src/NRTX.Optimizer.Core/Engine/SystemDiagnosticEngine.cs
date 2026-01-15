using System.Collections.Concurrent;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Engine;

public class TweakStatusReport
{
    public ITweak Tweak { get; set; } = null!;
    public bool IsApplied { get; set; }
}

public class SystemHealthReport
{
    public SystemSpecs Specs { get; set; } = null!;
    public List<TweakStatusReport> Statuses { get; set; } = [];
    public int AppliedCount { get; set; }
    public int TotalCount { get; set; }
    public int HealthScore { get; set; } // 0 - 100
}

public class SystemDiagnosticEngine
{
    private readonly TweakRegistry _registry;

    public SystemDiagnosticEngine(TweakRegistry registry)
    {
        _registry = registry;
    }

    public async Task<SystemHealthReport> ScanAsync(CancellationToken cancellationToken = default)
    {
        var specsTask = SystemSpecs.CollectAsync();
        var allTweaks = _registry.AllTweaks;
        var statusArray = new TweakStatusReport[allTweaks.Count];
        int appliedCount = 0;

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(4, Environment.ProcessorCount),
            CancellationToken = cancellationToken
        };

        var indexedTweaks = allTweaks.Select((tweak, index) => (tweak, index));

        var tweaksTask = Parallel.ForEachAsync(indexedTweaks, parallelOptions, async (item, ct) =>
        {
            bool isApplied = false;
            try
            {
                isApplied = await item.tweak.IsAppliedAsync();
            }
            catch { }

            if (isApplied)
            {
                Interlocked.Increment(ref appliedCount);
            }

            statusArray[item.index] = new TweakStatusReport
            {
                Tweak = item.tweak,
                IsApplied = isApplied
            };
        });

        await Task.WhenAll(specsTask, tweaksTask);

        var specs = await specsTask;
        var report = new SystemHealthReport
        {
            Specs = specs,
            TotalCount = allTweaks.Count,
            AppliedCount = appliedCount,
            Statuses = statusArray.Where(s => s != null).ToList()
        };

        // Health score calculation: Baseline 40 + (applied tweaks ratio * 60)
        double ratio = report.TotalCount > 0 ? (double)appliedCount / report.TotalCount : 0;
        report.HealthScore = (int)Math.Min(100, Math.Round(40 + (ratio * 60)));

        return report;
    }
}
