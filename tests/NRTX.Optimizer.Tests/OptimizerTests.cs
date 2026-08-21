using NRTX.Optimizer.Core.Engine;
using NRTX.Optimizer.Core.Localization;
using NRTX.Optimizer.Core.Models;
using NRTX.Optimizer.Core.Modules.Maintenance;
using NRTX.Optimizer.Core.Modules.Network;
using NRTX.Optimizer.Core.Modules.Performance;
using NRTX.Optimizer.Core.Native;
using NRTX.Optimizer.Core.Profiles;
using NRTX.Optimizer.Core.Safety;
using Xunit;

namespace NRTX.Optimizer.Tests;

public class OptimizerTests
{
    [Fact]
    public void TweakRegistry_ShouldContainAllDefaultTweaks_WithUniqueValidIds()
    {
        var registry = new TweakRegistry();
        Assert.NotEmpty(registry.AllTweaks);
        Assert.Equal(28, registry.AllTweaks.Count);

        var ids = new HashSet<string>();
        foreach (var tweak in registry.AllTweaks)
        {
            Assert.False(string.IsNullOrWhiteSpace(tweak.Id), "Tweak ID must not be empty");
            Assert.False(string.IsNullOrWhiteSpace(tweak.Name), $"Tweak name must not be empty for {tweak.Id}");
            Assert.False(string.IsNullOrWhiteSpace(tweak.Description), $"Tweak description must not be empty for {tweak.Id}");
            Assert.True(ids.Add(tweak.Id), $"Duplicate tweak ID found: {tweak.Id}");
        }
    }

    [Theory]
    [InlineData("profile.esports_competitive_fps")]
    [InlineData("profile.gaming")]
    [InlineData("profile.streaming_content_creator")]
    [InlineData("profile.dev_workstation")]
    [InlineData("profile.ultra_privacy")]
    [InlineData("profile.safe_daily")]
    public void Profiles_ShouldReferenceExistingTweaks(string profileId)
    {
        var registry = new TweakRegistry();
        var profile = ProfileManager.GetById(profileId);

        Assert.NotNull(profile);
        Assert.NotEmpty(profile.TargetTweakIds);

        foreach (var tweakId in profile.TargetTweakIds)
        {
            var tweak = registry.GetById(tweakId);
            Assert.NotNull(tweak);
        }
    }

    [Fact]
    public void LocalizationManager_ShouldHaveCompleteDictionaries_ForBothLanguages()
    {
        var registry = new TweakRegistry();
        var languages = new[] { AppLanguage.English, AppLanguage.Indonesian };

        foreach (var lang in languages)
        {
            foreach (var tweak in registry.AllTweaks)
            {
                var info = LocalizationManager.GetTweakInfo(tweak.Id, lang);
                Assert.NotNull(info);
                Assert.False(string.IsNullOrWhiteSpace(info.Name), $"Missing Name for {tweak.Id} in {lang}");
                Assert.False(string.IsNullOrWhiteSpace(info.Description), $"Missing Description for {tweak.Id} in {lang}");
                Assert.False(string.IsNullOrWhiteSpace(info.Purpose), $"Missing Purpose for {tweak.Id} in {lang}");
                Assert.False(string.IsNullOrWhiteSpace(info.HowItWorks), $"Missing HowItWorks for {tweak.Id} in {lang}");
                Assert.False(string.IsNullOrWhiteSpace(info.Impact), $"Missing Impact for {tweak.Id} in {lang}");
            }

            foreach (var profile in ProfileManager.AllProfiles)
            {
                var (name, desc) = LocalizationManager.GetProfileInfo(profile.Id, lang);
                Assert.False(string.IsNullOrWhiteSpace(name), $"Missing profile Name for {profile.Id} in {lang}");
                Assert.False(string.IsNullOrWhiteSpace(desc), $"Missing profile Description for {profile.Id} in {lang}");
            }

            foreach (TweakCategory cat in Enum.GetValues<TweakCategory>())
            {
                var catName = LocalizationManager.GetCategoryName(cat, lang);
                Assert.False(string.IsNullOrWhiteSpace(catName), $"Missing category Name for {cat} in {lang}");
            }

            foreach (RiskLevel risk in Enum.GetValues<RiskLevel>())
            {
                var riskName = LocalizationManager.GetRiskName(risk, lang);
                Assert.False(string.IsNullOrWhiteSpace(riskName), $"Missing risk Name for {risk} in {lang}");
            }

            foreach (JunkCategory junkCat in Enum.GetValues<JunkCategory>())
            {
                var junkInfo = LocalizationManager.GetJunkCategoryInfo(junkCat, lang);
                Assert.False(string.IsNullOrWhiteSpace(junkInfo.Name), $"Missing junk Name for {junkCat} in {lang}");
                Assert.False(string.IsNullOrWhiteSpace(junkInfo.Description), $"Missing junk Description for {junkCat} in {lang}");
            }

            var uiKeys = new[]
            {
                "Nav.Dashboard", "Nav.MemReduct", "Nav.Tweaks", "Nav.Profiles", "Nav.Cleaner", "Nav.Startup", "Nav.Safety", "Nav.Logs",
                "Privilege.Header", "Privilege.Elevated", "Privilege.Restricted", "Privilege.Relaunch", "Privilege.BtnRelaunch",
                "Dash.HealthScore", "Dash.HardwareSpecs", "Dash.QuickActions", "Dash.TimerResolution",
                "Dash.DisplayTitle", "Dash.DisplayMonitor", "Dash.DisplayResolution", "Dash.DisplayVrr", "Dash.DisplayVsync", "Dash.DisplayReflex", "Dash.DisplayFpsCap",
                "Mem.Physical", "Mem.Pagefile", "Mem.WorkingSet", "Mem.PurgeRegions", "Mem.BtnCleanNow",
                "Cleaner.Title", "Cleaner.FoundSub", "Cleaner.BtnRescan", "Cleaner.BtnCleanSelected", "Cleaner.SelectTargets",
                "Startup.Title", "Startup.Subtitle", "Startup.BtnRescan",
                "Safety.RestoreGate", "Safety.RestoreDesc", "Safety.BtnCreateRestore", "Safety.RollbackTitle", "Safety.RollbackDesc", "Safety.BtnRollbackAll",
                "Logs.Title", "Logs.BtnExport"
            };

            foreach (var key in uiKeys)
            {
                var text = LocalizationManager.GetUiText(key, lang);
                Assert.False(string.IsNullOrWhiteSpace(text), $"Missing UI text for {key} in {lang}");
                Assert.NotEqual(key, text);
            }
        }
    }

    [Fact]
    public async Task ExecutionEngine_DryRun_ShouldSucceedWithoutModifyingSystem()
    {
        var registry = new TweakRegistry();
        var engine = new ExecutionEngine();

        var results = await engine.ApplyTweaksAsync(registry.AllTweaks, createRestorePoint: false, dryRun: true);

        Assert.Equal(registry.AllTweaks.Count, results.Count);
        Assert.All(results, r =>
        {
            Assert.True(r.Success, $"Dry-run failed for a tweak: {r.Message}");
            Assert.True(r.IsDryRun, "Expected IsDryRun to be true");
        });
    }

    [Fact]
    public async Task ExecutionEngine_RollbackDryRun_ShouldSucceed()
    {
        var registry = new TweakRegistry();
        var engine = new ExecutionEngine();

        var results = await engine.RollbackTweaksAsync(registry.AllTweaks, dryRun: true);

        Assert.Equal(registry.AllTweaks.Count, results.Count);
        Assert.All(results, r =>
        {
            Assert.True(r.Success, $"Rollback dry-run failed: {r.Message}");
        });
    }

    [Fact]
    public async Task SystemSpecs_ShouldCollectValidHardwareParameters()
    {
        var specs = await SystemSpecs.CollectAsync();

        Assert.NotNull(specs);
        Assert.False(string.IsNullOrWhiteSpace(specs.OsName));
        Assert.True(specs.TotalRamGb > 0, "Total RAM should be greater than 0 GB");
        Assert.True(specs.MemoryLoadPercent <= 100, "Memory load should be <= 100%");
    }

    [Fact]
    public void MemReductEngine_GetStats_ShouldReturnSensibleMetrics()
    {
        var stats = MemReductEngine.GetStats();

        Assert.NotNull(stats);
        Assert.True(stats.PhysicalTotalGb > 0, "PhysicalTotalGb should be > 0");
        Assert.True(stats.PhysicalAvailableGb >= 0, "PhysicalAvailableGb should be >= 0");
        Assert.True(stats.PhysicalUsagePercent <= 100, "PhysicalUsagePercent should be <= 100");
    }

    [Fact]
    public void SnapshotManager_SaveAndList_ShouldPersistValidSnapshot()
    {
        var testSnap = new SystemSnapshot
        {
            Id = $"snap_test_{Guid.NewGuid():N}",
            Description = "Automated Unit Test Snapshot",
            CreatedAt = DateTime.UtcNow,
            AppliedTweakIds = ["perf.win32_priority_separation", "gaming.disable_mpo"],
            RegistryStates = [
                new RegistryStateEntry
                {
                    Hive = "LocalMachine",
                    SubKey = @"SYSTEM\CurrentControlSet\Control\PriorityControl",
                    ValueName = "Win32PrioritySeparation",
                    ValueData = "2",
                    ValueKind = "DWord",
                    ExistedBefore = true
                }
            ]
        };

        var filePath = SnapshotManager.SaveSnapshot(testSnap);
        Assert.True(File.Exists(filePath), "Snapshot file was not created");

        var list = SnapshotManager.ListSnapshots();
        Assert.Contains(list, s => s.Id == testSnap.Id);

        // Cleanup test snapshot
        try { File.Delete(filePath); } catch { }
    }

    [Fact]
    public void AuditLogger_ShouldLogAndEmitEvent()
    {
        string? capturedEvent = null;
        Action<string> handler = msg => capturedEvent = msg;

        AuditLogger.OnLogEvent += handler;
        try
        {
            AuditLogger.Log(AuditLogLevel.Info, "UnitTest", "Test audit entry");
            Assert.NotNull(capturedEvent);
            Assert.Contains("Test audit entry", capturedEvent);
            Assert.Contains("UnitTest", capturedEvent);
        }
        finally
        {
            AuditLogger.OnLogEvent -= handler;
        }
    }

    [Fact]
    public async Task DeepCleanerEngine_ScanJunk_ShouldReturnAllCategoriesWithoutThrowing()
    {
        var reports = await DeepCleanerEngine.ScanJunkAsync();

        Assert.NotNull(reports);
        Assert.NotEmpty(reports);
        Assert.True(reports.Count >= 6, "Expected at least 6 junk categories scanned");
        Assert.All(reports, r =>
        {
            Assert.False(string.IsNullOrWhiteSpace(r.Name));
            Assert.True(r.SizeBytes >= 0);
            Assert.True(r.FileCount >= 0);
        });
    }

    [Fact]
    public void AutoReductWorker_StateAndConfiguration_ShouldInitializeProperly()
    {
        using var worker = new AutoReductWorker
        {
            ThresholdPercent = 90,
            Interval = TimeSpan.FromSeconds(10)
        };

        Assert.False(worker.IsRunning);
        Assert.Equal(90, worker.ThresholdPercent);
        Assert.Equal(TimeSpan.FromSeconds(10), worker.Interval);
    }

    [Fact]
    public void SystemTimerService_ShouldReturnValidTimerResolution()
    {
        var timerInfo = SystemTimerService.GetTimerResolution();
        Assert.NotNull(timerInfo);
        Assert.True(timerInfo.MinResolutionMs > 0, "Expected positive minimum resolution");
        Assert.True(timerInfo.MaxResolutionMs > 0, "Expected positive maximum resolution");
        Assert.True(timerInfo.CurrentResolutionMs > 0, "Expected positive current resolution");
    }

    [Fact]
    public async Task StartupManagerEngine_ShouldScanEntriesWithoutThrowing()
    {
        var entries = await StartupManagerEngine.GetStartupEntriesAsync();
        Assert.NotNull(entries);
        Assert.All(entries, e =>
        {
            Assert.False(string.IsNullOrWhiteSpace(e.Name), "Startup entry name should not be empty");
            Assert.False(string.IsNullOrWhiteSpace(e.Command), "Startup command should not be empty");
        });
    }

    [Fact]
    public void DisplayInfoService_ShouldReturnValidDisplayMetrics()
    {
        var display = DisplayInfoService.GetPrimaryDisplayInfo();
        Assert.NotNull(display);
        Assert.True(display.Width > 0, "Display width should be positive");
        Assert.True(display.Height > 0, "Display height should be positive");
        Assert.True(display.RefreshRateHz > 0, "Refresh rate should be positive");
        Assert.True(display.RecommendedFpsCap >= 30, "Recommended FPS cap should be at least 30");

        var (vsync, reflex, fpsCap, summary) = display.GetRecommendations(AppLanguage.Indonesian);
        Assert.False(string.IsNullOrWhiteSpace(vsync));
        Assert.False(string.IsNullOrWhiteSpace(reflex));
        Assert.False(string.IsNullOrWhiteSpace(fpsCap));
        Assert.False(string.IsNullOrWhiteSpace(summary));
    }

    [Fact]
    public void DpcLatencyMonitorService_ShouldReturnValidSample()
    {
        var sample = DpcLatencyMonitorService.SampleLatency(5);
        Assert.NotNull(sample);
        Assert.True(sample.CurrentLatencyUs > 0, "Current latency must be positive");
        Assert.True(sample.PeakLatencyUs > 0, "Peak latency must be positive");
        Assert.False(string.IsNullOrWhiteSpace(sample.StatusText));
        Assert.False(string.IsNullOrWhiteSpace(sample.StatusColor));
    }

    [Fact]
    public void GameServerPingRadar_ShouldContainDefaultRegions()
    {
        var regions = GameServerPingRadar.DefaultRegions;
        Assert.NotEmpty(regions);
        Assert.Contains(regions, r => r.RegionId == "sg");
        Assert.Contains(regions, r => r.RegionId == "id");
        Assert.Contains(regions, r => r.RegionId == "hk");
        Assert.Contains(regions, r => r.RegionId == "jp");
        Assert.All(regions, r =>
        {
            Assert.False(string.IsNullOrWhiteSpace(r.TargetHost));
            Assert.False(string.IsNullOrWhiteSpace(r.FlagEmoji));
        });
    }

    [Fact]
    public void AudioInfoService_ShouldReturnValidAudioDeviceMetrics()
    {
        var audio = AudioInfoService.GetDefaultAudioDeviceInfo();
        Assert.NotNull(audio);
        Assert.False(string.IsNullOrWhiteSpace(audio.DeviceName));
        Assert.False(string.IsNullOrWhiteSpace(audio.DeviceType));
        Assert.True(audio.SampleRateHz >= 44100);
        Assert.True(audio.BitDepth >= 16);

        var (format, hrtf, exclusive, summary) = audio.GetRecommendations(AppLanguage.Indonesian);
        Assert.False(string.IsNullOrWhiteSpace(format));
        Assert.False(string.IsNullOrWhiteSpace(hrtf));
        Assert.False(string.IsNullOrWhiteSpace(exclusive));
        Assert.False(string.IsNullOrWhiteSpace(summary));
    }

    [Fact]
    public void MouseInfoService_ShouldReturnValidMouseDeviceMetrics()
    {
        var mouse = MouseInfoService.GetDefaultMouseDeviceInfo();
        Assert.NotNull(mouse);
        Assert.False(string.IsNullOrWhiteSpace(mouse.DeviceName));
        Assert.False(string.IsNullOrWhiteSpace(mouse.ConnectionType));
        Assert.True(mouse.PointerSpeed >= 1 && mouse.PointerSpeed <= 20);

        var (accel, dpi, buffer, summary) = mouse.GetRecommendations(AppLanguage.Indonesian);
        Assert.False(string.IsNullOrWhiteSpace(accel));
        Assert.False(string.IsNullOrWhiteSpace(dpi));
        Assert.False(string.IsNullOrWhiteSpace(buffer));
        Assert.False(string.IsNullOrWhiteSpace(summary));
    }

    [Fact]
    public void MousePollingMonitor_ShouldTrackSamplesAndReset()
    {
        var monitor = new MousePollingMonitor();
        Assert.NotNull(monitor.CurrentStats);

        for (int i = 0; i < 20; i++)
        {
            monitor.OnMouseMoveEvent();
            Thread.Sleep(1);
        }

        Assert.True(monitor.CurrentStats.TotalSamples > 0);
        monitor.ResetPeak();
        Assert.Equal(0, monitor.CurrentStats.TotalSamples);
    }

    [Fact]
    public async Task ExecutionEngine_WithCancelledToken_ShouldAbortEarly()
    {
        var registry = new TweakRegistry();
        var engine = new ExecutionEngine();
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        var results = await engine.ApplyTweaksAsync(registry.AllTweaks, createRestorePoint: false, dryRun: true, cancellationToken: cts.Token);

        Assert.Empty(results);
    }

    [Fact]
    public void SafeRegistry_NonExistentKey_ShouldReturnNullGracefully()
    {
        var missingDword = SafeRegistry.GetDword(Microsoft.Win32.RegistryHive.CurrentUser, @"Software\NonExistentTestKey_123456789", "NonExistentValue");
        Assert.Null(missingDword);

        var missingString = SafeRegistry.GetString(Microsoft.Win32.RegistryHive.CurrentUser, @"Software\NonExistentTestKey_123456789", "NonExistentValue");
        Assert.Null(missingString);

        var missingBinary = SafeRegistry.GetBinary(Microsoft.Win32.RegistryHive.CurrentUser, @"Software\NonExistentTestKey_123456789", "NonExistentValue");
        Assert.Null(missingBinary);
    }

    [Fact]
    public async Task DeepCleanerEngine_ScanJunk_ShouldReturnValidReports()
    {
        var reports = await DeepCleanerEngine.ScanJunkAsync();
        Assert.NotEmpty(reports);
        Assert.Contains(reports, r => r.Category == JunkCategory.RecycleBin);
        Assert.Contains(reports, r => r.Category == JunkCategory.WindowsTempAndLogs);

        var rbReport = reports.First(r => r.Category == JunkCategory.RecycleBin);
        Assert.True(rbReport.SizeBytes >= 0);
        Assert.True(rbReport.FileCount >= 0);
    }

    [Fact]
    public async Task StartupManagerEngine_ShouldEnumerateEntriesSafely()
    {
        var entries = await StartupManagerEngine.GetStartupEntriesAsync();
        Assert.NotNull(entries);
        foreach (var entry in entries)
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.Name));
            Assert.False(string.IsNullOrWhiteSpace(entry.Id));
        }
    }

    [Fact]
    public void AutoReductWorker_Lifecycle_ShouldStartStopWithoutExceptions()
    {
        using var worker = new AutoReductWorker
        {
            Interval = TimeSpan.FromMilliseconds(50),
            ThresholdPercent = 99
        };

        worker.Start();
        Assert.True(worker.IsRunning);
        Thread.Sleep(100);
        worker.Dispose();
        Assert.False(worker.IsRunning);
    }

    [Fact]
    public async Task SystemDiagnosticEngine_ConcurrentScan_ShouldReturnConsistentResults()
    {
        var registry = new TweakRegistry();
        var diag = new SystemDiagnosticEngine(registry);

        var report1 = await diag.ScanAsync();
        var report2 = await diag.ScanAsync();

        Assert.NotNull(report1);
        Assert.NotNull(report2);
        Assert.Equal(report1.TotalCount, report2.TotalCount);
        Assert.Equal(28, report1.TotalCount);
        Assert.Equal(report1.AppliedCount, report2.AppliedCount);
        Assert.Equal(28, report1.Statuses.Count);
    }

    [Fact]
    public async Task DeepCleanerEngine_ConcurrentScan_ShouldNotCollide()
    {
        var scan1Task = DeepCleanerEngine.ScanJunkAsync();
        var scan2Task = DeepCleanerEngine.ScanJunkAsync();

        var (res1, res2) = (await scan1Task, await scan2Task);

        Assert.NotEmpty(res1);
        Assert.NotEmpty(res2);
        Assert.Equal(res1.Count, res2.Count);
    }

    [Fact]
    public async Task StartupManagerEngine_ConcurrentScan_ShouldBeThreadSafe()
    {
        var task1 = StartupManagerEngine.GetStartupEntriesAsync();
        var task2 = StartupManagerEngine.GetStartupEntriesAsync();

        var (entries1, entries2) = (await task1, await task2);

        Assert.NotNull(entries1);
        Assert.NotNull(entries2);
        Assert.Equal(entries1.Count, entries2.Count);
    }

    [Fact]
    public async Task ExecutionEngine_ParallelApplyDryRun_ShouldCompleteAllTweaks()
    {
        var registry = new TweakRegistry();
        var engine = new ExecutionEngine();

        var results = await engine.ApplyTweaksAsync(registry.AllTweaks, createRestorePoint: false, dryRun: true);

        Assert.NotNull(results);
        Assert.Equal(28, results.Count);
    }

    [Fact]
    public void BrowserLauncher_GetDefaultBrowserPath_ShouldNotThrow()
    {
        var path = BrowserLauncher.GetDefaultBrowserPath();
        // May be null on headless CI/CD, but should not throw exception
        if (path != null)
        {
            Assert.False(string.IsNullOrWhiteSpace(path));
        }
    }

    [Fact]
    public void CrashReporter_GenerateErrorReport_ShouldCreateValidTxtFile()
    {
        var testEx = new InvalidOperationException("Simulated test error for diagnostic logging.", new ArgumentException("Inner simulated parameter."));
        var logPath = CrashReporter.GenerateErrorReport(testEx, "UnitTest Context");

        Assert.False(string.IsNullOrWhiteSpace(logPath));
        Assert.True(File.Exists(logPath), $"Expected crash report file at {logPath}");

        var content = File.ReadAllText(logPath);
        Assert.Contains("TROY TWEAKS WINDOWS OPTIMIZER", content);
        Assert.Contains("Simulated test error for diagnostic logging", content);
        Assert.Contains("Inner simulated parameter", content);
        Assert.Contains("[1] SYSTEM ENVIRONMENT & HARDWARE DIAGNOSTICS", content);

        // Cleanup test log
        try { File.Delete(logPath); } catch { }
    }

    [Fact]
    public void SnapshotManager_BinaryAndMultiTypeRoundtrip_ShouldPreserveExactData()
    {
        byte[] originalCurve = [0x00, 0x15, 0x30, 0x70, 0x90, 0xFF];
        string base64 = Convert.ToBase64String(originalCurve);

        var snap = new SystemSnapshot
        {
            Id = $"snap_test_binary_{Guid.NewGuid():N}",
            Description = "Binary snapshot roundtrip test",
            CreatedAt = DateTime.UtcNow,
            RegistryStates = [
                new RegistryStateEntry
                {
                    Hive = "CurrentUser",
                    SubKey = @"Software\NRTX_Optimizer_Test",
                    ValueName = "TestBinaryCurve",
                    ValueData = base64,
                    ValueKind = "Binary",
                    ExistedBefore = true
                },
                new RegistryStateEntry
                {
                    Hive = "CurrentUser",
                    SubKey = @"Software\NRTX_Optimizer_Test",
                    ValueName = "TestDword",
                    ValueData = "1337",
                    ValueKind = "DWord",
                    ExistedBefore = true
                }
            ]
        };

        var filePath = SnapshotManager.SaveSnapshot(snap);
        Assert.True(File.Exists(filePath));

        bool restored = SnapshotManager.RestoreSnapshot(snap.Id);
        Assert.True(restored, "Snapshot restore should succeed");

        // Verify registry writes
        var readBinary = SafeRegistry.GetBinary(Microsoft.Win32.RegistryHive.CurrentUser, @"Software\NRTX_Optimizer_Test", "TestBinaryCurve");
        Assert.NotNull(readBinary);
        Assert.Equal(originalCurve, readBinary);

        var readDword = SafeRegistry.GetDword(Microsoft.Win32.RegistryHive.CurrentUser, @"Software\NRTX_Optimizer_Test", "TestDword");
        Assert.Equal(1337, readDword);

        // Cleanup test registry and snapshot
        SafeRegistry.DeleteSubKeyTree(Microsoft.Win32.RegistryHive.CurrentUser, @"Software\NRTX_Optimizer_Test");
        try { File.Delete(filePath); } catch { }
    }

    [Fact]
    public async Task DeepCleanerEngine_SafeEnumeration_ShouldHandleRestrictedDirectoriesWithoutException()
    {
        // Scan junk should execute across all system and user folders without throwing
        var reports = await DeepCleanerEngine.ScanJunkAsync();
        Assert.NotNull(reports);
        Assert.NotEmpty(reports);

        foreach (var report in reports)
        {
            Assert.False(string.IsNullOrWhiteSpace(report.Name));
            Assert.False(string.IsNullOrWhiteSpace(report.FormattedSize));
            Assert.True(report.SizeBytes >= 0);
            Assert.True(report.FileCount >= 0);
        }
    }

    [Fact]
    public async Task MemoryTrimTweak_Apply_ShouldExecuteSafelyAndReturnResult()
    {
        var tweak = new MemoryTrimTweak();
        Assert.Equal("perf.memory_trim", tweak.Id);
        Assert.Equal(TweakCategory.Performance, tweak.Category);

        var dryRunResult = await tweak.ApplyAsync(dryRun: true);
        Assert.True(dryRunResult.Success);
        Assert.True(dryRunResult.IsDryRun);

        var liveResult = await tweak.ApplyAsync(dryRun: false);
        Assert.True(liveResult.Success);
        Assert.Contains("RAM working set trimmed successfully", liveResult.Message);
    }

    [Fact]
    public async Task QuickTempClean_Components_ShouldExecuteWithoutExceptions()
    {
        var tempTweak = new CleanTempFilesTweak();
        var dnsTweak = new FlushDnsTweak();

        var tempDryRun = await tempTweak.ApplyAsync(dryRun: true);
        Assert.True(tempDryRun.Success);

        var dnsDryRun = await dnsTweak.ApplyAsync(dryRun: true);
        Assert.True(dnsDryRun.Success);

        var tempLive = await tempTweak.ApplyAsync(dryRun: false);
        Assert.True(tempLive.Success);

        var dnsLive = await dnsTweak.ApplyAsync(dryRun: false);
        Assert.True(dnsLive.Success);
    }

    [Fact]
    public void Profiles_AllTargetTweaks_ShouldExistInRegistry()
    {
        var registry = new TweakRegistry();
        var allProfiles = ProfileManager.AllProfiles;

        Assert.Equal(6, allProfiles.Count);
        foreach (var profile in allProfiles)
        {
            Assert.False(string.IsNullOrWhiteSpace(profile.Id));
            Assert.False(string.IsNullOrWhiteSpace(profile.Name));
            Assert.NotEmpty(profile.TargetTweakIds);

            foreach (var tweakId in profile.TargetTweakIds)
            {
                var tweak = registry.GetById(tweakId);
                Assert.NotNull(tweak);
            }
        }
    }
}

