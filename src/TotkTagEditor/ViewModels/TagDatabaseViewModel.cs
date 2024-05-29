using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using Revrs.Buffers;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Text;
using TotkTagEditor.Core;
using TotkTagEditor.Extensions;
using TotkTagEditor.Models;
using TotkTagEditor.Views;
using VYaml.Emitter;
using VYaml.Parser;

namespace TotkTagEditor.ViewModels;

public partial class TagDatabaseViewModel : Document
{
    private readonly TagDatabaseView _view;
    private readonly string _path;

    public string EntriesYaml {
        get => _view.EntriesTextEditor.Text;
        set => _view.EntriesTextEditor.Text = value;
    }

    public string TagsYaml {
        get => _view.TagsTextEditor.Text;
        set => _view.TagsTextEditor.Text = value;
    }

    [ObservableProperty]
    private byte[] _rankTable;

    public TagDatabaseViewModel(string path, Stream input) : base(path.GetRomfsParentFolderName(), path, Symbol.Tag)
    {
        _path = path;

        int size = Convert.ToInt32(input.Length);
        using ArraySegmentOwner<byte> data = ArraySegmentOwner<byte>.Allocate(size);
        input.Read(data.Segment);

        TagDatabase database = new(data.Segment);
        _rankTable = database.RankTableCache;

        using ArrayPoolBufferWriter<byte> writer = new();
        Utf8YamlEmitter emitter = new(writer);

        _view = new TagDatabaseView {
            DataContext = this
        };

        database.WriteYamlEntries(ref emitter);
        int entriesOffset = writer.WrittenSpan.Length;
        EntriesYaml = Encoding.UTF8.GetString(writer.WrittenSpan);

        database.WriteYamlTags(ref emitter);
        TagsYaml = Encoding.UTF8.GetString(writer.WrittenSpan[entriesOffset..]);

        Content = _view;
    }

    public override Task<bool> Save()
    {
        return SaveAs(_path);
    }

    public override async Task<bool> SaveAs(string path)
    {
        using (FileStream fs = File.Create(path)) {
            GetTagDatabase().Save(fs, path.EndsWith(".zs"));
        }

        await Dialogs.Success(
            $"File successfully saved to '{path}'",
            "Saved Successful");
        return true;
    }

    private TagDatabase GetTagDatabase()
    {
        return new(GetEntries(), GetTags(), RankTable);
    }

    private ObservableCollection<TagDatabaseEntry> GetEntries()
    {
        int utf8Size = Encoding.UTF8.GetByteCount(EntriesYaml);
        using ArraySegmentOwner<byte> utf8Buffer = ArraySegmentOwner<byte>.Allocate(utf8Size);
        Encoding.UTF8.GetBytes(EntriesYaml, utf8Buffer.Segment);
        ReadOnlySequence<byte> utf8Yaml = new(utf8Buffer.Segment);

        YamlParser parser = new(utf8Yaml);
        return TagDatabaseYaml.ReadEntries(ref parser);
    }

    private ObservableCollection<string> GetTags()
    {
        int utf8Size = Encoding.UTF8.GetByteCount(TagsYaml);
        using ArraySegmentOwner<byte> utf8Buffer = ArraySegmentOwner<byte>.Allocate(utf8Size);
        Encoding.UTF8.GetBytes(TagsYaml, utf8Buffer.Segment);
        ReadOnlySequence<byte> utf8Yaml = new(utf8Buffer.Segment);

        YamlParser parser = new(utf8Yaml);
        return TagDatabaseYaml.ReadTags(ref parser);
    }
}
