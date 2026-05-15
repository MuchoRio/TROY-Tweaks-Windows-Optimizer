using System;
using System.Windows;
using System.Windows.Threading;
using NRTX.Optimizer.Core.Safety;

namespace NRTX.Optimizer.Gui;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var stack = e.Exception.StackTrace ?? string.Empty;

        // Silently ignore benign framework telemetry / shutdown / dispose / unload artifacts during window close or exit
        if (stack.Contains("ControlsTraceLogger") ||
            stack.Contains("CriticalShutdown") ||
            stack.Contains("UpdateWindowListsOnClose") ||
            stack.Contains("InternalDispose") ||
            stack.Contains("WmDestroy") ||
            stack.Contains("ModuleUninitializer") ||
            stack.Contains("__scrt_uninitialize"))
        {
            e.Handled = true;
            return;
        }

        var reportPath = CrashReporter.GenerateErrorReport(e.Exception, "WPF Dispatcher UI Thread");
        var shortMsg = e.Exception.Message;

        MessageBox.Show(
            $"TROY Tweaks Windows Optimizer encountered an unexpected error:\n\n" +
            $"❌ {shortMsg}\n\n" +
            $"📄 An error report (.txt) has been automatically generated and saved to:\n" +
            $"{reportPath}\n\n" +
            $"Please share this log file if you need technical assistance.",
            "TROY Tweaks Windows Optimizer - Error Log Created",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            var stack = ex.StackTrace ?? string.Empty;

            // Silently ignore benign framework telemetry / shutdown / unload artifacts during exit
            if (e.IsTerminating && (
                stack.Contains("ControlsTraceLogger") ||
                stack.Contains("CriticalShutdown") ||
                stack.Contains("UpdateWindowListsOnClose") ||
                stack.Contains("InternalDispose") ||
                stack.Contains("WmDestroy") ||
                stack.Contains("ModuleUninitializer") || 
                stack.Contains("__scrt_uninitialize") || 
                stack.Contains("_app_exit_callback") ||
                stack.Contains("__std_type_info_destroy_list")))
            {
                return;
            }

            var reportPath = CrashReporter.GenerateErrorReport(ex, "AppDomain Unhandled (Fatal)");

            MessageBox.Show(
                $"A critical error occurred:\n\n" +
                $"❌ {ex.Message}\n\n" +
                $"📄 A crash diagnostic report (.txt) has been saved to:\n" +
                $"{reportPath}",
                "TROY Tweaks Windows Optimizer - Fatal Error Log Created",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var reportPath = CrashReporter.GenerateErrorReport(e.Exception, "Unobserved Task Exception");
        AuditLogger.Log(AuditLogLevel.Warn, "TaskScheduler", $"Unobserved background task exception logged to: {reportPath}");
        e.SetObserved();
    }
}
