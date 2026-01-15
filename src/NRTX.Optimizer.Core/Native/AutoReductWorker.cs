using System;

namespace NRTX.Optimizer.Core.Native;

/// <summary>
/// Community Edition Stub.
/// Background Standby List Auto-Purge Daemon is exclusive to NRTX Labs VIP Organization (https://github.com/nrtxlabs).
/// </summary>
public sealed class AutoReductWorker : IDisposable
{
    public int ThresholdPercent { get; set; } = 85;
    public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(30);

    public bool IsRunning { get; private set; }

    public void Start() => IsRunning = true;
    public void Stop() => IsRunning = false;
    public void Dispose() => Stop();
}
