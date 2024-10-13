namespace AiChatSample;

public record class AiChatSampleSettings(
    Uri OllamaEndpointUri,
    string AiModelId,
    string SystemPrompt = ""
    )
{ }