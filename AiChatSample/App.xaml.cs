using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace AiChatSample;

public partial class App : Application
{
    private readonly IHost appHost;

    public App()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        builder.Services.AddAiChatSampleSettingsAsSingleton();

        builder.Services.AddChatClient(builder =>
        {
            AiChatSampleSettings settings = builder.Services.GetRequiredService<AiChatSampleSettings>();
            return builder
                .UseFunctionInvocation()
                .Use(new OllamaChatClient(settings.OllamaEndpointUri, modelId: settings.AiModelId));
        });

        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddScoped<ChatService>();

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
