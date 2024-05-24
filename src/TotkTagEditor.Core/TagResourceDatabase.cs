using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using TotkTagEditor.Core.Models;

namespace TotkTagEditor.Core;

public partial class TagResourceDatabase : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> _tags;

    [ObservableProperty]
    private ObservableCollection<ActorTagInfo> _actors;

    public TagResourceDatabase(ArraySegment<byte> data)
    {

    }
}
