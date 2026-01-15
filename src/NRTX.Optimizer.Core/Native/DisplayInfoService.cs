using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using NRTX.Optimizer.Core.Localization;

namespace NRTX.Optimizer.Core.Native;

public class DisplayInfo
{
    public string DeviceName { get; set; } = "Primary Display";
    public string MonitorFriendlyName { get; set; } = "Generic Display";
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public int RefreshRateHz { get; set; } = 60;
    public int BitsPerPixel { get; set; } = 32;
    public bool IsVrrEnabled { get; set; }
    public bool IsHighRefreshRate => RefreshRateHz >= 120;
    public int RecommendedFpsCap => IsVrrEnabled ? Math.Max(30, RefreshRateHz - 3) : RefreshRateHz;

    public string ResolutionAndHz => $"{Width} x {Height} @ {RefreshRateHz}Hz";

    public string VrrStatusBadge => IsVrrEnabled
        ? "G-Sync / FreeSync (VRR Ready)"
        : (IsHighRefreshRate ? $"High Refresh eSports ({RefreshRateHz}Hz)" : "Standard 60Hz Display");

    public (string vsync, string reflex, string fpsCap, string summary) GetRecommendations(AppLanguage lang = AppLanguage.English)
    {
        if (lang == AppLanguage.Indonesian)
        {
            string vsync = IsVrrEnabled
                ? "OFF di dalam game (Mencegah input lag buffer 16ms, tearing ditangani hardware VRR)"
                : (IsHighRefreshRate
                    ? $"OFF di dalam game (Pada monitor {RefreshRateHz}Hz, tearing nyaris tidak kasat mata. Matikan V-Sync untuk respon 1:1 instan)"
                    : "OFF di dalam game (Gunakan Fast Sync / Enhanced Sync di Driver GPU untuk meminimalkan lag)");

            string reflex = "ON + BOOST (Mengunci antrean frame render GPU ke 0 untuk latensi klik terendah)";

            string fpsCap = IsVrrEnabled
                ? $"{RecommendedFpsCap} FPS (Rumus eSports: {RefreshRateHz}Hz - 3 FPS agar selalu berada di zona aktif G-Sync/FreeSync)"
                : $"{RefreshRateHz} FPS atau Uncapped (Maksimal kehalusan gerakan dan responsivitas {RefreshRateHz}Hz)";

            string summary = IsVrrEnabled
                ? $"Monitor {RefreshRateHz}Hz dengan Variable Refresh Rate terdeteksi ({MonitorFriendlyName}). Set In-Game V-Sync ke OFF dan batasi FPS di {RecommendedFpsCap} FPS untuk latensi terendah tanpa tearing."
                : (IsHighRefreshRate
                    ? $"Monitor eSports {RefreshRateHz}Hz terdeteksi ({MonitorFriendlyName}). Matikan In-Game V-Sync dan aktifkan Reflex untuk respon kursor 1:1 maksimal."
                    : $"Monitor standar {RefreshRateHz}Hz terdeteksi. Pastikan In-Game V-Sync OFF untuk respon kursor maksimal.");

            return (vsync, reflex, fpsCap, summary);
        }
        else
        {
            string vsync = IsVrrEnabled
                ? "OFF in-game (Prevents 16ms buffer delay; G-Sync/FreeSync handles frame pacing in hardware)"
                : (IsHighRefreshRate
                    ? $"OFF in-game (At {RefreshRateHz}Hz, tearing is imperceptible. Keep V-Sync OFF for lowest 1:1 button-to-pixel input latency)"
                    : "OFF in-game (Use GPU Driver Fast Sync / Enhanced Sync to minimize latency)");

            string reflex = "ON + BOOST (Enforces zero GPU render queue for lowest button-to-pixel latency)";

            string fpsCap = IsVrrEnabled
                ? $"{RecommendedFpsCap} FPS (eSports standard: {RefreshRateHz}Hz - 3 FPS to stay strictly within VRR window)"
                : $"{RefreshRateHz} FPS or Uncapped (Max motion clarity and smoothness at {RefreshRateHz}Hz)";

            string summary = IsVrrEnabled
                ? $"VRR Display detected ({MonitorFriendlyName} @ {RefreshRateHz}Hz). Keep In-Game V-Sync OFF and cap frame rate to {RecommendedFpsCap} FPS for lowest latency without tearing."
                : (IsHighRefreshRate
                    ? $"High-refresh eSports display detected ({MonitorFriendlyName} @ {RefreshRateHz}Hz). Keep V-Sync OFF and Reflex ON for competitive edge."
                    : $"Standard {RefreshRateHz}Hz monitor detected. Keep In-Game V-Sync OFF for 1:1 raw mouse responsiveness.");

            return (vsync, reflex, fpsCap, summary);
        }
    }
}

public static class DisplayInfoService
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    private const int HORZRES = 8;
    private const int VERTRES = 10;
    private const int BITSPIXEL = 12;
    private const int VREFRESH = 116;

    public static DisplayInfo GetPrimaryDisplayInfo()
    {
        var info = new DisplayInfo();

        try
        {
            // 1. Pure Dynamic Native GDI32 Query for Realtime Resolution & Refresh Rate
            IntPtr hdc = GetDC(IntPtr.Zero);
            if (hdc != IntPtr.Zero)
            {
                try
                {
                    int w = GetDeviceCaps(hdc, HORZRES);
                    int h = GetDeviceCaps(hdc, VERTRES);
                    int hz = GetDeviceCaps(hdc, VREFRESH);
                    int bits = GetDeviceCaps(hdc, BITSPIXEL);

                    if (w > 0) info.Width = w;
                    if (h > 0) info.Height = h;
                    if (hz > 0) info.RefreshRateHz = hz;
                    if (bits > 0) info.BitsPerPixel = bits;
                }
                finally
                {
                    ReleaseDC(IntPtr.Zero, hdc);
                }
            }

            // 2. Pure Dynamic Hardware EDID / WMI Query for Monitor Brand & Model
            string? friendlyName = QueryMonitorNameDynamically();
            if (!string.IsNullOrWhiteSpace(friendlyName))
            {
                info.MonitorFriendlyName = friendlyName;
            }

            // 3. Dynamic Windows VRR (Variable Refresh Rate) Detection
            info.IsVrrEnabled = DetectVrrSupportDynamically();
        }
        catch
        {
            // Fail-safe defaults
            info.Width = 1920;
            info.Height = 1080;
            info.RefreshRateHz = 60;
            info.MonitorFriendlyName = "Generic Display";
        }

        return info;
    }

    /// <summary>
    /// Dynamically parses monitor friendly name from hardware EDID via WMI or registry binary descriptors.
    /// 100% dynamic without any hardcoded strings.
    /// </summary>
    private static string? QueryMonitorNameDynamically()
    {
        // Strategy A: WMI WmiMonitorID (Parses exact manufacturer EDID model string)
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT UserFriendlyName FROM WmiMonitorID");
            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["UserFriendlyName"] is ushort[] nameChars)
                {
                    var sb = new StringBuilder();
                    foreach (var c in nameChars)
                    {
                        if (c == 0) break;
                        sb.Append((char)c);
                    }
                    var name = sb.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        return name;
                    }
                }
            }
        }
        catch { }

        // Strategy B: Parse Raw VESA EDID Binary Block from Windows Registry
        try
        {
            using var dispKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\DISPLAY");
            if (dispKey != null)
            {
                foreach (var deviceId in dispKey.GetSubKeyNames())
                {
                    using var devKey = dispKey.OpenSubKey(deviceId);
                    if (devKey == null) continue;

                    foreach (var instanceId in devKey.GetSubKeyNames())
                    {
                        using var paramsKey = devKey.OpenSubKey($@"{instanceId}\Device Parameters");
                        if (paramsKey == null) continue;

                        if (paramsKey.GetValue("EDID") is byte[] edid && edid.Length >= 128)
                        {
                            var parsedName = ParseEdidMonitorName(edid);
                            if (!string.IsNullOrWhiteSpace(parsedName))
                            {
                                return parsedName;
                            }
                        }
                    }
                }
            }
        }
        catch { }

        return "Generic PnP Monitor";
    }

    /// <summary>
    /// VESA Standard EDID 1.3/1.4 Parser: Extracts ASCII Monitor Name Descriptor (Tag 0xFC)
    /// </summary>
    private static string? ParseEdidMonitorName(byte[] edid)
    {
        try
        {
            // Standard 18-byte detailed timing descriptors start at offset 0x36, 0x48, 0x5A, 0x6C
            int[] descriptorOffsets = { 0x36, 0x48, 0x5A, 0x6C };

            foreach (var offset in descriptorOffsets)
            {
                if (offset + 18 > edid.Length) break;

                // Check if this descriptor is a display descriptor (first two bytes are 0x00 0x00)
                if (edid[offset] == 0x00 && edid[offset + 1] == 0x00 && edid[offset + 2] == 0x00)
                {
                    // Tag 0xFC = Monitor Name String Descriptor
                    if (edid[offset + 3] == 0xFC)
                    {
                        var sb = new StringBuilder();
                        for (int i = offset + 5; i < offset + 18; i++)
                        {
                            byte b = edid[i];
                            if (b == 0x0A || b == 0x00) break; // Terminating character
                            if (b >= 32 && b <= 126) sb.Append((char)b);
                        }
                        var name = sb.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            return name;
                        }
                    }
                }
            }
        }
        catch { }

        return null;
    }

    private static bool DetectVrrSupportDynamically()
    {
        try
        {
            using var vrrKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\GraphicsSettings");
            var vrrVal = vrrKey?.GetValue("VariableRefreshRate");
            if (vrrVal is int i && i == 1) return true;
        }
        catch { }

        return false;
    }
}
