using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using Revrs.Buffers;
using System.Text;
using TotkTagEditor.Core;
using TotkTagEditor.Extensions;
using TotkTagEditor.Models;
using TotkTagEditor.Views;
using VYaml.Emitter;

namespace TotkTagEditor.ViewModels;

public partial class TagDatabaseViewModel : Document
{
    [ObservableProperty]
    private string _entriesYaml;

    [ObservableProperty]
    private string _tagsYaml;

    public TagDatabaseViewModel(string path, Stream input) : base(path.GetRomfsParentFolderName(), path, Symbol.Tag)
    {
        int size = Convert.ToInt32(input.Length);
        using ArraySegmentOwner<byte> data = ArraySegmentOwner<byte>.Allocate(size);
        input.Read(data.Segment);
        TagDatabase database = new(data.Segment);

        using ArrayPoolBufferWriter<byte> writer = new();
        Utf8YamlEmitter emitter = new(writer);

        database.WriteYamlEntries(ref emitter);
        int entriesOffset = writer.WrittenSpan.Length;
        _entriesYaml = Encoding.UTF8.GetString(writer.WrittenSpan);

        database.WriteYamlTags(ref emitter);
        _tagsYaml = Encoding.UTF8.GetString(writer.WrittenSpan[entriesOffset..]);

        TagDatabaseView view = new() {
            DataContext = this
        };

        view.EntriesTextEditor.Text = _entriesYaml;
        view.TagsTextEditor.Text = _tagsYaml;

        Content = view;
    }
}
