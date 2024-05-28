using Revrs;

namespace TotkTagEditor.Core;

public class TagBitTable : List<TagBitTableEntry>
{
    private readonly TagBitTableEntry[] _entries;

    public TagBitTable(ref RevrsReader reader, int entrySize, int pathCount)
    {
        _entries = new TagBitTableEntry[pathCount];

        int bitOffset = 0;
        byte current = reader.Read<byte>();
        for (int i = 0; i < pathCount; i++) {
            _entries[i] = new TagBitTableEntry(ref reader, entrySize, ref current, ref bitOffset);
        }
    }

    public IEnumerable<string> GetTags(int index, IEnumerable<string> tags)
    {
        TagBitTableEntry entry = _entries[index];
        return tags.Where((_, i) => entry.HasTag(i));
    }
}
