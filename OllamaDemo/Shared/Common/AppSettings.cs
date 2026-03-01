namespace OllamaDemo.Shared.Common;

public sealed class AppSettings
{
    public Uri OllamaEndpointUri { get; init; } = new Uri("http://localhost:11434/");
    public IList<string> ChatModels { get; init; } = [];
    public IList<string> EmbeddingModels { get; init; } = [];

}
