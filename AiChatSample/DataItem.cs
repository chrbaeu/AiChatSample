using Microsoft.Extensions.VectorData;

namespace AiChatSample;
public class DataItem
{
    [VectorStoreRecordKey]
    public string Key { get; set; } = "";

    [VectorStoreRecordData]
    public string Title { get; set; } = "";

    [VectorStoreRecordData]
    public string Content { get; set; } = "";

    [VectorStoreRecordVector(384, DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}