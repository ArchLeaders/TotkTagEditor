using FluentAvalonia.UI.Controls;

namespace TotkTagEditor.Extensions;

public static class Dialogs
{
    public static async Task Success(string message, string title)
    {
        ContentDialog contentDialog = new() {
            Title = title,
            Content = message,
            DefaultButton = ContentDialogButton.Primary,
            PrimaryButtonText = "Most Splendid",
        };

        await contentDialog.ShowAsync();
    }
}
