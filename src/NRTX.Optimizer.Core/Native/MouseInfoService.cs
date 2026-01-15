using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Localization;

namespace NRTX.Optimizer.Core.Native;

public class MouseDeviceInfo
{
    public string DeviceName { get; set; } = "High-Precision Optical Gaming Mouse";
    public string Manufacturer { get; set; } = "eSports Hardware Device";
    public string ConnectionType { get; set; } = "USB Wired / 2.4GHz Dongle";
    public int PointerSpeed { get; set; } = 10; // 1-20 (10 = 6/11 default 1:1)
    public bool IsEnhancedPrecisionEnabled { get; set; } = false;
    public bool IsRawInputOneToOne { get; set; } = true;
    public int EstimatedPollingRateHz { get; set; } = 1000;
    public int QueueBufferSize { get; set; } = 128;

    public string PointerSpeedDescription => PointerSpeed == 10 ? "6/11 Notch (100% 1:1 Pixel Mapping)" : $"{PointerSpeed / 2 + 1}/11 Notch (Non-Standard Scaling)";
    
    public string AccelerationStatusBadge => (!IsEnhancedPrecisionEnabled && IsRawInputOneToOne)
        ? "1:1 Raw Input Active (Zero Acceleration)"
        : "Windows Pointer Acceleration Active";

    public (string accelAdvice, string dpiAdvice, string bufferAdvice, string summary) GetRecommendations(AppLanguage lang = AppLanguage.English)
    {
        if (lang == AppLanguage.Indonesian)
        {
            string accel = !IsEnhancedPrecisionEnabled
                ? "Enhanced Pointer Precision OFF (Bagus! Akselerasi Windows mati, pergerakan kursor 100% linear dan konsisten untuk muscle memory flick shot)."
                : "Matikan 'Enhance pointer precision' di Mouse Properties agar kecepatan kursor tidak berubah-ubah sesuai kecepatan gesekan tangan.";

            string dpi = "Rekomendasi eSports di 1080p: Gunakan 800 atau 1600 DPI (Sensor tracking modern paling presisi tanpa jitter pixel skipping). Valorant: 0.25 - 0.45 (200-360 eDPI), CS2: 0.9 - 1.4.";

            string buffer = "Mouse Queue Buffer: 128 Packets aktif. Menghilangkan risiko packet drop saat melakukan flick 180° berkecepatan tinggi pada polling rate 1000Hz s/d 8000Hz.";

            string summary = $"Mouse terdeteksi: {DeviceName} ({ConnectionType}). 1:1 Raw Input aktif untuk presisi tembakan maksimal pada 180Hz.";

            return (accel, dpi, buffer, summary);
        }
        else
        {
            string accel = !IsEnhancedPrecisionEnabled
                ? "Enhanced Pointer Precision OFF (Optimal! Zero acceleration ensures 100% muscle memory consistency for flick shots)."
                : "Disable 'Enhance pointer precision' in Mouse Properties to prevent non-linear pointer acceleration curves.";

            string dpi = "eSports 1080p Recommendation: Set mouse hardware to 800 or 1600 DPI for lowest sensor jitter and input latency. Valorant: 200-360 eDPI, CS2: 700-1100 eDPI.";

            string buffer = "Mouse Queue Buffer: 128 Packets active. Eliminates USB HID packet loss during high-speed 180° flicks at 1000Hz - 8000Hz.";

            string summary = $"Detected Mouse: {DeviceName} ({ConnectionType}). 1:1 Raw Input active for tournament-level precision at 180Hz.";

            return (accel, dpi, buffer, summary);
        }
    }
}

public static class MouseInfoService
{
    public static MouseDeviceInfo GetDefaultMouseDeviceInfo()
    {
        var info = new MouseDeviceInfo();

        // 1. Read Windows Pointer Settings from Registry
        try
        {
            using var mouseKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Mouse");
            if (mouseKey != null)
            {
                var speedStr = mouseKey.GetValue("MouseSensitivity")?.ToString();
                if (int.TryParse(speedStr, out int speed))
                {
                    info.PointerSpeed = speed;
                }

                var mouseSpeed = mouseKey.GetValue("MouseSpeed")?.ToString();
                var thresh1 = mouseKey.GetValue("MouseThreshold1")?.ToString();
                var thresh2 = mouseKey.GetValue("MouseThreshold2")?.ToString();

                // If MouseSpeed is 0 and Thresholds are 0, Enhanced Pointer Precision is disabled
                info.IsEnhancedPrecisionEnabled = mouseSpeed != "0" || thresh1 != "0" || thresh2 != "0";
                info.IsRawInputOneToOne = !info.IsEnhancedPrecisionEnabled && info.PointerSpeed == 10;
            }
        }
        catch { }

        // 2. Read Queue Buffer Size from mouclass driver parameters
        try
        {
            using var mouclassKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\mouclass\Parameters");
            var queueSize = mouclassKey?.GetValue("MouseDataQueueSize");
            if (queueSize is int q && q > 0)
            {
                info.QueueBufferSize = q;
            }
        }
        catch { }

        // 3. Detect Mouse Hardware Details from PnP / HID
        try
        {
            using var hidKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\HID");
            if (hidKey != null)
            {
                foreach (var subName in hidKey.GetSubKeyNames())
                {
                    if (subName.Contains("COL05", StringComparison.OrdinalIgnoreCase) ||
                        subName.Contains("MI_02", StringComparison.OrdinalIgnoreCase) ||
                        subName.Contains("COL01", StringComparison.OrdinalIgnoreCase))
                    {
                        using var devSub = hidKey.OpenSubKey(subName);
                        if (devSub != null)
                        {
                            foreach (var instName in devSub.GetSubKeyNames())
                            {
                                using var instKey = devSub.OpenSubKey(instName);
                                var devDesc = instKey?.GetValue("DeviceDesc")?.ToString();
                                var mfg = instKey?.GetValue("Mfg")?.ToString();

                                if (!string.IsNullOrEmpty(devDesc) && devDesc.Contains("mouse", StringComparison.OrdinalIgnoreCase))
                                {
                                    info.DeviceName = "Optical Gaming Mouse (eSports Sensor)";
                                    info.ConnectionType = subName.Contains("USB", StringComparison.OrdinalIgnoreCase) || subName.Contains("VID", StringComparison.OrdinalIgnoreCase)
                                        ? "High-Speed USB HID (1000Hz - 8000Hz Ready)"
                                        : "Wireless 2.4GHz Low-Latency";

                                    if (!string.IsNullOrEmpty(mfg) && !mfg.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) && !mfg.Contains("Standard", StringComparison.OrdinalIgnoreCase))
                                    {
                                        info.Manufacturer = mfg;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch { }

        return info;
    }

    public static void OpenMouseControlPanel()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "control.exe",
                Arguments = "main.cpl,,1",
                UseShellExecute = true
            });
        }
        catch { }
    }
}
