using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TotkTagEditor.Models;

namespace TotkTagEditor.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Document> _documents = [];

    [ObservableProperty]
    private Document? _current;

    [RelayCommand]
    public async Task OpenFile()
    {

    }

    [RelayCommand]
    public void ShowSettings()
    {
        if (Documents.FirstOrDefault(x => x is SettingsViewModel) is SettingsViewModel target) {
            Current = target;
            return;
        }

        Documents.Add(new SettingsViewModel());
        Current = Documents[^1];
    }
}
