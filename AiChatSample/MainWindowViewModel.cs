using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace AiChatSample;

public partial class MainWindowViewModel(
    ChatService chatService
    ) : ObservableObject
{
    [ObservableProperty]
    string message = "";

    [ObservableProperty]
    bool useTools = false;

    [ObservableProperty]
    string temprature = "";

    [ObservableProperty]
    string imagePath = "";


    [ObservableProperty]
    BitmapSource? image;

    [RelayCommand]
    private async Task SendMessage()
    {
        float? temperatureValue = null;
        if (!string.IsNullOrEmpty(Temprature) && float.TryParse(Temprature, out float temp))
        {
            temperatureValue = temp;
        }
        await chatService.SendMessageAsync(Message, UseTools, temperatureValue, !string.IsNullOrEmpty(ImagePath) ? ImagePath : null);
        Message = "";
        ImagePath = "";
        Image = null;
    }

    [RelayCommand]
    private void LoadImage()
    {
        OpenFileDialog openFileDialog = new()
        {
            Filter = "Image files (*.bmp;*.png;*.jpg)|*.bmp,*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
        };
        if (openFileDialog.ShowDialog() == true)
        {
            ImagePath = openFileDialog.FileName;
            Image = ImageProcessor.GetDownscaledImage(ImagePath, 128);
        }
    }
}
