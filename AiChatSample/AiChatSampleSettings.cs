namespace AiChatSample;

public record class AiChatSampleSettings
{
    public Uri OllamaEndpointUri { get; init; } = new Uri("http://localhost/");

    public string ChatModelId { get; init; } = "";
    public string ChatModelSystemPrompt { get; init; } = "";

    public string VisionModelId { get; init; } = "";
    public string VisionModelSystemPrompt { get; init; } = "";

    public string EmbeddingsModelId { get; init; } = "";
    public string EmbeddingsDataFilePath { get; init; } = "";
    public string UseEmbeddingsPrompt { get; init; } = "";
    public ushort MaxEmbeddings { get; init; } = 3;
}