using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace AiChatSample;

public partial class App : Application
{
    private readonly IHost host;

    public App()
    {
        host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddAiChatSampleSettingsAsSingleton();
                services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
                services.AddSingleton<ThemeService>();

                services.AddChatClient(builder =>
                {
                    AiChatSampleSettings settings = builder.Services.GetRequiredService<AiChatSampleSettings>();
                    return builder
                        .UseFunctionInvocation()
                        .Use(new OllamaChatClient(settings.OllamaEndpointUri, modelId: settings.AiModelId));
                });

                services.AddScoped<ChatService>();
                services.AddScoped<MainWindowViewModel>();
                services.AddScoped<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await host.StartAsync();
        Current.MainWindow = host.Services.GetRequiredService<MainWindow>();
        Current.MainWindow.Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await host.StopAsync();
        host.Dispose();
        base.OnExit(e);
    }
}
