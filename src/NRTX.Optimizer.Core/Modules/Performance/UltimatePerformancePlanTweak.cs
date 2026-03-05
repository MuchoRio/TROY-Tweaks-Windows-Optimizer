using System.Diagnostics;
using System.Runtime.InteropServices;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Native;

namespace NRTX.Optimizer.Core.Modules.Performance;

public class UltimatePerformancePlanTweak : ITweak
{
    public string Id => "perf.ultimate_power_plan";
    public string Name => "Unlock & Activate Ultimate Performance Power Plan";
    public string Description => "Duplicates and activates the official Microsoft Ultimate Performance power plan, removing all CPU throttling and latency penalties.";
    public TweakCategory Category => TweakCategory.Performance;
    public RiskLevel Risk => RiskLevel.Recommended;
    public bool RequiresRestart => false;

    public Task<bool> IsAppliedAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                if (NativeMethods.PowerGetActiveScheme(IntPtr.Zero, out var guidPtr) == 0 && guidPtr != IntPtr.Zero)
                {
                    var activeGuid = Marshal.PtrToStructure<Guid>(guidPtr);
                    NativeMethods.LocalFree(guidPtr);
                    return activeGuid == NativeMethods.GUID_ULTIMATE_PERFORMANCE || activeGuid == NativeMethods.GUID_HIGH_PERFORMANCE;
                }
            }
            catch { }
            return false;
        });
    }

    public Task<ExecutionResult> ApplyAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Ultimate Performance Plan would be unlocked and set active.", isDryRun: true));

        return Task.Run(() =>
        {
            try
            {
                // Try activating directly via Powrprof
                var ultimateGuid = NativeMethods.GUID_ULTIMATE_PERFORMANCE;
                uint res = NativeMethods.PowerSetActiveScheme(IntPtr.Zero, ref ultimateGuid);
                if (res == 0)
                {
                    return ExecutionResult.Ok("Ultimate Performance plan activated successfully.");
                }

                // If not unlocked on system, duplicate scheme via powercfg
                var psi = new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "-duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };

                using var proc = Process.Start(psi);
                var output = proc?.StandardOutput.ReadToEnd() ?? "";
                proc?.WaitForExit(5000);

                // Now set active
                res = NativeMethods.PowerSetActiveScheme(IntPtr.Zero, ref ultimateGuid);
                if (res == 0)
                {
                    return ExecutionResult.Ok("Ultimate Performance power plan duplicated and activated.");
                }

                // Fallback to High Performance
                var highGuid = NativeMethods.GUID_HIGH_PERFORMANCE;
                NativeMethods.PowerSetActiveScheme(IntPtr.Zero, ref highGuid);
                return ExecutionResult.Ok("High Performance power plan activated.");
            }
            catch (Exception ex)
            {
                return ExecutionResult.Fail("Failed to configure Ultimate Performance power scheme.", ex);
            }
        });
    }

    public Task<ExecutionResult> RollbackAsync(bool dryRun = false)
    {
        if (dryRun) return Task.FromResult(ExecutionResult.Ok("Dry-run: Power plan would be reverted to Balanced.", isDryRun: true));

        return Task.Run(() =>
        {
            var balancedGuid = NativeMethods.GUID_BALANCED;
            NativeMethods.PowerSetActiveScheme(IntPtr.Zero, ref balancedGuid);
            return ExecutionResult.Ok("Power plan restored to Balanced.");
        });
    }
}
