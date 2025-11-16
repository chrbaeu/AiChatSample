using Microsoft.Extensions.VectorData;

namespace AiChatSample;

public class DataItem
{
    [VectorStoreKey]
    public string Key { get; set; } = "";

    [VectorStoreData]
    public string Title { get; set; } = "";

    [VectorStoreData]
    public string Content { get; set; } = "";

    [VectorStoreVector(512, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}