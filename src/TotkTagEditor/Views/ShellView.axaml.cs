using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using TotkTagEditor.Models;
using TotkTagEditor.ViewModels;

namespace TotkTagEditor.Views;

public partial class ShellView : UserControl
{
    public ShellView()
    {
        InitializeComponent();
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
}
