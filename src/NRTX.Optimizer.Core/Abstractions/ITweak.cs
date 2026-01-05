using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Core.Abstractions;

public interface ITweak
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    TweakCategory Category { get; }
    RiskLevel Risk { get; }
    bool RequiresRestart { get; }

    Task<bool> IsAppliedAsync();
    Task<ExecutionResult> ApplyAsync(bool dryRun = false);
    Task<ExecutionResult> RollbackAsync(bool dryRun = false);
}
