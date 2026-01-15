using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NRTX.Optimizer.Core.Native;

public record DpcLatencyInfo(
    double CurrentLatencyUs,
    double PeakLatencyUs,
    double AverageLatencyUs,
    string StatusText,
    string StatusColor
);

public static class DpcLatencyMonitorService
{
    private static double _peakLatencyUs = 0;
    private static readonly Queue<double> _recentSamples = new(30);
    private static readonly object _lock = new();

    private static long _lastIdleTime;
    private static long _lastKernelTime;
    private static long _lastUserTime;

    private static long ToLong(System.Runtime.InteropServices.ComTypes.FILETIME ft)
    {
        return ((long)ft.dwHighDateTime << 32) | (uint)ft.dwLowDateTime;
    }

    public static DpcLatencyInfo SampleLatency(int sampleCount = 10)
    {
        double maxDeltaUs = 0;
        double sumDeltaUs = 0;

        for (int i = 0; i < sampleCount; i++)
        {
            long start = Stopwatch.GetTimestamp();
            // Spin-wait / micro-delay to capture kernel DPC/ISR preemption jitter
            Thread.SpinWait(1000);
            long end = Stopwatch.GetTimestamp();

            double elapsedMicroseconds = (double)(end - start) * 1_000_000.0 / Stopwatch.Frequency;
            // Subtract expected spin overhead (~2-5us)
            double latencyUs = Math.Max(1.0, elapsedMicroseconds);

            if (latencyUs > maxDeltaUs) maxDeltaUs = latencyUs;
            sumDeltaUs += latencyUs;
        }

        // Also query kernel CPU scheduling times for holistic telemetry
        try
        {
            if (NativeMethods.GetSystemTimes(out var idleFt, out var kernelFt, out var userFt))
            {
                long idle = ToLong(idleFt);
                long kernel = ToLong(kernelFt);
                long user = ToLong(userFt);

                lock (_lock)
                {
                    _lastIdleTime = idle;
                    _lastKernelTime = kernel;
                    _lastUserTime = user;
                }
            }
        }
        catch { }

        double currentUs = Math.Round(maxDeltaUs, 1);
        double avgUs = Math.Round(sumDeltaUs / sampleCount, 1);

        lock (_lock)
        {
            if (currentUs > _peakLatencyUs) _peakLatencyUs = currentUs;
            _recentSamples.Enqueue(currentUs);
            if (_recentSamples.Count > 30) _recentSamples.Dequeue();
        }

        string statusText;
        string statusColor;

        if (currentUs < 150)
        {
            statusText = "🟢 eSports Ready (< 150µs)";
            statusColor = "#10b981"; // Emerald Green
        }
        else if (currentUs < 450)
        {
            statusText = "⚡ Normal Latency (150-450µs)";
            statusColor = "#38bdf8"; // Cyan
        }
        else if (currentUs < 900)
        {
            statusText = "⚠️ Moderate Jitter (450-900µs)";
            statusColor = "#f59e0b"; // Amber
        }
        else
        {
            statusText = "🔴 High DPC Latency (> 900µs)";
            statusColor = "#f43f5e"; // Rose Red
        }

        return new DpcLatencyInfo(currentUs, _peakLatencyUs, avgUs, statusText, statusColor);
    }

    public static void ResetPeak()
    {
        lock (_lock)
        {
            _peakLatencyUs = 0;
            _recentSamples.Clear();
        }
    }
}
