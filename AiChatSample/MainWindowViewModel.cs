using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;

namespace AiChatSample;

public partial class MainWindowViewModel(
    ChatService chatService,
    IOptions<AiChatSampleSettings> settings
    ) : ObservableObject
{
    [ObservableProperty]
    public partial string Message { get; set; } = "";

    [ObservableProperty]
    public partial bool UseTools { get; set; }

    [ObservableProperty]
    public partial bool UseEmbeddings { get; set; }

    [ObservableProperty]
    public partial bool EnableUseEmbeddings { get; set; } = settings.Value.EmbeddingsModelId is string { Length: > 0 };

    [ObservableProperty]
    public partial string Temperature { get; set; } = "";

    [ObservableProperty]
    public partial string ImagePath { get; set; } = "";

    [ObservableProperty]
    public partial BitmapSource? Image { get; set; }

    [ObservableProperty]
    public partial bool EnableUseVision { get; set; } = settings.Value.VisionModelId is string { Length: > 0 };

    [RelayCommand]
    private async Task SendMessage()
    {
        float? temperatureValue = null;
        if (!string.IsNullOrEmpty(Temperature) && float.TryParse(Temperature, out float temp))
        {
            temperatureValue = temp;
        }
        await chatService.SendMessageAsync(Message, UseTools, UseEmbeddings, temperatureValue, !string.IsNullOrEmpty(ImagePath) ? ImagePath : null);
        Message = "";
        ImagePath = "";
        Image = null;
    }

    [RelayCommand]
    private void LoadImage()
    {
        var initialDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\..\SampleData"));
        OpenFileDialog openFileDialog = new()
        {
            InitialDirectory = initialDirectory,
            Filter = "Image files (*.bmp;*.png;*.jpg)|*.bmp;*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
        };
        if (openFileDialog.ShowDialog() == true)
        {
            ImagePath = openFileDialog.FileName;
            Image = ImageProcessor.GetDownscaledImage(ImagePath, 128);
        }
        else
        {
            ImagePath = "";
            Image = null;
        }
    }
}
