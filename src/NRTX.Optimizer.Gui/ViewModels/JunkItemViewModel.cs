using CommunityToolkit.Mvvm.ComponentModel;
using NRTX.Optimizer.Core.Localization;
using NRTX.Optimizer.Core.Modules.Maintenance;

namespace NRTX.Optimizer.Gui.ViewModels;

public partial class JunkItemViewModel : ObservableObject
{
    public JunkItemReport Report { get; }

    [ObservableProperty]
    private bool _isSelected;

    public JunkCategory Category => Report.Category;
    public string Name => LocalizationManager.GetJunkCategoryInfo(Report.Category).Name;
    public string Description => LocalizationManager.GetJunkCategoryInfo(Report.Category).Description;
    public long SizeBytes => Report.SizeBytes;
    public int FileCount => Report.FileCount;
    public string FormattedSize => Report.FormattedSize;

    public JunkItemViewModel(JunkItemReport report)
    {
        Report = report;
        _isSelected = report.IsSelected;
    }

    public void NotifyLocalizationChanged()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Description));
    }
}
