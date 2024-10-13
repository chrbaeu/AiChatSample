using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace AiChatSample;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost host;

    public App()
    {
        host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => ConfigureServices(services))
            .Build();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(static x => x.GetRequiredService<IConfiguration>().GetRequiredSection(nameof(AiChatSampleSettings))
            .Get<AiChatSampleSettings>() ?? throw new InvalidOperationException($"Configuration for '{nameof(AiChatSampleSettings)}' in 'appsettings.json' is not valid!"));

        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        services.AddChatClient(builder =>
        {
            AiChatSampleSettings settings = builder.Services.GetRequiredService<AiChatSampleSettings>();
            return builder
                .UseFunctionInvocation()
                .Use(new OllamaChatClient(settings.OllamaEndpointUri, modelId: settings.AiModelId));
        });

        services.AddScoped<ChatService>();

        services.AddScoped<MainWindow>();
        services.AddScoped<MainWindowViewModel>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await host.StartAsync();
        MainWindow mainWindow = host.Services.GetRequiredService<MainWindow>();
        Current.MainWindow = mainWindow;
        mainWindow.Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await host.StopAsync();
        host.Dispose();
        base.OnExit(e);
    }
}