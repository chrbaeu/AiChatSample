using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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

    [RelayCommand]
    private async Task SendMessage()
    {
        float? temperatureValue = null;
        if (!string.IsNullOrEmpty(Temprature) && float.TryParse(Temprature, out float temp))
        {
            temperatureValue = temp;
        }
        await chatService.SendMessageAsync(Message, UseTools, temperatureValue);
        Message = "";
    }
}
