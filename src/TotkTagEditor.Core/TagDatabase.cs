using BymlLibrary;
using BymlLibrary.Nodes.Containers;
using CommunityToolkit.Mvvm.ComponentModel;
using Revrs;
using Revrs.Buffers;
using System.Collections.ObjectModel;
using TotkCommon;
using TotkTagEditor.Core.Models;

namespace TotkTagEditor.Core;

public partial class TagDatabase : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string> _tags;

    [ObservableProperty]
    private ObservableCollection<TagDatabaseEntry> _actors = [];

    /// <summary>
    /// The raw RankTable bytes (unused in TotK)
    /// </summary>
    [ObservableProperty]
    private byte[] _rankTableCache;

    public TagDatabase(ArraySegment<byte> data)
    {
        Byml byml;

        if (Zstd.IsCompressed(data)) {
            int size = Zstd.GetDecompressedSize(data);
            using ArraySegmentOwner<byte> decompressedBuffer = ArraySegmentOwner<byte>.Allocate(size);
            Totk.Zstd.Decompress(data, decompressedBuffer.Segment);
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
            _actors.Add(entry);
        }
    }
}
