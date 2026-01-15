using System.Diagnostics;
using System.Runtime.InteropServices;
using NRTX.Optimizer.Core.Native;

namespace NRTX.Optimizer.Core.Safety;

public static class RestorePointManager
{
    public static async Task<bool> CreateRestorePointAsync(string description = "NRTX Optimizer Safety Snapshot")
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!PrivilegeGuard.IsAdministrator())
                {
                    return false;
                }

                // 1. Try Native Srclient API first
                var rpInfo = new NativeMethods.RESTOREPOINTINFO
                {
                    dwEventType = NativeMethods.BEGIN_SYSTEM_CHANGE,
                    dwRestorePtType = NativeMethods.MODIFY_SETTINGS,
                    llSequenceNumber = 0,
                    szDescription = description
                };

                if (NativeMethods.SRSetRestorePointW(ref rpInfo, out var status))
                {
                    // End system change
                    var endInfo = new NativeMethods.RESTOREPOINTINFO
                    {
                        dwEventType = NativeMethods.END_SYSTEM_CHANGE,
                        llSequenceNumber = status.llSequenceNumber
                    };
                    NativeMethods.SRSetRestorePointW(ref endInfo, out _);
                    return true;
                }

                // 2. Fallback to PowerShell WMI / Checkpoint-Computer
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"Enable-ComputerRestore -Drive 'C:\\'; Checkpoint-Computer -Description '{description}' -RestorePointType 'MODIFY_SETTINGS'\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    proc.WaitForExit(30000);
                    return proc.ExitCode == 0;
                }

                return false;
            }
            catch
            {
                return false;
            }
        });
    }
}
