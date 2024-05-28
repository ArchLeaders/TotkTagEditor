using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using TotkCommon;
using TotkTagEditor.Models;
using TotkTagEditor.Views;

namespace TotkTagEditor.ViewModels;

public partial class SettingsViewModel : Document
{
    [ObservableProperty]
    private string _gamePath;

    public SettingsViewModel() : base("Settings", "Settings", Symbol.SettingsFilled)
    {
        _gamePath = Totk.Config.GamePath;

        Content = new SettingsView {
            DataContext = this
        };
    }

    partial void OnGamePathChanged(string value)
    {
        Totk.Config.GamePath = value;
    }
}
