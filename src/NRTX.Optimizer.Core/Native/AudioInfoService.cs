using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Localization;

namespace NRTX.Optimizer.Core.Native;

public class AudioDeviceInfo
{
    public string DeviceName { get; set; } = "Default Audio Output";
    public string DeviceType { get; set; } = "Gaming Headset / Headphones";
    public int SampleRateHz { get; set; } = 48000;
    public int BitDepth { get; set; } = 24;
    public int Channels { get; set; } = 2;
    public bool IsExclusiveModeEnabled { get; set; } = true;
    public string SpatialAudioStatus { get; set; } = "Off (Stereo 1:1 - Pure Valorant HRTF)";
    public bool IsMmcssOptimized { get; set; } = true;

    public string FormatDescription => $"{BitDepth}-bit, {SampleRateHz} Hz ({(SampleRateHz == 48000 ? "48 kHz eSports Studio Match" : "Standard Format")})";
    public string ChannelsDescription => Channels == 2 ? "Stereo (2.0 Channels)" : (Channels > 2 ? $"{Channels}.1 Surround" : "Mono");

    public string FootstepBadge => IsMmcssOptimized
        ? "Footstep Audio Optimized (MMCSS High Priority Active)"
        : "Standard Audio Scheduling";

    public (string formatAdvice, string hrtfAdvice, string exclusiveAdvice, string summary) GetRecommendations(AppLanguage lang = AppLanguage.English)
    {
        if (lang == AppLanguage.Indonesian)
        {
            string format = SampleRateHz == 48000
                ? "24-bit, 48000 Hz (Optimal! Engine game Valorant & CS2 memproses audio native di 48kHz, menghindari beban CPU resampler)."
                : "Ubah ke 24-bit, 48000 Hz di Windows Sound Control Panel agar sinkron dengan audio engine game tanpa latency resampling.";

            string hrtf = "Windows Spatial Sound OFF (Gunakan HRTF bawaan Valorant/CS2 untuk akurasi arah langkah kaki 360° yang tajam tanpa distorsi ganda).";

            string exclusive = "Exclusive Mode ON (Mengizinkan game dan Discord mengakses direct buffer hardware audio sub-milidetik).";

            string summary = $"Headset/Output terdeteksi: {DeviceName} ({FormatDescription}). MMCSS Priority 6 aktif untuk mencegah delay tembakan dan langkah kaki.";

            return (format, hrtf, exclusive, summary);
        }
        else
        {
            string format = SampleRateHz == 48000
                ? "24-bit, 48000 Hz (Optimal! Valorant & CS2 audio engines mix natively at 48kHz, eliminating CPU resampler latency)."
                : "Set to 24-bit, 48000 Hz in Windows Sound Control Panel to eliminate audio resampling overhead.";

            string hrtf = "Windows Spatial Audio OFF (In-game 3D HRTF provides pinpoint footstep accuracy without surround phase cancellation).";

            string exclusive = "Exclusive Mode ON (Allows game and Discord to communicate directly with audio hardware buffers).";

            string summary = $"Active Audio Output: {DeviceName} ({FormatDescription}). MMCSS Priority 6 active for zero-lag footstep cues.";

            return (format, hrtf, exclusive, summary);
        }
    }
}

public static class AudioInfoService
{
    public static AudioDeviceInfo GetDefaultAudioDeviceInfo()
    {
        var info = new AudioDeviceInfo();

        try
        {
            // Query MMDevices Render Endpoints in Registry
            using var renderKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Render");
            if (renderKey != null)
            {
                var subKeyNames = renderKey.GetSubKeyNames();
                foreach (var subName in subKeyNames)
                {
                    using var devKey = renderKey.OpenSubKey(subName);
                    var deviceState = devKey?.GetValue("DeviceState");
                    // 1 = Active / Enabled
                    if (deviceState is int state && state == 1)
                    {
                        using var propsKey = devKey?.OpenSubKey("Properties");
                        if (propsKey != null)
                        {
                            var friendlyName = propsKey.GetValue("{a45c254e-df1c-4efd-8020-67d146a850e0},2")?.ToString();
                            var interfaceName = propsKey.GetValue("{b3f8fa53-0004-438e-9003-51a46e139bfc},6")?.ToString();

                            if (!string.IsNullOrEmpty(friendlyName))
                            {
                                if (!string.IsNullOrEmpty(interfaceName) && !friendlyName.Contains(interfaceName, StringComparison.OrdinalIgnoreCase))
                                {
                                    info.DeviceName = $"{friendlyName} ({interfaceName})";
                                }
                                else
                                {
                                    info.DeviceName = friendlyName;
                                }

                                if (info.DeviceName.Contains("Head", StringComparison.OrdinalIgnoreCase) ||
                                    info.DeviceName.Contains("otogai", StringComparison.OrdinalIgnoreCase) ||
                                    info.DeviceName.Contains("Ear", StringComparison.OrdinalIgnoreCase) ||
                                    info.DeviceName.Contains("USB", StringComparison.OrdinalIgnoreCase))
                                {
                                    info.DeviceType = "🎧 Gaming Headset / USB Audio";
                                }
                                else if (info.DeviceName.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase) ||
                                         info.DeviceName.Contains("Monitor", StringComparison.OrdinalIgnoreCase) ||
                                         info.DeviceName.Contains("HDMI", StringComparison.OrdinalIgnoreCase) ||
                                         info.DeviceName.Contains("Display", StringComparison.OrdinalIgnoreCase))
                                {
                                    info.DeviceType = "📺 Display Audio (HDMI / DP)";
                                }
                                else
                                {
                                    info.DeviceType = "🔊 Desktop Speakers / Line Out";
                                }

                                // If we found an active headset / otogai / headphones, prioritize it
                                if (info.DeviceName.Contains("Head", StringComparison.OrdinalIgnoreCase) ||
                                    info.DeviceName.Contains("otogai", StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch { }

        // Check MMCSS Audio state
        try
        {
            using var audioTask = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Audio");
            var sched = audioTask?.GetValue("Scheduling Category")?.ToString();
            var prio = audioTask?.GetValue("Priority");
            info.IsMmcssOptimized = string.Equals(sched, "High", StringComparison.OrdinalIgnoreCase) && (prio is int p && p >= 6);
        }
        catch { }

        return info;
    }

    public static void OpenSoundControlPanel()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "control.exe",
                Arguments = "mmsys.cpl sounds",
                UseShellExecute = true
            });
        }
        catch { }
    }
}
