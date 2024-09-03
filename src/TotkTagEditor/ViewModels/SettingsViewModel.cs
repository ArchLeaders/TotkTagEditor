using Avalonia.Platform.Storage;
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

    public override Task<bool> SaveAs(IStorageFile _)
    {
        return Save();
    }

    public override Task<bool> Save()
    {
        Totk.Config.Save();
        return Task.FromResult(true);
    }

    partial void OnGamePathChanged(string value)
    {
        try {
            Totk.Config.GamePath = value;
        }
        catch (Exception ex) {
            throw new InvalidOperationException(
                "Invalid Game Path", ex);
        }

        Save();
    }
}
