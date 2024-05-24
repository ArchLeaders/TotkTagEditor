using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FluentAvalonia.UI.Windowing;

namespace TotkTagEditor.Views;
public partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Bitmap bitmap = new(AssetLoader.Open(new Uri("avares://TotkTagEditor/Assets/icon.ico")));
        Icon = bitmap.CreateScaledBitmap(new(48, 48), BitmapInterpolationMode.HighQuality);
    }
}
