using BymlLibrary;
using BymlLibrary.Nodes.Containers;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.Mvvm.ComponentModel;
using Revrs;
using Revrs.Buffers;
using System.Buffers;
using System.Collections.ObjectModel;
using TotkCommon;
using VYaml.Emitter;

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

    public static TagDatabase FromYaml(string yaml)
    {
        return TagDatabaseYaml.FromYaml(yaml);
    }

    public static TagDatabase FromYaml(ReadOnlySequence<byte> data)
    {
        return TagDatabaseYaml.FromYaml(data);
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
        {
            emitter.WriteString("Entries");
            WriteYamlEntries(ref emitter);

            emitter.WriteString("Tags");
            WriteYamlTags(ref emitter);

            emitter.WriteString("RankTable");
            emitter.Tag("!!binary");
            emitter.WriteString(Convert.ToBase64String(RankTableCache));
        }
        emitter.EndMapping();
    }

    public void WriteYamlEntries(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginMapping();
        foreach (TagDatabaseEntry entry in Entries) {
            emitter.WriteString($"{entry.Prefix}|{entry.Name}|{entry.Suffix}");

            emitter.BeginSequence();
            foreach (string tag in entry.Tags) {
                emitter.WriteString(tag);
            }
            emitter.EndSequence();
        }
        emitter.EndMapping();
    }

    public void WriteYamlTags(ref Utf8YamlEmitter emitter)
    {
        emitter.BeginSequence();
        foreach (string tag in Tags) {
            emitter.WriteString(tag);
        }
        emitter.EndSequence();
    }

    public void Save(Stream output, bool compress)
    {
        Sort();

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

        if (compress) {
            using SpanOwner<byte> compressed = SpanOwner<byte>.Allocate(raw.Length);
            int size = Totk.Zstd.Compress(raw, compressed.Span, _dictionaryId);

            output.Write(compressed.Span[..size]);
            return;
        }

        output.Write(raw);
    }

    public void Sort()
    {
        // Entries = [.. Entries.OrderBy(x => x.Name, StringComparer.Ordinal)];
        Tags = [.. Tags.Order(StringComparer.Ordinal)];
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
