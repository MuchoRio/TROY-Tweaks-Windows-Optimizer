using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Native;

namespace NRTX.Optimizer.Core.Modules.Network;

public class FlushDnsTweak : ITweak
{
    public string Id => "network.flush_dns";
    public string Name => "Flush Windows DNS Resolver Cache";
    public string Description => "Purges the local DNS resolver cache using the native Win32 DnsFlushResolverCache API, resolving stale domain queries.";
    public TweakCategory Category => TweakCategory.Network;
    public RiskLevel Risk => RiskLevel.Safe;
    public bool RequiresRestart => false;

    public Task<bool> IsAppliedAsync()
    {
        return Task.FromResult(false);
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: DNS resolver cache would be flushed.", isDryRun: true));

        try
        {
            int result = NativeMethods.DnsFlushResolverCache();
            return Task.FromResult(result != 0
                ? ExecutionResult.Ok("DNS Resolver Cache flushed successfully.")
                : ExecutionResult.Fail("DnsFlushResolverCache returned failure code."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ExecutionResult.Fail("Exception during DNS flush.", ex));
        }
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        return Task.FromResult(ExecutionResult.Ok("DNS flush is an instant action; no rollback required."));
    }
}
