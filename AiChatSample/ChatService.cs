using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace AiChatSample;

public class ChatService(
        IMessenger messenger,
        IChatClient chatClient,
        ThemeService themeService,
        EmbeddingService embeddingService,
        IOptions<AiChatSampleSettings> settings,
        IOptions<EmbeddingsSampleSettings> embeddingsSettings
    )
{
    private readonly List<ChatMessage> conversation = string.IsNullOrEmpty(settings.Value.SystemPrompt) ? [] : [new ChatMessage(ChatRole.System, settings.Value.SystemPrompt)];

    public IEnumerable<ChatMessage> Messages => conversation.Where(m => (m.Role == ChatRole.User || m.Role == ChatRole.Assistant) && !string.IsNullOrEmpty(m.ToString()));

    public async Task SendMessageAsync(string message, bool useTools = false, bool useEmbeddings = false, float? temperature = null, string? imagePath = null)
    {
        if (useEmbeddings)
        {
            var data = await embeddingService.Search(message);
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine(embeddingsSettings.Value.UseEmbeddingsPrompt);
            foreach (var item in data)
            {
                stringBuilder.Append(item.Title);
                stringBuilder.Append(": ");
                stringBuilder.Append(item.Content);
                stringBuilder.AppendLine();
            }
            conversation.Add(new(ChatRole.User, stringBuilder.ToString()));
        }
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
