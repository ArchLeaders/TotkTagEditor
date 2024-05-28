using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using Revrs.Buffers;
using TotkTagEditor.Core;
using TotkTagEditor.Extensions;
using TotkTagEditor.Models;

namespace TotkTagEditor.ViewModels;

public partial class TagDatabaseViewModel : Document
{
    [ObservableProperty]
    private TagDatabase _database;

    public TagDatabaseViewModel(string path, Stream input) : base(path.GetRomfsParentFolderName(), path, Symbol.Tag)
    {
        int size = Convert.ToInt32(input.Length);
        using ArraySegmentOwner<byte> data = ArraySegmentOwner<byte>.Allocate(size);
        input.Read(data.Segment);
        _database = new(data.Segment);
    }
}
