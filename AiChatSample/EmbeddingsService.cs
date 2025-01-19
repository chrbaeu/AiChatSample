using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using System.IO;
using System.Text.Json;

namespace AiChatSample;

public class EmbeddingService(
    IOptions<EmbeddingsSampleSettings> settings,
    IVectorStore vectorStore,
    IEmbeddingGenerator<string, Embedding<float>> generator
    ) : IHostedService
{

    public async Task<List<DataItem>> Search(string query)
    {
        ReadOnlyMemory<float> queryEmbedding = await generator.GenerateEmbeddingVectorAsync(query);
        VectorSearchOptions searchOptions = new() { Top = 3, VectorPropertyName = "Vector" };
        IVectorStoreRecordCollection<Guid, DataItem> data = vectorStore.GetCollection<Guid, DataItem>("data");
        VectorSearchResults<DataItem> results = await data.VectorizedSearchAsync(queryEmbedding, searchOptions);
        List<DataItem> dataItems = [];
        await foreach (VectorSearchResult<DataItem> result in results.Results)
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
        IVectorStoreRecordCollection<Guid, DataItem> data = vectorStore.GetCollection<Guid, DataItem>("data");
        await data.CreateCollectionIfNotExistsAsync();
        foreach (DataItem item in dataItems)
        {
            item.Vector = await generator.GenerateEmbeddingVectorAsync(item.Title + "\n" + item.Content);
            await data.UpsertAsync(item);
        }
    }
}