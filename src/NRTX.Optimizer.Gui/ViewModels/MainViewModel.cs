using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Engine;
using NRTX.Optimizer.Core.Localization;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Modules.Maintenance;
using NRTX.Optimizer.Core.Modules.Network;
using NRTX.Optimizer.Core.Native;
using NRTX.Optimizer.Core.Profiles;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Gui.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly TweakRegistry _registry = new();
    private readonly ExecutionEngine _engine = new();
    private readonly SystemDiagnosticEngine _diagnostics;
    private readonly GameBoosterWatcher _gameWatcher = new();
    private readonly DispatcherTimer _liveMonitorTimer = new();

    [ObservableProperty]
    private string _selectedNav = "Dashboard";

    [ObservableProperty]
    private SystemSpecs _specs = new();

    [ObservableProperty]
    private DetailedMemoryStats _memStats = new();

    [ObservableProperty]
    private TimerResolutionInfo _timerResolution = new(15.625, 0.5, 15.625, false);

    [ObservableProperty]
    private DpcLatencyInfo _dpcLatency = new(18.5, 45.0, 22.0, "🟢 eSports Ready (< 150µs)", "#10b981");

    // TROY FastRoute Radar
    public ObservableCollection<GameServerPingResult> PingRadarResults { get; } = [];

    [ObservableProperty]
    private bool _isPingingRadar;

    [ObservableProperty]
    private string _radarStatusText = "Click 'Scan Radar' to test regional game server routes";

    [ObservableProperty]
    private string _bestServerRoute = "🇸🇬 Singapore (SEA)";

    // Smart eSports Audio & Headset Advisor
    [ObservableProperty]
    private AudioDeviceInfo _audioInfo = new();

    // Smart eSports Mouse & Sensor Advisor
    [ObservableProperty]
    private MouseDeviceInfo _mouseInfo = new();

    [ObservableProperty]
    private MousePollingStats _mousePolling = new();

    private readonly MousePollingMonitor _mousePollingMonitor = new();

    [ObservableProperty]
    private int _healthScore = 50;

    [ObservableProperty]
    private string _healthScoreStatus = "Needs Optimization";

    [ObservableProperty]
    private string _healthScoreColor = "#f43f5e";

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private double _scanProgress;

    [ObservableProperty]
    private string _scanStatusText = "Ready to scan";

    [ObservableProperty]
    private string _healthScanButtonText = "🔍 Run Health Scan";

    [ObservableProperty]
    private int _activeTweaksCount;

    [ObservableProperty]
    private int _totalTweaksCount;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private string _selectedCategoryFilter = "All";

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    // Deep Junk Cleaner
    [ObservableProperty]
    private string _totalJunkFormatted = "0 MB";

    [ObservableProperty]
    private int _totalJunkFiles = 0;

    // Startup Manager
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartupTotalAppsSummary))]
    private int _totalStartupCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StartupEnabledAppsSummary))]
    private int _enabledStartupCount;

    // Game Booster
    [ObservableProperty]
    private bool _isGameBoosterActive;

    [ObservableProperty]
    private string _activeGameStatus = "Idle (Monitoring Competitive Games)";

    // MemReduct Settings
    [ObservableProperty]
    private bool _cleanWorkingSet = true;

    [ObservableProperty]
    private bool _cleanSystemFileCache = true;

    [ObservableProperty]
    private bool _cleanStandbyList = true;

    [ObservableProperty]
    private bool _cleanModifiedPageList = true;

    [ObservableProperty]
    private bool _combineMemoryLists = true;

    [ObservableProperty]
    private bool _cleanRegistryCache = true;

    [ObservableProperty]
    private bool _enableAutoReduct = false;

    [ObservableProperty]
    private int _autoReductThreshold = 85;

    [ObservableProperty]
    private string _lastMemCleanSummary = "No clean performed yet.";

    public ObservableCollection<TweakItemViewModel> AllTweaks { get; } = [];
    public ObservableCollection<TweakItemViewModel> FilteredTweaks { get; } = [];
    public ObservableCollection<ProfileItemViewModel> Profiles { get; } = [];
    public ObservableCollection<JunkItemViewModel> JunkReports { get; } = [];
    public ObservableCollection<StartupItemViewModel> StartupEntries { get; } = [];
    public ObservableCollection<string> LogEntries { get; } = [];

    public MainViewModel()
    {
        _diagnostics = new SystemDiagnosticEngine(_registry);
        _engine.OnLog += msg => Application.Current?.Dispatcher?.Invoke(() =>
        {
            LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {msg}");
            StatusMessage = msg;
        });

        _engine.OnProgress += (msg, val) => Application.Current?.Dispatcher?.Invoke(() =>
        {
            StatusMessage = msg;
            ProgressValue = val * 100;
        });

        _gameWatcher.OnGameBoostStateChanged += (game, isActive) => Application.Current?.Dispatcher?.Invoke(() =>
        {
            if (isActive)
            {
                ActiveGameStatus = $"🎮 Boosting: {game} (High Priority Active)";
                LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] 🚀 Game Booster activated for {game}.");
            }
            else
            {
                ActiveGameStatus = "Idle (Monitoring Competitive Games)";
            }
        });

        InitializeProfiles();
        InitializeTweaks();
        SetupLiveMonitor();
    }

    partial void OnSpecsChanged(SystemSpecs value)
    {
        NotifyAllUiProperties();
    }

    private void SetupLiveMonitor()
    {
        _liveMonitorTimer.Interval = TimeSpan.FromSeconds(2);
        _liveMonitorTimer.Tick += async (s, e) =>
        {
            MemStats = MemReductEngine.GetStats();
            TimerResolution = SystemTimerService.GetTimerResolution();
            DpcLatency = DpcLatencyMonitorService.SampleLatency();

            // Auto-Reduct Check
            if (EnableAutoReduct && MemStats.PhysicalUsagePercent >= AutoReductThreshold)
            {
                await DeepCleanMemoryAsync(isAuto: true);
            }
        };
        _liveMonitorTimer.Start();
    }

    private void InitializeProfiles()
    {
        foreach (var profile in ProfileManager.AllProfiles)
        {
            Profiles.Add(new ProfileItemViewModel(profile));
        }
    }

    private void InitializeTweaks()
    {
        foreach (var tweak in _registry.AllTweaks)
        {
            var vm = new TweakItemViewModel(tweak);
            AllTweaks.Add(vm);
            FilteredTweaks.Add(vm);
        }
        TotalTweaksCount = AllTweaks.Count;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsLoading = true;
        StatusMessage = "Analyzing system parameters...";
        try
        {
            Specs = await SystemSpecs.CollectAsync();
            MemStats = MemReductEngine.GetStats();
            TimerResolution = SystemTimerService.GetTimerResolution();
            AudioInfo = AudioInfoService.GetDefaultAudioDeviceInfo();
            MouseInfo = MouseInfoService.GetDefaultMouseDeviceInfo();
            MousePolling = _mousePollingMonitor.CurrentStats;
            NotifyAllUiProperties();
            await RefreshAllTweaksStateAsync();
            await ScanJunkAsync();
            await RefreshStartupItemsAsync();
            _ = ScanPingRadarAsync();
        }
        catch (Exception ex)
        {
            LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] Initialization error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StatusMessage = "Ready";
        }
    }

    [ObservableProperty]
    private string _currentLanguage = "en-US";

    [RelayCommand]
    public void SetNavigation(string nav)
    {
        SelectedNav = nav;
    }

    [RelayCommand]
    public void SwitchLanguage(string langCode)
    {
        CurrentLanguage = langCode;
        if (langCode.Equals("id", StringComparison.OrdinalIgnoreCase) || langCode.Equals("id-ID", StringComparison.OrdinalIgnoreCase))
        {
            LocalizationManager.SetLanguage(AppLanguage.Indonesian);
            StatusMessage = "Bahasa diubah ke Bahasa Indonesia (ID).";
        }
        else
        {
            LocalizationManager.SetLanguage(AppLanguage.English);
            StatusMessage = "Language switched to English (US).";
        }
        NotifyAllUiProperties();
        ApplyFilter();
    }

    partial void OnIsGameBoosterActiveChanged(bool value)
    {
        if (value)
        {
            _gameWatcher.Start();
            ActiveGameStatus = "🟢 Active (Monitoring Valorant & Games)";
            StatusMessage = "Smart Game Booster daemon activated.";
        }
        else
        {
            _gameWatcher.Stop();
            ActiveGameStatus = "Disabled";
            StatusMessage = "Smart Game Booster daemon stopped.";
        }
    }

    [RelayCommand]
    public void OptimizeTimerResolutionNow()
    {
        bool ok = SystemTimerService.RequestDesiredResolution(5000); // 0.5ms
        TimerResolution = SystemTimerService.GetTimerResolution();
        StatusMessage = ok ? "⚡ 0.5000ms Global Kernel Timer Resolution active & locked." : "Timer resolution request completed.";
    }

    [RelayCommand]
    public async Task RefreshStartupItemsAsync()
    {
        try
        {
            StartupEntries.Clear();
            var list = await StartupManagerEngine.GetStartupEntriesAsync();
            int enabled = 0;
            foreach (var item in list)
            {
                if (item.IsEnabled) enabled++;
                var vm = new StartupItemViewModel(item);
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(StartupItemViewModel.IsEnabled))
                    {
                        UpdateStartupCounts();
                    }
                };
                StartupEntries.Add(vm);
            }
            TotalStartupCount = list.Count;
            EnabledStartupCount = enabled;
        }
        catch (Exception ex)
        {
            LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] Failed to scan startup items: {ex.Message}");
        }
    }

    private void UpdateStartupCounts()
    {
        TotalStartupCount = StartupEntries.Count;
        EnabledStartupCount = StartupEntries.Count(e => e.IsEnabled);
    }

    private void NotifyAllUiProperties()
    {
        OnPropertyChanged(nameof(NavDashboard));
        OnPropertyChanged(nameof(NavTweaks));
        OnPropertyChanged(nameof(NavProfiles));
        OnPropertyChanged(nameof(NavCleaner));
        OnPropertyChanged(nameof(NavStartup));
        OnPropertyChanged(nameof(NavSafety));
        OnPropertyChanged(nameof(NavLogs));
        OnPropertyChanged(nameof(PrivilegeHeader));
        OnPropertyChanged(nameof(PrivilegeElevated));
        OnPropertyChanged(nameof(PrivilegeRestricted));
        OnPropertyChanged(nameof(PrivilegeBtnRelaunch));
        OnPropertyChanged(nameof(PrivilegeStatusText));
        OnPropertyChanged(nameof(PrivilegeRelaunchVisibility));
        OnPropertyChanged(nameof(HeaderSubtitle));
        OnPropertyChanged(nameof(HeaderPlan));
        OnPropertyChanged(nameof(DashHealthScore));
        OnPropertyChanged(nameof(DashActiveTweaks));
        OnPropertyChanged(nameof(DashHealthyMessage));
        OnPropertyChanged(nameof(DashNeedsOptMessage));
        OnPropertyChanged(nameof(DashHardwareSpecs));
        OnPropertyChanged(nameof(DashOs));
        OnPropertyChanged(nameof(DashCpu));
        OnPropertyChanged(nameof(DashGpu));
        OnPropertyChanged(nameof(DashRam));
        OnPropertyChanged(nameof(DashTimerResolution));
        OnPropertyChanged(nameof(DashTimerResolutionSub));
        OnPropertyChanged(nameof(DashDisplayTitle));
        OnPropertyChanged(nameof(DashDisplaySubtitle));
        OnPropertyChanged(nameof(DashDisplayMonitor));
        OnPropertyChanged(nameof(DashDisplayResolution));
        OnPropertyChanged(nameof(DashDisplayVrr));
        OnPropertyChanged(nameof(DashDisplayVsync));
        OnPropertyChanged(nameof(DashDisplayReflex));
        OnPropertyChanged(nameof(DashDisplayFpsCap));
        OnPropertyChanged(nameof(DisplayMonitorName));
        OnPropertyChanged(nameof(DisplayResolutionAndHz));
        OnPropertyChanged(nameof(DisplayVrrBadge));
        OnPropertyChanged(nameof(DisplayVsyncAdvice));
        OnPropertyChanged(nameof(DisplayReflexAdvice));
        OnPropertyChanged(nameof(DisplayFpsCapAdvice));
        OnPropertyChanged(nameof(DisplaySummaryAdvice));
        OnPropertyChanged(nameof(AudioDeviceName));
        OnPropertyChanged(nameof(AudioDeviceType));
        OnPropertyChanged(nameof(AudioFormatDescription));
        OnPropertyChanged(nameof(AudioFootstepBadge));
        OnPropertyChanged(nameof(AudioFormatAdvice));
        OnPropertyChanged(nameof(AudioHrtfAdvice));
        OnPropertyChanged(nameof(AudioExclusiveAdvice));
        OnPropertyChanged(nameof(AudioSummaryAdvice));
        OnPropertyChanged(nameof(MouseDeviceName));
        OnPropertyChanged(nameof(MouseConnectionType));
        OnPropertyChanged(nameof(MousePointerSpeedDescription));
        OnPropertyChanged(nameof(MouseAccelerationStatusBadge));
        OnPropertyChanged(nameof(MouseAccelAdvice));
        OnPropertyChanged(nameof(MouseDpiAdvice));
        OnPropertyChanged(nameof(MouseBufferAdvice));
        OnPropertyChanged(nameof(MouseSummaryAdvice));
        OnPropertyChanged(nameof(DashQuickActions));
        OnPropertyChanged(nameof(MemPhysical));
        OnPropertyChanged(nameof(MemInUse));
        OnPropertyChanged(nameof(MemAvailable));
        OnPropertyChanged(nameof(MemTotal));
        OnPropertyChanged(nameof(MemPagefile));
        OnPropertyChanged(nameof(MemCommitted));
        OnPropertyChanged(nameof(MemLimit));
        OnPropertyChanged(nameof(MemWorkingSet));
        OnPropertyChanged(nameof(MemWorkingSetSub));
        OnPropertyChanged(nameof(MemWorkingSetDetail));
        OnPropertyChanged(nameof(MemPurgeRegions));
        OnPropertyChanged(nameof(MemRegionWorkingSet));
        OnPropertyChanged(nameof(MemRegionFileCache));
        OnPropertyChanged(nameof(MemRegionStandby));
        OnPropertyChanged(nameof(MemRegionModified));
        OnPropertyChanged(nameof(MemRegionCombine));
        OnPropertyChanged(nameof(MemRegionRegistry));
        OnPropertyChanged(nameof(MemBtnCleanNow));
        OnPropertyChanged(nameof(MemAutoReductTitle));
        OnPropertyChanged(nameof(MemAutoReductCheck));
        OnPropertyChanged(nameof(MemResult));
        OnPropertyChanged(nameof(TweaksAllCategories));
        OnPropertyChanged(nameof(TweaksCategoryPrivacy));
        OnPropertyChanged(nameof(TweaksCategoryPerformance));
        OnPropertyChanged(nameof(TweaksCategoryGaming));
        OnPropertyChanged(nameof(TweaksCategoryNetwork));
        OnPropertyChanged(nameof(TweaksCategoryServices));
        OnPropertyChanged(nameof(TweaksCategoryDebloater));
        OnPropertyChanged(nameof(TweaksCategoryMaintenance));
        OnPropertyChanged(nameof(TweaksSearchPlaceholder));
        OnPropertyChanged(nameof(TweaksBtnApplyAll));
        OnPropertyChanged(nameof(ProfilesTitle));
        OnPropertyChanged(nameof(ProfilesSubtitle));
        OnPropertyChanged(nameof(CleanerTitle));
        OnPropertyChanged(nameof(CleanerSubtitle));
        OnPropertyChanged(nameof(CleanerFoundSub));
        OnPropertyChanged(nameof(CleanerSelectTargets));
        OnPropertyChanged(nameof(CleanerBtnRescan));
        OnPropertyChanged(nameof(CleanerBtnCleanSelected));
        OnPropertyChanged(nameof(CleanerFoundFilesSummary));
        OnPropertyChanged(nameof(StartupTitle));
        OnPropertyChanged(nameof(StartupSubtitle));
        OnPropertyChanged(nameof(StartupBtnRescan));
        OnPropertyChanged(nameof(StartupTotalAppsSummary));
        OnPropertyChanged(nameof(StartupEnabledAppsSummary));
        OnPropertyChanged(nameof(SafetyRestoreGate));
        OnPropertyChanged(nameof(SafetyRestoreDesc));
        OnPropertyChanged(nameof(SafetyRestoreGateSub));
        OnPropertyChanged(nameof(SafetyBtnCreateRestore));
        OnPropertyChanged(nameof(SafetyRollbackTitle));
        OnPropertyChanged(nameof(SafetyRollbackDesc));
        OnPropertyChanged(nameof(SafetyRollbackSub));
        OnPropertyChanged(nameof(SafetyBtnRollbackAll));
        OnPropertyChanged(nameof(LogsTitle));
        OnPropertyChanged(nameof(LogsSubtitle));
        OnPropertyChanged(nameof(LogsBtnExport));

        OnPropertyChanged(nameof(NavHeader));
        OnPropertyChanged(nameof(DashBtnAutoFixAll));
        OnPropertyChanged(nameof(DashBtnQuickClean));
        OnPropertyChanged(nameof(DashBtnQuickTempClean));
        OnPropertyChanged(nameof(DashBtnQuickCreateRestore));
        OnPropertyChanged(nameof(DashBtnQuickRollback));
        OnPropertyChanged(nameof(ActiveTweaksSummary));
        OnPropertyChanged(nameof(DashVipBannerTitle));
        OnPropertyChanged(nameof(DashVipBannerDesc));
        OnPropertyChanged(nameof(DashVipBannerBtn));

        OnPropertyChanged(nameof(ProfilesBtnApply));

        foreach (var s in StartupEntries) s.NotifyLocalizationChanged();
        foreach (var j in JunkReports) j.NotifyLocalizationChanged();
    }

    // Localized UI text helper properties
    public string NavHeader => LocalizationManager.GetUiText("Nav.Header");
    public string NavDashboard => LocalizationManager.GetUiText("Nav.Dashboard");
    public string NavTweaks => LocalizationManager.GetUiText("Nav.Tweaks");
    public string NavProfiles => LocalizationManager.GetUiText("Nav.Profiles");
    public string NavCleaner => LocalizationManager.GetUiText("Nav.Cleaner");
    public string NavStartup => LocalizationManager.GetUiText("Nav.Startup");
    public string NavSafety => LocalizationManager.GetUiText("Nav.Safety");
    public string NavLogs => LocalizationManager.GetUiText("Nav.Logs");

    public string PrivilegeHeader => LocalizationManager.GetUiText("Privilege.Header");
    public string PrivilegeElevated => LocalizationManager.GetUiText("Privilege.Elevated");
    public string PrivilegeRestricted => LocalizationManager.GetUiText("Privilege.Restricted");
    public string PrivilegeBtnRelaunch => LocalizationManager.GetUiText("Privilege.BtnRelaunch");
    public string PrivilegeStatusText => Specs?.IsAdmin == true ? PrivilegeElevated : PrivilegeRestricted;
    public Visibility PrivilegeRelaunchVisibility => Specs?.IsAdmin == true ? Visibility.Collapsed : Visibility.Visible;

    public string HeaderSubtitle => LocalizationManager.GetUiText("Header.Subtitle");
    public string HeaderPlan => LocalizationManager.GetUiText("Header.Plan");

    public string DashHealthScore => LocalizationManager.GetUiText("Dash.HealthScore");
    public string DashActiveTweaks => LocalizationManager.GetUiText("Dash.ActiveTweaks");
    public string DashHealthyMessage => LocalizationManager.GetUiText("Dash.HealthyMessage");
    public string DashNeedsOptMessage => LocalizationManager.GetUiText("Dash.NeedsOptMessage");
    public string DashHardwareSpecs => LocalizationManager.GetUiText("Dash.HardwareSpecs");
    public string DashOs => LocalizationManager.GetUiText("Dash.Os");
    public string DashCpu => LocalizationManager.GetUiText("Dash.Cpu");
    public string DashGpu => LocalizationManager.GetUiText("Dash.Gpu");
    public string DashRam => LocalizationManager.GetUiText("Dash.Ram");
    public string DashTimerResolution => LocalizationManager.GetUiText("Dash.TimerResolution");
    public string DashTimerResolutionSub => LocalizationManager.GetUiText("Dash.TimerResolutionSub");
    public string DashDisplayTitle => LocalizationManager.GetUiText("Dash.DisplayTitle");
    public string DashDisplaySubtitle => LocalizationManager.GetUiText("Dash.DisplaySubtitle");
    public string DashDisplayMonitor => LocalizationManager.GetUiText("Dash.DisplayMonitor");
    public string DashDisplayResolution => LocalizationManager.GetUiText("Dash.DisplayResolution");
    public string DashDisplayVrr => LocalizationManager.GetUiText("Dash.DisplayVrr");
    public string DashDisplayVsync => LocalizationManager.GetUiText("Dash.DisplayVsync");
    public string DashDisplayReflex => LocalizationManager.GetUiText("Dash.DisplayReflex");
    public string DashDisplayFpsCap => LocalizationManager.GetUiText("Dash.DisplayFpsCap");
    public string DashBtnAutoFixAll => LocalizationManager.GetUiText("Dash.BtnAutoFixAll");
    public string DashBtnQuickClean => LocalizationManager.GetUiText("Dash.BtnQuickClean");
    public string DashBtnQuickTempClean => LocalizationManager.GetUiText("Dash.BtnQuickTempClean");
    public string DashBtnQuickCreateRestore => LocalizationManager.GetUiText("Dash.BtnQuickCreateRestore");
    public string DashBtnQuickRollback => LocalizationManager.GetUiText("Dash.BtnQuickRollback");
    public string ActiveTweaksSummary => string.Format(LocalizationManager.GetUiText("Dash.TweaksActiveFormat"), ActiveTweaksCount);
    public string DashVipBannerTitle => LocalizationManager.GetUiText("Dash.VipBannerTitle");
    public string DashVipBannerDesc => LocalizationManager.GetUiText("Dash.VipBannerDesc");
    public string DashVipBannerBtn => LocalizationManager.GetUiText("Dash.VipBannerBtn");

    public string DisplayMonitorName => Specs?.Display?.MonitorFriendlyName ?? "Primary Display";
    public string DisplayResolutionAndHz => Specs?.Display?.ResolutionAndHz ?? "1920 x 1080 @ 60Hz";
    public string DisplayVrrBadge => Specs?.Display?.VrrStatusBadge ?? "⚪ Standard 60Hz Display";
    public string DisplayVsyncAdvice => Specs?.Display?.GetRecommendations(LocalizationManager.CurrentLanguage).vsync ?? "";
    public string DisplayReflexAdvice => Specs?.Display?.GetRecommendations(LocalizationManager.CurrentLanguage).reflex ?? "";
    public string DisplayFpsCapAdvice => Specs?.Display?.GetRecommendations(LocalizationManager.CurrentLanguage).fpsCap ?? "";
    public string DisplaySummaryAdvice => Specs?.Display?.GetRecommendations(LocalizationManager.CurrentLanguage).summary ?? "";

    public string AudioDeviceName => AudioInfo.DeviceName;
    public string AudioDeviceType => AudioInfo.DeviceType;
    public string AudioFormatDescription => AudioInfo.FormatDescription;
    public string AudioFootstepBadge => AudioInfo.FootstepBadge;
    public string AudioFormatAdvice => AudioInfo.GetRecommendations(LocalizationManager.CurrentLanguage).formatAdvice;
    public string AudioHrtfAdvice => AudioInfo.GetRecommendations(LocalizationManager.CurrentLanguage).hrtfAdvice;
    public string AudioExclusiveAdvice => AudioInfo.GetRecommendations(LocalizationManager.CurrentLanguage).exclusiveAdvice;
    public string AudioSummaryAdvice => AudioInfo.GetRecommendations(LocalizationManager.CurrentLanguage).summary;

    public string MouseDeviceName => MouseInfo.DeviceName;
    public string MouseConnectionType => MouseInfo.ConnectionType;
    public string MousePointerSpeedDescription => MouseInfo.PointerSpeedDescription;
    public string MouseAccelerationStatusBadge => MouseInfo.AccelerationStatusBadge;
    public string MouseAccelAdvice => MouseInfo.GetRecommendations(LocalizationManager.CurrentLanguage).accelAdvice;
    public string MouseDpiAdvice => MouseInfo.GetRecommendations(LocalizationManager.CurrentLanguage).dpiAdvice;
    public string MouseBufferAdvice => MouseInfo.GetRecommendations(LocalizationManager.CurrentLanguage).bufferAdvice;
    public string MouseSummaryAdvice => MouseInfo.GetRecommendations(LocalizationManager.CurrentLanguage).summary;

    public string DashQuickActions => LocalizationManager.GetUiText("Dash.QuickActions");

    public string MemPhysical => LocalizationManager.GetUiText("Mem.Physical");
    public string MemInUse => LocalizationManager.GetUiText("Mem.InUse");
    public string MemAvailable => LocalizationManager.GetUiText("Mem.Available");
    public string MemTotal => LocalizationManager.GetUiText("Mem.Total");
    public string MemPagefile => LocalizationManager.GetUiText("Mem.Pagefile");
    public string MemCommitted => LocalizationManager.GetUiText("Mem.Committed");
    public string MemLimit => LocalizationManager.GetUiText("Mem.Limit");
    public string MemWorkingSet => LocalizationManager.GetUiText("Mem.WorkingSet");
    public string MemWorkingSetSub => LocalizationManager.GetUiText("Mem.WorkingSetSub");
    public string MemWorkingSetDetail => LocalizationManager.GetUiText("Mem.WorkingSetDetail");
    public string MemPurgeRegions => LocalizationManager.GetUiText("Mem.PurgeRegions");
    public string MemRegionWorkingSet => LocalizationManager.GetUiText("Mem.RegionWorkingSet");
    public string MemRegionFileCache => LocalizationManager.GetUiText("Mem.RegionFileCache");
    public string MemRegionStandby => LocalizationManager.GetUiText("Mem.RegionStandby");
    public string MemRegionModified => LocalizationManager.GetUiText("Mem.RegionModified");
    public string MemRegionCombine => LocalizationManager.GetUiText("Mem.RegionCombine");
    public string MemRegionRegistry => LocalizationManager.GetUiText("Mem.RegionRegistry");
    public string MemBtnCleanNow => LocalizationManager.GetUiText("Mem.BtnCleanNow");
    public string MemAutoReductTitle => LocalizationManager.GetUiText("Mem.AutoReductTitle");
    public string MemAutoReductCheck => LocalizationManager.GetUiText("Mem.AutoReductCheck");
    public string MemResult => LocalizationManager.GetUiText("Mem.Result");

    public string TweaksAllCategories => LocalizationManager.GetUiText("Tweaks.AllCategories");
    public string TweaksCategoryPrivacy => LocalizationManager.GetUiText("Tweaks.CategoryPrivacy");
    public string TweaksCategoryPerformance => LocalizationManager.GetUiText("Tweaks.CategoryPerformance");
    public string TweaksCategoryGaming => LocalizationManager.GetUiText("Tweaks.CategoryGaming");
    public string TweaksCategoryNetwork => LocalizationManager.GetUiText("Tweaks.CategoryNetwork");
    public string TweaksCategoryServices => LocalizationManager.GetUiText("Tweaks.CategoryServices");
    public string TweaksCategoryDebloater => LocalizationManager.GetUiText("Tweaks.CategoryDebloater");
    public string TweaksCategoryMaintenance => LocalizationManager.GetUiText("Tweaks.CategoryMaintenance");
    public string TweaksSearchPlaceholder => LocalizationManager.GetUiText("Tweaks.SearchPlaceholder");
    public string TweaksBtnApplyAll => LocalizationManager.GetUiText("Tweaks.BtnApplyAll");

    public string ProfilesTitle => LocalizationManager.GetUiText("Profiles.Title");
    public string ProfilesSubtitle => LocalizationManager.GetUiText("Profiles.Subtitle");
    public string ProfilesBtnApply => LocalizationManager.GetUiText("Profiles.BtnApply");

    public string CleanerTitle => LocalizationManager.GetUiText("Cleaner.Title");
    public string CleanerSubtitle => LocalizationManager.GetUiText("Cleaner.Subtitle");
    public string CleanerFoundSub => LocalizationManager.GetUiText("Cleaner.FoundSub");
    public string CleanerFoundFilesSummary => string.Format(LocalizationManager.GetUiText("Cleaner.FoundFilesFormat"), TotalJunkFiles);
    public string CleanerSelectTargets => LocalizationManager.GetUiText("Cleaner.SelectTargets");
    public string CleanerBtnRescan => LocalizationManager.GetUiText("Cleaner.BtnRescan");
    public string CleanerBtnCleanSelected => LocalizationManager.GetUiText("Cleaner.BtnCleanSelected");

    public string StartupTitle => LocalizationManager.GetUiText("Startup.Title");
    public string StartupSubtitle => LocalizationManager.GetUiText("Startup.Subtitle");
    public string StartupBtnRescan => LocalizationManager.GetUiText("Startup.BtnRescan");
    public string StartupTotalAppsSummary => string.Format(LocalizationManager.GetUiText("Startup.TotalAppsFormat"), TotalStartupCount);
    public string StartupEnabledAppsSummary => string.Format(LocalizationManager.GetUiText("Startup.EnabledAppsFormat"), EnabledStartupCount);

    public string SafetyRestoreGate => LocalizationManager.GetUiText("Safety.RestoreGate");
    public string SafetyRestoreDesc => LocalizationManager.GetUiText("Safety.RestoreDesc");
    public string SafetyRestoreGateSub => LocalizationManager.GetUiText("Safety.RestoreGateSub");
    public string SafetyBtnCreateRestore => LocalizationManager.GetUiText("Safety.BtnCreateRestore");
    public string SafetyRollbackTitle => LocalizationManager.GetUiText("Safety.RollbackTitle");
    public string SafetyRollbackDesc => LocalizationManager.GetUiText("Safety.RollbackDesc");
    public string SafetyRollbackSub => LocalizationManager.GetUiText("Safety.RollbackSub");
    public string SafetyBtnRollbackAll => LocalizationManager.GetUiText("Safety.BtnRollbackAll");

    public string LogsTitle => LocalizationManager.GetUiText("Logs.Title");
    public string LogsSubtitle => LocalizationManager.GetUiText("Logs.Subtitle");
    public string LogsBtnExport => LocalizationManager.GetUiText("Logs.BtnExport");

    [RelayCommand]
    public void FilterCategory(string category)
    {
        SelectedCategoryFilter = category;
        ApplyFilter();
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        FilteredTweaks.Clear();
        foreach (var tweak in AllTweaks)
        {
            bool matchCategory = SelectedCategoryFilter == "All" ||
                                  tweak.Category.ToString().Equals(SelectedCategoryFilter, StringComparison.OrdinalIgnoreCase);

            bool matchSearch = string.IsNullOrWhiteSpace(SearchQuery) ||
                               tweak.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                               tweak.Description.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);

            if (matchCategory && matchSearch)
            {
                FilteredTweaks.Add(tweak);
            }
        }
    }

    [RelayCommand]
    public async Task RefreshAllTweaksStateAsync()
    {
        if (IsScanning) return;
        IsScanning = true;
        HealthScanButtonText = "🔄 Scanning...";
        ScanProgress = 0;
        StatusMessage = "🔍 Starting deep kernel & system optimization health scan...";
        LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] 🔍 Deep System Health Scan initiated...");

        int active = 0;
        int completed = 0;
        var activeCommunityTweaks = AllTweaks.ToList();
        int total = activeCommunityTweaks.Count;

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(4, Environment.ProcessorCount)
        };

        await Parallel.ForEachAsync(activeCommunityTweaks, parallelOptions, async (tweakVm, ct) =>
        {
            await tweakVm.RefreshStateAsync();
            if (tweakVm.IsApplied)
            {
                Interlocked.Increment(ref active);
            }

            int currentCompleted = Interlocked.Increment(ref completed);
            if (currentCompleted % 3 == 0 || currentCompleted == total)
            {
                Application.Current?.Dispatcher?.BeginInvoke(() =>
                {
                    ActiveTweaksCount = active;
                    ScanProgress = Math.Round((double)currentCompleted / total * 100, 0);
                    ScanStatusText = $"Checking ({currentCompleted}/{total}): {tweakVm.Name}";
                    StatusMessage = ScanStatusText;
                    HealthScore = total > 0 ? (int)((double)active / total * 100) : 50;
                    UpdateHealthScoreBadge();
                });
            }
        });

        ActiveTweaksCount = active;
        HealthScore = total > 0 ? (int)((double)active / total * 100) : 50;
        UpdateHealthScoreBadge();

        IsScanning = false;
        HealthScanButtonText = "🔍 Run Health Scan";
        ScanStatusText = $"Scan Complete: {active}/{total} Tweaks Active (Health Score: {HealthScore}/100)";
        StatusMessage = ScanStatusText;
        LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] ✅ Health Scan Completed: {active}/{total} Optimizations Active. Health Score: {HealthScore}/100 ({HealthScoreStatus}).");
    }

    private void UpdateHealthScoreBadge()
    {
        if (HealthScore >= 80)
        {
            HealthScoreStatus = "🛡️ Peak Competitive Condition";
            HealthScoreColor = "#10b981"; // Emerald Green
        }
        else if (HealthScore >= 40)
        {
            HealthScoreStatus = "⚡ Moderately Optimized";
            HealthScoreColor = "#f59e0b"; // Amber Gold
        }
        else
        {
            HealthScoreStatus = "⚠️ Needs Optimization";
            HealthScoreColor = "#f43f5e"; // Rose Red
        }
    }

    [RelayCommand]
    public async Task QuickAutoFixAsync()
    {
        if (IsLoading || IsScanning) return;
        
        IsLoading = true;
        StatusMessage = "⚡ Applying recommended system optimizations...";
        LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] ⚡ Quick Auto-Fix initiated...");

        try
        {
            var recommendedProfile = ProfileManager.GetById("profile.esports_competitive_fps") 
                                  ?? ProfileManager.AllProfiles.FirstOrDefault();
            
            if (recommendedProfile != null)
            {
                var tweaksToApply = recommendedProfile.TargetTweakIds
                    .Select(id => _registry.GetById(id))
                    .Where(t => t != null)
                    .Cast<ITweak>()
                    .ToList();

                var results = await _engine.ApplyTweaksAsync(tweaksToApply, createRestorePoint: true);
                int succeeded = results.Count(r => r.Success);
                
                LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] 🚀 Auto-Fix applied: {succeeded}/{tweaksToApply.Count} optimizations successful.");
                StatusMessage = $"Auto-Fix applied: {succeeded}/{tweaksToApply.Count} optimizations active.";
            }

            // Trigger real-time animated health scan to update score and UI
            await RefreshAllTweaksStateAsync();
            Specs = await SystemSpecs.CollectAsync();
        }
        catch (Exception ex)
        {
            LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] ❌ Auto-Fix error: {ex.Message}");
            StatusMessage = $"Auto-Fix error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ApplyProfileAsync(ProfileItemViewModel profileVm)
    {
        IsLoading = true;
        StatusMessage = $"Applying profile: {profileVm.Name}...";
        try
        {
            var tweaksToApply = profileVm.Profile.TargetTweakIds
                .Select(id => _registry.GetById(id))
                .Where(t => t != null)
                .Cast<ITweak>()
                .ToList();

            var results = await _engine.ApplyTweaksAsync(tweaksToApply, createRestorePoint: true);
            await RefreshAllTweaksStateAsync();
            Specs = await SystemSpecs.CollectAsync();

            int succeeded = results.Count(r => r.Success);
            StatusMessage = $"Profile applied: {succeeded}/{tweaksToApply.Count} optimizations successful.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ApplyFilteredTweaksAsync()
    {
        var tweaksToApply = FilteredTweaks
            .Where(t => !t.IsApplied)
            .Select(t => t.Tweak)
            .ToList();

        if (tweaksToApply.Count == 0)
        {
            MessageBox.Show("All visible tweaks are already applied.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        IsLoading = true;
        StatusMessage = $"Applying {tweaksToApply.Count} tweaks...";
        try
        {
            await _engine.ApplyTweaksAsync(tweaksToApply, createRestorePoint: true);
            await RefreshAllTweaksStateAsync();
        }
        finally
        {
            IsLoading = false;
            StatusMessage = "Batch tweaks applied.";
        }
    }

    [RelayCommand]
    public void ResetDpcPeak()
    {
        DpcLatencyMonitorService.ResetPeak();
        DpcLatency = DpcLatencyMonitorService.SampleLatency();
    }

    [RelayCommand]
    public async Task ScanPingRadarAsync()
    {
        if (IsPingingRadar) return;
        IsPingingRadar = true;
        RadarStatusText = "🛰️ Probing regional eSports game routes (SG, JKT, HK, TYO, SYD)...";
        PingRadarResults.Clear();

        try
        {
            var results = await GameServerPingRadar.PingAllRegionsAsync();
            foreach (var res in results)
            {
                PingRadarResults.Add(res);
            }

            var best = results.Where(r => r.PingMs < 999).OrderBy(r => r.PingMs).FirstOrDefault();
            if (best != null)
            {
                BestServerRoute = $"{best.FlagEmoji} {best.RegionName} ({best.PingMs}ms)";
            }

            RadarStatusText = $"Radar Scan Complete: {PingRadarResults.Count} regional routes probed.";
            LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] 🛰️ FastRoute Radar: Best Route -> {BestServerRoute}.");
        }
        catch (Exception ex)
        {
            RadarStatusText = $"Radar Scan failed: {ex.Message}";
        }
        finally
        {
            IsPingingRadar = false;
        }
    }

    [RelayCommand]
    public void OpenSoundControlPanel()
    {
        AudioInfoService.OpenSoundControlPanel();
        StatusMessage = "Opened Windows Sound Control Panel (mmsys.cpl).";
    }

    [RelayCommand]
    public void RefreshAudioInfo()
    {
        AudioInfo = AudioInfoService.GetDefaultAudioDeviceInfo();
        NotifyAllUiProperties();
        StatusMessage = $"Audio endpoints refreshed: {AudioInfo.DeviceName}";
    }

    [RelayCommand]
    public void OpenMouseControlPanel()
    {
        MouseInfoService.OpenMouseControlPanel();
        StatusMessage = "Opened Windows Mouse Properties Control Panel (main.cpl).";
    }

    [RelayCommand]
    public void RefreshMouseInfo()
    {
        MouseInfo = MouseInfoService.GetDefaultMouseDeviceInfo();
        NotifyAllUiProperties();
        StatusMessage = $"Mouse hardware refreshed: {MouseInfo.DeviceName}";
    }

    [RelayCommand]
    public void RecordMouseMovement()
    {
        _mousePollingMonitor.OnMouseMoveEvent();
        MousePolling = _mousePollingMonitor.CurrentStats;
    }

    [RelayCommand]
    public void ResetMousePollingRadar()
    {
        _mousePollingMonitor.ResetPeak();
        MousePolling = _mousePollingMonitor.CurrentStats;
        StatusMessage = "Mouse Polling Rate Radar reset.";
    }

    [RelayCommand]
    public async Task DeepCleanMemoryAsync(bool isAuto = false)
    {
        IsLoading = !isAuto;
        StatusMessage = "Executing direct NT Kernel memory syscalls...";
        try
        {
            var before = MemReductEngine.GetStats();
            var (freedBytes, summary) = await MemReductEngine.CleanMemoryAsync(new MemReductOptions
            {
                CleanWorkingSet = CleanWorkingSet,
                CleanSystemFileCache = CleanSystemFileCache,
                CleanStandbyList = CleanStandbyList,
                CleanModifiedPageList = CleanModifiedPageList,
                CombineMemoryLists = CombineMemoryLists,
                CleanRegistryCache = CleanRegistryCache
            });

            await Task.Delay(300);
            var after = MemReductEngine.GetStats();
            MemStats = after;

            double freedMb = freedBytes / (1024.0 * 1024.0);
            LastMemCleanSummary = summary;
            LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] 🧠 {summary}");

            if (!isAuto)
            {
                StatusMessage = $"Memory trimmed successfully! Freed {freedMb:F1} MB physical RAM.";
            }
        }
        finally
        {
            if (!isAuto) IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task QuickTempCleanAsync()
    {
        IsLoading = true;
        StatusMessage = "Purging temporary files and DNS resolver cache...";
        try
        {
            var tempTweak = new CleanTempFilesTweak();
            var dnsTweak = new FlushDnsTweak();

            var resTemp = await tempTweak.ApplyAsync();
            var resDns = await dnsTweak.ApplyAsync();

            LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] 🧹 {resTemp.Message}");
            LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] 🌐 {resDns.Message}");
            StatusMessage = resTemp.Message;

            await ScanJunkAsync();
        }
        catch (Exception ex)
        {
            LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] ❌ Quick Temp Clean failed: {ex.Message}");
            StatusMessage = $"Temp clean failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task ScanJunkAsync()
    {
        IsLoading = true;
        StatusMessage = "Scanning storage and browser cache junk...";
        try
        {
            JunkReports.Clear();
            var reports = await DeepCleanerEngine.ScanJunkAsync();
            long totalBytes = 0;
            int totalFiles = 0;

            foreach (var r in reports)
            {
                totalBytes += r.SizeBytes;
                totalFiles += r.FileCount;
                JunkReports.Add(new JunkItemViewModel(r));
            }

            TotalJunkFiles = totalFiles;
            TotalJunkFormatted = totalBytes >= 1024 * 1024 * 1024
                ? $"{(double)totalBytes / (1024 * 1024 * 1024):F2} GB"
                : $"{(double)totalBytes / (1024 * 1024):F1} MB";

            StatusMessage = $"Scan completed: {TotalJunkFormatted} junk found in {TotalJunkFiles} files.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task CleanSelectedJunkAsync()
    {
        var selectedReports = JunkReports
            .Where(j => j.IsSelected)
            .Select(j => j.Report)
            .ToList();

        if (selectedReports.Count == 0)
        {
            MessageBox.Show("Please select at least one junk category to clean.", "No Target Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Are you sure you want to clean {selectedReports.Count} selected junk categories?\nThis will permanently delete temporary files and browser cache.",
            "Confirm Deep Clean",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        StatusMessage = "Purging selected junk categories...";
        try
        {
            var (freedBytes, cleanedFiles) = await DeepCleanerEngine.CleanJunkAsync(selectedReports.Select(r => r.Category));

            string freedFormatted = freedBytes >= 1024 * 1024 * 1024
                ? $"{(double)freedBytes / (1024 * 1024 * 1024):F2} GB"
                : $"{(double)freedBytes / (1024 * 1024):F1} MB";

            LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] 🧹 Cleaned {cleanedFiles} junk files ({freedFormatted} freed).");
            MessageBox.Show($"Deep Clean Complete!\n\nSuccessfully removed {cleanedFiles} files and freed {freedFormatted} of disk space.", "Clean Succeeded", MessageBoxButton.OK, MessageBoxImage.Information);

            await ScanJunkAsync();
        }
        finally
        {
            IsLoading = false;
            StatusMessage = "Ready";
        }
    }

    [RelayCommand]
    public async Task CreateRestorePointAsync()
    {
        if (!Specs.IsAdmin)
        {
            MessageBox.Show("Administrator privileges are required to create a System Restore Point.", "Elevation Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        StatusMessage = "Creating Windows System Restore Point...";
        try
        {
            bool ok = await RestorePointManager.CreateRestorePointAsync("Manual Snapshot via NRTX Optimizer GUI");
            if (ok)
            {
                LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] ✅ System Restore Point created successfully.");
                MessageBox.Show("System Restore Point created successfully!", "Restore Point", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                LogEntries.Insert(0, $"[{DateTime.Now:HH:mm:ss}] ❌ Failed to create restore point.");
                MessageBox.Show("Failed to create restore point. Ensure System Protection is enabled on C: drive.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            IsLoading = false;
            StatusMessage = "Ready";
        }
    }

    [RelayCommand]
    public async Task RollbackAllAsync()
    {
        var confirm = MessageBox.Show(
            "Are you sure you want to rollback all tweaks to default Windows settings?",
            "Confirm Rollback",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (confirm != MessageBoxResult.Yes) return;

        IsLoading = true;
        StatusMessage = "Rolling back all optimizations...";
        try
        {
            await _engine.RollbackTweaksAsync(_registry.AllTweaks);
            await RefreshAllTweaksStateAsync();
            Specs = await SystemSpecs.CollectAsync();
        }
        finally
        {
            IsLoading = false;
            StatusMessage = "Rollback finished.";
        }
    }

    [RelayCommand]
    public void RelaunchAsAdmin()
    {
        if (PrivilegeGuard.RelaunchAsAdmin())
        {
            Application.Current.Shutdown();
        }
    }

    [RelayCommand]
    public void ExportLogsToFile()
    {
        try
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktop, $"troy_optimizer_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllLines(filePath, LogEntries);
            MessageBox.Show($"Execution logs successfully saved to:\n{filePath}", "Logs Exported", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to export logs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    public void OpenVipUpgrade()
    {
        BrowserLauncher.OpenUrl(BrowserLauncher.DefaultVipUrl);
    }

    public void Dispose()
    {
        try
        {
            _liveMonitorTimer.Stop();
            _gameWatcher.Stop();
            _gameWatcher.Dispose();
        }
        catch { }
        GC.SuppressFinalize(this);
    }
}
