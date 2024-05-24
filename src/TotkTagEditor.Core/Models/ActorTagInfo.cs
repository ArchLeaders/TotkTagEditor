using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace TotkTagEditor.Core.Models;

public partial class ActorTagInfo(string prefix, string name, string suffix) : ObservableObject
{
    [ObservableProperty]
    private string _prefix = prefix;

    [ObservableProperty]
    private string _name = name;

    [ObservableProperty]
    private string _suffix = suffix;

    [ObservableProperty]
    private ObservableCollection<string> _tags = [];

    public ActorTagInfo(string prefix, string name, string suffix, IEnumerable<string> tags) : this(prefix, name, suffix)
    {
        _tags = [.. tags];
    }
}
