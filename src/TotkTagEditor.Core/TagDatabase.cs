using BymlLibrary;
using BymlLibrary.Nodes.Containers;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.Mvvm.ComponentModel;
using Revrs;
using Revrs.Buffers;
using System.Buffers;
using System.Collections.ObjectModel;
using TotkCommon;
using TotkTagEditor.Core.Models;
using VYaml.Emitter;
using VYaml.Parser;

namespace TotkTagEditor.Core;

public partial class TagDatabase : ObservableObject
{
    private readonly int _dictionaryId = 1;

    [ObservableProperty]
    private ObservableCollection<TagDatabaseEntry> _entries = [];

    [ObservableProperty]
    private ObservableCollection<string> _tags;

    /// <summary>
    /// The raw RankTable bytes (unused in TotK)
    /// </summary>
    [ObservableProperty]
    private byte[] _rankTableCache;

    public static TagDatabase FromFile(string path)
    {
        using FileStream fs = File.OpenRead(path);
        int size = Convert.ToInt32(fs.Length);
        using ArraySegmentOwner<byte> buffer = ArraySegmentOwner<byte>.Allocate(size);
        fs.Read(buffer.Segment);
        return new(buffer.Segment);
    }

    public static TagDatabase FromYaml(ReadOnlySequence<byte> data)
    {
        ObservableCollection<TagDatabaseEntry> entries = [];
        ObservableCollection<string> tags = [];
        byte[] rankTable = [];

        YamlParser parser = new(data);
        parser.SkipAfter(ParseEventType.SequenceStart);
        while (parser.CurrentEventType != ParseEventType.SequenceEnd) {
            entries.Add(TagDatabaseEntry.FromYaml(ref parser));
        }

        parser.SkipAfter(ParseEventType.SequenceStart);
        while (parser.CurrentEventType != ParseEventType.SequenceEnd) {
            if (parser.TryReadScalarAsString(out string? tag) && tag is not null) {
                tags.Add(tag);
            }
        }

        parser.SkipAfter(ParseEventType.SequenceEnd);
        parser.SkipCurrentNode();

        if (parser.TryReadScalarAsString(out string? rankTableBase64) && rankTableBase64 is not null) {
            rankTable = Convert.FromBase64String(rankTableBase64);
        }

        return new(entries, tags, rankTable);
    }

    public TagDatabase(ArraySegment<byte> data)
    {
        Byml byml;

        if (Zstd.IsCompressed(data)) {
            int size = Zstd.GetDecompressedSize(data);
            using ArraySegmentOwner<byte> decompressedBuffer = ArraySegmentOwner<byte>.Allocate(size);
            Totk.Zstd.Decompress(data, decompressedBuffer.Segment, out _dictionaryId);
            byml = Byml.FromBinary(decompressedBuffer.Segment);
        }
        else {
            byml = Byml.FromBinary(data);
        }

        BymlMap root = byml.GetMap();
        _tags = [.. root["TagList"].GetArray().Select(x => x.GetString())];
        _rankTableCache = root["RankTable"].GetBinary();

        BymlArray actorPaths = root["PathList"].GetArray();
        int actorPathCount = actorPaths.Count / 3;

        RevrsReader bitTableReader = RevrsReader.Native(root["BitTable"].GetBinary());
        TagBitTable bitTable = new(ref bitTableReader, _tags.Count, actorPathCount);

        for (int i = 0; i < actorPaths.Count;) {
            string prefix = actorPaths[i].GetString();
            string name = actorPaths[++i].GetString();
            string suffix = actorPaths[++i].GetString();

            int index = (++i / 3) - 1;
            TagDatabaseEntry entry = new(prefix, name, suffix, bitTable.GetTags(index, _tags));
            _entries.Add(entry);
        }
    }

    public TagDatabase(ObservableCollection<TagDatabaseEntry> entries, ObservableCollection<string> tags, byte[] rankTable)
    {
        _entries = entries;
        _tags = tags;
        _rankTableCache = rankTable;
    }

    public void WriteYaml(IBufferWriter<byte> writer)
    {
        Utf8YamlEmitter emitter = new(writer);

        emitter.BeginMapping();
        emitter.WriteString("Entries");
        emitter.BeginSequence();
        foreach (TagDatabaseEntry entry in Entries) {
            emitter.BeginMapping();
            emitter.WriteString($"{entry.Prefix}|{entry.Name}|{entry.Suffix}");

            emitter.BeginSequence();
            foreach (string tag in entry.Tags) {
                emitter.WriteString(tag);
            }
            emitter.EndSequence();
            emitter.EndMapping();
        }
        emitter.EndSequence();

        emitter.WriteString("Tags");
        emitter.BeginSequence();
        foreach (string tag in Tags) {
            emitter.WriteString(tag);
        }
        emitter.EndSequence();

        emitter.WriteString("RankTable");
        emitter.Tag("!!binary");
        emitter.WriteString(Convert.ToBase64String(RankTableCache));
        emitter.EndMapping();
    }

    public void Save(Stream output)
    {
        MemoryStream ms = new();
        Byml root = new BymlMap() {
            { "BitTable", CompileBitTable() },
            { "PathList", CompilePaths() },
            { "RankTable", RankTableCache },
            { "TagList", new BymlArray(Tags.Select(x => (Byml)x)) }
        };

        root.WriteBinary(ms, Endianness.Little, version: 7);
        ms.Seek(0, SeekOrigin.Begin);
        byte[] raw = ms.ToArray();

        using SpanOwner<byte> compressed = SpanOwner<byte>.Allocate(raw.Length);
        Totk.Zstd.Compress(raw, compressed.Span, _dictionaryId);

        output.Write(compressed.Span);
    }

    private byte[] CompileBitTable()
    {
        BitTableWriter writer = new(Tags, Entries);
        return writer.Compile();
    }

    private BymlArray CompilePaths()
    {
        BymlArray paths = new(Entries.Count * 3);
        foreach (TagDatabaseEntry entry in Entries) {
            paths.Add(entry.Prefix);
            paths.Add(entry.Name);
            paths.Add(entry.Suffix);
        }

        return paths;
    }
}
