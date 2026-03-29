using Microsoft.Extensions.AI;
using OllamaDemo.Shared.Common;
using OllamaSharp;
using System.Threading;
using System.Globalization;
using System.Text;

namespace OllamaDemo.LlmTaskRunner.Common;

internal sealed partial class LlmTaskService(string modelId, Uri ollamaUrl) : IDisposable
{
    private readonly IChatClient client = new OllamaApiClient(ollamaUrl, modelId);

    public async Task<string> RunTaskAsync(string systemPromt, string prompt, IReadOnlyDictionary<string, string> item, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(systemPromt)) { return "Du bist ein hilfsbereiter Assistent."; }
        if (string.IsNullOrWhiteSpace(prompt)) { prompt = "{1}"; }
        var byIndexDict = item.Select((value, index) => (value, index)).ToDictionary(x => (x.index + 1).ToString(CultureInfo.InvariantCulture), x => x.value.Value);
        systemPromt = systemPromt.ReplacePlaceholders(byIndexDict);
        prompt = prompt.ReplacePlaceholders(byIndexDict);
        systemPromt = systemPromt.ReplacePlaceholders(item);
        prompt = prompt.ReplacePlaceholders(item);
        List<ChatMessage> messages = [
            new ChatMessage(ChatRole.System, systemPromt),
            new ChatMessage(ChatRole.User, prompt)
        ];
        StringBuilder sb = new();
        await foreach (var data in client.GetStreamingResponseAsync(messages, cancellationToken: cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.Append(data.Text);
        }
        return sb.ToString();
    }

    public void Dispose()
    {
        client.Dispose();
    }

}