using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using TotkTagEditor.Views;

namespace TotkTagEditor.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [RelayCommand]
    public async Task OpenFile()
    {

    }

    [RelayCommand]
    public static async Task ShowSettings()
    {
        TaskDialog taskDialog = new() {
            Title = "Settings",
            Buttons = [TaskDialogButton.OKButton],
            XamlRoot = (Application.Current as App)?.GetRoot(),
            Content = new SettingsView()
        };

        await taskDialog.ShowAsync();
    }
}
