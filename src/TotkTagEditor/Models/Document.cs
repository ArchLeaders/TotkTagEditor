using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;

namespace TotkTagEditor.Models;

public abstract partial class Document : ObservableObject
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private IconSource _icon;

    [ObservableProperty]
    private object? _content;

    public abstract Task<bool> Save();
    public abstract Task<bool> SaveAs(IStorageFile target);

    public virtual Task<bool> CloseRequested()
    {
        return Task.FromResult(true);
    }

    public Document(string title, string name, Symbol icon = Symbol.Document)
    {
        _title = title;
        _name = name;
        _icon = new SymbolIconSource {
            Symbol = icon
        };
    }
}
