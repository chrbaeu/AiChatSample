using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using System.Windows;

namespace AiChatSample;

public partial class App : Application
{
    private readonly IHost appHost;

    public App()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        AiChatSampleSettings settings = builder.Configuration.GetRequiredSection(nameof(AiChatSampleSettings)).Get<AiChatSampleSettings>() ?? new();
        builder.Services.AddOptions<AiChatSampleSettings>().BindConfiguration(nameof(AiChatSampleSettings)).ValidateOnStart();

        // Chat
        builder.Services.AddChatClient(builder =>
        {
            AiChatSampleSettings settings = builder.GetRequiredService<IOptions<AiChatSampleSettings>>().Value;
            return new OllamaChatClient(settings.OllamaEndpointUri, modelId: settings.ChatModelId);
        }).UseFunctionInvocation();

        // Vision
        if (settings.VisionModelId is string { Length: > 0 })
        {
            builder.Services.AddKeyedChatClient("Vision", builder =>
            {
                AiChatSampleSettings settings = builder.GetRequiredService<IOptions<AiChatSampleSettings>>().Value;
                return new OllamaChatClient(settings.OllamaEndpointUri, modelId: settings.VisionModelId);
            }).UseFunctionInvocation();
        }

        // Embeddings
        if (settings.EmbeddingsModelId is string { Length: > 0 })
        {
            builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
            builder.Services.AddEmbeddingGenerator<string, Embedding<float>>(builder =>
            {
                AiChatSampleSettings settings = builder.GetRequiredService<IOptions<AiChatSampleSettings>>().Value;
                return new OllamaEmbeddingGenerator(settings.OllamaEndpointUri, modelId: settings.EmbeddingsModelId);
            });
            builder.Services.AddSingleton<EmbeddingService>();
            builder.Services.AddHostedService<EmbeddingService>(x => x.GetRequiredService<EmbeddingService>());
        }

        // Services
        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddScoped<ChatService>();

        // Views & ViewModels
        builder.Services.AddScoped<MainWindowViewModel>();
        builder.Services.AddScoped<MainWindow>();

        appHost = builder.Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await appHost.StartAsync();
        Current.MainWindow = appHost.Services.GetRequiredService<MainWindow>();
        Current.MainWindow.Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await appHost.StopAsync();
        appHost.Dispose();
        base.OnExit(e);
    }
}
