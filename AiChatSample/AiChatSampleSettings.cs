namespace AiChatSample;

public record class AiChatSampleSettings
{
    public Uri OllamaEndpointUri { get; init; } = new Uri("http://localhost/");
    public string AiModelId { get; init; } = "";
    public string SystemPrompt { get; init; } = "";
}