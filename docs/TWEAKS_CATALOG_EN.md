# 🌸 TROY Tweaks Windows Optimizer Technical Catalog (English - EN_US)

This document provides a comprehensive technical manual for all **58 Native Optimization Modules** included in **TROY Tweaks Windows Optimizer Enterprise**. Each module is designed to be deterministic, isolated, and strictly compliant with **P0 Safety Governance**.

---

## 📑 Table of Contents
1. [Privacy & Telemetry Defense (10 Modules)](#1-privacy--telemetry-defense-10-modules)
2. [Performance, CPU & Storage Optimization (14 Modules)](#2-performance-cpu--storage-optimization-14-modules)
3. [eSports Gaming, Audio & Low-Latency Input (21 Modules)](#3-esports-gaming-audio--low-latency-input-21-modules)
4. [TROY FastRoute, QoS & TCP/IP Network (9 Modules)](#4-troy-fastroute-qos--tcpip-network-9-modules)
5. [Windows Services & Debloater (2 Modules)](#5-windows-services--debloater-2-modules)
6. [Deep System & Browser Junk Cleaner (2 Modules + 7 Categories)](#6-deep-system--browser-junk-cleaner-2-modules--7-categories)
7. [Low-Level NT Kernel MemReduct Pro Engine](#7-low-level-nt-kernel-memreduct-pro-engine)
8. [Startup Manager & Task Scheduler Autoruns](#8-startup-manager--task-scheduler-autoruns)
9. [P0 Safety Governance & System Restore Gate](#9-p0-safety-governance--system-restore-gate)

---

## 1. Privacy & Telemetry Defense (10 Modules)

### 1. `privacy.disable_telemetry` - Disable Windows Telemetry & Diagnostic Data Collection
- **Purpose**: Stops Windows from sending diagnostic logs, app usage habits, and crash dumps to Microsoft telemetry servers.
- **Mechanism**: Sets group policy registry `HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection` (`AllowTelemetry = 0`, `MaxTelemetryAllowed = 0`).
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 2. `privacy.disable_diagtrack_service` - Disable Connected User Experiences & Telemetry Service (DiagTrack)
- **Purpose**: Shuts down the primary background telemetry daemon services in Windows.
- **Mechanism**: Sets the startup type of `DiagTrack` and `dmwappushservice` to `Disabled` and stops active instances.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 3. `privacy.disable_advertising_id` - Disable Advertising ID & Targeted Ads Tracking
- **Purpose**: Prevents Windows and Store applications from building personalized advertising profiles.
- **Mechanism**: Configures `AdvertisingInfo` (`Enabled = 0`) and group policy `DisabledByGroupPolicy = 1`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 4. `privacy.disable_activity_history` - Disable Windows Activity History & Timeline Tracking
- **Purpose**: Stops Windows from recording local file opening timestamps and syncing activities to Microsoft cloud.
- **Mechanism**: Sets Windows System policies `PublishUserActivities = 0`, `UploadUserActivities = 0`, and `EnableActivityFeed = 0`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 5. `privacy.disable_cortana` - Disable Cortana Background Process & Bing Web Search
- **Purpose**: Prevents Start Menu search queries from querying Bing web servers over the internet for local file searches.
- **Mechanism**: Configures `AllowCortana = 0`, `DisableWebSearch = 1`, and `BingSearchEnabled = 0`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 6. `privacy.disable_feedback_prompts` - Disable Windows Feedback Surveys & Notifications
- **Purpose**: Stops intrusive feedback survey popups asking *"How likely are you to recommend Windows?"*.
- **Mechanism**: Sets SIUF period frequency to `0` and `DoNotShowFeedbackNotifications = 1`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 7. `privacy.disable_location_tracking` - Disable Windows Master Location Tracking Sensor
- **Purpose**: Prevents background services and apps from triangulating physical hardware coordinates via Wi-Fi and IP.
- **Mechanism**: Sets `DisableLocation = 1` under `LocationAndSensors` policy.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 8. `privacy.disable_edge_background_bloat` - Disable Microsoft Edge Background Preload & Startup Boost
- **Purpose**: Prevents Microsoft Edge from keeping silent worker processes (`msedge.exe`) running in the background when closed.
- **Mechanism**: Sets `HKLM\SOFTWARE\Policies\Microsoft\Edge` (`StartupBoostEnabled = 0`, `BackgroundModeEnabled = 0`).
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 9. `privacy.disable_consumer_features` - Disable Windows Consumer Features & Auto Cloud App Installs
- **Purpose**: Stops Windows from silently downloading sponsored games (Candy Crush, Disney) onto clean installations.
- **Mechanism**: Configures `CloudContent` (`DisableWindowsConsumerFeatures = 1`).
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 10. `privacy.disable_telemetry_scheduled_tasks` - Disable Windows Telemetry & Compatibility Appraiser Tasks
- **Purpose**: Disables automatic scheduled tasks that wake up CPU and disk to index app compatibility and telemetry data.
- **Mechanism**: Disables tasks under `\Microsoft\Windows\Application Experience\` and `\Customer Experience Improvement Program\`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

---

## 2. Performance, CPU & Storage Optimization (14 Modules)

### 11. `perf.ultimate_performance_plan` - Unlock & Activate Windows Ultimate Performance Power Scheme
- **Purpose**: Unlocks the hidden Windows Workstation power plan, disabling CPU core sleep states and power throttling.
- **Mechanism**: Executes `powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61` and sets it as active.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 12. `perf.win32_priority_separation` - Optimize Win32 Priority Separation for Foreground Applications
- **Purpose**: Allocates maximum CPU quantum slices to the focused active window (games and pro tools).
- **Mechanism**: Sets `HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl\Win32PrioritySeparation = 0x26` (Hex 26 / Decimal 38).
- **Risk Level**: 🔵 **Recommended**.

### 13. `perf.trim_working_set` - Execute Instant Process Working Set Trim & RAM Optimization
- **Purpose**: Flushes idle and unreferenced memory pages from running applications directly to physical standby pool.
- **Mechanism**: Invokes `psapi.dll!EmptyWorkingSet` with process handle isolation safeguards.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 14. `perf.disable_visual_bloat` - Disable Unnecessary Desktop Animations & Visual Effects
- **Purpose**: Disables sluggish window fade, menu slide, and cursor shadow animations for instant responsiveness.
- **Mechanism**: Configures `VisualFXSetting = 2` (Custom/Best Performance) and tunes `UserPreferencesMask`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 15. `perf.disable_hibernation` - Disable Windows Hibernation & Free RAM-Equivalent SSD Space
- **Purpose**: Deletes `hiberfil.sys` and reclaims disk space equal to 75%-100% of installed RAM capacity.
- **Mechanism**: Executes `powercfg -h off` and removes boot hibernation flags.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 16. `perf.disable_fast_startup` - Disable Windows Fast Startup & Hybrid Sleep
- **Purpose**: Resolves motherboard POST hardware initialization glitches, display flickers, and driver cold-boot corruption.
- **Mechanism**: Sets `HiberbootEnabled = 0` in `Control\Session Manager\Power`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 17. `perf.optimize_memory_paging` - Keep Kernel & Drivers Resident in Physical RAM (Disable Paging Executive)
- **Purpose**: Forces core NT kernel subsystems and device drivers to stay in physical RAM instead of being paged out to disk.
- **Mechanism**: Sets `DisablePagingExecutive = 1` and `LargeSystemCache = 0`.
- **Risk Level**: 🔵 **Recommended**.

### 18. `perf.disable_ntfs_8dot3_name_creation` - Disable NTFS 8.3 Short Name Creation & Last Access Time Update
- **Purpose**: Boosts NVMe and SSD filesystem throughput by eliminating legacy DOS 8.3 alias lookups and metadata writes.
- **Mechanism**: Sets `NtfsDisable8dot3NameCreation = 1` and `NtfsDisableLastAccessUpdate = 1`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 19. `perf.enable_ssd_trim` - Enable Native SSD & NVMe TRIM Garbage Collection
- **Purpose**: Ensures the storage controller runs background garbage collection to maintain write speeds over time.
- **Mechanism**: Configures `DisableDeleteNotify = 0` via filesystem registry.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 20. `perf.cpu_core_parking_disable` - Disable CPU Core Parking (Unpark All Intel & AMD Cores)
- **Purpose**: Prevents Windows power governor from parking secondary CPU cores during heavy workloads.
- **Mechanism**: Sets PowerSettings ProcessorSubgroup `ValueMin=100` and `ValueMax=100` across all processor cores.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 21. `perf.intel_cppc_speed_shift` - Optimize Intel Speed Shift & Thread Director (EPP 0)
- **Purpose**: Forces Intel Core processors (including 12th/13th/14th/Ultra Gen P/E-Core hybrid CPUs) to prioritize Performance Cores.
- **Mechanism**: Sets Energy Performance Preference (EPP) to `0` (Maximum Performance) and heterogenous scheduling to P-Core priority.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 22. `perf.amd_ryzen_cppc_x3d_boost` - Optimize AMD Ryzen CPPC & Dynamic 3D V-Cache (X3D) Boost
- **Purpose**: Directs game threads on AMD Ryzen and Ryzen X3D (7800X3D, 7950X3D, 5800X3D) processors to the 3D V-Cache CCD.
- **Mechanism**: Enables CPPC Autonomous Mode v2 and preferred core priority in power schemes.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 23. `perf.nvme_storage_msi_mode` - Enable MSI Mode & High Priority for NVMe / SATA Controllers
- **Purpose**: Switches storage controllers to Message Signaled Interrupts with High Priority to reduce CPU queue overhead.
- **Mechanism**: Sets `MSISupported = 1` and `DevicePriority = 3` on storage class controller entries in `Enum\PCI\`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 24. `perf.gaming_laptop_hybrid_gpu` - Optimize Gaming Laptop Dual-GPU Routing & Power Throttling Bypass
- **Purpose**: Forces competitive games to route directly to discrete NVIDIA GeForce / AMD Radeon dGPU instead of passing through integrated Intel/AMD iGPU.
- **Mechanism**: Configures DirectX graphics preference registry and sets aggressive GPU high-performance flags.
- **Risk Level**: 🔵 **Recommended**.

---

## 3. eSports Gaming, Audio & Low-Latency Input (21 Modules)

### 25. `gaming.system_responsiveness` - Maximize Multimedia & Game CPU Priority (SystemResponsiveness 0%)
- **Purpose**: Eliminates Windows default 20% CPU reservation for background services, giving 100% CPU capacity to games.
- **Mechanism**: Sets `SystemResponsiveness = 0` in `Multimedia\SystemProfile`.
- **Risk Level**: 🔵 **Recommended**.

### 26. `gaming.network_throttling_index` - Disable Windows Network Packet Throttling Index
- **Purpose**: Removes packet throttling caps on network cards during media playback and gaming.
- **Mechanism**: Sets `NetworkThrottlingIndex = 0xFFFFFFFF` (Disabled).
- **Risk Level**: 🔵 **Recommended**.

### 27. `gaming.gpu_priority` - Optimize MMCSS GPU Priority for Games (DirectX & Vulkan)
- **Purpose**: Gives games highest scheduling priority (`Priority 6`, `GPU Priority 8`) in Windows Multimedia Class Scheduler.
- **Mechanism**: Tunes `SystemProfile\Tasks\Games` parameters.
- **Risk Level**: 🔵 **Recommended**.

### 28. `gaming.obs_streaming_gpu_priority` - Optimize OBS Studio & Streaming GPU Priority (Zero Dropped Frames)
- **Purpose**: Guarantees OBS Studio and streaming encoders real-time GPU scheduling access to prevent dropped frames.
- **Mechanism**: Registers OBS Studio in MMCSS with `Scheduling Category = High`.
- **Risk Level**: 🔵 **Recommended**.

### 29. `gaming.disable_gamedvr` - Disable Xbox GameDVR Background Capture & Overlay Latency
- **Purpose**: Disables background game clip recording and Game Bar popups that cause micro-stutters and input lag.
- **Mechanism**: Configures `AppCaptureEnabled = 0` and `GameDVR_Enabled = 0`.
- **Risk Level**: 🔵 **Recommended**.

### 30. `gaming.disable_fullscreen_optimizations` - Optimize Fullscreen Window Display Layer (Low-Latency DWM)
- **Purpose**: Enhances borderless and fullscreen window presentation queues to reduce display input latency.
- **Mechanism**: Configures `GameConfigStore` flags and DirectFlip presentation modes.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 31. `gaming.disable_mpo` - Disable Multi-Plane Overlay (MPO) DWM Stutter & Flicker Fix
- **Purpose**: Resolves desktop flickering, black screens, and Discord/Chrome hardware acceleration stutters on NVIDIA/AMD GPUs.
- **Mechanism**: Sets `OverlayTestMode = 5` in `GraphicsDrivers`.
- **Risk Level**: 🔵 **Recommended**.

### 32. `gaming.enable_hags` - Enable Hardware-Accelerated GPU Scheduling (HAGS Low-Latency)
- **Purpose**: Offloads video memory management directly to GPU dedicated scheduling hardware.
- **Mechanism**: Sets `HwSchMode = 2` in `GraphicsDrivers`.
- **Risk Level**: 🔵 **Recommended**.

### 33. `gaming.disable_hyperv` - Disable Hyper-V Hypervisor Launch (Bare-Metal CPU Gaming Mode)
- **Purpose**: Disables the Type-1 Hypervisor layer so games execute directly on bare-metal hardware without virtualization latency.
- **Mechanism**: Executes `bcdedit /set hypervisorlaunchtype off`.
- **Risk Level**: 🟠 **Advanced (Virtualization disabled)**.

### 34. `gaming.disable_vbs_hvci` - Disable Virtualization-Based Security (VBS) & Memory Integrity (HVCI)
- **Purpose**: Removes CPU-intensive memory page verification checks to unlock 5% - 15% higher 1% Low FPS.
- **Mechanism**: Sets `EnableVirtualizationBasedSecurity = 0` and `HypervisorEnforcedCodeIntegrity = 0`.
- **Risk Level**: 🟠 **Advanced**.

### 35. `gaming.raw_mouse_input` - Disable Windows Mouse Acceleration & Enhance Pointer Precision
- **Purpose**: Disables variable mouse acceleration for 1:1 hardware pixel-perfect aim in competitive FPS games.
- **Mechanism**: Sets `MouseSpeed=0`, `MouseThreshold1=0`, and `MouseThreshold2=0`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 36. `gaming.timer_resolution_low_latency` - Optimize High-Precision Timer Resolution & Force Invariant TSC
- **Purpose**: Locks system timer resolution to 0.5ms precision for maximum frame-time consistency.
- **Mechanism**: Configures `bcdedit /set disabledynamictick yes`, `useplatformclock false`, and `GlobalTimerResolutionRequests=1`.
- **Risk Level**: 🔵 **Recommended**.

### 37. `gaming.nvidia_driver_power_latency` - Optimize NVIDIA GeForce Driver Latency & Disable Telemetry
- **Purpose**: Disables `NvTelemetryContainer` daemons and sets power management to *Prefer Maximum Performance*.
- **Mechanism**: Stops telemetry service and tunes D3D hardware preemption registry.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 38. `gaming.nvidia_rtx_reflex_queue` - Optimize NVIDIA Shader Cache Limit (10GB) & Direct Flip
- **Purpose**: Expands shader cache size to 10GB to eliminate shader compilation stutter in DirectX 11/12 and Vulkan games.
- **Mechanism**: Sets NVIDIA Global `MaxShaderCacheSize = 10240` (10GB) and `PreRenderedFrames = 1`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 39. `gaming.amd_radeon_anti_lag_ulps` - Disable AMD Radeon Ultra Low Power State (ULPS)
- **Purpose**: Prevents AMD Radeon GPUs from downclocking compute units during fast scene transitions.
- **Mechanism**: Sets `EnableUlps = 0` and `EnableUlps_NA = 0` across video class drivers.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 40. `gaming.gpu_msi_mode` - Enable MSI Mode (Message Signaled Interrupts) on GPU
- **Purpose**: Switches GPU interrupt handling to modern vector-based MSI Mode with High Priority.
- **Mechanism**: Sets `MSISupported = 1` and `DevicePriority = 3` on display adapter PCI device registry keys.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 41. `gaming.low_latency_audio_footstep` - Ultra-Low Audio Latency & MMCSS Pro Audio Tuning (Footstep Emas)
- **Purpose**: Tunes Windows audio engine buffer latency and elevates spatial sound processing for clear enemy footstep detection.
- **Mechanism**: Configures MMCSS `Audio` and `Pro Audio` task scheduling (`Clock Rate = 10000`, `Scheduling Category = High`).
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 42. `gaming.mouse_raw_sensor_input` - 1:1 Raw Mouse Sensor Input & Zero-Smoothing
- **Purpose**: Enforces zero Windows desktop pointer smoothing and zero angle snapping.
- **Mechanism**: Sets linear sensitivity tables in `HKCU\Control Panel\Mouse`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 43. `gaming.mouse_markc_fix` - MarkC Windows 11 Mouse Acceleration Linear Curve Fix
- **Purpose**: Applies the renowned MarkC linear curve patch for exact 100% 1-to-1 pixel response at default Windows DPI.
- **Mechanism**: Sets custom binary `SmoothMouseXCurve` and `SmoothMouseYCurve` coordinate matrices.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 44. `gaming.mouse_hid_queue_buffer` - Expand Mouse Driver HID Data Queue Buffer (1000Hz - 8000Hz Anti-Packet Drop)
- **Purpose**: Prevents USB packet drops when using high polling rate gaming mice (1000Hz, 4000Hz, 8000Hz).
- **Mechanism**: Expands `MouseDataQueueSize` from 100 to 256 packets in `mouclass` driver registry.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 45. `gaming.mouse_usb_power_throttling` - Disable USB Power Throttling & Selective Suspend for Gaming Mice
- **Purpose**: Prevents Windows USB power management from putting mouse sensor controller to sleep mid-gameplay.
- **Mechanism**: Disables `SelectiveSuspendEnabled` and USB power throttling policies.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

---

## 4. TROY FastRoute, QoS & TCP/IP Network (9 Modules)

### 46. `network.tcp_nodelay` - Disable Nagle's Algorithm & TCP Delayed ACKs (Lowest Latency Ping)
- **Purpose**: Disables TCP packet bundling, sending small game packets immediately without 200ms ACK delay.
- **Mechanism**: Sets `TcpAckFrequency = 1` and `TCPNoDelay = 1` across all network interface GUIDs.
- **Risk Level**: 🔵 **Recommended**.

### 47. `network.tcp_autotuning` - Optimize TCP Window Auto-Tuning & Disable Chimney Scaling
- **Purpose**: Unlocks modern dynamic socket receive window scaling and disables buggy legacy chimney offload.
- **Mechanism**: Executes `netsh int tcp set global autotuninglevel=normal chimney=disabled`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 48. `network.tcp_congestion_provider` - Configure Modern Low-Latency TCP Congestion Provider (CTCP / CUBIC)
- **Purpose**: Replaces legacy NewReno with modern Compound TCP (CTCP) or CUBIC for fast recovery during packet loss.
- **Mechanism**: Executes `netsh int tcp set supplemental template=custom congestionprovider=ctcp`.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 49. `network.fastroute_qos_dscp46` - TROY FastRoute: Game Packet QoS & DSCP 46 Expedited Forwarding
- **Purpose**: Tags competitive game packets with DSCP 46 (Expedited Forwarding) for top router/ISP queue priority (similar to ExitLag technology).
- **Mechanism**: Sets `Do not use NLA = 1` and configures QoS policy registries for gaming executables.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 50. `network.fastroute_anti_jitter_pacing` - TROY FastRoute: Anti-Jitter UDP Packet Pacing & Bufferbloat Fix
- **Purpose**: Mitigates bufferbloat and packet jitter on high-speed fiber and Wi-Fi networks.
- **Mechanism**: Configures Winsock NonBlockingSendRate and UDP send pacing thresholds.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 51. `network.streaming_network_pacing` - Optimize Live Streaming RTMP/SRT/Discord Network Pacing
- **Purpose**: Prioritizes live streaming bitrate streams to eliminate frame drops on Twitch, YouTube, and Discord.
- **Mechanism**: Configures network interface send queues and media streaming QoS tags.
- **Risk Level**: 🔵 **Recommended**.

### 52. `network.disable_lso` - Disable Large Send Offload (LSO) & Hardware Checksum Delay
- **Purpose**: Prevents network adapter hardware buffer spikes that cause micro packet bursts and rubber-banding.
- **Mechanism**: Disables `*LsoV2IPv4` and `*LsoV2IPv6` across NIC adapter registries.
- **Risk Level**: 🔵 **Recommended**.

### 53. `network.flush_dns` - Flush Windows DNS Resolver Cache
- **Purpose**: Clears stale IP resolution caches to resolve game server connection drops.
- **Mechanism**: Executes `ipconfig /flushdns` and resets DNS client resolver state.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 54. `network.disable_netbios` - Disable NetBIOS over TCP/IP (Reduce LAN Broadcasts)
- **Purpose**: Stops background LAN NetBIOS broadcast traffic on private and gaming networks.
- **Mechanism**: Sets `NetbiosOptions = 2` across active network adapter parameters.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

---

## 5. Windows Services & Debloater (2 Modules)

### 55. `services.optimize_unnecessary_services` - Disable Unnecessary Background Bloat Services
- **Purpose**: Safely disables background services not needed for performance (MapsBroker, RetailDemo, WER, Fax, Biometry where unused).
- **Mechanism**: Configures service startup modes to `Disabled` with whitelist protection for core OS services.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 56. `debloater.remove_uwp_bloatware` - Remove Pre-installed Windows 10/11 UWP Bloatware Apps
- **Purpose**: Uninstalls sponsored bloatware apps (Bing Weather, Solitaire, Xbox TCUI where unused, Clipchamp) to save disk space and RAM.
- **Mechanism**: Executes PowerShell AppX removal scripts targeting non-system packages.
- **Risk Level**: 🔵 **Recommended**.

---

## 6. Deep System & Browser Junk Cleaner (2 Modules + 7 Categories)

### 57. `maintenance.clean_temp_files` - Clean User & System Temporary Cache Files
- **Purpose**: Deletes accumulated cache and temp files from `%TEMP%`, `C:\Windows\Temp`, and browser profiles.
- **Mechanism**: Scans and removes non-locked temporary files with active file-handle protection.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

### 58. `maintenance.clean_windows_update_cache` - Clean Windows Update Download Cache (SoftwareDistribution)
- **Purpose**: Clears cached update packages from `C:\Windows\SoftwareDistribution\Download` to reclaim gigabytes of disk space.
- **Mechanism**: Temporarily halts `wuauserv`, purges the directory, and safely restarts the service.
- **Risk Level**: 🟢 **Safe (100% Safe)**.

---

## 7. Low-Level NT Kernel MemReduct Pro Engine

MemReduct executes direct kernel syscalls via `ntdll.dll!NtSetSystemInformation` with elevated process privileges:

- **Working Set Clean**: Flushes unreferenced memory pages from all userland and system processes (`psapi.dll!EmptyWorkingSet`). Protected with per-process error isolation to prevent crashes against anti-cheat and PPL protected drivers.
- **System File Cache**: Purges buffered NT filesystem cache handles (`SystemFileCacheInformation` = 21).
- **Standby List Cache**: Purges filled standby memory lists (`MemoryPurgeStandbyList` & `MemoryPurgeLowPriorityStandbyList`).
- **Modified Page List**: Flushes dirty memory pages to disk commit pool (`MemoryFlushModifiedList`).
- **Combine Memory Lists**: Triggers page deduplication across system RAM (`MemoryCombineMemoryLists`).
- **Registry Cache**: Flushes unreferenced in-memory registry hives (`MemoryEmptyRegistryCache`).
- **Auto-Reduct Background Worker**: Periodically monitors RAM usage and automatically executes memory trimming when usage surpasses the threshold (e.g. 85%).

---

## 8. Startup Manager & Task Scheduler Autoruns

The Startup Manager provides a comprehensive inspection and control interface for autorun entries across 6 distinct subsystems:

1. **Current User Registry**: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
2. **Local Machine Registry**: `HKLM\Software\Microsoft\Windows\CurrentVersion\Run`
3. **32-Bit Wow6432Node**: `HKLM\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run`
4. **User Startup Folder**: `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`
5. **Common Startup Folder**: `%PROGRAMDATA%\Microsoft\Windows\Start Menu\Programs\Startup`
6. **Task Scheduler Logon Autoruns**: Queries scheduled tasks triggered upon user logon (`schtasks.exe /query`) with 1-Click enable/disable toggling.

---

## 9. P0 Safety Governance & System Restore Gate

All modifications in TROY Tweaks Windows Optimizer adhere strictly to the **P0 Safety Governance**:

- **Windows System Restore Gate**: Native integration with `srclient.dll` to create a verified restore point prior to applying optimization presets.
- **State Snapshot Engine (`SnapshotManager.cs`)**: Captures exact pre-modification registry keys and states into `%LOCALAPPDATA%\NRTX_Optimizer\Snapshots\`.
- **1-Click Emergency Rollback**: Reverts all modified registry values, services, and power policies back to default Windows state.
- **Thread-Safe Structured Audit Logger (`AuditLogger.cs`)**: Records every operation to `%LOCALAPPDATA%\NRTX_Optimizer\Logs\troy_audit.log` with automatic 5MB rotation.
- **100% Anti-Cheat Safe**: Pure OS/driver level configurations without runtime memory hooking or DLL injection; 100% compliant with Riot Vanguard, Easy Anti-Cheat, BattlEye, and Valve Anti-Cheat.

---

<div align="center">

*Authored & verified by **Kaela Kovalskia (Ela 🌸)** for **Rio** (@rioogp).*  
*© 2026 NRTX Labs. All rights reserved.*

</div>
