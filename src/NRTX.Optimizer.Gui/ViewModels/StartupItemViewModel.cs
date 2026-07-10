using System.ComponentModel;
using System.Runtime.CompilerServices;
using NRTX.Optimizer.Core.Engine;
using NRTX.Optimizer.Core.Localization;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Gui.ViewModels;

public class StartupItemViewModel : INotifyPropertyChanged
{
    private readonly StartupEntry _entry;

    public StartupEntry Entry => _entry;

    public string Name => _entry.Name;
    public string Command => _entry.Command;
    public string Publisher => _entry.Publisher;
    public string Location => _entry.Location switch
    {
        StartupLocation.CurrentUserRegistry => LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Registri HKCU" : "HKCU Registry",
        StartupLocation.LocalMachineRegistry => LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Registri HKLM" : "HKLM Registry",
        StartupLocation.LocalMachineWow64Registry => LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Registri HKLM (32-bit)" : "HKLM (32-bit)",
        StartupLocation.UserStartupFolder => LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Folder Startup Pengguna" : "User Startup Folder",
        StartupLocation.CommonStartupFolder => LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Folder Startup Bersama" : "All Users Startup Folder",
        StartupLocation.TaskSchedulerLogon => LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Penjadwal Tugas" : "Task Scheduler",
        _ => _entry.Location.ToString()
    };

    public string ImpactText => _entry.Impact switch
    {
        StartupImpact.High => LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Dampak Tinggi" : "High Impact",
        StartupImpact.Medium => LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Dampak Sedang" : "Medium Impact",
        _ => LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Dampak Rendah" : "Low Impact"
    };

    public string ImpactColor => _entry.Impact switch
    {
        StartupImpact.High => "#ef4444",
        StartupImpact.Medium => "#f59e0b",
        _ => "#10b981"
    };

    public bool IsEnabled
    {
        get => _entry.IsEnabled;
        set
        {
            if (_entry.IsEnabled != value)
            {
                StartupManagerEngine.ToggleStartupEntry(_entry, value);
                OnPropertyChanged(nameof(IsEnabled));
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public string StatusText => IsEnabled
        ? (LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Aktif" : "Enabled")
        : (LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Nonaktif" : "Disabled");

    public StartupItemViewModel(StartupEntry entry)
    {
        _entry = entry;
    }

    public void NotifyLocalizationChanged()
    {
        OnPropertyChanged(nameof(ImpactText));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(Location));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
