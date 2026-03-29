using Microsoft.Extensions.AI;
using OllamaDemo.Shared.Common;
using OllamaSharp;
using System.Text;
using System.Text.Json;

namespace OllamaDemo.StructuredData.Common;

internal sealed class StructuredDataService(string modelId, Uri ollamaUrl) : IDisposable
{
    private const string ResponseInstructions = """
        Du lieferst strukturierte Daten.
        Antworte ausschließlich mit gültigem JSON ohne Markdown-Codeblock.
        Erzeuge entweder ein JSON-Array von Objekten oder ein JSON-Objekt mit der Eigenschaft \"rows\", die ein Array von Objekten enthält.
        Nutze pro Objekt konsistente Spaltennamen.
        Werte dürfen Strings, Zahlen, Booleans oder null sein. Verschachtelte Objekte und Arrays bitte nur, wenn sie unvermeidbar sind.
        """;

    private readonly IChatClient client = new OllamaApiClient(ollamaUrl, modelId);

    public async Task<TableData> RunAsync(string systemPrompt, string userPrompt)
    {
        if (string.IsNullOrWhiteSpace(userPrompt)) { return new(); }

        var effectiveSystemPrompt = string.IsNullOrWhiteSpace(systemPrompt)
            ? ResponseInstructions
            : $"{systemPrompt}{Environment.NewLine}{Environment.NewLine}{ResponseInstructions}";

        List<ChatMessage> messages = [
            new ChatMessage(ChatRole.System, effectiveSystemPrompt),
            new ChatMessage(ChatRole.User, userPrompt)
        ];

        StringBuilder sb = new();
        await foreach (var data in client.GetStreamingResponseAsync(messages))
        {
            sb.Append(data.Text);
        }

        return ParseTableData(sb.ToString());
    }

    private static TableData ParseTableData(string response)
    {
        foreach (var candidate in GetJsonCandidates(response))
        {
            if (string.IsNullOrWhiteSpace(candidate)) { continue; }
            try
            {
                using var document = JsonDocument.Parse(candidate);
                return ConvertToTableData(document.RootElement);
            }
            catch (JsonException)
            {
            }
        }

        throw new FormatException("Die Antwort konnte nicht als JSON für eine Tabelle interpretiert werden.");
    }

    private static IEnumerable<string> GetJsonCandidates(string response)
    {
        var trimmed = response.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            yield break;
        }

        yield return trimmed;

        if (trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineEnd = trimmed.IndexOf('\n');
            if (firstLineEnd >= 0)
            {
                var fenced = trimmed[(firstLineEnd + 1)..];
                var closingFence = fenced.LastIndexOf("```", StringComparison.Ordinal);
                if (closingFence >= 0)
                {
                    yield return fenced[..closingFence].Trim();
                }
            }
        }

        foreach (var candidate in new[]
        {
            ExtractJsonCandidate(trimmed, '[', ']'),
            ExtractJsonCandidate(trimmed, '{', '}')
        })
        {
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                yield return candidate;
            }
        }
    }

    private static string ExtractJsonCandidate(string text, char startChar, char endChar)
    {
        var start = text.IndexOf(startChar);
        var end = text.LastIndexOf(endChar);
        if (start < 0 || end <= start)
        {
            return string.Empty;
        }

        return text[start..(end + 1)].Trim();
    }

    private static TableData ConvertToTableData(JsonElement root)
    {
        var rows = new List<OrderedDictionary<string, string>>();
        foreach (var element in EnumerateRows(root))
        {
            rows.Add(ConvertRow(element));
        }

        return new(rows);
    }

    private static IEnumerable<JsonElement> EnumerateRows(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
            {
                yield return item;
            }

            yield break;
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var propertyName in new[] { "rows", "data", "items", "result" })
            {
                if (root.TryGetProperty(propertyName, out var rowsElement) && rowsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in rowsElement.EnumerateArray())
                    {
                        yield return item;
                    }

                    yield break;
                }
            }
        }

        yield return root;
    }

    private static OrderedDictionary<string, string> ConvertRow(JsonElement row)
    {
        OrderedDictionary<string, string> result = new();
        if (row.ValueKind != JsonValueKind.Object)
        {
            result["Wert"] = ConvertValue(row);
            return result;
        }

        foreach (var property in row.EnumerateObject())
        {
            result[property.Name] = ConvertValue(property.Value);
        }

        return result;
    }

    private static string ConvertValue(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => bool.TrueString,
            JsonValueKind.False => bool.FalseString,
            JsonValueKind.Null => string.Empty,
            JsonValueKind.Undefined => string.Empty,
            _ => value.GetRawText()
        };
    }

    public void Dispose()
    {
        client.Dispose();
    }
}
