using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Localization;
using NRTX.Optimizer.Core.Models;

namespace NRTX.Optimizer.Gui.ViewModels;

public partial class TweakItemViewModel : ObservableObject
{
    public ITweak Tweak { get; }

    [ObservableProperty]
    private bool _isApplied;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusText = "Checking...";

    public string Id => Tweak.Id;
    public string Name => LocalizationManager.GetTweakInfo(Tweak.Id).Name is { Length: > 0 } n ? n : Tweak.Name;
    public string Description => LocalizationManager.GetTweakInfo(Tweak.Id).Description is { Length: > 0 } d ? d : Tweak.Description;
    public string Purpose => LocalizationManager.GetTweakInfo(Tweak.Id).Purpose;
    public string HowItWorks => LocalizationManager.GetTweakInfo(Tweak.Id).HowItWorks;
    public string Impact => LocalizationManager.GetTweakInfo(Tweak.Id).Impact;
    public TweakCategory Category => Tweak.Category;
    public string DisplayCategory => LocalizationManager.GetCategoryName(Tweak.Category);
    public RiskLevel Risk => Tweak.Risk;
    public string DisplayRisk => LocalizationManager.GetRiskName(Tweak.Risk);
    public bool RequiresRestart => Tweak.RequiresRestart;

    public string RiskBadgeColor => Risk switch
    {
        RiskLevel.Safe => "#10b981",
        RiskLevel.Recommended => "#38bdf8",
        RiskLevel.Advanced => "#f59e0b",
        _ => "#94a3b8"
    };

    public string CategoryBadgeColor => Category switch
    {
        TweakCategory.Privacy => "#8b5cf6",
        TweakCategory.Performance => "#ec4899",
        TweakCategory.Gaming => "#f43f5e",
        TweakCategory.Network => "#06b6d4",
        TweakCategory.Services => "#eab308",
        TweakCategory.Debloater => "#f97316",
        TweakCategory.Maintenance => "#10b981",
        _ => "#64748b"
    };

    public TweakItemViewModel(ITweak tweak)
    {
        Tweak = tweak;
        LocalizationManager.OnLanguageChanged += _ =>
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(Purpose));
            OnPropertyChanged(nameof(HowItWorks));
            OnPropertyChanged(nameof(Impact));
            OnPropertyChanged(nameof(DisplayCategory));
            OnPropertyChanged(nameof(DisplayRisk));
            UpdateStatusText();
        };
    }

    private void UpdateStatusText()
    {
        StatusText = IsApplied
            ? (LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Aktif" : "Active")
            : (LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Bawaan" : "Default");
    }

    public async Task RefreshStateAsync()
    {
        try
        {
            IsApplied = await Tweak.IsAppliedAsync();
            UpdateStatusText();
        }
        catch
        {
            StatusText = LocalizationManager.CurrentLanguage == AppLanguage.Indonesian ? "Tidak Diketahui" : "Unknown";
        }
    }

    [RelayCommand]
    public async Task ToggleAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            if (IsApplied)
            {
                var res = await Tweak.RollbackAsync();
                IsApplied = await Tweak.IsAppliedAsync();
                UpdateStatusText();
            }
            else
            {
                var res = await Tweak.ApplyAsync();
                IsApplied = await Tweak.IsAppliedAsync();
                UpdateStatusText();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
