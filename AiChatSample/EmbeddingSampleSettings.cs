namespace AiChatSample;

public record class EmbeddingsSampleSettings
{
    public Uri OllamaEndpointUri { get; init; } = new Uri("http://localhost/");
    public string EmbeddingsModelId { get; init; } = "";
    public string EmbeddingsDataFilePath { get; init; } = "";
    public string UseEmbeddingsPrompt { get; init; } = "";
}