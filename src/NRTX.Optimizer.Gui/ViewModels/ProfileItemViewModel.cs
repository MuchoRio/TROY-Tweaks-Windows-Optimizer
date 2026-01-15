using CommunityToolkit.Mvvm.ComponentModel;
using NRTX.Optimizer.Core.Abstractions;
using NRTX.Optimizer.Core.Localization;

namespace NRTX.Optimizer.Gui.ViewModels;

public partial class ProfileItemViewModel : ObservableObject
{
    public IProfile Profile { get; }

    public string Id => Profile.Id;
    public string Name => LocalizationManager.GetProfileInfo(Profile.Id).Name;
    public string Description => LocalizationManager.GetProfileInfo(Profile.Id).Description;
    public string Icon => Profile.Icon;
    public int TweakCount => Profile.TargetTweakIds.Count;

    [ObservableProperty]
    private bool _isBusy;

    public ProfileItemViewModel(IProfile profile)
    {
        Profile = profile;
        LocalizationManager.OnLanguageChanged += _ =>
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Description));
        };
    }
}
