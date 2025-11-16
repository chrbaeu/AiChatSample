using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using System.IO;
using System.Text.Json;

namespace AiChatSample;

public class EmbeddingService(
    IOptions<AiChatSampleSettings> settings,
    VectorStore vectorStore,
    IEmbeddingGenerator<string, Embedding<float>> generator
    ) : IHostedService
{
    public async Task<List<DataItem>> Search(string query)
    {
        var embedding = await generator.GenerateAsync(query, new EmbeddingGenerationOptions() { Dimensions = 512 });
        var data = vectorStore.GetCollection<string, DataItem>("data");
        List<DataItem> dataItems = [];
        await foreach (VectorSearchResult<DataItem> result in data.SearchAsync(embedding.Vector, 5))
        {
            if (result.Score > 0.05)
            {
                dataItems.Add(result.Record);
            }
        }
        return dataItems;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.Run(InitVectorStore, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task InitVectorStore()
    {
        List<DataItem> dataItems = JsonSerializer.Deserialize<List<DataItem>>(File.ReadAllText(settings.Value.EmbeddingsDataFilePath)) ?? [];
        var data = vectorStore.GetCollection<string, DataItem>("data");
        await data.EnsureCollectionExistsAsync();
        foreach (DataItem item in dataItems)
        {
            item.Vector = await generator.GenerateVectorAsync(item.Title + "\n" + item.Content, new EmbeddingGenerationOptions() { Dimensions = 512 });
            await data.UpsertAsync(item);
        }
    }
}