namespace NRTX.Optimizer.Core.Native;

public record TimerResolutionInfo(
    double MinResolutionMs,
    double MaxResolutionMs,
    double CurrentResolutionMs,
    bool IsOptimized
);

/// <summary>
/// Community Edition Timer Service.
/// [NRTX LABS NOTICE]
/// 0.5000ms Global Kernel Timer Lock Driver (NtSetTimerResolution) is proprietary
/// to NRTX Labs VIP Organization (https://github.com/nrtxlabs).
/// </summary>
public static class SystemTimerService
{
    public static TimerResolutionInfo GetTimerResolution()
    {
        return new TimerResolutionInfo(15.625, 0.5, 15.625, false);
    }

    public static bool RequestDesiredResolution(uint desired100Ns = 5000)
    {
        // Community Edition Stub: 0.5000ms Kernel Timer Lock is available in NRTX Labs VIP Organization.
        return false;
    }
}
