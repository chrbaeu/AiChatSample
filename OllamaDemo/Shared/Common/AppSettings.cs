namespace OllamaDemo.Shared.Common;

public class AppSettings
{
    public Uri OllamaEndpointUri { get; init; } = new Uri("http://localhost:11434/");
    public List<string> ChatModels { get; init; } = [];
    public List<string> EmbeddingModels { get; init; } = [];

}
