using FluentAvalonia.UI.Controls;
using TotkTagEditor.Extensions;
using TotkTagEditor.Models;

namespace TotkTagEditor.ViewModels;

public partial class TagDatabaseViewModel : Document
{
    public TagDatabaseViewModel(string path) : base(path.GetRomfsParentFolderName(), path, Symbol.Tag)
    {
        
    }
}
