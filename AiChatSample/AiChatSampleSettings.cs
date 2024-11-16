using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiChatSample;

public record class AiChatSampleSettings(
    Uri OllamaEndpointUri,
    string AiModelId,
    string SystemPrompt = ""
    )
{ }

public static class AiChatSampleSettingsExtensions
{
    public static void AddAiChatSampleSettingsAsSingleton(this IServiceCollection services)
    {
        services.AddSingleton(static sp => sp.GetRequiredService<IConfiguration>()
                .GetRequiredSection(nameof(AiChatSampleSettings))
                .Get<AiChatSampleSettings>() ?? throw new InvalidOperationException($"Invalid configuration for '{nameof(AiChatSampleSettings)}'")
            );
    }
}