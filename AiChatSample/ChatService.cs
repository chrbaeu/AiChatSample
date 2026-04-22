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
    private readonly string initialSystemPrompt = settings.Value.ChatModelSystemPrompt;
    private readonly List<ChatMessage> conversation = string.IsNullOrEmpty(settings.Value.ChatModelSystemPrompt) ? [] : [new ChatMessage(ChatRole.System, settings.Value.ChatModelSystemPrompt)];

    public IEnumerable<ChatMessage> Messages => conversation.Where(m => (m.Role == ChatRole.User || m.Role == ChatRole.Assistant) && !string.IsNullOrEmpty(m.ToString()));

    public async Task SendMessageAsync(string message, bool useTools = false, bool useEmbeddings = false, float? temperature = null, string? imagePath = null)
    {
        //await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);
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
        var chosenChatClient = chatClient;
        ChatOptions chatOptions = new();
        if (temperature.HasValue)
        {
            chatOptions.Temperature = temperature.Value;
        }
        conversation.Add(new(ChatRole.User, message));
        messenger.Send(conversation);
        if (imagePath != null)
        {
            conversation[^1].Contents.Add(new DataContent(ImageProcessor.ConvertBitmapSourceToJpegByteArray(ImageProcessor.GetDownscaledImage(imagePath, 512)), "image/jpeg"));
        }
        if (conversation.Any(x => x.Role == ChatRole.User && x.Contents.Any(x => x is DataContent { MediaType: "image/jpeg" })))
        {
            chosenChatClient = serviceProvider.GetRequiredKeyedService<IChatClient>("Vision");
        }
        if (useTools)
        {
            chatOptions.Tools = [
                AIFunctionFactory.Create(GetTime),
                AIFunctionFactory.Create(themeService.SetDarkMode),
                AIFunctionFactory.Create(themeService.IsDarkMode)
            ];
        }
        conversation.Add(new(ChatRole.Assistant, ""));
        messenger.Send(conversation);
        await foreach (var response in chosenChatClient.GetStreamingResponseAsync(conversation, chatOptions))
        {
            conversation[^1] = new(ChatRole.Assistant, conversation[^1].Text + response.Text);
            messenger.Send(conversation.ToList());
        }
    }

    public void Clear()
    {
        conversation.Clear();
        if (!string.IsNullOrEmpty(initialSystemPrompt))
        {
            conversation.Add(new(ChatRole.System, initialSystemPrompt));
        }
        messenger.Send(conversation);
    }

    [Description("Gets the current date and time")]
    private string GetTime()
    {
        return DateTime.Now.ToString(CultureInfo.InvariantCulture);
    }

}
