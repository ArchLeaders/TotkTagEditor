using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using TotkTagEditor.Models;
using TotkTagEditor.ViewModels;

namespace TotkTagEditor.Views;

public partial class ShellView : UserControl
{
    public ShellView()
    {
        InitializeComponent();

        DragRegion.AddHandler(DragDrop.DropEvent, DragDropEvent);
    }

#pragma warning disable IDE0051 // Used by the XAML
    private async void TabItemCloseRequested(TabViewItem _, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is Document doc && await doc.CloseRequested() && DataContext is ShellViewModel vm) {
            int index = vm.Documents.IndexOf(doc);
            vm.Documents.RemoveAt(index);
            vm.Current = null;

            if (vm.Documents.Count == 0) {
                return;
            }

            vm.Current = index == vm.Documents.Count
                ? vm.Documents[--index] : vm.Documents[index];
        }
    }
#pragma warning restore IDE0051

    public async void DragDropEvent(object? sender, DragEventArgs e)
    {
        if (DataContext is not ShellViewModel vm) {
            return;
        }

        if (e.Data.GetFiles() is IEnumerable<IStorageItem> files) {
            foreach (IStorageFile file in files.Where(x => x.Path.IsFile).Cast<IStorageFile>()) {
                TagDatabaseViewModel tagDatabaseViewModel = new(file.Path.LocalPath, await file.OpenReadAsync());
                vm.Documents.Add(tagDatabaseViewModel);
                vm.Current = vm.Documents[^1];
            }
        }
    }
}
