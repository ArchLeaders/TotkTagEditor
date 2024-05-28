namespace TotkTagEditor.Extensions;

public static class RomfsPathExtension
{
    public static string GetRomfsParentFolderName(this string path)
    {
        ReadOnlySpan<char> span = path;

        int index = span.LastIndexOf("romfs", StringComparison.OrdinalIgnoreCase);

        return index switch {
            > -1 => Path.GetFileName(span[..--index]).ToString(),
            _ => Path.GetFileName(Path.GetDirectoryName(span)).ToString()
        };
    }
}
