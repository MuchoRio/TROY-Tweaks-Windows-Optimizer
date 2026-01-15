using System.Diagnostics;

namespace NRTX.Optimizer.Core.Safety;

public static class RegistryBackupEngine
{
    private static readonly string BackupDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NRTX_Optimizer",
        "Backups"
    );

    public static string EnsureBackupDirectory()
    {
        if (!Directory.Exists(BackupDir))
        {
            Directory.CreateDirectory(BackupDir);
        }
        return BackupDir;
    }

    public static bool BackupKey(string registryPath, string filenamePrefix)
    {
        try
        {
            EnsureBackupDirectory();
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var outPath = Path.Combine(BackupDir, $"{filenamePrefix}_{timestamp}.reg");

            var psi = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"export \"{registryPath}\" \"{outPath}\" /y",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var proc = Process.Start(psi);
            proc?.WaitForExit(5000);
            return proc != null && proc.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
