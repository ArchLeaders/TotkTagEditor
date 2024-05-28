using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using VYaml.Parser;

namespace TotkTagEditor.Core.Models;

public partial class TagDatabaseEntry(string prefix, string name, string suffix) : ObservableObject
{
    [ObservableProperty]
    private string _prefix = prefix;

    [ObservableProperty]
    private string _name = name;

    [ObservableProperty]
    private string _suffix = suffix;

    [ObservableProperty]
    private ObservableCollection<string> _tags = [];

    public static TagDatabaseEntry FromYaml(ref YamlParser parser)
    {
        parser.SkipAfter(ParseEventType.MappingStart);

        string name = parser.ReadScalarAsString()
            ?? throw new InvalidDataException("""
                Tag entry path cannot be null.
                """);
        string[] parts = name.Split('|');

        if (parts.Length != 3) {
            throw new InvalidDataException($"""
                Invalid tag name, expected 3 pipe-delimited sections but found {parts.Length}: '{name}'
                """);
        }

        List<string> tags = [];
        parser.SkipAfter(ParseEventType.SequenceStart);
        while (parser.CurrentEventType != ParseEventType.SequenceEnd) {
            if (parser.TryReadScalarAsString(out string? tag) && tag is not null) {
                tags.Add(tag);
            }
        }

        parser.SkipAfter(ParseEventType.MappingEnd);
        return new(parts[0], parts[1], parts[2], tags);
    }

    public TagDatabaseEntry(string prefix, string name, string suffix, IEnumerable<string> tags) : this(prefix, name, suffix)
    {
        _tags = [.. tags];
    }
}
