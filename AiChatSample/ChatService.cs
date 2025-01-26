using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace AiChatSample;

public class ChatService(
        IMessenger messenger,
        IChatClient chatClient,
        ThemeService themeService,
        IOptions<AiChatSampleSettings> settings,
        IServiceProvider serviceProvider
    )
{
    private readonly List<ChatMessage> conversation = string.IsNullOrEmpty(settings.Value.ChatModelSystemPrompt) ? [] : [new ChatMessage(ChatRole.System, settings.Value.ChatModelSystemPrompt)];

    public IEnumerable<ChatMessage> Messages => conversation.Where(m => (m.Role == ChatRole.User || m.Role == ChatRole.Assistant) && !string.IsNullOrEmpty(m.ToString()));

    public async Task SendMessageAsync(string message, bool useTools = false, bool useEmbeddings = false, float? temperature = null, string? imagePath = null)
    {
        if (useEmbeddings)
        {
            List<DataItem> data = await serviceProvider.GetRequiredService<EmbeddingService>().Search(message);
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine(settings.Value.UseEmbeddingsPrompt);
            foreach (DataItem item in data)
            {
                stringBuilder.Append(item.Title);
                stringBuilder.Append(": ");
                stringBuilder.Append(item.Content);
                stringBuilder.AppendLine();
            }
            conversation.Add(new(ChatRole.Tool, stringBuilder.ToString()));
        }
        ChatOptions chatOptions = new();
        if (temperature.HasValue)
        {
            chatOptions.Temperature = temperature.Value;
        }
        conversation.Add(new(ChatRole.User, message));
        messenger.Send(conversation);
        if (imagePath != null)
        {
            List<ChatMessage> visionConversation = string.IsNullOrEmpty(settings.Value.VisionModelSystemPrompt) ? [] : [new ChatMessage(ChatRole.System, settings.Value.VisionModelSystemPrompt)];
            visionConversation.Add(new(ChatRole.User, message));
            visionConversation.Add(new(ChatRole.User, [new ImageContent(ImageProcessor.ConvertBitmapSourceToByteArray(ImageProcessor.GetDownscaledImage(imagePath, 512)))]));
            IChatClient visionClient = serviceProvider.GetRequiredKeyedService<IChatClient>("Vision");
            ChatCompletion response = await visionClient.CompleteAsync(visionConversation, chatOptions);
            conversation.Add(response.Message);
            messenger.Send(conversation);
            return;
        }
        else if (useTools)
        {
            chatOptions.Tools = [
                AIFunctionFactory.Create(GetTime),
                AIFunctionFactory.Create(themeService.SetDarkMode),
                AIFunctionFactory.Create(themeService.IsDarkMode)
            ];
            ChatCompletion response = await chatClient.CompleteAsync(conversation, chatOptions);
            conversation.Add(response.Message);
            messenger.Send(conversation);
            return;
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
