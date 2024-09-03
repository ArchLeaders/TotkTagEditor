using Avalonia;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TotkCommon;
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
        if ((Application.Current as App)?.GetStorageProvider() is not IStorageProvider storageProvider) {
            return;
        }

        IReadOnlyList<IStorageFile> results = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
            AllowMultiple = true,
            Title = "Open Tag Resource Database File(s)",
            FileTypeFilter = [new FilePickerFileType("Tag Resource Database Files (Tag.Product.rstbl)") {
                Patterns = ["*Tag.Product.*.rstbl.byml", "*Tag.Product.*.rstbl.byml.zs"]
            }],
        });

        foreach (IStorageFile result in results) {
            Documents.Add(await TagDatabaseViewModel.FromStorageFileAsync(result));
            Current = Documents[^1];
        }
    }

    [RelayCommand]
    public async Task Save()
    {
        if (Current is null) {
            return;
        }

        await Current.Save();
    }

    [RelayCommand]
    public async Task SaveAs()
    {
        if (Current is null) {
            return;
        }

        if ((Application.Current as App)?.GetStorageProvider() is not IStorageProvider storageProvider) {
            return;
        }

        IStorageFile? result = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
            Title = "Open Tag Resource Database File(s)",
            SuggestedFileName = $"Tag.Product.{Totk.Config.Version}.rstbl.byml.zs",
            FileTypeChoices = [new("App Resource Database Files (Tag.Product.rstbl)") {
                Patterns = ["*Tag.Product.*.rstbl.byml", "*Tag.Product.*.rstbl.byml.zs"]
            }],
        });

        if (result is null) {
            return;
        }

        await Current.SaveAs(result);
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
