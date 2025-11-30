using Microsoft.Extensions.VectorData;

namespace OllamaDemo.SemanticSearch.Common;

public class DataItem
{
    [VectorStoreKey]
    public string Key { get; set; } = "";

    [VectorStoreData]
    public string Text { get; set; } = "";

    [VectorStoreVector(768, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> EmbeddingVector { get; set; }
}
