using Revrs;

namespace TotkTagEditor.Core;

public class TagBitTableEntry
{
    private readonly bool[] _flags;

    public TagBitTableEntry(ref RevrsReader reader, int entrySize, ref byte current, ref int bitOffset)
    {
        _flags = new bool[entrySize];

        for (int i = 0; i < entrySize; i++) {
            _flags[i] = ((current >> bitOffset) & 1) == 1;

            switch (bitOffset) { 
                case 7 when reader.Position < reader.Length:
                    bitOffset = 0;
                    current = reader.Read<byte>();
                    break;
                default: {
                    bitOffset++;
                    break;
                }
            }
        }
    }

    public bool HasTag(int tagIndex)
    {
        return _flags[tagIndex];
    }
}
