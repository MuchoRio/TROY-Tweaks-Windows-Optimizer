using System;

namespace NRTX.Optimizer.Core.Native;

public record MousePollingStats(
    int CurrentPollingRateHz = 0,
    int PeakPollingRateHz = 0,
    string StatusText = "Idle",
    long TotalSamples = 0
);

/// <summary>
/// Community Edition Stub.
/// 8000Hz Mouse Radar & Sensor Advisor is exclusive to NRTX Labs VIP Organization (https://github.com/nrtxlabs).
/// </summary>
public sealed class MousePollingMonitor : IDisposable
{
    public bool IsActive { get; private set; }
    public MousePollingStats CurrentStats { get; private set; } = new();

    public void Start() => IsActive = true;
    public void Stop() => IsActive = false;
    public void ResetPeak() => CurrentStats = new(0, 0, "Reset", 0);
    public void OnMouseMoveEvent() => CurrentStats = new(1000, 1000, "1000 Hz", CurrentStats.TotalSamples + 1);
    public void Dispose() => Stop();
}
