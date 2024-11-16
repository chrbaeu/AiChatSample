using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Globalization;

namespace AiChatSample;

public class ChatService(
        IMessenger messenger,
        IChatClient chatClient,
        ThemeService themeService,
        AiChatSampleSettings settings
    )
{
    private readonly List<ChatMessage> conversation = string.IsNullOrEmpty(settings.SystemPrompt) ? [] : [new ChatMessage(ChatRole.System, settings.SystemPrompt)];

    public IEnumerable<ChatMessage> Messages => conversation.Where(m => (m.Role == ChatRole.User || m.Role == ChatRole.Assistant) && !string.IsNullOrEmpty(m.ToString()));

    public async Task SendMessageAsync(string message, bool useTools = false, float? temperature = null, string? imagePath = null)
    {
        conversation.Add(new(ChatRole.User, message));
        if (imagePath != null)
        {
            conversation.Add(new(ChatRole.User, [new ImageContent(ImageProcessor.ConvertBitmapSourceToByteArray(ImageProcessor.GetDownscaledImage(imagePath, 512)))]));
        }
        messenger.Send(conversation);
        ChatOptions chatOptions = new();
        if (useTools)
        {
            chatOptions.Tools = [
                AIFunctionFactory.Create(GetTime),
                AIFunctionFactory.Create(themeService.SetDarkMode),
                AIFunctionFactory.Create(themeService.IsDarkMode)
                ];
        }
        if (temperature.HasValue)
        {
            chatOptions.Temperature = temperature.Value;
        }
        if (useTools)
        {
            ChatCompletion response = await chatClient.CompleteAsync(conversation, chatOptions);
            conversation.Add(response.Message);
            messenger.Send(conversation);
        }
        else
        {
            ChatMessage responseMessage = new(ChatRole.Assistant, "");
            conversation.Add(responseMessage);
            messenger.Send(conversation);
            await foreach (StreamingChatCompletionUpdate response in chatClient.CompleteStreamingAsync(conversation, chatOptions))
            {
                responseMessage.Text += response.Text;
                messenger.Send(conversation);
            }
        }
    }

    public void Clear()
    {
        conversation.Clear();
        messenger.Send(conversation);
    }

    [Description("Gets the current date and time")]
    private string GetTime()
    {
        return DateTime.Now.ToString(CultureInfo.InvariantCulture);
    }

}
