using CommunityToolkit.Mvvm.ComponentModel;
using TotkCommon;

namespace TotkTagEditor.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _gamePath;

    public SettingsViewModel()
    {
        _gamePath = Totk.Config.GamePath;
    }

    partial void OnGamePathChanged(string value)
    {
        Totk.Config.GamePath = value;
    }
}
