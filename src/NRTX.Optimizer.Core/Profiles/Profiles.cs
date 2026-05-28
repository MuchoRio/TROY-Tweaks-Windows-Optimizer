using NRTX.Optimizer.Core.Abstractions;

namespace NRTX.Optimizer.Core.Profiles;

public class EsportsCompetitiveFpsProfile : IProfile
{
    public string Id => "profile.esports_competitive_fps";
    public string Name => "eSports & Competitive FPS Preset";
    public string Description => "Community low-latency preset: 1:1 raw mouse sensor, Win32 Quantum 0x26, GPU priority, GameDVR disable, and TCP NoDelay.";
    public string Icon => "FPS";

    public IReadOnlyList<string> TargetTweakIds => [
        "perf.ultimate_power_plan",
        "perf.win32_priority_separation",
        "perf.disable_fast_startup_clean_boot",
        "gaming.system_responsiveness",
        "gaming.disable_network_throttling",
        "gaming.gpu_mmcss_priority",
        "gaming.disable_game_dvr",
        "gaming.fse_behavior",
        "gaming.raw_mouse_input",
        "network.tcp_nodelay_ack",
        "network.disable_lso",
        "network.flush_dns",
        "privacy.disable_telemetry",
        "privacy.disable_diagtrack_service"
    ];
}

public class GamingProfile : IProfile
{
    public string Id => "profile.gaming";
    public string Name => "Gaming & Responsiveness Preset";
    public string Description => "Maximizes GPU priority, reduces network latency, disables GameDVR overhead, unlocks Ultimate Power Scheme, and pauses telemetry.";
    public string Icon => "GAME";

    public IReadOnlyList<string> TargetTweakIds => [
        "perf.ultimate_power_plan",
        "perf.win32_priority_separation",
        "perf.snappy_ui_effects",
        "perf.disable_fast_startup_clean_boot",
        "gaming.system_responsiveness",
        "gaming.disable_network_throttling",
        "gaming.gpu_mmcss_priority",
        "gaming.disable_game_dvr",
        "gaming.fse_behavior",
        "gaming.raw_mouse_input",
        "network.tcp_nodelay_ack",
        "network.tcp_autotuning_heuristic",
        "network.disable_lso",
        "privacy.disable_telemetry",
        "privacy.disable_diagtrack_service"
    ];
}

public class DevWorkstationProfile : IProfile
{
    public string Id => "profile.dev_workstation";
    public string Name => "Developer & Power User Preset";
    public string Description => "Optimizes responsiveness, removes UI delays, tunes TCP/network for high packet streaming, SSD TRIM, and cleans telemetry bloat.";
    public string Icon => "DEV";

    public IReadOnlyList<string> TargetTweakIds => [
        "perf.ultimate_power_plan",
        "perf.snappy_ui_effects",
        "perf.enable_ssd_trim",
        "network.tcp_nodelay_ack",
        "network.tcp_autotuning_heuristic",
        "network.flush_dns",
        "privacy.disable_telemetry",
        "privacy.disable_advertising_id",
        "privacy.disable_activity_history",
        "services.optimize_background_junk"
    ];
}

public class UltraPrivacyProfile : IProfile
{
    public string Id => "profile.ultra_privacy";
    public string Name => "Ultra Privacy & Security Preset";
    public string Description => "Comprehensive lockdown on telemetry, diagnostic tracking, advertising IDs, location sensors, Cortana background web search, and feedback.";
    public string Icon => "PRIV";

    public IReadOnlyList<string> TargetTweakIds => [
        "privacy.disable_telemetry",
        "privacy.disable_diagtrack_service",
        "privacy.disable_advertising_id",
        "privacy.disable_activity_history",
        "privacy.disable_cortana",
        "privacy.disable_feedback_prompts",
        "privacy.disable_location_tracking",
        "services.optimize_background_junk"
    ];
}

public class SafeDailyProfile : IProfile
{
    public string Id => "profile.safe_daily";
    public string Name => "Safe Daily Routine Preset";
    public string Description => "100% safe essential optimizations for everyday users: cleans temp cache, trims idle RAM, optimizes snappy UI, SSD TRIM, and disables telemetry.";
    public string Icon => "SAFE";

    public IReadOnlyList<string> TargetTweakIds => [
        "perf.snappy_ui_effects",
        "perf.enable_ssd_trim",
        "privacy.disable_telemetry",
        "privacy.disable_advertising_id",
        "maintenance.clean_temp_files",
        "network.flush_dns"
    ];
}

public class StreamingContentCreatorProfile : IProfile
{
    public string Id => "profile.streaming_content_creator";
    public string Name => "Content Creator & Streaming Preset";
    public string Description => "Smooth performance preset for content creators: GPU priority, System responsiveness, SSD TRIM, and disables background telemetry.";
    public string Icon => "LIVE";

    public IReadOnlyList<string> TargetTweakIds => [
        "perf.ultimate_power_plan",
        "perf.win32_priority_separation",
        "perf.disable_fast_startup_clean_boot",
        "gaming.system_responsiveness",
        "gaming.disable_network_throttling",
        "gaming.gpu_mmcss_priority",
        "gaming.disable_game_dvr",
        "gaming.fse_behavior",
        "network.tcp_nodelay_ack",
        "network.disable_lso",
        "privacy.disable_telemetry",
        "perf.enable_ssd_trim"
    ];
}

public static class ProfileManager
{
    public static readonly IReadOnlyList<IProfile> AllProfiles = [
        new EsportsCompetitiveFpsProfile(),
        new GamingProfile(),
        new StreamingContentCreatorProfile(),
        new DevWorkstationProfile(),
        new UltraPrivacyProfile(),
        new SafeDailyProfile()
    ];

    public static IProfile? GetById(string id)
        => AllProfiles.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
