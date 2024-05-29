using Revrs.Buffers;
using System.Buffers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using VYaml.Parser;

namespace TotkTagEditor.Core;

public class TagDatabaseYaml
{
    public static TagDatabase FromYaml(string yaml)
    {
        int utf8Size = Encoding.UTF8.GetByteCount(yaml);
        using ArraySegmentOwner<byte> buffer = ArraySegmentOwner<byte>.Allocate(utf8Size);
        Encoding.UTF8.GetBytes(yaml, buffer.Segment);
        return FromYaml(new ReadOnlySequence<byte>(buffer.Segment));
    }

    public static TagDatabase FromYaml(ReadOnlySequence<byte> utf8Yaml)
    {
        YamlParser parser = new(utf8Yaml);
        return new(ReadEntries(ref parser), ReadTags(ref parser), ReadRankTable(ref parser));
    }

    public static ObservableCollection<TagDatabaseEntry> ReadEntries(ref YamlParser parser)
    {
        ObservableCollection<TagDatabaseEntry> entries = [];
        parser.SkipAfter(ParseEventType.MappingStart);
        while (parser.CurrentEventType != ParseEventType.MappingEnd) {
            entries.Add(TagDatabaseEntry.FromYaml(ref parser));
        }

        return entries;
    }

    public static ObservableCollection<string> ReadTags(ref YamlParser parser)
    {
        ObservableCollection<string> tags = [];
        parser.SkipAfter(ParseEventType.SequenceStart);
        while (parser.CurrentEventType != ParseEventType.SequenceEnd) {
            if (parser.TryReadScalarAsString(out string? tag) && tag is not null) {
                tags.Add(tag);
            }
        }

        return tags;
    }

    public static byte[] ReadRankTable(ref YamlParser parser)
    {
        if (parser.TryReadScalarAsString(out string? rankTableBase64) && rankTableBase64 is not null) {
            return Convert.FromBase64String(rankTableBase64);
        }

        return [];
    }
}
