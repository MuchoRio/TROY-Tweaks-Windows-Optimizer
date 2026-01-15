using System.Diagnostics;
using NRTX.Optimizer.Core.Native;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Core.Engine;

public class GameBoosterWatcher : IDisposable
{
    private static readonly HashSet<string> TargetGameProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "VALORANT-Win64-Shipping",
        "r5apex",
        "cs2",
        "Overwatch",
        "FortniteClient-Win64-Shipping",
        "RainbowSix",
        "pubg",
        "TslGame",
        "cod",
        "ModernWarfare",
        "Dota2",
        "LeagueClientUx",
        "GenshinImpact",
        "StarRail"
    };

    private CancellationTokenSource? _cts;
    private Task? _watcherTask;
    private string? _lastActiveGame;

    public bool IsRunning { get; private set; }
    public event Action<string, bool>? OnGameBoostStateChanged;

    public void Start(TimeSpan? interval = null)
    {
        if (IsRunning) return;

        var scanInterval = interval ?? TimeSpan.FromSeconds(3);
        _cts = new CancellationTokenSource();
        IsRunning = true;

        AuditLogger.Log(AuditLogLevel.Info, "GameBooster", "Smart Game Booster daemon started.");

        _watcherTask = Task.Run(async () =>
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    CheckActiveGames();
                    await Task.Delay(scanInterval, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AuditLogger.Log(AuditLogLevel.Error, "GameBooster", $"Watcher loop exception: {ex.Message}");
                }
            }
        }, _cts.Token);
    }

    public void Stop()
    {
        if (!IsRunning) return;

        _cts?.Cancel();
        try
        {
            _watcherTask?.Wait(1000);
        }
        catch { }

        _cts?.Dispose();
        _cts = null;
        _watcherTask = null;
        IsRunning = false;
        _lastActiveGame = null;

        AuditLogger.Log(AuditLogLevel.Info, "GameBooster", "Smart Game Booster daemon stopped.");
    }

    private void CheckActiveGames()
    {
        string? activeGame = null;

        foreach (var procName in TargetGameProcesses)
        {
            var procs = Process.GetProcessesByName(procName);
            if (procs.Length > 0)
            {
                activeGame = procName;
                foreach (var p in procs)
                {
                    try
                    {
                        if (p.PriorityClass != ProcessPriorityClass.High)
                        {
                            p.PriorityClass = ProcessPriorityClass.High;
                        }
                    }
                    catch { }
                    finally
                    {
                        p.Dispose();
                    }
                }
                break;
            }
        }

        if (activeGame != null && _lastActiveGame == null)
        {
            // Game newly started!
            _lastActiveGame = activeGame;
            AuditLogger.Log(AuditLogLevel.Success, "GameBooster", $"Competitive game '{activeGame}' detected in foreground. Purging standby memory & setting High priority.");
            
            // Purge Standby List & Cache
            _ = MemReductEngine.CleanMemoryAsync(new MemReductOptions
            {
                CleanWorkingSet = true,
                CleanSystemFileCache = true,
                CleanStandbyList = true,
                CleanModifiedPageList = false,
                CombineMemoryLists = true,
                CleanRegistryCache = true
            });

            OnGameBoostStateChanged?.Invoke(activeGame, true);
        }
        else if (activeGame == null && _lastActiveGame != null)
        {
            // Game closed
            AuditLogger.Log(AuditLogLevel.Info, "GameBooster", $"Game '{_lastActiveGame}' exited. System returned to balanced state.");
            _lastActiveGame = null;
            OnGameBoostStateChanged?.Invoke(string.Empty, false);
        }
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
