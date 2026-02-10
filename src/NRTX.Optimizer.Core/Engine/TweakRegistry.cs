using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Modules.Debloater;
using NRTX.Optimizer.Core.Modules.Gaming;
using NRTX.Optimizer.Core.Modules.Maintenance;
using NRTX.Optimizer.Core.Modules.Network;
using NRTX.Optimizer.Core.Modules.Performance;
using NRTX.Optimizer.Core.Modules.Privacy;
using NRTX.Optimizer.Core.Modules.Services;

namespace NRTX.Optimizer.Core.Engine;

/// <summary>
/// Community Edition Tweak Registry: Contains 28 essential, safe, and powerful optimization tweaks.
/// (Advanced eSports kernel tweaks, 0.5ms timer locks, and QoS routers are reserved for NRTX Labs VIP Organization).
/// </summary>
public class TweakRegistry
{
    private readonly List<ITweak> _tweaks = [];

    public IReadOnlyList<ITweak> AllTweaks => _tweaks.AsReadOnly();

    public TweakRegistry()
    {
        RegisterCommunityTweaks();
    }

    private void RegisterCommunityTweaks()
    {
        // 1. Privacy & Telemetry Guard (7 Tweaks)
        _tweaks.Add(new DisableTelemetryTweak());
        _tweaks.Add(new DisableDiagTrackServiceTweak());
        _tweaks.Add(new DisableAdvertisingIdTweak());
        _tweaks.Add(new DisableActivityHistoryTweak());
        _tweaks.Add(new DisableCortanaTweak());
        _tweaks.Add(new DisableFeedbackTrackingTweak());
        _tweaks.Add(new DisableLocationTrackingTweak());

        // 2. Essential Performance & Power (6 Tweaks)
        _tweaks.Add(new UltimatePerformancePlanTweak());
        _tweaks.Add(new Win32PrioritySeparationTweak());
        _tweaks.Add(new MemoryTrimTweak());
        _tweaks.Add(new DisableVisualBloatTweak());
        _tweaks.Add(new DisableHibernationTweak());
        _tweaks.Add(new DisableFastStartupTweak());

        // 3. Essential Gaming & Responsiveness (6 Tweaks)
        _tweaks.Add(new SystemResponsivenessTweak());
        _tweaks.Add(new NetworkThrottlingTweak());
        _tweaks.Add(new GpuPriorityTweak());
        _tweaks.Add(new DisableGameDvrTweak());
        _tweaks.Add(new DisableFullscreenOptimizationsTweak());
        _tweaks.Add(new RawMouseInputTweak());

        // 4. Basic Network Tuning (4 Tweaks)
        _tweaks.Add(new TcpNoDelayTweak());
        _tweaks.Add(new TcpAutoTuningTweak());
        _tweaks.Add(new DisableNetworkLsoTweak());
        _tweaks.Add(new FlushDnsTweak());

        // 5. Windows Services (1 Tweak)
        _tweaks.Add(new OptimizeWindowsServicesTweak());

        // 6. UWP Bloatware Debloater (1 Tweak)
        _tweaks.Add(new UwpDebloaterTweak());

        // 7. System Maintenance & Cleanup (3 Tweaks)
        _tweaks.Add(new CleanTempFilesTweak());
        _tweaks.Add(new CleanWindowsUpdateCacheTweak());
        _tweaks.Add(new EnableSsdTrimTweak());
    }

    public ITweak? GetById(string id)
        => _tweaks.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<ITweak> GetByCategory(TweakCategory category)
        => _tweaks.Where(t => t.Category == category);
}
