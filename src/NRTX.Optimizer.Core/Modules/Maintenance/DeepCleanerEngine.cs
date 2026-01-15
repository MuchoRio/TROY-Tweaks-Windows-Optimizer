using System.Diagnostics;
using System.Runtime.InteropServices;
using NRTX.Optimizer.Core.Native;

namespace NRTX.Optimizer.Core.Modules.Maintenance;

public enum JunkCategory
{
    RecycleBin,
    WindowsTempAndLogs,
    BrowserCaches,
    DirectXShaderCaches,
    WindowsDeliveryOptimization,
    WindowsUpdateDownloads,
    ExplorerThumbnailCaches
}

public class JunkItemReport
{
    public JunkCategory Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public int FileCount { get; set; }
    public bool IsSelected { get; set; } = true;

    public string FormattedSize => SizeBytes switch
    {
        > 1024 * 1024 * 1024 => $"{SizeBytes / (1024.0 * 1024 * 1024):F2} GB",
        > 1024 * 1024 => $"{SizeBytes / (1024.0 * 1024):F1} MB",
        > 1024 => $"{SizeBytes / 1024.0:F0} KB",
        _ => $"{SizeBytes} B"
    };
}

public static class DeepCleanerEngine
{
    public static async Task<List<JunkItemReport>> ScanJunkAsync(CancellationToken cancellationToken = default)
    {
        var tempTask = Task.Run(() =>
        {
            var tempDirs = new[]
            {
                Path.GetTempPath(),
                @"C:\Windows\Temp",
                @"C:\Windows\Minidump",
                @"C:\ProgramData\Microsoft\Windows\WER\ReportArchive",
                @"C:\ProgramData\Microsoft\Windows\WER\ReportQueue"
            };
            return CalculateDirectoriesSize(
                JunkCategory.WindowsTempAndLogs,
                "Windows Temp, Crash Dumps & Error Reports",
                "User & System temp files, kernel crash dumps, and Windows Error Reporting archives.",
                tempDirs
            );
        }, cancellationToken);

        var browserTask = Task.Run(() =>
        {
            var browserDirs = GetBrowserCacheDirectories();
            return CalculateDirectoriesSize(
                JunkCategory.BrowserCaches,
                "Browser Caches (Chrome, Edge, Brave, Firefox)",
                "Temporary web page assets, code caches, GPU shader caches, and media cookies.",
                browserDirs
            );
        }, cancellationToken);

        var shaderTask = Task.Run(() =>
        {
            var shaderDirs = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "D3DSCache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NVIDIA", "DXCache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NVIDIA", "GLCache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AMD", "DxCache")
            };
            return CalculateDirectoriesSize(
                JunkCategory.DirectXShaderCaches,
                "DirectX & GPU Shader Caches",
                "Outdated precompiled GPU shaders from games and 3D applications.",
                shaderDirs
            );
        }, cancellationToken);

        var doTask = Task.Run(() =>
        {
            var doDirs = new[]
            {
                @"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache"
            };
            return CalculateDirectoriesSize(
                JunkCategory.WindowsDeliveryOptimization,
                "Windows Delivery Optimization Cache",
                "Peer-to-peer Windows update chunks stored on disk.",
                doDirs
            );
        }, cancellationToken);

        var wuTask = Task.Run(() =>
        {
            var wuDirs = new[]
            {
                @"C:\Windows\SoftwareDistribution\Download"
            };
            return CalculateDirectoriesSize(
                JunkCategory.WindowsUpdateDownloads,
                "Windows Update Download Leftovers",
                "Old downloaded installer packages in SoftwareDistribution\\Download.",
                wuDirs
            );
        }, cancellationToken);

        var explorerTask = Task.Run(() =>
        {
            var explorerDirs = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Explorer")
            };
            return CalculateDirectoriesSize(
                JunkCategory.ExplorerThumbnailCaches,
                "Explorer Thumbnail & Icon Caches",
                "Cached database files of file previews and system icons.",
                explorerDirs,
                searchPattern: "*.db"
            );
        }, cancellationToken);

        var recycleBinTask = Task.Run(() =>
        {
            var (rbBytes, rbFiles) = QueryRecycleBinStats();
            return new JunkItemReport
            {
                Category = JunkCategory.RecycleBin,
                Name = "Windows Recycle Bin",
                Description = "Deleted files across all local drive recycle bins.",
                SizeBytes = rbBytes,
                FileCount = rbFiles,
                IsSelected = true
            };
        }, cancellationToken);

        var results = await Task.WhenAll(
            tempTask, browserTask, shaderTask, doTask, wuTask, explorerTask, recycleBinTask
        );

        return results.ToList();
    }

    public static async Task<(long freedBytes, int cleanedFiles)> CleanJunkAsync(IEnumerable<JunkCategory> categories, CancellationToken cancellationToken = default)
    {
        var catSet = new HashSet<JunkCategory>(categories);
        var tasks = new List<Task<(long bytes, int files)>>();

        if (catSet.Contains(JunkCategory.RecycleBin))
        {
            tasks.Add(Task.Run(() =>
            {
                long rbFreed = 0;
                int rbCount = 0;
                try
                {
                    var (rbBytes, rbFiles) = QueryRecycleBinStats();
                    uint hr = NativeMethods.SHEmptyRecycleBin(
                        IntPtr.Zero,
                        null,
                        NativeMethods.SHERB_NOCONFIRMATION | NativeMethods.SHERB_NOPROGRESSUI | NativeMethods.SHERB_NOSOUND
                    );
                    if (hr == 0 || hr == 0x80004005)
                    {
                        rbFreed = rbBytes;
                        rbCount = rbFiles;
                    }
                }
                catch { }
                return (rbFreed, rbCount);
            }, cancellationToken));
        }

        if (catSet.Contains(JunkCategory.WindowsTempAndLogs))
        {
            tasks.Add(Task.Run(() => PurgeDirectories(new[]
            {
                Path.GetTempPath(),
                @"C:\Windows\Temp",
                @"C:\Windows\Minidump",
                @"C:\ProgramData\Microsoft\Windows\WER\ReportArchive",
                @"C:\ProgramData\Microsoft\Windows\WER\ReportQueue"
            }), cancellationToken));
        }

        if (catSet.Contains(JunkCategory.BrowserCaches))
        {
            tasks.Add(Task.Run(() => PurgeDirectories(GetBrowserCacheDirectories()), cancellationToken));
        }

        if (catSet.Contains(JunkCategory.DirectXShaderCaches))
        {
            tasks.Add(Task.Run(() => PurgeDirectories(new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "D3DSCache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NVIDIA", "DXCache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NVIDIA", "GLCache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AMD", "DxCache")
            }), cancellationToken));
        }

        if (catSet.Contains(JunkCategory.WindowsDeliveryOptimization))
        {
            tasks.Add(Task.Run(() => PurgeDirectories(new[]
            {
                @"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache"
            }), cancellationToken));
        }

        if (catSet.Contains(JunkCategory.WindowsUpdateDownloads))
        {
            tasks.Add(Task.Run(() => PurgeDirectories(new[]
            {
                @"C:\Windows\SoftwareDistribution\Download"
            }), cancellationToken));
        }

        if (catSet.Contains(JunkCategory.ExplorerThumbnailCaches))
        {
            tasks.Add(Task.Run(() => PurgeDirectories(new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Windows\Explorer")
            }, searchPattern: "*.db"), cancellationToken));
        }

        var results = await Task.WhenAll(tasks);
        long totalFreed = results.Sum(r => r.bytes);
        int totalFiles = results.Sum(r => r.files);

        return (totalFreed, totalFiles);
    }

    private static string[] GetBrowserCacheDirectories()
    {
        var localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var list = new List<string>
        {
            // Chrome
            Path.Combine(localApp, @"Google\Chrome\User Data\Default\Cache"),
            Path.Combine(localApp, @"Google\Chrome\User Data\Default\Code Cache"),
            Path.Combine(localApp, @"Google\Chrome\User Data\Default\GPUCache"),
            
            // Edge
            Path.Combine(localApp, @"Microsoft\Edge\User Data\Default\Cache"),
            Path.Combine(localApp, @"Microsoft\Edge\User Data\Default\Code Cache"),
            Path.Combine(localApp, @"Microsoft\Edge\User Data\Default\GPUCache"),

            // Brave
            Path.Combine(localApp, @"BraveSoftware\Brave-Browser\User Data\Default\Cache"),
            Path.Combine(localApp, @"BraveSoftware\Brave-Browser\User Data\Default\Code Cache"),

            // Opera
            Path.Combine(localApp, @"Opera Software\Opera Stable\Cache")
        };

        // Firefox (specifically target cache2 and startupCache in each profile)
        var ffProfilesDir = Path.Combine(localApp, @"Mozilla\Firefox\Profiles");
        if (Directory.Exists(ffProfilesDir))
        {
            try
            {
                foreach (var p in Directory.GetDirectories(ffProfilesDir))
                {
                    var c2 = Path.Combine(p, "cache2");
                    if (Directory.Exists(c2)) list.Add(c2);
                    var sc = Path.Combine(p, "startupCache");
                    if (Directory.Exists(sc)) list.Add(sc);
                }
            }
            catch { }
        }

        return list.ToArray();
    }

    private static readonly EnumerationOptions SafeEnumOptions = new()
    {
        IgnoreInaccessible = true,
        RecurseSubdirectories = true,
        AttributesToSkip = FileAttributes.ReparsePoint
    };

    private static JunkItemReport CalculateDirectoriesSize(
        JunkCategory cat,
        string name,
        string desc,
        IEnumerable<string> dirs,
        string searchPattern = "*")
    {
        long totalBytes = 0;
        int count = 0;

        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir)) continue;

            try
            {
                var di = new DirectoryInfo(dir);
                foreach (var file in di.EnumerateFiles(searchPattern, SafeEnumOptions))
                {
                    try
                    {
                        totalBytes += file.Length;
                        count++;
                    }
                    catch { }
                }
            }
            catch { }
        }

        return new JunkItemReport
        {
            Category = cat,
            Name = name,
            Description = desc,
            SizeBytes = totalBytes,
            FileCount = count,
            IsSelected = true
        };
    }

    private static (long sizeBytes, int fileCount) QueryRecycleBinStats()
    {
        long totalSize = 0;
        long totalCount = 0;

        try
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Removable) continue;

                var rbInfo = new NativeMethods.SHQUERYRBINFO
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SHQUERYRBINFO))
                };

                int hr = NativeMethods.SHQueryRecycleBin(drive.Name, ref rbInfo);
                if (hr == 0 && rbInfo.i64Size > 0)
                {
                    totalSize += rbInfo.i64Size;
                    totalCount += rbInfo.i64NumItems;
                }
            }
        }
        catch { }

        if (totalSize == 0 && totalCount == 0)
        {
            try
            {
                var globalRb = new NativeMethods.SHQUERYRBINFO
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SHQUERYRBINFO))
                };
                if (NativeMethods.SHQueryRecycleBin(null, ref globalRb) == 0)
                {
                    totalSize = globalRb.i64Size;
                    totalCount = globalRb.i64NumItems;
                }
            }
            catch { }
        }

        return (totalSize, (int)Math.Min(totalCount, int.MaxValue));
    }

    private static (long bytes, int files) PurgeDirectories(IEnumerable<string> dirs, string searchPattern = "*")
    {
        long freed = 0;
        int count = 0;

        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir)) continue;

            try
            {
                var di = new DirectoryInfo(dir);
                foreach (var file in di.EnumerateFiles(searchPattern, SafeEnumOptions))
                {
                    try
                    {
                        long size = file.Length;
                        file.Delete();
                        freed += size;
                        count++;
                    }
                    catch { }
                }

                if (searchPattern == "*")
                {
                    try
                    {
                        foreach (var sub in di.EnumerateDirectories("*", new EnumerationOptions { IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint }))
                        {
                            try
                            {
                                sub.Delete(true);
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        return (freed, count);
    }
}
