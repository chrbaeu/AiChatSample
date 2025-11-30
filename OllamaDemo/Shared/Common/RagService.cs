namespace OllamaDemo.Shared.Common;

public interface IRagService
{
    public IAsyncEnumerable<(double Score, string Key, string Text)> SearchAsync(string query, int top = -1);
}

public class RagService : IRagService
{
    public IRagService? RagServiceInstance { get; set; }

    public IAsyncEnumerable<(double Score, string Key, string Text)> SearchAsync(string query, int top = -1)
    {
        return RagServiceInstance?.SearchAsync(query, top) ?? AsyncEnumerable.Empty<(double, string, string)>();
    }
}
