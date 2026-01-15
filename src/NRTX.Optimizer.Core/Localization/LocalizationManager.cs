using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Modules.Maintenance;

namespace NRTX.Optimizer.Core.Localization;

public enum AppLanguage
{
    English,
    Indonesian
}

public class TweakLocalizationInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string HowItWorks { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
}

public class JunkCategoryLocalizationInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public static class LocalizationManager
{
    public static AppLanguage CurrentLanguage { get; set; } = AppLanguage.English;
    public static event Action<AppLanguage>? OnLanguageChanged;

    public static void SetLanguage(AppLanguage language)
    {
        if (CurrentLanguage == language) return;
        CurrentLanguage = language;
        OnLanguageChanged?.Invoke(language);
    }

    private static readonly Dictionary<string, TweakLocalizationInfo> EnglishTweaks = new(StringComparer.OrdinalIgnoreCase)
    {
        // ==========================================
        // PRIVACY TWEAKS (10 Modules)
        // ==========================================
        ["privacy.disable_telemetry"] = new()
        {
            Name = "Disable Windows Telemetry & Data Collection",
            Description = "Sets Windows Diagnostic data collection to Security/Disabled (0), preventing background telemetry transmissions.",
            Purpose = "Stop Microsoft from collecting operating system diagnostic logs, app usage habits, and hardware telemetric data.",
            HowItWorks = "Configures HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection (AllowTelemetry = 0, MaxTelemetryAllowed = 0).",
            Impact = "Reduces background network transmissions and disk write activity."
        },
        ["privacy.disable_diagtrack_service"] = new()
        {
            Name = "Disable Connected User Experiences & Telemetry Service (DiagTrack)",
            Description = "Stops and sets DiagTrack and dmwappushservice startup type to Disabled.",
            Purpose = "Completely shuts down the background telemetry daemon services in Windows.",
            HowItWorks = "Calls sc.exe config \"DiagTrack\" start=disabled and stops running service instance.",
            Impact = "Saves ~20-50MB RAM and prevents periodic background CPU spikes."
        },
        ["privacy.disable_advertising_id"] = new()
        {
            Name = "Disable Advertising ID & Targeted Ads Tracking",
            Description = "Prevents Windows and apps from using your Advertising ID for tailored experiences and tracking.",
            Purpose = "Protects user advertising privacy across Microsoft Store applications and Edge.",
            HowItWorks = "Sets HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo (Enabled = 0) and policy DisabledByGroupPolicy = 1.",
            Impact = "Blocks cross-app advertising profiling."
        },
        ["privacy.disable_activity_history"] = new()
        {
            Name = "Disable Windows Activity History & Timeline Tracking",
            Description = "Stops Windows from tracking app history and syncing user activities to Microsoft cloud.",
            Purpose = "Eliminates local and cloud tracking of opened files, visited websites, and app timestamps.",
            HowItWorks = "Sets HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System (PublishUserActivities=0, UploadUserActivities=0, EnableActivityFeed=0).",
            Impact = "Saves disk writes and protects private file usage history."
        },
        ["privacy.disable_cortana"] = new()
        {
            Name = "Disable Cortana Background Process & Web Search",
            Description = "Disables Cortana and prevents Windows Search from querying Bing web servers for local files.",
            Purpose = "Stops local Start Menu search queries from being sent to Bing servers over the internet.",
            HowItWorks = "Sets HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\Windows Search (AllowCortana=0, DisableWebSearch=1) and BingSearchEnabled=0.",
            Impact = "Makes Start Menu search instantaneous with zero web delay and 100% offline privacy."
        },
        ["privacy.disable_feedback_prompts"] = new()
        {
            Name = "Disable Windows Feedback Surveys & Diagnostic Notifications",
            Description = "Prevents Windows from periodically popping up feedback surveys and diagnostic prompts.",
            Purpose = "Stops annoying modal dialogs asking 'How likely are you to recommend Windows?'.",
            HowItWorks = "Sets HKCU\\Software\\Microsoft\\Siuf\\Rules (NumberOfSIUFsInPeriod=0) and DoNotShowFeedbackNotifications=1.",
            Impact = "Uninterrupted workflow and gaming without survey popups."
        },
        ["privacy.disable_location_tracking"] = new()
        {
            Name = "Disable Windows Master Location Tracking Sensor",
            Description = "Disables the master location sensor service and prevents background geotracking.",
            Purpose = "Prevents apps and background services from triangulating your physical location via Wi-Fi/IP.",
            HowItWorks = "Sets HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\LocationAndSensors (DisableLocation=1).",
            Impact = "Improved battery life and location privacy."
        },
        ["privacy.disable_edge_background_bloat"] = new()
        {
            Name = "Disable Microsoft Edge Background Preloading & Startup Boost",
            Description = "Stops Microsoft Edge from running background processes on boot and keeps it closed when tabs exit.",
            Purpose = "Reclaims hundreds of megabytes of RAM stolen by Edge preloading itself silently on boot.",
            HowItWorks = "Sets HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge (BackgroundModeEnabled=0, StartupBoostEnabled=0).",
            Impact = "Frees 300MB - 800MB RAM and eliminates Edge background tasks."
        },
        ["privacy.disable_consumer_features"] = new()
        {
            Name = "Disable Windows Start Menu Sponsored Apps & Cloud Content",
            Description = "Prevents Windows from auto-installing sponsored apps, suggested store games, and third-party promotions.",
            Purpose = "Stops Windows from silently downloading Candy Crush, TikTok, and Disney+ into your Start Menu.",
            HowItWorks = "Sets HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\CloudContent (DisableWindowsConsumerFeatures=1, DisableSoftLanding=1).",
            Impact = "Keeps the Start Menu clean and reduces network background bandwidth waste."
        },
        ["privacy.disable_telemetry_scheduled_tasks"] = new()
        {
            Name = "Disable Windows Telemetry & Diagnostic Scheduled Tasks",
            Description = "Disables background scheduled tasks that gather telemetry and trigger heavy CPU disk wakeups.",
            Purpose = "Stops Microsoft Compatibility Appraiser, ProgramDataUpdater, and CEIP tasks from scanning all files on disk.",
            HowItWorks = "Executes schtasks.exe /Change /TN on CEIP Consolidator, UsbCeip, and Compatibility Appraiser.",
            Impact = "Eliminates random 100% disk usage and background CPU throttling while working or gaming."
        },

        // ==========================================
        // PERFORMANCE TWEAKS (8 Modules)
        // ==========================================
        ["perf.ultimate_power_plan"] = new()
        {
            Name = "Unlock & Activate Ultimate Performance Power Plan",
            Description = "Duplicates and activates the official Microsoft Ultimate Performance power plan, removing all CPU throttling.",
            Purpose = "Forces CPU cores to run at 100% performance state without frequency scaling delays.",
            HowItWorks = "Invokes Powrprof.dll PowerSetActiveScheme with GUID_ULTIMATE_PERFORMANCE (e9a42b02-d5df-448d-aa00-03f14749eb61).",
            Impact = "Eliminates CPU clock stuttering and delivers lowest input latency."
        },
        ["perf.win32_priority_separation"] = new()
        {
            Name = "Optimize CPU Thread Quantum & Priority Separation (0x26 Gaming Low-Latency)",
            Description = "Configures Win32PrioritySeparation to 0x26 (38 decimal), dedicating shorter variable CPU time slices with 3:1 foreground priority boost.",
            Purpose = "Gives the active foreground window/game 3x more CPU scheduler priority over all background processes.",
            HowItWorks = "Sets HKLM\\SYSTEM\\CurrentControlSet\\Control\\PriorityControl (Win32PrioritySeparation = 38 / 0x26).",
            Impact = "Significantly increases 1% Low FPS and eliminates UI micro-stutters."
        },
        ["perf.memory_trim"] = new()
        {
            Name = "Flush & Trim System Process Working Set (Instant RAM Free)",
            Description = "Uses native Psapi EmptyWorkingSet to flush idle unreferenced memory pages from all active processes.",
            Purpose = "Frees gigabytes of RAM held by background processes without closing them.",
            HowItWorks = "Iterates userland processes and executes native psapi.dll!EmptyWorkingSet.",
            Impact = "Instantly frees 1GB - 4GB of physical RAM."
        },
        ["perf.snappy_ui_effects"] = new()
        {
            Name = "Optimize Windows UI Responsiveness & Remove Menu Delay",
            Description = "Sets MenuShowDelay to 0ms and minimizes window animation latency for instant snappy navigation.",
            Purpose = "Removes the default 400ms Windows artificial delay when clicking menus and opening tooltips.",
            HowItWorks = "Sets HKCU\\Control Panel\\Desktop (MenuShowDelay = 0, HungAppTimeout = 1000).",
            Impact = "Instant, crisp window opening and navigation feedback."
        },
        ["perf.disable_hibernation"] = new()
        {
            Name = "Disable Hibernation & Delete hiberfil.sys (Free 8-32GB Storage)",
            Description = "Disables Windows Hibernation file (hiberfil.sys) reclaiming SSD space equivalent to 75-100% of RAM size.",
            Purpose = "Reclaims 8GB to 32GB+ of fast SSD drive storage space.",
            HowItWorks = "Executes powercfg.exe -h off to remove C:\\hiberfil.sys.",
            Impact = "Instantly frees huge SSD storage space and reduces SSD wear."
        },
        ["perf.disable_fast_startup_clean_boot"] = new()
        {
            Name = "Disable Windows Fast Startup & Hybrid Sleep (Fix Motherboard Beep & Display Glitches)",
            Description = "Disables Windows Fast Startup (HiberbootEnabled = 0), forcing 100% clean shutdown and clean hardware POST.",
            Purpose = "Eliminates motherboard long beep warnings, dirty RAM resume failures, and GPU display handshake timeouts on 180Hz monitors.",
            HowItWorks = "Sets HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Power (HiberbootEnabled = 0) and executes powercfg -h off.",
            Impact = "Guarantees 100% stable clean cold boot, eliminates boot beep warnings, and prevents timer/driver state corruption."
        },
        ["perf.disable_paging_executive"] = new()
        {
            Name = "Keep Kernel & Drivers Resident in RAM (Disable Paging Executive)",
            Description = "Forces Windows Kernel and Device Drivers to stay resident in physical RAM rather than paging to disk.",
            Purpose = "Prevents NT kernel drivers from being paged out to the slower pagefile on disk.",
            HowItWorks = "Sets HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management (DisablePagingExecutive = 1).",
            Impact = "Much faster kernel DPC/ISR response times and snappier multitasking."
        },
        ["perf.disable_ntfs_8dot3_and_last_access"] = new()
        {
            Name = "Disable NTFS 8.3 Name Creation & Last Access Update (SSD & NVMe Boost)",
            Description = "Disables legacy MS-DOS 8.3 short filename generation and file read timestamp updates on NTFS volumes.",
            Purpose = "Removes redundant legacy disk writes every time a file is accessed or read.",
            HowItWorks = "Sets HKLM\\SYSTEM\\CurrentControlSet\\Control\\FileSystem (NtfsDisable8dot3NameCreation=1, NtfsDisableLastAccessUpdate=1).",
            Impact = "Improves directory lookup speeds by up to 25% on folders with many files and extends SSD lifetime."
        },
        ["perf.enable_ssd_trim"] = new()
        {
            Name = "Enable Native SSD & NVMe TRIM Garbage Collection (DisableDeleteNotify 0)",
            Description = "Ensures NTFS/ReFS TRIM command pass-through is active on all solid-state drives.",
            Purpose = "Guarantees SSD flash controller receives deletion notifications so flash blocks are wiped in background.",
            HowItWorks = "Executes fsutil.exe behavior set DisableDeleteNotify 0.",
            Impact = "Prevents SSD write performance degradation over time."
        },
        ["perf.cpu_core_parking_disable"] = new()
        {
            Name = "Disable CPU Core Parking (Unpark All Intel & AMD Ryzen Cores)",
            Description = "Forces Windows power manager to keep 100% of logical CPU cores unparked and awake, preventing wake-up frame drops.",
            Purpose = "Stops CPU cores from going into sleep state during intense gaming matches, eliminating sudden FPS drops.",
            HowItWorks = "Sets PowerSettings ProcessorSubgroup 0cc5b647-c1df-4637-891a-dec35c318583 ValueMin=100 and ValueMax=100.",
            Impact = "Significantly stabilizes 1% Low FPS and eliminates frame stuttering."
        },
        ["perf.intel_cppc_speed_shift"] = new()
        {
            Name = "Optimize Intel Speed Shift & Thread Director (EPP 0 Maximum P-Core Boost)",
            Description = "Sets Energy Performance Preference (EPP) to 0 (Max Performance) and optimizes Intel P/E-Core scheduling response time.",
            Purpose = "Ensures Intel 12th/13th/14th/Ultra Gen CPUs execute foreground game threads exclusively on high-frequency P-Cores.",
            HowItWorks = "Sets PowerSettings EPP 36687f9e-e3a5-4dbf-b1dc-15eb381c6863 to 0 and Hetero Thread Policy to 0.",
            Impact = "Lowest latency CPU frequency boost and optimal core scheduling for Intel CPUs."
        },
        ["perf.amd_ryzen_cppc_x3d_boost"] = new()
        {
            Name = "Optimize AMD Ryzen CPPC & Dynamic 3D V-Cache (X3D) Core Allocation",
            Description = "Enforces CPPC preferred core order and optimizes CCD cache scheduling for lowest cross-CCD latency.",
            Purpose = "Ensures AMD Ryzen and Ryzen X3D processors automatically prioritize the 3D V-Cache CCD for competitive games.",
            HowItWorks = "Sets PowerSettings AutonomousModeGuid 8baa4a82-14c1-4477-80db-cb2192212238 to 1 with CPPC v2 active.",
            Impact = "Higher FPS in cache-sensitive games like Valorant, CS2, and Apex Legends."
        },
        ["perf.nvme_storage_msi_mode"] = new()
        {
            Name = "Enable MSI Mode & High Priority for NVMe/SATA Storage Controllers",
            Description = "Enables Message Signaled Interrupts (MSI) on NVMe, PCIe SSDs, and SATA storage controllers for faster disk I/O.",
            Purpose = "Replaces legacy line-based IRQ sharing on SSD storage controllers with high-priority direct vector CPU interrupts.",
            HowItWorks = "Sets MSISupported=1 and DevicePriority=3 in Enum\\PCI Storage Controller Device Parameters.",
            Impact = "Faster texture and asset streaming in open-world games and zero disk interrupt latency spikes."
        },
        ["perf.laptop_hybrid_gpu_high_perf"] = new()
        {
            Name = "Optimize Gaming Laptop Dual-GPU Routing & Power Throttling Bypass (Pavilion, TUF, Helios)",
            Description = "Forces DirectX High-Performance discrete GPU (dGPU) preference for games & streaming apps and bypasses ACPI power throttling on AC power.",
            Purpose = "Prevents hybrid graphics cross-adapter copy bottlenecks on gaming laptops (HP Pavilion, ASUS TUF, Acer Helios, Lenovo Legion) during gaming and streaming.",
            HowItWorks = "Sets PowerThrottlingOff=1, enables HAGS HwSchMode=2, and assigns GpuPreference=2 to streamer binaries.",
            Impact = "Eliminates 30-50% hybrid GPU frame drops and stops CPU downclocking under heavy gaming/streaming loads."
        },

        // ==========================================
        // GAMING TWEAKS (15 Modules)
        // ==========================================
        ["gaming.system_responsiveness"] = new()
        {
            Name = "Maximize Multimedia & Game CPU Priority (SystemResponsiveness 0%)",
            Description = "Removes the default 20% CPU reservation for background services, dedicating 100% of CPU cycles to the active game.",
            Purpose = "Eliminates Windows background CPU throttling reserve during gaming.",
            HowItWorks = "Sets HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile (SystemResponsiveness = 0).",
            Impact = "More consistent frame times and higher maximum FPS."
        },
        ["gaming.disable_network_throttling"] = new()
        {
            Name = "Disable Windows Network Packet Throttling Index",
            Description = "Disables Windows default network packet rate-limiting mechanism during gaming and heavy network loads.",
            Purpose = "Prevents Windows from artificially throttling non-multimedia network packets to 10,000 packets/sec.",
            HowItWorks = "Sets HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile (NetworkThrottlingIndex = 0xFFFFFFFF).",
            Impact = "Smooth, unconstrained packet flow during online multiplayer matches."
        },
        ["gaming.gpu_mmcss_priority"] = new()
        {
            Name = "Optimize MMCSS GPU Priority for Games (DirectX/Vulkan Scheduling)",
            Description = "Configures Multimedia Class Scheduler Service (MMCSS) to assign highest GPU & CPU priority (GPU Priority 8, Priority 6).",
            Purpose = "Forces Windows GPU scheduler to prioritize gaming DirectX/Vulkan contexts above all else.",
            HowItWorks = "Sets HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\\Tasks\\Games (GPU Priority=8, Priority=6, Scheduling Category=High).",
            Impact = "Drastically improves GPU frametime consistency."
        },
        ["gaming.obs_streaming_gpu_priority"] = new()
        {
            Name = "Optimize OBS Studio & Streaming GPU Priority (Zero Dropped Frames)",
            Description = "Allocates dedicated GPU rendering budget & High CPU/IO execution to OBS Studio, Streamlabs, and Discord Screen Share.",
            Purpose = "Eliminates stream encoder lag and dropped frames when games are pushing GPU utilization to 99%.",
            HowItWorks = "Sets MMCSS Tasks\\Capture (GPU Priority=8, Priority=6) and configures IFEO High Priority for obs64.exe and Streamlabs.",
            Impact = "Butter-smooth 60fps livestreaming & recordings with zero dropped frames or encoder overloads."
        },
        ["gaming.disable_game_dvr"] = new()
        {
            Name = "Disable Xbox GameDVR Background Capture & Overlay Latency",
            Description = "Disables Xbox Game Bar background recording, reducing micro-stuttering and input lag in games.",
            Purpose = "Stops background NVENC/VCE video encoding when you are not actively recording clips.",
            HowItWorks = "Sets HKCU\\System\\GameConfigStore (GameDVR_Enabled = 0) and HKLM policy AllowGameDVR = 0.",
            Impact = "Saves 5-15% GPU video engine load and removes overlay input delay."
        },
        ["gaming.fse_behavior"] = new()
        {
            Name = "Optimize Fullscreen Window Display Layer (Low-Latency DWM)",
            Description = "Configures Desktop Window Manager (DWM) for direct swapchain presentation and reduced composition delay.",
            Purpose = "Enables hardware independent flip (DirectX hardware flip model) for borderless and fullscreen games.",
            HowItWorks = "Sets HKCU\\System\\GameConfigStore (GameDVR_DXGIHonorFSEWindowsCompatible=1, GameDVR_FSEBehavior=2).",
            Impact = "Lowest possible display presentation latency similar to exclusive fullscreen."
        },
        ["gaming.disable_mpo"] = new()
        {
            Name = "Disable Multi-Plane Overlay (MPO) DWM Stutter & Flicker Fix",
            Description = "Disables Multi-Plane Overlay in DWM, fixing micro-stutters, black screens, and driver timeouts on GPU overlays.",
            Purpose = "Fixes well-known multi-monitor and hardware video overlay stutter bugs on NVIDIA & AMD graphics cards.",
            HowItWorks = "Sets HKLM\\SOFTWARE\\Microsoft\\Windows\\Dwm (OverlayTestMode = 5).",
            Impact = "Eliminates stuttering and black screen flashes when Alt-Tabbing or running Discord/OBS overlays."
        },
        ["gaming.enable_hags"] = new()
        {
            Name = "Enable Hardware-Accelerated GPU Scheduling (HAGS Low-Latency)",
            Description = "Enables HAGS, allowing modern GPUs to directly manage their VRAM scheduling for reduced input lag.",
            Purpose = "Allows GPU dedicated scheduling processor to manage frame queues instead of CPU context switching.",
            HowItWorks = "Sets HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers (HwSchMode = 2).",
            Impact = "Lower input lag and enables DLSS 3 Frame Generation support on RTX 40/50 series."
        },
        ["gaming.raw_mouse_input"] = new()
        {
            Name = "Disable Windows Mouse Acceleration & Enhance Pointer Precision (1:1 Raw Input)",
            Description = "Disables Windows cursor acceleration curve and sets mouse speed to true 1:1 linear mapping for pixel-perfect FPS aim.",
            Purpose = "Eliminates variable cursor distance acceleration, giving 100% consistent muscle memory aim in Valorant, CS2, and Apex Legends.",
            HowItWorks = "Sets HKCU\\Control Panel\\Mouse (MouseSpeed=0, MouseThreshold1=0, MouseThreshold2=0, SmoothLinearCurve).",
            Impact = "Perfect 1:1 mouse tracking with zero OS acceleration."
        },
        ["gaming.timer_resolution_low_latency"] = new()
        {
            Name = "Optimize High-Precision Timer Resolution & Force Invariant TSC (0.5ms Clock)",
            Description = "Configures dynamic tick and invariant TSC clock policies, reducing micro-stutters and timer drift in competitive games.",
            Purpose = "Forces Windows system timer to maximum precision (0.5ms) without synthetic clock jitter for Riot Vanguard / Easy Anti-Cheat.",
            HowItWorks = "Executes bcdedit /set disabledynamictick yes, useplatformclock false, tscsyncpolicy Enhanced and sets GlobalTimerResolutionRequests=1.",
            Impact = "Lowest input polling delay and ultra-smooth frame pacing."
        },
        ["gaming.nvidia_driver_power_latency"] = new()
        {
            Name = "Optimize NVIDIA GeForce GTX/RTX Driver Latency & Disable Telemetry",
            Description = "Disables NvTelemetryContainer background daemons and configures driver power state to Prefer Maximum Performance.",
            Purpose = "Stops NVIDIA driver telemetry spikes and locks GPU clocks in D3D state without downclocking delays.",
            HowItWorks = "Stops NvTelemetryContainer service and sets EnablePreemption=1 in GraphicsDrivers Scheduler.",
            Impact = "Eliminates random driver DPC latency spikes during matches."
        },
        ["gaming.nvidia_rtx_reflex_queue"] = new()
        {
            Name = "Optimize NVIDIA Shader Cache Limit (10GB) & Direct Flip Queue Presentation",
            Description = "Expands DirectX/Vulkan shader cache size to 10GB, eliminating in-game shader compilation stuttering on GTX & RTX cards.",
            Purpose = "Prevents shader re-compilation stutters in Apex Legends, Warzone, and CS2 by expanding disk shader cache to 10GB.",
            HowItWorks = "Sets HKLM\\SOFTWARE\\NVIDIA Corporation\\Global (MaxShaderCacheSize=10240, PreRenderedFrames=1).",
            Impact = "Completely eliminates shader-caching frame drops during fights."
        },
        ["gaming.amd_radeon_anti_lag_ulps"] = new()
        {
            Name = "Disable AMD Radeon Ultra Low Power State (ULPS) & Anti-Lag Boost",
            Description = "Disables ULPS in AMD Radeon display driver registry, preventing sudden core clock downthrottling during intensive FPS matches.",
            Purpose = "Prevents AMD Radeon GPUs from putting secondary compute units or clocks to sleep mid-game.",
            HowItWorks = "Sets EnableUlps=0 and EnableUlps_NA=0 across video driver class entries in Registry.",
            Impact = "Rock-solid GPU clock stability on AMD Radeon RX 5000/6000/7000 series."
        },
        ["gaming.disable_hyperv_hypervisor"] = new()
        {
            Name = "Disable Hyper-V Hypervisor Launch (Bare-Metal CPU Gaming Mode)",
            Description = "Sets hypervisorlaunchtype off via BCDEdit, removing Type-1 hypervisor overhead for lowest DPC latency.",
            Purpose = "Unloads the Windows Type-1 hypervisor kernel so CPU instructions talk bare-metal directly to silicon.",
            HowItWorks = "Executes bcdedit.exe /set hypervisorlaunchtype off.",
            Impact = "Reduces CPU DPC latency spikes to the absolute minimum for competitive eSports."
        },
        ["gaming.disable_vbs_hvci"] = new()
        {
            Name = "Disable Virtualization-Based Security (VBS) & Memory Integrity (HVCI)",
            Description = "Disables VBS and HVCI in Windows Kernel, eliminating CPU virtualization overhead in games.",
            Purpose = "Removes the CPU penalty caused by continuous kernel code integrity virtualization checks.",
            HowItWorks = "Sets HKLM\\SYSTEM\\CurrentControlSet\\Control\\DeviceGuard (EnableVirtualizationBasedSecurity=0, HypervisorEnforcedCodeIntegrity\\Enabled=0).",
            Impact = "Increases gaming performance and 1% low FPS by 5% to 15% on Intel & AMD Ryzen CPUs."
        },
        ["gaming.gpu_msi_mode"] = new()
        {
            Name = "Enable MSI Mode (Message Signaled Interrupts) & High Priority on GPU",
            Description = "Switches GPU hardware interrupt handling from legacy line-based IRQ to modern Message Signaled Interrupts (MSI) with High Priority.",
            Purpose = "Eliminates shared IRQ conflicts and audio crackling, delivering lowest possible GPU interrupt latency for GeForce & Radeon.",
            HowItWorks = "Sets MSISupported=1 and DevicePriority=3 in Enum\\PCI Video Class Device Parameters.",
            Impact = "Completely eliminates micro-stutters and DPC latency spikes caused by graphics driver IRQ queue delays."
        },
        ["gaming.audio_exclusive_latency"] = new()
        {
            Name = "Ultra-Low Audio Latency & MMCSS Pro Audio Tuning (Footstep Emas)",
            Description = "Configures MMCSS Audio and Pro Audio scheduler to maximum priority (Priority 6, Scheduling High, Clock Rate 10000) and disables audio power throttling.",
            Purpose = "Delivers sub-millisecond audio response and eliminates DPC crackle when game sounds and Discord audio overlap.",
            HowItWorks = "Configures HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\\Tasks (Audio & Pro Audio).",
            Impact = "In-game footstep audio arrives 5-10ms faster with crystal clear spatial clarity."
        },
        ["gaming.mouse_raw_input_1to1"] = new()
        {
            Name = "1:1 Raw Mouse Sensor Input & Disable Windows Acceleration",
            Description = "Enforces true 1:1 pixel mapping by disabling Windows cursor acceleration (MouseSpeed=0, MouseThreshold1/2=0) and setting pointer speed to 6/11 default notch.",
            Purpose = "Guarantees linear cursor tracking with zero acceleration curves for tournament-consistent flick shots in Valorant, CS2, and Apex Legends.",
            HowItWorks = "Configures HKCU\\Control Panel\\Mouse (MouseSpeed=0, MouseThreshold1=0, MouseThreshold2=0, MouseSensitivity=10).",
            Impact = "100% linear muscle memory aim consistency with zero acceleration."
        },
        ["gaming.mouse_markc_acceleration_fix"] = new()
        {
            Name = "MarkC Windows 11 Mouse Acceleration Linear Curve Fix (100% Scaling)",
            Description = "Applies the legendary MarkC Windows Mouse Fix binary curves (SmoothMouseXCurve and SmoothMouseYCurve) calibrated for 100% desktop DPI scaling.",
            Purpose = "Eliminates legacy Windows kernel cursor acceleration table distortions that interfere with low-sens mouse flicks.",
            HowItWorks = "Injects calibrated 1:1 linear curve binary tables into HKCU\\Control Panel\\Mouse (SmoothMouseXCurve & SmoothMouseYCurve).",
            Impact = "Ultra-smooth 1:1 count-to-pixel scaling with zero curve warping."
        },
        ["gaming.mouse_hid_queue_buffer_tuning"] = new()
        {
            Name = "Expand Mouse Driver HID Data Queue Buffer (1000Hz - 8000Hz Anti-Packet Drop)",
            Description = "Expands the mouclass kernel driver MouseDataQueueSize buffer from 100 to 128 packets, preventing buffer overflow during ultra-fast mouse flicks.",
            Purpose = "Prevents USB HID buffer overflow when playing on high-polling rate sensors (1000Hz, 2000Hz, 4000Hz, 8000Hz).",
            HowItWorks = "Sets HKLM\\SYSTEM\\CurrentControlSet\\Services\\mouclass\\Parameters\\MouseDataQueueSize to 128 packets.",
            Impact = "Zero micro-stutters and zero packet loss during intense 180° flick turns."
        },
        ["gaming.mouse_usb_power_throttling_disable"] = new()
        {
            Name = "Disable USB Power Throttling & Selective Suspend for Gaming Mice",
            Description = "Disables Windows USB selective suspend and power throttling, keeping USB controller and optical sensors continuously active at full polling rate.",
            Purpose = "Prevents mouse optical sensor from entering low-power micro-sleep while holding angles in Valorant or CS2.",
            HowItWorks = "Sets PowerThrottlingOff=1 and disables USB Selective Suspend via powercfg on active power scheme.",
            Impact = "Instant sensor responsiveness with zero wake-up micro-delay."
        },

        // ==========================================
        // NETWORK TWEAKS (8 Modules)
        // ==========================================
        ["network.tcp_nodelay_ack"] = new()
        {
            Name = "Disable Nagle's Algorithm & TCP Delayed ACKs (Lowest Latency Ping)",
            Description = "Configures TCPNoDelay=1 and TcpAckFrequency=1 on network adapters to eliminate artificial packet delay buffers.",
            Purpose = "Forces TCP packets and ACKs to be transmitted instantly without buffering small chunks.",
            HowItWorks = "Sets TCPNoDelay=1, TcpAckFrequency=1, TcpDelAckTicks=0 across active network interfaces in Registry.",
            Impact = "Dramatically reduces in-game multiplayer ping and eliminates rubberbanding packet delay."
        },
        ["network.tcp_autotuning_heuristic"] = new()
        {
            Name = "Optimize TCP Window Auto-Tuning & Disable Chimney Scaling",
            Description = "Tunes Windows TCP stack for high throughput and consistent packet streaming, disabling legacy heuristics.",
            Purpose = "Allows TCP receive window to dynamically scale to max fiber-optic bandwidth speeds.",
            HowItWorks = "Executes netsh int tcp set global autotuninglevel=normal, rss=enabled, heuristics disabled.",
            Impact = "Max download/upload speeds and lower packet loss."
        },
        ["network.tcp_congestion_provider"] = new()
        {
            Name = "Configure Modern Low-Latency TCP Congestion Provider (CTCP / CUBIC)",
            Description = "Sets Windows supplemental TCP congestion control algorithm to CTCP / CUBIC for faster throughput ramp-up.",
            Purpose = "Replaces legacy TCP congestion algorithms with modern algorithms optimized for low-latency connections.",
            HowItWorks = "Executes netsh int tcp set supplemental template=custom congestionprovider=ctcp / cubic.",
            Impact = "Faster connection establishment and smoother live streaming/gaming packets."
        },
        ["network.fastroute_qos_dscp46"] = new()
        {
            Name = "TROY FastRoute: Game Packet QoS & DSCP 46 Expedited Forwarding (ExitLag Tech)",
            Description = "Tags game network packets with DSCP 46 (Expedited Forwarding) and unlocks Windows QoS traffic shaping for Valorant, CS2, Apex, and Fortnite.",
            Purpose = "Enforces VIP packet scheduling at the Windows kernel, router, and ISP level so game packets never wait behind downloads.",
            HowItWorks = "Creates Windows QoS policies with DSCP 46 and configures qWave & pacer.sys traffic shaping.",
            Impact = "Zero queue delay, prioritized game packets, and competitive routing advantages."
        },
        ["network.fastroute_anti_jitter_pacing"] = new()
        {
            Name = "TROY FastRoute: Anti-Jitter UDP Packet Pacing & Bufferbloat Fix",
            Description = "Optimizes UDP/TCP network socket buffers, expands user port limits to 65,534, and enables NonSack RTT resiliency.",
            Purpose = "Eliminates network bufferbloat, ping jitter spikes, and sudden packet bursts in intensive firefights.",
            HowItWorks = "Configures MaxUserPort=65534, TcpTimedWaitDelay=30, and NonSackRttResiliency=1 in Tcpip & Pacer.",
            Impact = "Ultra-consistent sub-millisecond packet pacing with 0% packet loss."
        },
        ["network.streaming_qos_pacing"] = new()
        {
            Name = "Optimize Live Streaming RTMP/SRT/Discord Network Pacing (Zero Bitrate Drops)",
            Description = "Tunes TCP socket resiliency, expands ephemeral port range (65534 ports), and eliminates packet bursts for crystal clear Twitch/YouTube streaming.",
            Purpose = "Prevents RTMP ingest buffer overflows and eliminates sudden bitrate drops during live broadcasts.",
            HowItWorks = "Sets NonSackRttResiliency=1, MaxUserPort=65534, and TcpTimedWaitDelay=30 in Tcpip Parameters.",
            Impact = "Rock-solid livestream bitrate stability and zero stream packet loss without increasing in-game ping."
        },
        ["network.disable_lso"] = new()
        {
            Name = "Disable Large Send Offload (LSO) & Hardware Checksum Delay",
            Description = "Disables network packet segmentation offloading (LSO), preventing buffer spikes in competitive games.",
            Purpose = "Prevents buggy network card drivers from holding and batching packets on NIC hardware.",
            HowItWorks = "Sets DisableTaskOffload=1 in Tcpip Parameters and disables *LSOv2 on network adapter drivers.",
            Impact = "Eliminates sudden micro-ping spikes during intensive multiplayer matches."
        },
        ["network.flush_dns"] = new()
        {
            Name = "Flush Windows DNS Resolver Cache",
            Description = "Purges local DNS resolver cache using native Win32 DnsFlushResolverCache API.",
            Purpose = "Clears expired, stale, or corrupted domain name mappings.",
            HowItWorks = "Calls native dnsapi.dll!DnsFlushResolverCache().",
            Impact = "Instantly fixes domain resolution errors and web page load stalls."
        },
        ["network.disable_netbios"] = new()
        {
            Name = "Disable NetBIOS over TCP/IP (Reduce LAN Broadcasts)",
            Description = "Disables legacy NetBIOS name query broadcast traffic across network adapters.",
            Purpose = "Stops unneeded LAN broadcast chatter and closes legacy NetBIOS attack vectors.",
            HowItWorks = "Sets NetbiosOptions = 2 across all NetBT network interfaces.",
            Impact = "Reduced network overhead and enhanced LAN security."
        },

        // ==========================================
        // SERVICES & DEBLOAT & MAINTENANCE (4 Modules)
        // ==========================================
        ["services.optimize_background_junk"] = new()
        {
            Name = "Disable Unnecessary Background Bloat Services (Maps, RetailDemo, WER)",
            Description = "Disables non-essential background services (MapsBroker, RetailDemo, WerSvc, wisvc, TroubleshootingSvc).",
            Purpose = "Stops unused background daemons from waking up the CPU and consuming RAM.",
            HowItWorks = "Configures service start types to Disabled via sc.exe config.",
            Impact = "Frees 50-150MB RAM and reduces background CPU context switching."
        },
        ["debloater.uwp_bloatware"] = new()
        {
            Name = "Remove Pre-installed Windows 10/11 UWP Bloatware Apps",
            Description = "Safely removes pre-installed sponsored junk UWP apps while strictly preserving Store, Terminal, & Calculator.",
            Purpose = "Removes Bing News, Clipchamp, Solitaire, Feedback Hub, Skype, and other OEM junk.",
            HowItWorks = "Executes PowerShell Remove-AppxPackage and Remove-AppxProvisionedPackage.",
            Impact = "Reclaims 1GB - 3GB disk space and cleans Start Menu."
        },
        ["maintenance.clean_temp_files"] = new()
        {
            Name = "Clean User & System Temporary Cache Files",
            Description = "Purges leftover cache files in %TEMP%, C:\\Windows\\Temp, CrashDumps, and system thumbnail caches.",
            Purpose = "Deletes orphan installer files, crash logs, and temporary app junk.",
            HowItWorks = "Iterates and safely purges target directories while catching locked file handles.",
            Impact = "Reclaims 500MB - 10GB+ of storage space."
        },
        ["maintenance.clean_windows_update_cache"] = new()
        {
            Name = "Clean Windows Update Download Cache (SoftwareDistribution)",
            Description = "Safely stops wuauserv, purges downloaded update installer leftovers, and restarts update service.",
            Purpose = "Deletes leftover downloaded cumulative update files after Windows has installed them.",
            HowItWorks = "Stops wuauserv/bits, purges C:\\Windows\\SoftwareDistribution\\Download, and restarts service.",
            Impact = "Frees 2GB - 15GB of disk space on drive C:."
        }
    };

    private static readonly Dictionary<string, TweakLocalizationInfo> IndonesianTweaks = new(StringComparer.OrdinalIgnoreCase)
    {
        // ==========================================
        // PRIVACY TWEAKS (10 Modules)
        // ==========================================
        ["privacy.disable_telemetry"] = new()
        {
            Name = "Nonaktifkan Telemetri & Pengumpulan Data Windows",
            Description = "Menyetel pengumpulan data diagnostik Windows ke Tingkat Keamanan/Mati (0), menghentikan pengiriman data berkala.",
            Purpose = "Menghentikan Microsoft mengumpulkan log diagnostik, riwayat penggunaan aplikasi, dan data telemetri perangkat.",
            HowItWorks = "Mengubah HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection (AllowTelemetry = 0, MaxTelemetryAllowed = 0).",
            Impact = "Menghemat bandwidth internet latar belakang dan mengurangi aktivitas tulis harddisk/SSD."
        },
        ["privacy.disable_diagtrack_service"] = new()
        {
            Name = "Nonaktifkan Layanan Telemetri & Diagnostik (DiagTrack)",
            Description = "Menghentikan dan mematikan startup service DiagTrack dan dmwappushservice secara permanen.",
            Purpose = "Mematikan daemon service pelacak telemetri utama di sistem operasi Windows.",
            HowItWorks = "Menjalankan sc.exe config \"DiagTrack\" start=disabled dan menghentikan proses service aktif.",
            Impact = "Menghemat ~20-50MB RAM dan menghilangkan lonjakan CPU latar belakang."
        },
        ["privacy.disable_advertising_id"] = new()
        {
            Name = "Nonaktifkan ID Iklan & Pelacakan Profil Iklan",
            Description = "Mencegah Windows dan aplikasi menggunakan Advertising ID untuk pelacakan dan penargetan iklan.",
            Purpose = "Menjaga privasi aktivitas pengguna dari profil iklan Microsoft Store & Edge.",
            HowItWorks = "Menyetel HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo (Enabled = 0) dan DisabledByGroupPolicy = 1.",
            Impact = "Mencegah pembuatan profil penargetan iklan lintas aplikasi."
        },
        ["privacy.disable_activity_history"] = new()
        {
            Name = "Nonaktifkan Riwayat Aktivitas & Timeline Windows",
            Description = "Menghentikan pencatatan riwayat aplikasi dan sinkronisasi aktivitas pengguna ke cloud Microsoft.",
            Purpose = "Menghapus pencatatan lokal dan cloud atas file yang dibuka, web yang dikunjungi, dan timeline aktivitas.",
            HowItWorks = "Menyetel HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System (PublishUserActivities=0, UploadUserActivities=0, EnableActivityFeed=0).",
            Impact = "Menghemat penulisan disk dan melindungi privasi riwayat pembukaan file pribadi."
        },
        ["privacy.disable_cortana"] = new()
        {
            Name = "Nonaktifkan Proses Latar Belakang Cortana & Pencarian Bing",
            Description = "Mematikan Cortana dan mencegah pencarian Start Menu mengirim kueri file lokal ke server Bing.",
            Purpose = "Mencegah teks yang kamu ketik di Start Menu terkirim ke internet melalui server web Bing.",
            HowItWorks = "Menyetel HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\Windows Search (AllowCortana=0, DisableWebSearch=1) dan BingSearchEnabled=0.",
            Impact = "Pencarian Start Menu menjadi instan tanpa delay web dan 100% offline privat."
        },
        ["privacy.disable_feedback_prompts"] = new()
        {
            Name = "Nonaktifkan Notifikasi Survei & Feedback Windows",
            Description = "Mencegah Windows memunculkan pop-up survei kepuasan dan notifikasi diagnostik berkala.",
            Purpose = "Menghilangkan pop-up menyebalkan 'Seberapa besar kemungkinan Anda merekomendasikan Windows?'.",
            HowItWorks = "Menyetel HKCU\\Software\\Microsoft\\Siuf\\Rules (NumberOfSIUFsInPeriod=0) dan DoNotShowFeedbackNotifications=1.",
            Impact = "Bekerja dan bermain game dengan tenang tanpa gangguan pop-up survei."
        },
        ["privacy.disable_location_tracking"] = new()
        {
            Name = "Nonaktifkan Sensor Pelacakan Lokasi Master Windows",
            Description = "Mematikan sensor lokasi master dan mencegah pelacakan posisi geografis di latar belakang.",
            Purpose = "Mencegah aplikasi dan background services melacak koordinat fisik kamu via Wi-Fi/IP.",
            HowItWorks = "Menyetel HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\LocationAndSensors (DisableLocation=1).",
            Impact = "Meningkatkan daya tahan baterai dan privasi lokasi fisik."
        },
        ["privacy.disable_edge_background_bloat"] = new()
        {
            Name = "Nonaktifkan Preload & Startup Boost Latar Belakang Microsoft Edge",
            Description = "Menghentikan proses Edge berjalan otomatis saat boot dan mematikannya saat seluruh tab ditutup.",
            Purpose = "Mengambil kembali ratusan megabyte RAM yang diam-diam dikonsumsi Edge sejak komputer menyala.",
            HowItWorks = "Menyetel HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge (BackgroundModeEnabled=0, StartupBoostEnabled=0).",
            Impact = "Membebaskan 300MB - 800MB RAM dan melenyapkan proses background Edge."
        },
        ["privacy.disable_consumer_features"] = new()
        {
            Name = "Nonaktifkan Aplikasi Sponsor & Konten Promosi Start Menu",
            Description = "Mencegah Windows menginstal otomatis aplikasi sponsor, game promo, dan konten iklan pihak ketiga.",
            Purpose = "Mencegah Windows diam-diam mendownload Candy Crush, TikTok, dan Disney+ ke Start Menu.",
            HowItWorks = "Menyetel HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\CloudContent (DisableWindowsConsumerFeatures=1, DisableSoftLanding=1).",
            Impact = "Start Menu tetap bersih dan menghemat kuota bandwidth internet latar belakang."
        },
        ["privacy.disable_telemetry_scheduled_tasks"] = new()
        {
            Name = "Nonaktifkan Jadwal Tugas Telemetri & Diagnostik (Scheduled Tasks)",
            Description = "Mematikan scheduled task latar belakang yang memicu lonjakan scan harddisk dan pemakaian CPU.",
            Purpose = "Menghentikan task Microsoft Compatibility Appraiser, ProgramDataUpdater, dan CEIP dari scanning file terus-menerus.",
            HowItWorks = "Menjalankan schtasks.exe /Change /TN untuk menonaktifkan task CEIP Consolidator, UsbCeip, dan Compatibility Appraiser.",
            Impact = "Menghilangkan fenomena 100% Disk Usage tiba-tiba dan stuttering CPU saat bermain game."
        },

        // ==========================================
        // PERFORMANCE TWEAKS (8 Modules)
        // ==========================================
        ["perf.ultimate_power_plan"] = new()
        {
            Name = "Buka & Aktifkan Skema Daya Ultimate Performance",
            Description = "Menduplikasi dan mengaktifkan profil resmi Microsoft Ultimate Performance tanpa pembatasan daya CPU.",
            Purpose = "Memaksa seluruh core prosesor bekerja pada performa 100% tanpa delay penurunan frekuensi daya.",
            HowItWorks = "Memanggil Powrprof.dll PowerSetActiveScheme dengan GUID_ULTIMATE_PERFORMANCE.",
            Impact = "Menghilangkan stutter clock CPU dan memberikan input latency terendah."
        },
        ["perf.win32_priority_separation"] = new()
        {
            Name = "Optimalkan Pembagian Quantum CPU (0x26 Gaming Low-Latency)",
            Description = "Mengatur Win32PrioritySeparation ke 0x26 (38 desimal), memberikan quantum CPU pendek dengan 3:1 foreground priority.",
            Purpose = "Memberikan aplikasi/game aktif di depan layar prioritas penjadwalan CPU 3x lebih besar dibanding background tasks.",
            HowItWorks = "Menyetel HKLM\\SYSTEM\\CurrentControlSet\\Control\\PriorityControl (Win32PrioritySeparation = 38 / 0x26).",
            Impact = "Meningkatkan 1% Low FPS secara drastis dan menghilangkan stutter micro pada game kompetitif."
        },
        ["perf.memory_trim"] = new()
        {
            Name = "Bersihkan Working Set Memori Proses (Bebaskan RAM Instan)",
            Description = "Menggunakan Win32 Psapi EmptyWorkingSet native untuk melepaskan halaman RAM nganggur dari seluruh proses.",
            Purpose = "Membebaskan memori RAM yang ditahan aplikasi latar belakang tanpa harus menutup aplikasinya.",
            HowItWorks = "Mengiterasi proses sistem dan memanggil native psapi.dll!EmptyWorkingSet.",
            Impact = "Membebaskan 1GB - 4GB RAM fisik secara instan."
        },
        ["perf.snappy_ui_effects"] = new()
        {
            Name = "Optimalkan Responsivitas UI & Hilangkan Delay Menu Windows",
            Description = "Mengatur MenuShowDelay ke 0ms dan mempercepat animasi jendela untuk navigasi yang responsif.",
            Purpose = "Menghilangkan jeda buatan default Windows sebesar 400ms saat mengklik menu dan membuka jendela.",
            HowItWorks = "Menyetel HKCU\\Control Panel\\Desktop (MenuShowDelay = 0, HungAppTimeout = 1000).",
            Impact = "Membuka jendela dan navigasi menu terasa sangat cepat dan instan."
        },
        ["perf.disable_hibernation"] = new()
        {
            Name = "Nonaktifkan Hibernasi & Hapus hiberfil.sys (Bebaskan 8-32GB Storage)",
            Description = "Mematikan file hibernasi Windows (hiberfil.sys) untuk merebut kembali ruang SSD sebesar kapasitas RAM.",
            Purpose = "Mengosongkan ruang penyimpanan SSD sebesar 8GB hingga 32GB+ yang tidak terpakai.",
            HowItWorks = "Menjalankan powercfg.exe -h off untuk menghapus file C:\\hiberfil.sys.",
            Impact = "Membebaskan ruang SSD berukuran besar dan mengurangi keausan penulisan SSD."
        },
        ["perf.disable_fast_startup_clean_boot"] = new()
        {
            Name = "Nonaktifkan Windows Fast Startup & Hybrid Sleep (Fix Bunyi Beep Motherboard & Display Glitch)",
            Description = "Mematikan Fast Startup Windows (HiberbootEnabled = 0), memaksa proses shutdown dan POST hardware BIOS bersih 100%.",
            Purpose = "Melenyapkan peringatan bunyi beep panjang motherboard, kegagalan resume RAM, dan timeout sinyal display pada monitor 180Hz.",
            HowItWorks = "Menyetel HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Power (HiberbootEnabled = 0) dan menjalankan powercfg -h off.",
            Impact = "Menjamin cold boot 100% stabil, menghilangkan bunyi beep peringatan boot, dan mencegah korupsi state driver/timer."
        },
        ["perf.disable_paging_executive"] = new()
        {
            Name = "Pertahankan Kernel & Driver di RAM Fisik (Disable Paging Executive)",
            Description = "Memaksa Kernel Windows dan Device Drivers tetap berada di RAM fisik dan tidak dilempar ke disk.",
            Purpose = "Mencegah driver inti sistem operasi dipindahkan ke pagefile harddisk yang lebih lambat.",
            HowItWorks = "Menyetel HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management (DisablePagingExecutive = 1).",
            Impact = "Responsivitas kernel DPC/ISR menjadi jauh lebih cepat saat multitasking berat."
        },
        ["perf.disable_ntfs_8dot3_and_last_access"] = new()
        {
            Name = "Nonaktifkan Nama File 8.3 & Update Waktu Akses NTFS (SSD & NVMe Boost)",
            Description = "Mematikan pembuatan nama file lawas MS-DOS 8.3 dan pencatatan timestamp akses file pada partisi NTFS.",
            Purpose = "Menghilangkan operasi tulis disk redundant setiap kali file dibaca atau dibuka.",
            HowItWorks = "Menyetel HKLM\\SYSTEM\\CurrentControlSet\\Control\\FileSystem (NtfsDisable8dot3NameCreation=1, NtfsDisableLastAccessUpdate=1).",
            Impact = "Meningkatkan kecepatan pembacaan folder hingga 25% dan memperpanjang umur SSD."
        },
        ["perf.enable_ssd_trim"] = new()
        {
            Name = "Aktifkan Perintah Native SSD/NVMe TRIM (DisableDeleteNotify 0)",
            Description = "Memastikan penerusan perintah TRIM pada partisi NTFS/ReFS aktif di seluruh drive SSD.",
            Purpose = "Menjamin pengontrol memori flash SSD membersihkan blok data terhapus di latar belakang.",
            HowItWorks = "Menjalankan fsutil.exe behavior set DisableDeleteNotify 0.",
            Impact = "Mencegah penurunan kecepatan tulis SSD seiring berjalannya waktu."
        },
        ["perf.cpu_core_parking_disable"] = new()
        {
            Name = "Nonaktifkan CPU Core Parking (Buka Kunci 100% Core Intel & AMD Ryzen)",
            Description = "Memaksa Windows menjaga 100% core CPU selalu siaga dan aktif, mencegah drop frame akibat delay wake-up core.",
            Purpose = "Mencegah core prosesor masuk mode tidur (sleep) saat bermain game, melenyapkan micro-stuttering tiba-tiba.",
            HowItWorks = "Menyetel registri PowerSettings 0cc5b647-c1df-4637-891a-dec35c318583 ValueMin=100 dan ValueMax=100.",
            Impact = "Meningkatkan kestabilan 1% Low FPS secara drastis pada monitor refresh rate tinggi (144Hz/240Hz/360Hz)."
        },
        ["perf.intel_cppc_speed_shift"] = new()
        {
            Name = "Optimalkan Intel Speed Shift & Thread Director (EPP 0 Prioritas P-Core)",
            Description = "Mengatur Energy Performance Preference (EPP) ke 0 (Max Performance) dan mengoptimalkan respon penjadwalan P-Core/E-Core.",
            Purpose = "Memastikan prosesor Intel Gen 12/13/14/Ultra menjalankan thread game aktif khusus di Performance Cores berkecepatan tertinggi.",
            HowItWorks = "Menyetel registri EPP ke 0 dan kebijakan penjadwalan heterogen ke prioritas core performa tinggi.",
            Impact = "Peningkatan clock boost CPU instan tanpa delay dan alokasi core optimal untuk prosesor Intel."
        },
        ["perf.amd_ryzen_cppc_x3d_boost"] = new()
        {
            Name = "Optimalkan AMD Ryzen CPPC & Dynamic 3D V-Cache (X3D) Core Allocation",
            Description = "Menegakkan urutan core prioritas CPPC dan optimasi cache CCD untuk latensi transfer antar-core terendah.",
            Purpose = "Memastikan prosesor AMD Ryzen dan Ryzen X3D otomatis memprioritaskan CCD 3D V-Cache untuk game kompetitif.",
            HowItWorks = "Mengaktifkan CPPC Autonomous Mode v2 pada skema daya Windows.",
            Impact = "Meningkatkan FPS game yang sensitif terhadap cache seperti Valorant, CS2, dan Apex Legends."
        },
        ["perf.nvme_storage_msi_mode"] = new()
        {
            Name = "Aktifkan Mode MSI & Prioritas Tinggi untuk Pengontrol Penyimpanan NVMe/SATA",
            Description = "Mengaktifkan Message Signaled Interrupts (MSI) pada controller NVMe, SSD PCIe, dan SATA untuk akses disk lebih cepat.",
            Purpose = "Menggantikan antrean IRQ sharing lambat pada storage controller dengan interupsi CPU vektor langsung berprioritas tinggi.",
            HowItWorks = "Menyetel MSISupported=1 dan DevicePriority=3 pada Device Parameters Pengontrol Penyimpanan di Registry.",
            Impact = "Streaming aset game open-world lebih cepat dan melenyapkan spike latensi interupsi disk."
        },
        ["perf.laptop_hybrid_gpu_high_perf"] = new()
        {
            Name = "Optimasi Routing Dual-GPU & Bypass Throttling Laptop Gaming (Pavilion, TUF, Helios)",
            Description = "Memaksa preferensi DirectX Discrete GPU (dGPU) performa tinggi untuk game & aplikasi streaming serta mematikan Power Throttling saat dicolok charger.",
            Purpose = "Mencegah bottleneck penyalinan antar-adapter (cross-adapter copy) pada laptop gaming (HP Pavilion, ASUS TUF, Acer Helios, Lenovo Legion) saat main game dan streaming.",
            HowItWorks = "Menyetel PowerThrottlingOff=1, mengaktifkan HAGS HwSchMode=2, dan menetapkan GpuPreference=2 pada proses streaming.",
            Impact = "Melenyapkan 30-50% frame drop hybrid GPU dan mencegah penurunan clock CPU saat gaming & streaming berat."
        },

        // ==========================================
        // GAMING TWEAKS (15 Modules)
        // ==========================================
        ["gaming.system_responsiveness"] = new()
        {
            Name = "Prioritas CPU Multimedia & Game Maksimum (SystemResponsiveness 0%)",
            Description = "Menghapus alokasi cadangan 20% CPU untuk background tasks, memberikan 100% siklus CPU ke game aktif.",
            Purpose = "Menghilangkan pembatasan CPU default Windows saat menjalankan game atau aplikasi multimedia berat.",
            HowItWorks = "Menyetel HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile (SystemResponsiveness = 0).",
            Impact = "Frametime game lebih konsisten dan framerate maksimum lebih stabil."
        },
        ["gaming.disable_network_throttling"] = new()
        {
            Name = "Nonaktifkan Pembatasan Paket Jaringan (Network Throttling Index)",
            Description = "Mematikan mekanisme pembatasan laju paket jaringan default Windows saat bermain game.",
            Purpose = "Mencegah Windows membatasi paket jaringan non-multimedia menjadi maksimal 10.000 paket/detik.",
            HowItWorks = "Menyetel HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile (NetworkThrottlingIndex = 0xFFFFFFFF).",
            Impact = "Aliran paket data game online menjadi lancar tanpa batasan buatan sistem operasi."
        },
        ["gaming.gpu_mmcss_priority"] = new()
        {
            Name = "Tingkatkan Prioritas GPU MMCSS Game (DirectX/Vulkan Scheduling)",
            Description = "Mengonfigurasi MMCSS untuk menetapkan prioritas penjadwalan GPU & CPU tertinggi (GPU Priority 8, Priority 6).",
            Purpose = "Memaksa scheduler GPU Windows memprioritaskan konteks DirectX/Vulkan game di atas aplikasi lain.",
            HowItWorks = "Menyetel registry Tasks\\Games (GPU Priority=8, Priority=6, Scheduling Category=High).",
            Impact = "Meningkatkan konsistensi render grafik GPU dan kestabilan frametime."
        },
        ["gaming.obs_streaming_gpu_priority"] = new()
        {
            Name = "Optimalkan Prioritas GPU OBS Studio & Live Streaming (Bebas Drop Frame)",
            Description = "Mengalokasikan anggaran render GPU khusus dan eksekusi CPU/IO Prioritas Tinggi untuk OBS Studio, Streamlabs, dan Discord Screen Share.",
            Purpose = "Melenyapkan lag encoder dan frame drop streaming saat game sedang memforsir utilisasi GPU hingga 99%.",
            HowItWorks = "Menyetel MMCSS Tasks\\Capture (GPU Priority=8, Priority=6) dan menetapkan IFEO High Priority untuk obs64.exe dan Streamlabs.",
            Impact = "Live streaming dan rekaman 60fps super mulus tanpa patah-patah atau encoder overload."
        },
        ["gaming.disable_game_dvr"] = new()
        {
            Name = "Nonaktifkan Perekaman Background Xbox GameDVR & Latensi Overlay",
            Description = "Mematikan perekaman klip latar belakang Xbox Game Bar untuk mengurangi micro-stuttering dan input lag.",
            Purpose = "Menghentikan pemborosan encoder video NVENC/AMD VCE saat kamu tidak berniat merekam klip gameplay.",
            HowItWorks = "Menyetel HKCU\\System\\GameConfigStore (GameDVR_Enabled = 0) dan policy AllowGameDVR = 0.",
            Impact = "Menghemat 5-15% beban GPU video engine dan menghilangkan input delay pada game."
        },
        ["gaming.fse_behavior"] = new()
        {
            Name = "Optimalkan Layer Tampilan Layar Penuh (Low-Latency DWM)",
            Description = "Mengonfigurasi DWM untuk presentasi direct swapchain dan meminimalkan delay komposisi desktop.",
            Purpose = "Mengaktifkan model hardware direct flip untuk game borderless windowed dan fullscreen.",
            HowItWorks = "Menyetel HKCU\\System\\GameConfigStore (GameDVR_DXGIHonorFSEWindowsCompatible=1, GameDVR_FSEBehavior=2).",
            Impact = "Latensi tampilan paling minim menyerupai exclusive fullscreen murni."
        },
        ["gaming.disable_mpo"] = new()
        {
            Name = "Nonaktifkan Multi-Plane Overlay (Fix Stutter & Layar Hitam MPO)",
            Description = "Mematikan Multi-Plane Overlay di DWM, mengatasi masalah stutter, layar berkedip, dan timeout driver GPU.",
            Purpose = "Memperbaiki bug umum GPU NVIDIA/AMD pada setup multi-monitor dan overlay video.",
            HowItWorks = "Menyetel HKLM\\SOFTWARE\\Microsoft\\Windows\\Dwm (OverlayTestMode = 5).",
            Impact = "Menghilangkan stutter dan kedipan layar hitam saat Alt-Tab atau saat overlay Discord/OBS aktif."
        },
        ["gaming.enable_hags"] = new()
        {
            Name = "Aktifkan Hardware-Accelerated GPU Scheduling (HAGS Low-Latency)",
            Description = "Mengaktifkan HAGS agar GPU modern mengelola penjadwalan VRAM mandiri untuk mengurangi input lag.",
            Purpose = "Memindahkan manajemen antrean frame dari switching CPU langsung ke prosesor khusus GPU.",
            HowItWorks = "Menyetel HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers (HwSchMode = 2).",
            Impact = "Input lag lebih rendah dan mengaktifkan fitur Frame Generation pada RTX 40/50 series."
        },
        ["gaming.raw_mouse_input"] = new()
        {
            Name = "Nonaktifkan Akselerasi Mouse Windows (1:1 Raw Pixel Input untuk FPS)",
            Description = "Mematikan kurva akselerasi kursor Windows dan menyetel pergerakan mouse ke rasio linear 1:1 murni untuk akurasi aim konsisten.",
            Purpose = "Menghilangkan akselerasi jarak kursor Windows sehingga muscle memory aim di Valorant, Apex, dan CS2 100% konsisten.",
            HowItWorks = "Menyetel HKCU\\Control Panel\\Mouse (MouseSpeed=0, MouseThreshold1=0, MouseThreshold2=0, SmoothLinearCurve).",
            Impact = "Tracking sensor mouse menjadi 100% presisi tanpa interferensi buatan Windows."
        },
        ["gaming.timer_resolution_low_latency"] = new()
        {
            Name = "Optimalkan Resolusi Timer Presisi Tinggi & Invariant TSC (Clock 0.5ms)",
            Description = "Mengonfigurasi dynamic tick dan Invariant TSC clock policy untuk latensi timer terendah di game kompetitif.",
            Purpose = "Memaksa timer sistem Windows bekerja pada presisi tinggi (0.5ms) tanpa jitter clock sintetis untuk Riot Vanguard / EAC.",
            HowItWorks = "Menjalankan bcdedit /set disabledynamictick yes, useplatformclock false, tscsyncpolicy Enhanced dan menyetel GlobalTimerResolutionRequests=1.",
            Impact = "Polling input keyboard/mouse lebih responsif dan frame pacing jauh lebih mulus."
        },
        ["gaming.nvidia_driver_power_latency"] = new()
        {
            Name = "Optimasi Latensi Driver NVIDIA GeForce GTX/RTX & Matikan Telemetri",
            Description = "Mematikan daemon background NvTelemetryContainer dan mengonfigurasi driver power state ke Prefer Maximum Performance.",
            Purpose = "Menghentikan lonjakan DPC latency driver NVIDIA dan mengunci clock GPU di status performa penuh tanpa delay downclock.",
            HowItWorks = "Menghentikan service NvTelemetryContainer dan mengaktifkan D3D preemption pada GraphicsDrivers Scheduler.",
            Impact = "Menghilangkan spike DPC latency driver NVIDIA di tengah baku tembak."
        },
        ["gaming.nvidia_rtx_reflex_queue"] = new()
        {
            Name = "Optimalkan Batas Shader Cache NVIDIA (10GB) & Antrean Render Direct Flip",
            Description = "Memperbesar ukuran shader cache DirectX/Vulkan ke 10GB untuk mencegah stuttering kompilasi shader pada GTX & RTX.",
            Purpose = "Mencegah stuttering saat shader baru dimuat di Apex Legends, Warzone, dan CS2 dengan memperluas cache ke 10GB.",
            HowItWorks = "Menyetel registry NVIDIA Global MaxShaderCacheSize=10240 dan PreRenderedFrames=1.",
            Impact = "Lenyapnya frame drop mendadak saat bertemu musuh atau efek skill baru."
        },
        ["gaming.amd_radeon_anti_lag_ulps"] = new()
        {
            Name = "Nonaktifkan AMD Radeon Ultra Low Power State (ULPS) & Anti-Lag Boost",
            Description = "Mematikan ULPS pada driver AMD Radeon, mencegah penurunan clock GPU mendadak saat game FPS berat.",
            Purpose = "Mencegah kartu grafis AMD Radeon menidurkan unit komputasi saat game sedang membutuhkan framerate konstan.",
            HowItWorks = "Menyetel EnableUlps=0 dan EnableUlps_NA=0 pada seluruh entri kelas display driver di Registry.",
            Impact = "Kestabilan clock GPU solid pada Radeon RX seri 5000/6000/7000."
        },
        ["gaming.disable_hyperv_hypervisor"] = new()
        {
            Name = "Nonaktifkan Peluncuran Hyper-V Hypervisor (Mode CPU Bare-Metal)",
            Description = "Menyetel hypervisorlaunchtype off via BCDEdit, menghapus layer Type-1 hypervisor untuk latensi DPC terendah.",
            Purpose = "Membongkar layer virtualisasi Type-1 Windows agar instruksi prosesor berjalan langsung ke silikon CPU.",
            HowItWorks = "Menjalankan bcdedit.exe /set hypervisorlaunchtype off.",
            Impact = "Menurunkan latensi DPC prosesor ke level terendah untuk gamer kompetitif."
        },
        ["gaming.disable_vbs_hvci"] = new()
        {
            Name = "Nonaktifkan Virtualization-Based Security (VBS) & Integritas Memori (HVCI)",
            Description = "Mematikan VBS dan HVCI pada Kernel Windows, menghilangkan beban virtualisasi CPU saat gaming.",
            Purpose = "Menghilangkan penurunan performa CPU akibat pengecekan integritas kernel berbasis virtualisasi terus-menerus.",
            HowItWorks = "Menyetel registry DeviceGuard (EnableVirtualizationBasedSecurity=0, HypervisorEnforcedCodeIntegrity=0).",
            Impact = "Meningkatkan performa gaming dan 1% Low FPS sebesar 5% hingga 15% pada prosesor Intel & AMD Ryzen."
        },
        ["gaming.gpu_msi_mode"] = new()
        {
            Name = "Aktifkan Mode MSI (Message Signaled Interrupts) & Prioritas Tinggi pada GPU",
            Description = "Mengubah penanganan interupsi hardware GPU dari model lama IRQ sharing ke Message Signaled Interrupts (MSI) berprioritas tinggi.",
            Purpose = "Melenyapkan konflik IRQ hardware dan audio crackling, memberikan latensi interupsi GPU terendah untuk NVIDIA & AMD.",
            HowItWorks = "Menyetel MSISupported=1 dan DevicePriority=3 pada Device Parameters Adapter Display di Registry.",
            Impact = "Melenyapkan micro-stuttering dan lonjakan DPC latency akibat penundaan antrean interupsi driver grafik."
        },
        ["gaming.audio_exclusive_latency"] = new()
        {
            Name = "Latensi Audio Ultra-Rendah & MMCSS Pro Audio Tuning (Footstep Emas)",
            Description = "Mengonfigurasi scheduler MMCSS Audio dan Pro Audio ke prioritas maksimal (Priority 6, Scheduling High, Clock Rate 10000) serta mematikan audio throttling.",
            Purpose = "Memberikan respons audio sub-milidetik dan menghilangkan distorsi audio saat suara tembakan dan Discord bertumpuk.",
            HowItWorks = "Menyetel HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\\Tasks (Audio & Pro Audio).",
            Impact = "Suara langkah kaki musuh di Valorant terdengar 5-10ms lebih cepat dengan posisi spasial yang sangat akurat."
        },
        ["gaming.mouse_raw_input_1to1"] = new()
        {
            Name = "1:1 Raw Mouse Sensor Input & Matikan Akselerasi Windows",
            Description = "Memaksa pemetaan kursor 1:1 tanpa akselerasi (MouseSpeed=0, MouseThreshold1/2=0) dan mengunci kecepatan kursor ke notch 6/11 standar.",
            Purpose = "Menjamin respon bidikan linear sempurna tanpa akselerasi untuk konsistensi muscle memory flick shot di Valorant, CS2, dan Apex Legends.",
            HowItWorks = "Mengonfigurasi HKCU\\Control Panel\\Mouse (MouseSpeed=0, MouseThreshold1=0, MouseThreshold2=0, MouseSensitivity=10).",
            Impact = "Pergerakan kursor 100% linear, presisi, dan bebas akselerasi."
        },
        ["gaming.mouse_markc_acceleration_fix"] = new()
        {
            Name = "MarkC Windows 11 Mouse Fix: Kurva Linear 1:1 (Skala 100% Desktop)",
            Description = "Menerapkan tabel kurva legendaris MarkC Windows Mouse Fix (SmoothMouseXCurve dan SmoothMouseYCurve) yang dikalibrasi untuk skala DPI desktop 100%.",
            Purpose = "Melenyapkan distorsi kurva akselerasi kernel Windows bawaan yang mengganggu akurasi tembakan jarak jauh / micro-adjustment.",
            HowItWorks = "Menyuntikkan tabel kurva biner 1:1 linear ke HKCU\\Control Panel\\Mouse (SmoothMouseXCurve & SmoothMouseYCurve).",
            Impact = "Tracking kursor super mulus dengan rasio 1 count = 1 pixel tanpa distorsi."
        },
        ["gaming.mouse_hid_queue_buffer_tuning"] = new()
        {
            Name = "Perbesar Buffer Antrean Driver Mouse HID (Anti-Packet Drop 1000Hz - 8000Hz)",
            Description = "Memperbesar buffer MouseDataQueueSize driver mouclass dari 100 menjadi 128 paket untuk mencegah buffer overflow saat flick berkecepatan tinggi.",
            Purpose = "Mencegah hilangnya paket input USB HID saat menggunakan mouse gaming ber-polling rate tinggi (1000Hz, 2000Hz, 4000Hz, 8000Hz).",
            HowItWorks = "Menyetel HKLM\\SYSTEM\\CurrentControlSet\\Services\\mouclass\\Parameters\\MouseDataQueueSize ke 128 paket.",
            Impact = "Zero micro-stutter dan bebas packet drop saat melakukan flick 180° cepat."
        },
        ["gaming.mouse_usb_power_throttling_disable"] = new()
        {
            Name = "Nonaktifkan USB Power Throttling & Selective Suspend untuk Mouse Gaming",
            Description = "Mematikan fitur hemat daya USB selective suspend Windows agar sensor optik mouse selalu aktif pada polling rate maksimal tanpa delay bangun dari sleep.",
            Purpose = "Mencegah sensor optik mouse masuk ke mode tidur mikro saat sedang diam menahan sudut (holding angle) di Valorant/CS2.",
            HowItWorks = "Menyetel PowerThrottlingOff=1 dan mematikan USB Selective Suspend via powercfg pada skema daya aktif.",
            Impact = "Sensor mouse selalu responsif instan tanpa jeda mikroskopis."
        },

        // ==========================================
        // NETWORK TWEAKS (8 Modules)
        // ==========================================
        ["network.tcp_nodelay_ack"] = new()
        {
            Name = "Nonaktifkan Algoritma Nagle & TCP Delayed ACKs (Ping Game Terendah)",
            Description = "Mengonfigurasi TCPNoDelay=1 dan TcpAckFrequency=1 untuk menghilangkan penundaan antrean paket.",
            Purpose = "Memaksa paket data TCP dan ACK langsung dikirim seketika tanpa ditahan di buffer.",
            HowItWorks = "Menyetel TCPNoDelay=1, TcpAckFrequency=1, TcpDelAckTicks=0 pada interface jaringan aktif di Registry.",
            Impact = "Menurunkan ping game online secara signifikan dan menghilangkan delay packet rubberbanding."
        },
        ["network.tcp_autotuning_heuristic"] = new()
        {
            Name = "Optimalkan TCP Window Auto-Tuning & Nonaktifkan Chimney Scaling",
            Description = "Menyetel stack TCP Windows untuk throughput tinggi dan streaming paket yang konsisten.",
            Purpose = "Mengizinkan ukuran receive window TCP membesar otomatis mengikuti kecepatan bandwidth internet fiber optik.",
            HowItWorks = "Menjalankan netsh int tcp set global autotuninglevel=normal, rss=enabled, heuristics disabled.",
            Impact = "Kecepatan download/upload maksimal dan meminimalkan packet loss."
        },
        ["network.tcp_congestion_provider"] = new()
        {
            Name = "Konfigurasi Algoritma TCP Congestion Rendah Latensi (CTCP / CUBIC)",
            Description = "Mengatur algoritma kontrol kemacetan TCP Windows ke CTCP / CUBIC untuk respons transfer cepat.",
            Purpose = "Mengganti algoritma kontrol kemacetan lawas dengan algoritma modern yang dirancang untuk koneksi berlatensi rendah.",
            HowItWorks = "Menjalankan netsh int tcp set supplemental template=custom congestionprovider=ctcp / cubic.",
            Impact = "Koneksi data lebih cepat stabil dan packet transfer live streaming/gaming lebih mulus."
        },
        ["network.fastroute_qos_dscp46"] = new()
        {
            Name = "TROY FastRoute: Game Packet QoS & Prioritas DSCP 46 (Teknologi ExitLag)",
            Description = "Menandai paket data game dengan DSCP 46 (Expedited Forwarding) dan mengaktifkan Windows QoS traffic shaping untuk Valorant, CS2, Apex, dan Fortnite.",
            Purpose = "Memberikan jalur antrean VIP pada kernel Windows, router, dan ISP sehingga paket game tidak pernah tertahan di belakang download.",
            HowItWorks = "Membuat kebijakan QoS Windows dengan DSCP 46 dan menyetel traffic shaping qWave & pacer.sys.",
            Impact = "Zero queue delay, paket game diprioritaskan utama, dan routing latensi lebih kompetitif."
        },
        ["network.fastroute_anti_jitter_pacing"] = new()
        {
            Name = "TROY FastRoute: Anti-Jitter UDP Packet Pacing & Perbaikan Bufferbloat",
            Description = "Mengoptimalkan buffer socket UDP/TCP, memperluas batas port dinamis ke 65.534, dan mengaktifkan NonSack RTT resiliency.",
            Purpose = "Melenyapkan bufferbloat jaringan, lonjakan ping (jitter spikes), dan packet burst mendadak saat baku tembak intensif.",
            HowItWorks = "Menyetel MaxUserPort=65534, TcpTimedWaitDelay=30, dan NonSackRttResiliency=1 pada Tcpip & Pacer.",
            Impact = "Pacing paket data sangat konsisten di level sub-milidetik dengan 0% packet loss."
        },
        ["network.streaming_qos_pacing"] = new()
        {
            Name = "Optimasi Pacing Jaringan RTMP/SRT/Discord Live Streaming (Bebas Drop Bitrate)",
            Description = "Menyetel ketahanan socket TCP, memperluas port dinamis (65534 port), dan melenyapkan lonjakan paket untuk streaming Twitch/YouTube jernih.",
            Purpose = "Mencegah buffer overflow pada server ingest RTMP dan mencegah drop bitrate tiba-tiba saat siaran langsung.",
            HowItWorks = "Menyetel NonSackRttResiliency=1, MaxUserPort=65534, dan TcpTimedWaitDelay=30 pada registry parameter TCP/IP.",
            Impact = "Kestabilan bitrate live streaming sangat solid dan bebas packet loss tanpa menaikkan ping game."
        },
        ["network.disable_lso"] = new()
        {
            Name = "Nonaktifkan Large Send Offload (LSO) & Delay Checksum Hardware",
            Description = "Mematikan segmentasi paket offload (LSO) untuk mencegah lonjakan buffer paket pada game online.",
            Purpose = "Mencegah driver kartu jaringan yang bermasalah menahan dan mengelompokkan paket di hardware NIC.",
            HowItWorks = "Menyetel DisableTaskOffload=1 pada parameter TCP/IP dan mematikan *LSOv2 pada adapter jaringan.",
            Impact = "Menghilangkan lonjakan ping mendadak (ping spike) saat bermain game multiplayer kompetitif."
        },
        ["network.flush_dns"] = new()
        {
            Name = "Bersihkan Cache Resolver DNS Windows",
            Description = "Membersihkan cache resolver DNS lokal menggunakan Win32 DnsFlushResolverCache API native.",
            Purpose = "Menghapus pemetaan nama domain yang kadaluwarsa, salah, atau korup.",
            HowItWorks = "Memanggil native Win32 dnsapi.dll!DnsFlushResolverCache().",
            Impact = "Memperbaiki error koneksi website dan mempercepat resolving domain baru."
        },
        ["network.disable_netbios"] = new()
        {
            Name = "Nonaktifkan NetBIOS over TCP/IP (Kurangi Broadcast LAN)",
            Description = "Mematikan lalu lintas siaran query nama NetBIOS lawas pada seluruh adapter jaringan.",
            Purpose = "Menghentikan kebisingan broadcast paket LAN yang tidak terpakai dan menutup celah keamanan NetBIOS lawas.",
            HowItWorks = "Menyetel NetbiosOptions = 2 pada seluruh interface NetBT di Registry.",
            Impact = "Mengurangi beban lalu lintas jaringan lokal dan meningkatkan keamanan LAN."
        },

        // ==========================================
        // SERVICES & DEBLOAT & MAINTENANCE (4 Modules)
        // ==========================================
        ["services.optimize_background_junk"] = new()
        {
            Name = "Nonaktifkan Layanan Latar Belakang Non-Esensial (Maps, RetailDemo, WER)",
            Description = "Mematikan layanan latar belakang yang tidak diperlukan (MapsBroker, RetailDemo, WerSvc, wisvc, TroubleshootingSvc).",
            Purpose = "Menghentikan daemon latar belakang tak terpakai agar tidak membebani CPU dan memakan RAM.",
            HowItWorks = "Mengubah tipe startup service menjadi Disabled via sc.exe config.",
            Impact = "Membebaskan 50-150MB RAM dan mengurangi context switching CPU latar belakang."
        },
        ["debloater.uwp_bloatware"] = new()
        {
            Name = "Hapus Aplikasi Bawaan Bloatware UWP Windows 10/11",
            Description = "Menghapus aplikasi sampah bawaan (Bing News, Clipchamp, Solitaire, dll) tanpa menyentuh Store & Calculator.",
            Purpose = "Membersihkan aplikasi sponsor pabrikan yang memakan ruang penyimpanan dan menjalankan background task.",
            HowItWorks = "Menjalankan PowerShell Remove-AppxPackage dan Remove-AppxProvisionedPackage.",
            Impact = "Menghemat 1GB - 3GB ruang disk dan membuat Start Menu bersih."
        },
        ["maintenance.clean_temp_files"] = new()
        {
            Name = "Bersihkan File Cache Sementara User & Sistem",
            Description = "Membersihkan file sampah sementara di %TEMP%, C:\\Windows\\Temp, CrashDumps, dan thumbnail cache.",
            Purpose = "Menghapus file sisa instalasi, log crash aplikasi, dan cache sementara yang menumpuk.",
            HowItWorks = "Mengiterasi dan menghapus file target secara aman dengan mengabaikan file yang sedang digunakan.",
            Impact = "Merebut kembali 500MB - 10GB+ ruang penyimpanan disk."
        },
        ["maintenance.clean_windows_update_cache"] = new()
        {
            Name = "Bersihkan Cache Download Windows Update (SoftwareDistribution)",
            Description = "Menghentikan wuauserv secara aman, menghapus sisa installer update lama, dan merestart service.",
            Purpose = "Menghapus file installer pembaruan Windows lama yang sudah selesai diinstal.",
            HowItWorks = "Menghentikan wuauserv/bits, menghapus isi C:\\Windows\\SoftwareDistribution\\Download, lalu merestart service.",
            Impact = "Membebaskan 2GB - 15GB ruang disk pada partisi C:."
        }
    };

    private static readonly Dictionary<JunkCategory, (JunkCategoryLocalizationInfo En, JunkCategoryLocalizationInfo Id)> JunkCategories = new()
    {
        [JunkCategory.RecycleBin] = (
            new() { Name = "Windows Recycle Bin", Description = "Deleted files residing in the recycle bin awaiting permanent purge." },
            new() { Name = "Tempat Sampah Windows (Recycle Bin)", Description = "File terhapus di tempat sampah yang menunggu dibersihkan secara permanen." }
        ),
        [JunkCategory.WindowsTempAndLogs] = (
            new() { Name = "Windows Temp, Crash Dumps & Error Reports", Description = "User & System temp files, kernel crash dumps, and Windows Error Reporting archives." },
            new() { Name = "File Temp, Crash Dump & Log Error Windows", Description = "File sementara pengguna & sistem, crash dump kernel, dan arsip laporan error Windows." }
        ),
        [JunkCategory.BrowserCaches] = (
            new() { Name = "Web Browser Caches (Chrome, Edge, Brave, Firefox)", Description = "Cached web assets, script caches, and downloaded media caches." },
            new() { Name = "Cache Browser Web (Chrome, Edge, Brave, Firefox)", Description = "Aset web tersimpan, cache skrip, dan cache media dari berbagai web browser." }
        ),
        [JunkCategory.DirectXShaderCaches] = (
            new() { Name = "DirectX Shader Caches (GPU D3D Cache)", Description = "Compiled shader binary cache for DirectX games and graphics APIs." },
            new() { Name = "Cache Shader DirectX (Cache GPU D3D)", Description = "Cache biner shader terkompilasi untuk game DirectX dan API rendering grafik." }
        ),
        [JunkCategory.WindowsDeliveryOptimization] = (
            new() { Name = "Windows Delivery Optimization Files", Description = "P2P Windows update cache files used for local network sharing." },
            new() { Name = "File Windows Delivery Optimization", Description = "File cache pembaruan P2P Windows yang dipakai untuk berbagi pembaruan di jaringan lokal." }
        ),
        [JunkCategory.WindowsUpdateDownloads] = (
            new() { Name = "Windows Update Download Cache", Description = "Downloaded cumulative and security update installer files in SoftwareDistribution." },
            new() { Name = "Cache Download Pembaruan Windows", Description = "File sisa download installer pembaruan kumulatif & keamanan di SoftwareDistribution." }
        ),
        [JunkCategory.ExplorerThumbnailCaches] = (
            new() { Name = "Windows Explorer Thumbnail Cache", Description = "Cached image/video thumbnail database files." },
            new() { Name = "Cache Thumbnail Windows Explorer", Description = "File database gambar mini (thumbnail) foto dan video yang di-cache di sistem." }
        )
    };

    public static JunkCategoryLocalizationInfo GetJunkCategoryInfo(JunkCategory cat, AppLanguage? lang = null)
    {
        var targetLang = lang ?? CurrentLanguage;
        if (JunkCategories.TryGetValue(cat, out var pair))
        {
            return targetLang == AppLanguage.Indonesian ? pair.Id : pair.En;
        }

        return new JunkCategoryLocalizationInfo
        {
            Name = cat.ToString(),
            Description = "System temporary files."
        };
    }

    public static TweakLocalizationInfo GetTweakInfo(string id, AppLanguage? lang = null)
    {
        var targetLang = lang ?? CurrentLanguage;
        var dict = targetLang == AppLanguage.Indonesian ? IndonesianTweaks : EnglishTweaks;

        if (dict.TryGetValue(id, out var info))
        {
            return info;
        }

        // Fallback to English if not found
        if (EnglishTweaks.TryGetValue(id, out var engInfo))
        {
            return engInfo;
        }

        return new TweakLocalizationInfo
        {
            Name = id,
            Description = "No description available.",
            Purpose = "General system optimization.",
            HowItWorks = "System modification.",
            Impact = "Improved performance."
        };
    }

    public static string GetCategoryName(TweakCategory category, AppLanguage? lang = null)
    {
        var targetLang = lang ?? CurrentLanguage;
        if (targetLang == AppLanguage.Indonesian)
        {
            return category switch
            {
                TweakCategory.Privacy => "Privasi",
                TweakCategory.Performance => "Performa",
                TweakCategory.Gaming => "Gaming",
                TweakCategory.Network => "Jaringan",
                TweakCategory.Services => "Layanan",
                TweakCategory.Debloater => "Debloater",
                TweakCategory.Maintenance => "Pemeliharaan",
                _ => category.ToString()
            };
        }

        return category.ToString();
    }

    public static string GetRiskName(RiskLevel risk, AppLanguage? lang = null)
    {
        var targetLang = lang ?? CurrentLanguage;
        if (targetLang == AppLanguage.Indonesian)
        {
            return risk switch
            {
                RiskLevel.Safe => "Aman (100% Safe)",
                RiskLevel.Recommended => "Direkomendasikan",
                RiskLevel.Advanced => "Lanjutan (Advanced)",
                _ => risk.ToString()
            };
        }

        return risk switch
        {
            RiskLevel.Safe => "Safe",
            RiskLevel.Recommended => "Recommended",
            RiskLevel.Advanced => "Advanced",
            _ => risk.ToString()
        };
    }

    public static (string Name, string Description) GetProfileInfo(string profileId, AppLanguage? lang = null)
    {
        var targetLang = lang ?? CurrentLanguage;
        if (targetLang == AppLanguage.Indonesian)
        {
            return profileId switch
            {
                "profile.esports_competitive_fps" => (
                    "Preset eSports & FPS Gaming Kompetitif (Valorant, Apex, CS2)",
                    "Preset latensi ultra-rendah untuk gamer kompetitif: 1:1 raw mouse sensor, timer presisi 0.5ms, unpark seluruh core CPU, GPU D3D/Reflex queue, Win32 Quantum 0x26, dan TCP NoDelay."
                ),
                "profile.gaming" => (
                    "Preset Gaming & Latensi Rendah",
                    "Memaksimalkan prioritas GPU, mengurangi latensi jaringan & ping, mematikan GameDVR, mengaktifkan Quantum CPU 0x26, MPO fix, dan menghentikan telemetri."
                ),
                "profile.streaming_content_creator" => (
                    "Preset Live Streaming & Kreator Konten (OBS, Twitch, YouTube, Laptop Gaming)",
                    "Preset zero-encoder-lag untuk streamer & laptop gaming: Alokasi prioritas GPU/CPU khusus OBS & Streamlabs, routing dGPU Laptop Dual-GPU, pacing jaringan RTMP/SRT, timer 0.5ms, MPO fix, dan prioritas audio MMCSS capture."
                ),
                "profile.dev_workstation" => (
                    "Preset Pengembang & Power User",
                    "Mengoptimalkan paging RAM untuk responsivitas kompilasi, menghilangkan delay UI, tuning stack TCP jaringan, TRIM SSD & NTFS 8.3 boost, dan membersihkan telemetri."
                ),
                "profile.ultra_privacy" => (
                    "Preset Privasi & Keamanan Maksimum",
                    "Penguncian komprehensif atas telemetri, pelacakan diagnostik, Advertising ID, sensor lokasi, pencarian web Cortana, scheduled tasks, dan bloatware sponsor."
                ),
                "profile.safe_daily" => (
                    "Preset Rutinitas Harian Aman (Safe Daily)",
                    "100% optimasi esensial yang aman untuk pengguna harian: membersihkan cache temp, trim RAM idle, responsivitas UI cepat, TRIM SSD, dan mematikan telemetri dasar & background Edge."
                ),
                _ => (profileId, "")
            };
        }

        return profileId switch
        {
            "profile.esports_competitive_fps" => (
                "eSports & Competitive FPS Low Latency (Valorant, Apex, CS2)",
                "Ultra-low latency competitive profile: 1:1 raw mouse sensor, 0.5ms timer resolution, CPU core unparking, GPU D3D & Reflex queue, Win32 Quantum 0x26, and TCP NoDelay."
            ),
            "profile.gaming" => (
                "Gaming & Low-Latency Preset",
                "Maximizes GPU priority, reduces network latency, disables GameDVR overhead, unlocks Ultimate Power Scheme, activates Win32 Quantum 0x26, MPO fix, and pauses telemetry."
            ),
            "profile.streaming_content_creator" => (
                "Live Streaming & Content Creator Preset (OBS, Twitch, YouTube, Laptops)",
                "Zero-encoder-lag preset for streamers & gaming laptops: Dedicated OBS GPU/CPU priority, Laptop Dual-GPU routing, RTMP network pacing, 0.5ms timer, MPO fix, and MMCSS capture audio priority."
            ),
            "profile.dev_workstation" => (
                "Developer & Power User Preset",
                "Optimizes RAM paging for compiler responsiveness, removes UI delays, tunes TCP/network for high packet streaming, SSD TRIM & NTFS 8.3 boost, and cleans telemetry bloat."
            ),
            "profile.ultra_privacy" => (
                "Ultra Privacy & Security Preset",
                "Comprehensive lockdown on telemetry, diagnostic tracking, advertising IDs, location sensors, Cortana background web search, scheduled tasks, and sponsored bloatware."
            ),
            "profile.safe_daily" => (
                "Safe Daily Routine Preset",
                "100% safe essential optimizations for everyday users: cleans temp cache, trims idle RAM, optimizes snappy UI, SSD TRIM, and disables basic telemetry & Edge bloat."
            ),
            _ => (profileId, "")
        };
    }

    public static string GetUiText(string key, AppLanguage? lang = null)
    {
        var targetLang = lang ?? CurrentLanguage;
        var isIndo = targetLang == AppLanguage.Indonesian;

        return key switch
        {
            // Navigation
            "Nav.Dashboard" => isIndo ? "Dashboard" : "Dashboard",
            "Nav.MemReduct" => isIndo ? "RAM & MemReduct" : "RAM & MemReduct",
            "Nav.Tweaks" => isIndo ? "Katalog Tweak" : "Tweaks Catalog",
            "Nav.Profiles" => isIndo ? "Profil & Preset" : "Profiles & Presets",
            "Nav.Cleaner" => isIndo ? "Pembersih Sampah" : "Deep Junk Cleaner",
            "Nav.Startup" => isIndo ? "Aplikasi Startup" : "Startup Apps",
            "Nav.Safety" => isIndo ? "Keamanan & Cadangan" : "Safety & Backups",
            "Nav.Logs" => isIndo ? "Log Eksekusi" : "Execution Logs",

            // Privilege
            "Privilege.Header" => isIndo ? "TINGKAT HAK AKSES" : "PRIVILEGE LEVEL",
            "Privilege.Elevated" => isIndo ? "ADMINISTRATOR (ELEVATED)" : "ELEVATED (ADMIN)",
            "Privilege.Restricted" => isIndo ? "PENGGUNA STANDAR (USER)" : "RESTRICTED (STANDARD USER)",
            "Privilege.Relaunch" => isIndo ? "Jalankan Ulang sebagai Admin" : "Relaunch as Admin",
            "Privilege.BtnRelaunch" => isIndo ? "Jalankan Ulang sebagai Admin" : "Relaunch as Admin",

            // Header & Stats
            "Header.Subtitle" => isIndo ? "Telemetri sistem realtime & optimasi langsung kernel NT" : "Real-time system telemetry and direct kernel optimization",
            "Header.Plan" => isIndo ? "Skema:" : "Plan:",

            // Dashboard
            "Dash.HealthScore" => isIndo ? "SKOR KESEHATAN OPTIMASI" : "OPTIMIZATION HEALTH SCORE",
            "Dash.ActiveTweaks" => isIndo ? "Tweak Aktif" : "Active Tweaks",
            "Dash.HealthyMessage" => isIndo ? "Sistem Windows Anda telah dioptimasi untuk performa maksimal dan latensi terendah." : "Your Windows environment is tuned for maximum performance and minimal latency.",
            "Dash.NeedsOptMessage" => isIndo ? "Sistem Windows membutuhkan optimasi untuk performa dan keamanan puncak." : "Your Windows system could benefit from recommended performance & privacy tweaks.",
            "Dash.HardwareSpecs" => isIndo ? "SPESIFIKASI PERANGKAT KERAS" : "SYSTEM SPECIFICATIONS",
            "Dash.Os" => isIndo ? "Sistem Operasi" : "Operating System",
            "Dash.Cpu" => isIndo ? "Prosesor (CPU)" : "Processor (CPU)",
            "Dash.Gpu" => isIndo ? "Kartu Grafis (GPU)" : "Graphics (GPU)",
            "Dash.Ram" => isIndo ? "Memori Sistem (RAM)" : "System Memory (RAM)",
            "Dash.TimerResolution" => isIndo ? "Resolusi Timer Sistem (ntdll)" : "System Timer Resolution (ntdll)",
            "Dash.TimerResolutionSub" => isIndo ? "Presisi Timer Invariant TSC 0.5ms" : "0.5ms Invariant TSC Precision",
            "Dash.DisplayTitle" => isIndo ? "TELEMETRI MONITOR & SMART VRR ADVISOR" : "DISPLAY & SMART VRR ADVISOR",
            "Dash.DisplaySubtitle" => isIndo ? "Refresh rate aktual & panduan setting in-game otomatis" : "Live hardware refresh rate & intelligent in-game settings guide",
            "Dash.DisplayMonitor" => isIndo ? "Monitor Aktif:" : "Active Monitor:",
            "Dash.DisplayResolution" => isIndo ? "Resolusi & Hz:" : "Resolution & Hz:",
            "Dash.DisplayVrr" => isIndo ? "Variable Refresh:" : "Variable Refresh:",
            "Dash.DisplayVsync" => isIndo ? "In-Game V-Sync:" : "In-Game V-Sync:",
            "Dash.DisplayReflex" => isIndo ? "Reflex / Anti-Lag:" : "Reflex / Anti-Lag:",
            "Dash.DisplayFpsCap" => isIndo ? "Batas FPS Ideal:" : "Optimal FPS Cap:",
            "Dash.QuickActions" => isIndo ? "AKSI CEPAT 1-KLIK" : "QUICK 1-CLICK ACTIONS",

            // MemReduct View
            "Mem.Physical" => isIndo ? "MEMORI FISIK (RAM)" : "PHYSICAL MEMORY (RAM)",
            "Mem.InUse" => isIndo ? "Terpakai" : "In Use",
            "Mem.Available" => isIndo ? "Tersedia:" : "Available:",
            "Mem.Total" => isIndo ? "Total:" : "Total:",
            "Mem.Pagefile" => isIndo ? "PAGEFILE & COMMIT CHARGE" : "PAGEFILE & COMMIT CHARGE",
            "Mem.Committed" => isIndo ? "Dialokasikan" : "Committed",
            "Mem.Limit" => isIndo ? "Batas:" : "Limit:",
            "Mem.WorkingSet" => isIndo ? "APP WORKING SET" : "APP WORKING SET",
            "Mem.WorkingSetSub" => isIndo ? "Memori Proses Residen" : "Resident Process Memory",
            "Mem.WorkingSetDetail" => isIndo ? "Halaman memori aktif di RAM fisik" : "Active working pages in physical RAM",
            "Mem.PurgeRegions" => isIndo ? "AREA MEMORI YANG AKAN DIBERSIHKAN (NT KERNEL API)" : "MEMORY REGIONS TO PURGE (NT KERNEL API)",
            "Mem.RegionWorkingSet" => isIndo ? "Working Set (Lepaskan halaman tak terpakai dari seluruh proses)" : "Working Set (Flush unreferenced pages across all user & system processes)",
            "Mem.RegionFileCache" => isIndo ? "Cache File Sistem (Bersihkan NT file cache & buffer disk)" : "System File Cache (Purge NT file cache & buffered disk handles)",
            "Mem.RegionStandby" => isIndo ? "Standby List Cache (Hilangkan stuttering akibat standby memory)" : "Standby List Cache (Eliminates micro-stuttering caused by filled standby memory)",
            "Mem.RegionModified" => isIndo ? "Modified Page List (Tulis halaman memori kotor ke pool commit/disk)" : "Modified Page List (Flush dirty memory pages to disk / commit pool)",
            "Mem.RegionCombine" => isIndo ? "Combine Memory Lists (Jalankan deduplikasi halaman Windows 10/11)" : "Combine Memory Lists (Execute Windows 10/11 page deduplication)",
            "Mem.RegionRegistry" => isIndo ? "Cache Registri (Bersihkan in-memory registry hives tak terpakai)" : "Registry Cache (Purge unused in-memory registry hives)",
            "Mem.BtnCleanNow" => isIndo ? "Bersihkan Memori Sekarang" : "Clean Memory Now",
            "Mem.AutoReductTitle" => isIndo ? "OTOMASI AUTO-REDUCT" : "AUTO-REDUCT AUTOMATION",
            "Mem.AutoReductCheck" => isIndo ? "Auto-clean saat pemakaian RAM melebihi:" : "Auto-clean when RAM exceeds:",
            "Mem.Result" => isIndo ? "Hasil: " : "Result: ",

            // Tweaks Catalog
            "Tweaks.AllCategories" => isIndo ? "Semua Kategori" : "All Categories",
            "Tweaks.SearchPlaceholder" => isIndo ? "Cari modul tweak berdasarkan nama, ID, deskripsi..." : "Search tweaks by name, id, category...",

            // Profiles View
            "Profiles.Title" => isIndo ? "PRESET OPTIMASI 1-KLIK" : "1-CLICK OPTIMIZATION PRESETS",
            "Profiles.TweaksIncluded" => isIndo ? "tweak disertakan" : "tweaks included",
            "Profiles.BtnApply" => isIndo ? "Terapkan Preset" : "Apply Preset",

            // Deep Junk Cleaner View
            "Cleaner.Title" => isIndo ? "PEMBERSIH SAMPAH MENDALAM SISTEM & BROWSER" : "DEEP SYSTEM & BROWSER JUNK CLEANER",
            "Cleaner.FoundSub" => isIndo ? "sampah aman dibersihkan ditemukan" : "of safe-to-clean junk found",
            "Cleaner.FilesAcross" => isIndo ? "Ditemukan di {0} file sementara tak terpakai" : "Found across {0} unreferenced temporary files",
            "Cleaner.FoundFilesFormat" => isIndo ? "Ditemukan di {0} file" : "Found across {0} files",
            "Cleaner.BtnRescan" => isIndo ? "Pindai Ulang" : "Re-Scan",
            "Cleaner.BtnCleanSelected" => isIndo ? "Bersihkan Sampah Terpilih" : "Clean Selected Junk",
            "Cleaner.SelectTargets" => isIndo ? "PILIH TARGET YANG AKAN DIBERSIHKAN" : "SELECT TARGETS TO PURGE",

            // Startup Applications Manager
            "Startup.Title" => isIndo ? "MANAJEMEN APLIKASI STARTUP WINDOWS" : "WINDOWS STARTUP APPLICATIONS MANAGER",
            "Startup.Subtitle" => isIndo ? "Periksa dan nonaktifkan aplikasi yang memperlambat boot Windows secara aman tanpa kehilangan data" : "Inspect and safely toggle boot delay applications across Registry hives & Startup folders",
            "Startup.BtnRescan" => isIndo ? "Pindai Ulang Startup" : "Rescan Startup Apps",
            "Startup.TotalAppsFormat" => isIndo ? "Total Aplikasi: {0}" : "Total Apps: {0}",
            "Startup.EnabledAppsFormat" => isIndo ? "Aktif: {0}" : "Enabled: {0}",

            // Safety & Backups View
            "Safety.RestoreGate" => isIndo ? "GERBANG TITIK PEMULIHAN SISTEM WINDOWS" : "WINDOWS SYSTEM RESTORE GATE",
            "Safety.RestoreDesc" => isIndo ? "Setiap optimasi otomatis membuat Titik Pemulihan (Restore Point). Anda juga dapat membuat snapshot manual kapan saja di bawah ini." : "Every batch optimization automatically triggers a Windows Restore Point. You can also manually trigger a snapshot below at any time.",
            "Safety.BtnCreateRestore" => isIndo ? "Buat Titik Pemulihan Manual" : "Create Manual Restore Point",
            "Safety.RollbackTitle" => isIndo ? "PENGEMBALIAN DARURAT (EMERGENCY ROLLBACK)" : "EMERGENCY ROLLBACK",
            "Safety.RollbackDesc" => isIndo ? "Mengembalikan seluruh kunci Registri, Layanan Windows, dan skema daya yang dimodifikasi ke setelan awal resmi Windows." : "Reverts all modified Registry keys, Windows Services, and power plans back to official Windows defaults.",
            "Safety.BtnRollbackAll" => isIndo ? "Kembalikan Seluruh Optimasi ke Bawaan" : "Rollback All Optimizations to Default",

            // Logs View
            "Logs.Title" => isIndo ? "LOG AUDIT EKSEKUSI" : "EXECUTION AUDIT LOGS",
            "Logs.BtnExport" => isIndo ? "Simpan Log ke File (Desktop)" : "Export Logs to Desktop File",

            // General & VIP Banner & Dialogs
            "Nav.Header" => isIndo ? "NAVIGASI" : "NAVIGATION",
            "Dash.BtnAutoFixAll" => isIndo ? "Perbaiki Otomatis & Optimasi" : "Auto-Fix & Optimize All",
            "Dash.BtnQuickClean" => isIndo ? "Bersihkan RAM & Cache" : "Clean RAM & Cache",
            "Dash.BtnQuickTempClean" => isIndo ? "Bersihkan %TEMP%" : "Clean %TEMP%",
            "Dash.BtnQuickCreateRestore" => isIndo ? "Buat Restore Point" : "Create Restore Point",
            "Dash.BtnQuickRollback" => isIndo ? "Rollback Semua" : "Rollback All",
            "Dash.TweaksActiveFormat" => isIndo ? "{0} / 28 Tweak Aktif" : "{0} / 28 Tweaks Active",
            "Dash.VipBannerTitle" => isIndo ? "Buka Kunci Kernel eSports & 30+ Modul Lanjutan" : "Unlock eSports Kernel & 30+ Advanced Modules",
            "Dash.VipBannerDesc" => isIndo 
                ? "Tingkatkan ke NRTX Labs VIP Organization ($350 USD / Rp 5.000.000 Seumur Hidup) untuk membuka Kunci Resolusi Timer Kernel 0.5000ms, TROY FastRoute QoS DSCP 46, Mode Gaming Bare-Metal, Audio MMCSS Footstep Emas, Radar Mouse 8000Hz & Smart Game Booster Daemon."
                : "Upgrade to NRTX Labs VIP Organization ($350 USD / Rp 5JT Lifetime) to unlock 0.5000ms Kernel Timer Resolution Lock, TROY FastRoute QoS DSCP 46, Bare-Metal Gaming Mode, Footstep Emas MMCSS Audio, 8000Hz Mouse Radar & Smart Game Booster Daemon.",
            "Dash.VipBannerBtn" => isIndo ? "Tingkatkan ke VIP (5JT Seumur Hidup)" : "Upgrade to VIP (5JT Lifetime)",

            // Tweaks Categories
            "Tweaks.CategoryPrivacy" => isIndo ? "Privasi" : "Privacy",
            "Tweaks.CategoryPerformance" => isIndo ? "Performa" : "Performance",
            "Tweaks.CategoryGaming" => isIndo ? "Gaming" : "Gaming",
            "Tweaks.CategoryNetwork" => isIndo ? "Jaringan" : "Network",
            "Tweaks.CategoryServices" => isIndo ? "Layanan" : "Services",
            "Tweaks.CategoryDebloater" => isIndo ? "Debloater" : "Debloater",
            "Tweaks.CategoryMaintenance" => isIndo ? "Pemeliharaan" : "Maintenance",

            _ => key
        };
    }
}
