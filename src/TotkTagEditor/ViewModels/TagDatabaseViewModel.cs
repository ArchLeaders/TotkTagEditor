using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using Revrs.Buffers;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Platform.Storage;
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
    private readonly IStorageFile _target;

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

    public static async Task<TagDatabaseViewModel> FromStorageFileAsync(IStorageFile target)
    {
        await using Stream input = await target.OpenReadAsync();
        int size = Convert.ToInt32(input.Length);
        byte[] data = ArrayPool<byte>.Shared.Rent(size);
        int read = await input.ReadAsync(data);
        TagDatabaseViewModel result = new(target, new(data, 0, read));
        ArrayPool<byte>.Shared.Return(data);
        return result;
    }

    public TagDatabaseViewModel(IStorageFile target, ArraySegment<byte> data) : base(target.Path.OriginalString.GetRomfsParentFolderName(), target.Name, Symbol.Tag)
    {
        _target = target;

        TagDatabase database = new(data);
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
        return SaveAs(_target);
    }

    public override async Task<bool> SaveAs(IStorageFile target)
    {
        await using (Stream output = await target.OpenWriteAsync()) {
            GetTagDatabase().Save(output, Path.GetExtension(target.Path.OriginalString.AsSpan()) is ".zs");
        }

        await Dialogs.Success(
            $"File successfully saved to '{target.Name}'",
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
