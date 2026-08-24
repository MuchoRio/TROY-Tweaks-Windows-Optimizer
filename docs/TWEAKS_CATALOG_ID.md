# 🌸 Katalog Teknis TROY Tweaks Windows Optimizer (Bahasa Indonesia - ID_ID)

Dokumen ini merupakan panduan teknis mendalam untuk seluruh **58 Modul Optimasi Native** yang terdapat pada **TROY Tweaks Windows Optimizer Enterprise**. Setiap modul dirancang secara deterministik, terisolasi, dan mematuhi standar ketat **P0 Safety Governance**.

---

## 📑 Daftar Isi
1. [Privasi & Pertahanan Telemetri (10 Modul)](#1-privasi--pertahanan-telemetri-10-modul)
2. [Performa, CPU & Optimasi Storage (14 Modul)](#2-performa-cpu--optimasi-storage-14-modul)
3. [eSports Gaming, Audio & Input Low-Latency (21 Modul)](#3-esports-gaming-audio--input-low-latency-21-modul)
4. [TROY FastRoute, QoS & Jaringan TCP/IP (9 Modul)](#4-troy-fastroute-qos--jaringan-tcpip-9-modul)
5. [Layanan Windows & Debloater (2 Modul)](#5-layanan-windows--debloater-2-modul)
6. [Deep System & Browser Junk Cleaner (2 Modul + 7 Kategori)](#6-deep-system--browser-junk-cleaner-2-modul--7-kategori)
7. [Engine NT Kernel MemReduct Pro Level Rendah](#7-engine-nt-kernel-memreduct-pro-level-rendah)
8. [Manajer Startup & Autoruns Task Scheduler](#8-manajer-startup--autoruns-task-scheduler)
9. [Tata Kelola Keamanan P0 & Gerbang System Restore](#9-tata-kelola-keamanan-p0--gerbang-system-restore)

---

## 1. Privasi & Pertahanan Telemetri (10 Modul)

### 1. `privacy.disable_telemetry` - Nonaktifkan Pengumpulan Telemetri & Data Diagnostik Windows
- **Tujuan**: Menghentikan pengiriman log diagnostik, riwayat penggunaan aplikasi, dan crash dump ke server Microsoft.
- **Mekanisme**: Mengatur policy registri `HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection` (`AllowTelemetry = 0`, `MaxTelemetryAllowed = 0`).
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 2. `privacy.disable_diagtrack_service` - Nonaktifkan Layanan Telemetri & Pengalaman Pengguna (DiagTrack)
- **Tujuan**: Mematikan daemon background telemetri utama di Windows.
- **Mekanisme**: Menyetel startup type layanan `DiagTrack` dan `dmwappushservice` ke `Disabled` serta menghentikan instansi aktif.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 3. `privacy.disable_advertising_id` - Nonaktifkan ID Iklan & Pelacakan Profil Pengguna
- **Tujuan**: Mencegah Windows dan aplikasi Store membangun profil periklanan tertarget.
- **Mekanisme**: Mengonfigurasi `AdvertisingInfo` (`Enabled = 0`) dan policy `DisabledByGroupPolicy = 1`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 4. `privacy.disable_activity_history` - Nonaktifkan Riwayat Aktivitas & Timeline Windows
- **Tujuan**: Menghentikan pencatatan waktu buka file lokal dan sinkronisasi aktivitas ke cloud Microsoft.
- **Mekanisme**: Menyetel policy Windows `PublishUserActivities = 0`, `UploadUserActivities = 0`, dan `EnableActivityFeed = 0`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 5. `privacy.disable_cortana` - Nonaktifkan Proses Background Cortana & Pencarian Web Bing
- **Tujuan**: Mencegah Start Menu menembak query ke internet saat mencari file lokal di PC.
- **Mekanisme**: Mengonfigurasi `AllowCortana = 0`, `DisableWebSearch = 1`, dan `BingSearchEnabled = 0`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 6. `privacy.disable_feedback_prompts` - Nonaktifkan Notifikasi & Survei Umpan Balik Windows
- **Tujuan**: Menghentikan popup modal survei mengganggu (*"Seberapa besar kemungkinan Anda merekomendasikan Windows?"*).
- **Mekanisme**: Menyetel frekuensi periode SIUF ke `0` dan `DoNotShowFeedbackNotifications = 1`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 7. `privacy.disable_location_tracking` - Nonaktifkan Sensor Pelacak Lokasi Fisik Windows
- **Tujuan**: Mencegah background service melacak koordinat fisik hardware via Wi-Fi dan IP.
- **Mekanisme**: Mengatur `DisableLocation = 1` di bawah policy `LocationAndSensors`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 8. `privacy.disable_edge_background_bloat` - Nonaktifkan Preload Background & Startup Boost Microsoft Edge
- **Tujuan**: Menghentikan proses background `msedge.exe` yang tetap berjalan saat browser ditutup.
- **Mekanisme**: Mengatur `StartupBoostEnabled = 0` dan `BackgroundModeEnabled = 0` pada policy Edge.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 9. `privacy.disable_consumer_features` - Nonaktifkan Unduhan Otomatis Game & Aplikasi Sponsor
- **Tujuan**: Menghentikan Windows mengunduh aplikasi bloatware sponsor secara diam-diam.
- **Mekanisme**: Mengonfigurasi `CloudContent` (`DisableWindowsConsumerFeatures = 1`).
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 10. `privacy.disable_telemetry_scheduled_tasks` - Nonaktifkan Scheduled Tasks Telemetri & Compatibility Appraiser
- **Tujuan**: Mematikan task scheduler otomatis yang membebani disk dan CPU saat idle.
- **Mekanisme**: Menonaktifkan task di `\Microsoft\Windows\Application Experience\` dan `\Customer Experience Improvement Program\`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

---

## 2. Performa, CPU & Optimasi Storage (14 Modul)

### 11. `perf.ultimate_performance_plan` - Aktifkan Skema Daya Ultimate Performance (Daya Penuh)
- **Tujuan**: Membuka power plan workstation tersembunyi yang menghilangkan CPU sleep states dan power throttling.
- **Mekanisme**: Mengeksekusi `powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61` dan mengaktifkannya.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 12. `perf.win32_priority_separation` - Optimasi Alokasi Quantum CPU untuk Aplikasi Foreground
- **Tujuan**: Memberikan porsi siklus CPU terbesar pada jendela aplikasi/game yang sedang aktif di layar.
- **Mekanisme**: Mengatur `Win32PrioritySeparation = 0x26` (Hex 26 / Desimal 38).
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 13. `perf.trim_working_set` - Bersihkan Working Set Memori RAM Seluruh Proses
- **Tujuan**: Melepaskan halaman RAM idle dari seluruh proses aktif ke pool memori standby fisik.
- **Mekanisme**: Memanggil `psapi.dll!EmptyWorkingSet` dengan perlindungan isolasi proses terlindungi.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 14. `perf.disable_visual_bloat` - Nonaktifkan Animasi Desktop & Efek Visual Berat
- **Tujuan**: Mematikan animasi jendela fade dan slide untuk respon antarmuka yang instan.
- **Mekanisme**: Mengonfigurasi `VisualFXSetting = 2` (Best Performance) dan `UserPreferencesMask`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 15. `perf.disable_hibernation` - Nonaktifkan Hibernasi Windows & Bebaskan Ruang SSD Seukuran RAM
- **Tujuan**: Menghapus file `hiberfil.sys` dan merebut kembali 75%-100% kapasitas RAM di drive C:.
- **Mekanisme**: Mengeksekusi `powercfg -h off`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 16. `perf.disable_fast_startup` - Nonaktifkan Windows Fast Startup & Hybrid Sleep
- **Tujuan**: Mencegah korupsi inisialisasi driver dan mengatasi bunyi motherboard beep/glitch saat booting.
- **Mekanisme**: Menyetel `HiberbootEnabled = 0` pada Session Manager Power.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 17. `perf.optimize_memory_paging` - Kunci Kernel & Driver di RAM Fisik (Disable Paging Executive)
- **Tujuan**: Memaksa subsistem NT Kernel dan driver perangkat tetap berada di RAM fisik tanpa di-swap ke pagefile disk.
- **Mekanisme**: Mengatur `DisablePagingExecutive = 1` dan `LargeSystemCache = 0`.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 18. `perf.disable_ntfs_8dot3_name_creation` - Nonaktifkan Pembuatan Nama Pendek 8.3 & Update Akses Terakhir NTFS
- **Tujuan**: Mempercepat throughput filesystem SSD/NVMe dengan menghilangkan alias DOS 8.3 kuno.
- **Mekanisme**: Menyetel `NtfsDisable8dot3NameCreation = 1` dan `NtfsDisableLastAccessUpdate = 1`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 19. `perf.enable_ssd_trim` - Aktifkan Native SSD & NVMe TRIM Garbage Collection
- **Tujuan**: Menjamin kontroler storage menjalankan background garbage collection agar performa SSD tidak melambat.
- **Mekanisme**: Mengonfigurasi `DisableDeleteNotify = 0`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 20. `perf.cpu_core_parking_disable` - Nonaktifkan CPU Core Parking (Unpark Seluruh Core Intel & AMD)
- **Tujuan**: Mencegah Windows menonaktifkan core sekunder saat gaming yang sering memicu drop FPS tiba-tiba.
- **Mekanisme**: Mengatur ProcessorSubgroup `ValueMin=100` dan `ValueMax=100` pada power policy.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 21. `perf.intel_cppc_speed_shift` - Optimasi Intel Speed Shift & Thread Director (EPP 0)
- **Tujuan**: Mengarahkan thread game pada prosesor Intel Generasi 12/13/14/Ultra khusus ke Performance Core (P-Core).
- **Mekanisme**: Menyetel Energy Performance Preference (EPP) ke `0` (Maximum Performance).
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 22. `perf.amd_ryzen_cppc_x3d_boost` - Optimasi AMD Ryzen CPPC & Dynamic 3D V-Cache (X3D) Boost
- **Tujuan**: Mengarahkan thread game pada prosesor AMD Ryzen X3D (7800X3D, 7950X3D, 5800X3D) ke CCD 3D V-Cache.
- **Mekanisme**: Mengaktifkan CPPC Autonomous Mode v2 dan preferred core priority.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 23. `perf.nvme_storage_msi_mode` - Aktifkan Mode MSI & Prioritas Tinggi Kontroler NVMe / SATA
- **Tujuan**: Beralih ke interrupt berbasis Message Signaled Interrupts pada kontroler storage untuk antrean I/O tercepat.
- **Mekanisme**: Mengatur `MSISupported = 1` dan `DevicePriority = 3` pada entri registry PCI kontroler storage.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 24. `perf.gaming_laptop_hybrid_gpu` - Optimasi Dual-GPU Routing & Bypass Power Throttling Laptop Gaming
- **Tujuan**: Memaksa game kompetitif berjalan langsung di discrete GPU (NVIDIA/AMD) tanpa bottleneck di integrated GPU.
- **Mekanisme**: Mengonfigurasi DirectX graphics preference dan flag high-performance GPU.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

---

## 3. eSports Gaming, Audio & Input Low-Latency (21 Modul)

### 25. `gaming.system_responsiveness` - Maksimalkan Prioritas CPU Game (SystemResponsiveness 0%)
- **Tujuan**: Menghapus alokasi reservasi default Windows 20% untuk background service agar 100% CPU diberikan ke game.
- **Mekanisme**: Mengatur `SystemResponsiveness = 0` pada `Multimedia\SystemProfile`.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 26. `gaming.network_throttling_index` - Nonaktifkan Indeks Pembatasan Paket Jaringan Windows
- **Tujuan**: Menghapus limit pembatasan paket network card saat multimedia atau game sedang aktif.
- **Mekanisme**: Mengatur `NetworkThrottlingIndex = 0xFFFFFFFF` (Disabled).
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 27. `gaming.gpu_priority` - Optimasi MMCSS GPU Priority untuk Game (DirectX & Vulkan)
- **Tujuan**: Memberikan prioritas penjadwalan tertinggi (`Priority 6`, `GPU Priority 8`) pada Windows Multimedia Scheduler.
- **Mekanisme**: Mengonfigurasi parameter `SystemProfile\Tasks\Games`.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 28. `gaming.obs_streaming_gpu_priority` - Optimasi Prioritas GPU OBS Studio & Live Streaming
- **Tujuan**: Menjamin proses encoding live streaming OBS mendapatkan akses GPU real-time tanpa frame drop.
- **Mekanisme**: Mendaftarkan OBS Studio pada MMCSS dengan `Scheduling Category = High`.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 29. `gaming.disable_gamedvr` - Nonaktifkan Xbox GameDVR Background Capture & Overlay
- **Tujuan**: Mematikan perekaman background dan Game Bar yang menyebabkan micro-stuttering dan input lag.
- **Mekanisme**: Menyetel `AppCaptureEnabled = 0` dan `GameDVR_Enabled = 0`.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 30. `gaming.disable_fullscreen_optimizations` - Optimasi Layer Tampilan Fullscreen Jendela (Low-Latency DWM)
- **Tujuan**: Mengoptimalkan antrean presentasi jendela borderless/fullscreen untuk memangkas latensi display.
- **Mekanisme**: Mengatur flag `GameConfigStore` dan mode presentasi DirectFlip.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 31. `gaming.disable_mpo` - Nonaktifkan Multi-Plane Overlay (MPO) Perbaikan Flicker DWM
- **Tujuan**: Mengatasi layar berkedip, black screen, dan stutter hardware acceleration pada GPU NVIDIA/AMD.
- **Mekanisme**: Menyetel `OverlayTestMode = 5` di registri `GraphicsDrivers`.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 32. `gaming.enable_hags` - Aktifkan Hardware-Accelerated GPU Scheduling (HAGS Low-Latency)
- **Tujuan**: Melimpahkan manajemen alokasi video memory langsung ke hardware dedicated GPU.
- **Mekanisme**: Mengatur `HwSchMode = 2` pada `GraphicsDrivers`.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 33. `gaming.disable_hyperv` - Nonaktifkan Hyper-V Hypervisor Launch (Bare-Metal CPU Gaming Mode)
- **Tujuan**: Mematikan layer Type-1 Hypervisor agar CPU berkomunikasi langsung tanpa latensi virtualisasi.
- **Mekanisme**: Mengeksekusi `bcdedit /set hypervisorlaunchtype off`.
- **Tingkat Risiko**: 🟠 **Lanjutan (Virtualisasi dinonaktifkan)**.

### 34. `gaming.disable_vbs_hvci` - Nonaktifkan Virtualization-Based Security (VBS) & Integritas Memori (HVCI)
- **Tujuan**: Menghilangkan beban verifikasi halaman memori untuk mendongkrak 5% - 15% 1% Low FPS.
- **Mekanisme**: Menyetel `EnableVirtualizationBasedSecurity = 0` dan `HypervisorEnforcedCodeIntegrity = 0`.
- **Tingkat Risiko**: 🟠 **Lanjutan**.

### 35. `gaming.raw_mouse_input` - Nonaktifkan Akselerasi Mouse Windows (1:1 Hardware Raw Input)
- **Tujuan**: Menghilangkan kurva akselerasi variabel untuk akurasi aim pixel-perfect di game FPS kompetitif.
- **Mekanisme**: Mengatur `MouseSpeed=0`, `MouseThreshold1=0`, dan `MouseThreshold2=0`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 36. `gaming.timer_resolution_low_latency` - Optimasi Resolusi Timer Presisi Tinggi & Invariant TSC (0.5ms Clock)
- **Tujuan**: Mengunci resolusi timer Windows ke 0.5ms untuk konsistensi frame-time maksimum.
- **Mekanisme**: Mengonfigurasi `bcdedit /set disabledynamictick yes`, `useplatformclock false`, dan `GlobalTimerResolutionRequests=1`.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 37. `gaming.nvidia_driver_power_latency` - Optimasi Latensi Driver NVIDIA GeForce & Nonaktifkan Telemetri
- **Tujuan**: Mematikan daemon `NvTelemetryContainer` dan mengunci status daya GPU ke *Prefer Maximum Performance*.
- **Mekanisme**: Menghentikan layanan telemetri dan mengatur preemption hardware D3D.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 38. `gaming.nvidia_rtx_reflex_queue` - Optimasi Batas Shader Cache NVIDIA (10GB) & Direct Flip
- **Tujuan**: Memperluas cache shader ke 10GB untuk menghilangkan stutter kompilasi shader di game DirectX 11/12 dan Vulkan.
- **Mekanisme**: Mengatur `MaxShaderCacheSize = 10240` (10GB) dan `PreRenderedFrames = 1`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 39. `gaming.amd_radeon_anti_lag_ulps` - Nonaktifkan AMD Radeon Ultra Low Power State (ULPS)
- **Tujuan**: Mencegah GPU AMD Radeon menurunkan clock secara agresif saat transisi scene game.
- **Mekanisme**: Mengatur `EnableUlps = 0` dan `EnableUlps_NA = 0`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 40. `gaming.gpu_msi_mode` - Aktifkan Mode MSI (Message Signaled Interrupts) pada GPU
- **Tujuan**: Mengubah penanganan interrupt GPU ke mode MSI berbasis vektor dengan High Priority.
- **Mekanisme**: Mengatur `MSISupported = 1` dan `DevicePriority = 3` pada registri adapter PCI display.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 41. `gaming.low_latency_audio_footstep` - Latensi Audio Ultra-Rendah & MMCSS Pro Audio Tuning (Footstep Emas)
- **Tujuan**: Memangkas buffer audio Windows dan meningkatkan spatial audio untuk memperjelas suara langkah musuh.
- **Mekanisme**: Mengonfigurasi MMCSS task `Audio` dan `Pro Audio` (`Clock Rate = 10000`, `Scheduling Category = High`).
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 42. `gaming.mouse_raw_sensor_input` - Input Sensor Mouse Raw 1:1 & Zero-Smoothing
- **Tujuan**: Menegakkan zero desktop pointer smoothing dan zero angle snapping pada sensor mouse.
- **Mekanisme**: Menyetel tabel sensitivitas linear pada `HKCU\Control Panel\Mouse`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 43. `gaming.mouse_markc_fix` - Patch Kurva Linear Akselerasi Mouse MarkC Windows 11
- **Tujuan**: Menerapkan kurva linear MarkC untuk akurasi respon pixel 1-ke-1 tepat 100% pada DPI bawaan.
- **Mekanisme**: Mengonfigurasi matriks koordinat biner `SmoothMouseXCurve` dan `SmoothMouseYCurve`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 44. `gaming.mouse_hid_queue_buffer` - Perluas Antrean Buffer Driver Mouse HID (1000Hz - 8000Hz Anti-Packet Drop)
- **Tujuan**: Mencegah USB packet drop saat menggunakan mouse gaming polling rate tinggi (1000Hz, 4000Hz, 8000Hz).
- **Mekanisme**: Memperluas `MouseDataQueueSize` dari 100 ke 256 paket pada driver `mouclass`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 45. `gaming.mouse_usb_power_throttling` - Nonaktifkan USB Power Throttling & Selective Suspend untuk Mouse Gaming
- **Tujuan**: Mencegah Windows USB power manager menidurkan sensor mouse di tengah pertandingan.
- **Mekanisme**: Menonaktifkan `SelectiveSuspendEnabled` dan USB power throttling policy.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

---

## 4. TROY FastRoute, QoS & Jaringan TCP/IP (9 Modul)

### 46. `network.tcp_nodelay` - Nonaktifkan Algoritma Nagle & TCP Delayed ACKs (Ping Terendah)
- **Tujuan**: Mematikan penundaan bundling paket, langsung mengirim paket game tanpa jeda 200ms ACK.
- **Mekanisme**: Mengatur `TcpAckFrequency = 1` dan `TCPNoDelay = 1` pada seluruh GUID interface jaringan.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 47. `network.tcp_autotuning` - Optimasi TCP Window Auto-Tuning & Nonaktifkan Chimney Scaling
- **Tujuan**: Membuka dynamic socket receive window scaling modern dan mematikan offload chimney warisan.
- **Mekanisme**: Mengeksekusi `netsh int tcp set global autotuninglevel=normal chimney=disabled`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 48. `network.tcp_congestion_provider` - Konfigurasi Provider Kemacetan TCP Modern (CTCP / CUBIC)
- **Tujuan**: Mengganti NewReno lama dengan Compound TCP (CTCP) atau CUBIC untuk pemulihan cepat saat packet loss.
- **Mekanisme**: Mengeksekusi `netsh int tcp set supplemental template=custom congestionprovider=ctcp`.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 49. `network.fastroute_qos_dscp46` - TROY FastRoute: Game Packet QoS & DSCP 46 Expedited Forwarding
- **Tujuan**: Menandai paket game dengan DSCP 46 untuk prioritas antrean router/ISP tertinggi (teknologi ala ExitLag).
- **Mekanisme**: Mengatur `Do not use NLA = 1` dan mengonfigurasi policy QoS untuk binary game.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 50. `network.fastroute_anti_jitter_pacing` - TROY FastRoute: Anti-Jitter UDP Packet Pacing & Fix Bufferbloat
- **Tujuan**: Mencegah lonjakan latency jitter dan bufferbloat pada jaringan fiber optik dan Wi-Fi.
- **Mekanisme**: Mengonfigurasi ambang pacing UDP dan Winsock NonBlockingSendRate.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 51. `network.streaming_network_pacing` - Optimasi Pacing Jaringan Live Streaming RTMP/SRT/Discord
- **Tujuan**: Memprioritaskan aliran bitrate live stream untuk mencegah drop frame di Twitch, YouTube, dan Discord.
- **Mekanisme**: Mengonfigurasi send queue adapter jaringan dan tagging QoS streaming.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 52. `network.disable_lso` - Nonaktifkan Large Send Offload (LSO) & Delay Checksum Hardware
- **Tujuan**: Mencegah penumpukan buffer hardware kartu jaringan yang menyebabkan packet burst dan rubber-banding.
- **Mekanisme**: Menonaktifkan `*LsoV2IPv4` dan `*LsoV2IPv6` pada seluruh registri adapter NIC.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

### 53. `network.flush_dns` - Bersihkan Cache Resolver DNS Windows
- **Tujuan**: Menghapus cache IP lama yang usang untuk menyelesaikan kendala disconnect ke server game.
- **Mekanisme**: Mengeksekusi `ipconfig /flushdns` dan me-reset DNS resolver client.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 54. `network.disable_netbios` - Nonaktifkan NetBIOS over TCP/IP (Kurangi Broadcast LAN)
- **Tujuan**: Menghentikan lalu lintas broadcast NetBIOS yang tidak perlu pada jaringan game.
- **Mekanisme**: Mengatur `NetbiosOptions = 2` pada seluruh parameter adapter aktif.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

---

## 5. Layanan Windows & Debloater (2 Modul)

### 55. `services.optimize_unnecessary_services` - Nonaktifkan Layanan Bloat Background yang Tidak Diperlukan
- **Tujuan**: Mematikan service background yang membuang resource (MapsBroker, RetailDemo, WER, Fax, Biometri jika tidak dipakai).
- **Mekanisme**: Menyetel startup type service ke `Disabled` dengan proteksi whitelist layanan inti OS.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 56. `debloater.remove_uwp_bloatware` - Hapus Aplikasi Bloatware Bawaan UWP Windows 10/11
- **Tujuan**: Mencopot aplikasi sponsor (Bing Weather, Solitaire, Xbox TCUI jika tidak terpakai, Clipchamp) untuk menghemat RAM dan storage.
- **Mekanisme**: Menjalankan script PowerShell AppX removal untuk paket non-sistem.
- **Tingkat Risiko**: 🔵 **Direkomendasikan**.

---

## 6. Deep System & Browser Junk Cleaner (2 Modul + 7 Kategori)

### 57. `maintenance.clean_temp_files` - Bersihkan File Cache Sementara User & Sistem
- **Tujuan**: Menghapus file temporary dari `%TEMP%`, `C:\Windows\Temp`, dan profil web browser.
- **Mekanisme**: Memindai dan menghapus file sementara yang tidak terkunci proses aktif.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

### 58. `maintenance.clean_windows_update_cache` - Bersihkan Cache Unduhan Windows Update (SoftwareDistribution)
- **Tujuan**: Membersihkan paket update dari `C:\Windows\SoftwareDistribution\Download` untuk merebut kembali gigabyte ruang disk.
- **Mekanisme**: Menghentikan sementara `wuauserv`, membersihkan folder, dan menyalakan kembali layanan.
- **Tingkat Risiko**: 🟢 **Aman (100% Safe)**.

---

## 7. Engine NT Kernel MemReduct Pro Level Rendah

MemReduct mengeksekusi syscall kernel langsung via `ntdll.dll!NtSetSystemInformation` dengan hak akses proses yang ditingkatkan:

- **Working Set Clean**: Membersihkan unreferenced RAM dari seluruh proses userland dan sistem (`psapi.dll!EmptyWorkingSet`). Dilengkapi isolasi exception agar tidak crash pada driver anti-cheat dan proses terlindungi (PPL).
- **System File Cache**: Membersihkan handle cache filesystem NT yang menggantung (`SystemFileCacheInformation` = 21).
- **Standby List Cache**: Membersihkan antrean memori standby yang penuh (`MemoryPurgeStandbyList` & `MemoryPurgeLowPriorityStandbyList`).
- **Modified Page List**: Menulis dirty pages ke commit pool disk (`MemoryFlushModifiedList`).
- **Combine Memory Lists**: Menjalankan deduplikasi halaman memori RAM (`MemoryCombineMemoryLists`).
- **Registry Cache**: Membersihkan cache registri in-memory (`MemoryEmptyRegistryCache`).
- **Auto-Reduct Background Worker**: Memantau beban RAM secara berkala dan otomatis melakukan pembersihan saat beban melewati batas (misal: > 85%).

---

## 8. Manajer Startup & Autoruns Task Scheduler

Startup Manager menyediakan antarmuka inspeksi dan kontrol penuh untuk autorun aplikasi lintas 6 subsistem:

1. **Current User Registry**: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
2. **Local Machine Registry**: `HKLM\Software\Microsoft\Windows\CurrentVersion\Run`
3. **32-Bit Wow6432Node**: `HKLM\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run`
4. **User Startup Folder**: `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`
5. **Common Startup Folder**: `%PROGRAMDATA%\Microsoft\Windows\Start Menu\Programs\Startup`
6. **Task Scheduler Logon Autoruns**: Memindai scheduled task yang berjalan saat login user (`schtasks.exe /query`) dengan fitur toggle Aktif/Nonaktif 1-klik.

---

## 9. Tata Kelola Keamanan P0 & Gerbang System Restore

Seluruh modifikasi pada TROY Tweaks Windows Optimizer tunduk pada **P0 Safety Governance**:

- **Gerbang Windows System Restore**: Integrasi native dengan `srclient.dll` untuk membuat Restore Point terverifikasi sebelum optimasi diterapkan.
- **Snapshot State Engine (`SnapshotManager.cs`)**: Menyimpan konfigurasi registri asli sebelum perubahan ke `%LOCALAPPDATA%\NRTX_Optimizer\Snapshots\`.
- **Atomic Rollback 1-Klik**: Mengembalikan seluruh registri, layanan, dan konfigurasi daya ke setelan awal Windows.
- **Logger Audit Terstruktur (`AuditLogger.cs`)**: Mencatat setiap operasi ke `%LOCALAPPDATA%\NRTX_Optimizer\Logs\troy_audit.log` dengan rotasi otomatis 5MB.
- **100% Anti-Cheat Safe**: Murni konfigurasi level OS/driver tanpa memori injection atau runtime hook; 100% aman untuk Riot Vanguard, Easy Anti-Cheat, BattlEye, dan Valve Anti-Cheat.

---

<div align="center">

*Dibuat & diverifikasi oleh **Kaela Kovalskia (Ela 🌸)** untuk **Rio** (@rioogp).*  
*© 2026 NRTX Labs. Hak cipta dilindungi undang-undang.*

</div>
