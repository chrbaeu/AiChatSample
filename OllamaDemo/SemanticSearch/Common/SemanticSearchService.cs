using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OllamaSharp;

namespace OllamaDemo.SemanticSearch.Common;

public sealed class SemanticSearchService(string embeddingModelId, Uri ollamaUrl) : IDisposable
{
    private readonly EmbeddingGenerationOptions options = new() { Dimensions = 768 };
    private readonly VectorStore vectorStore = new InMemoryVectorStore();
    private readonly IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = new OllamaApiClient(ollamaUrl, embeddingModelId);

    public async Task LoadDataAsync(IEnumerable<DataItem> items, Action<int>? progress = null)
    {
        var collection = vectorStore.GetCollection<string, DataItem>(nameof(DataItem));
        await collection.EnsureCollectionExistsAsync();
        int count = 0;
        foreach (var item in items)
        {
            var embedding = await embeddingGenerator.GenerateAsync(item.Text, options);
            item.EmbeddingVector = embedding.Vector.ToArray();
            await collection.UpsertAsync(item);
            progress?.Invoke(++count);
        }
    }

    public async IAsyncEnumerable<VectorSearchResult<DataItem>> SearchAsync(string query, double? threshold, int top = -1)
    {
        var collection = vectorStore.GetCollection<string, DataItem>(nameof(DataItem));
        var embedding = await embeddingGenerator.GenerateAsync(query, options);
        await foreach (VectorSearchResult<DataItem> result in collection.SearchAsync(embedding.Vector, top))
        {
            if (threshold.HasValue && result.Score < threshold) { yield break; }
            yield return result;
        }
    }
    public void Dispose()
    {
        vectorStore.Dispose();
        embeddingGenerator.Dispose();
    }

}
