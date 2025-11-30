using Microsoft.Extensions.AI;
using OllamaDemo.Shared.Common;
using Radzen;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;

namespace OllamaDemo.LlmChat.Common;

public sealed class AiChatService(IChatClient chatClient, AiChatTools aiChatTools, RagService ragService) : IAIChatService
{
    private readonly ConcurrentDictionary<string, ConversationSession> _sessions = new();

    public ConversationSession GetOrCreateSession(string? sessionId = null)
    {
        sessionId ??= Guid.NewGuid().ToString();
        return _sessions.GetOrAdd(sessionId, id => new ConversationSession
        {
            Id = id,
            CreatedAt = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow,
            MaxMessages = 100,
            Messages = []
        });
    }

    public IEnumerable<ConversationSession> GetActiveSessions() => _sessions.Values;

    public void ClearSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var s))
        {
            s.Clear();
            s.LastUpdated = DateTime.UtcNow;
        }
    }

    public void CleanupOldSessions(int maxAgeHours = 24)
    {
        var cutoff = DateTime.UtcNow.AddHours(-maxAgeHours);
        foreach (var kv in _sessions)
        {
            if (kv.Value.LastUpdated < cutoff)
            {
                _sessions.TryRemove(kv.Key, out _);
            }
        }
    }

    public async IAsyncEnumerable<string> GetCompletionsAsync(
        string userInput,
        string? sessionId = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default,
        string? model = null,
        string? systemPrompt = null,
        double? temperature = null,
        int? maxTokens = null,
        string? endpoint = null,
        string? proxy = null,
        string? apiKey = null,
        string? apiKeyHeader = null)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            yield break;

        await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

        var session = GetOrCreateSession(sessionId);

        if (apiKeyHeader == "UseEmbeddings")
        {
            StringBuilder esb = new("The following text is additional context retrieved from a knowledge base. " +
                    "Use it to answer the question when it is relevant.");
            esb.AppendLine();
            await foreach (var embedding in ragService.SearchAsync(userInput, 3))
            {
                esb.AppendLine(embedding.Key);
                esb.AppendLine(embedding.Text);
                esb.AppendLine();
            }
            session.AddMessage("system", esb.ToString());
        }

        session.AddMessage("user", userInput);
        session.LastUpdated = DateTime.UtcNow;

        var history = new List<Microsoft.Extensions.AI.ChatMessage>();
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            history.Add(new(ChatRole.System, systemPrompt));
        }

        foreach (var m in session.Messages)
        {
            var role = m.IsUser ? ChatRole.User : session.Messages.IndexOf(m) == 0 ? ChatRole.System : ChatRole.Assistant;
            history.Add(new(role, m.Content ?? string.Empty));
        }

        var options = new ChatOptions
        {
            ModelId = string.IsNullOrWhiteSpace(model) ? null : model,
            Temperature = temperature is null ? null : (float?)temperature,
            MaxOutputTokens = maxTokens,
            AllowMultipleToolCalls = true,
            ToolMode = ChatToolMode.Auto,
            Tools = [
                AIFunctionFactory.Create(aiChatTools.GetCurrentDate),
                AIFunctionFactory.Create(aiChatTools.SetDarkMode),
                AIFunctionFactory.Create(aiChatTools.IsDarkMode),
            ]
        };

        StringBuilder sb = new();
        await foreach (var update in chatClient.GetStreamingResponseAsync(history, options, cancellationToken).ConfigureAwait(false))
        {
            if (!string.IsNullOrEmpty(update.Text))
            {
                sb.Append(update.Text);
                yield return update.Text;
            }
        }

        session.AddMessage("assistant", sb.ToString());
        session.LastUpdated = DateTime.UtcNow;

    }
}
