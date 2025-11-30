using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaDemo.LlmChat.ViewModels;
using OllamaDemo.LlmTaskRunner.ViewModels;
using OllamaDemo.SemanticSearch.ViewModels;
using OllamaDemo.Shared.Common;
using OllamaDemo.Shared.ViewModels;
using OllamaDemo.Shared.Views;
using System.Windows;

namespace OllamaDemo;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App(IHostApplicationBuilder hostApplicationBuilder)
    {
        hostApplicationBuilder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        hostApplicationBuilder.Services.AddSingleton<IDialogService, DialogServiceWpf>();

        AppSettings appSettings = hostApplicationBuilder.Configuration
            .GetRequiredSection(nameof(AppSettings)).Get<AppSettings>() ?? new();
        hostApplicationBuilder.Services.AddSingleton(appSettings);

        hostApplicationBuilder.Services.AddSingleton<ExcelDataService>();

        hostApplicationBuilder.Services.AddSingleton<MainViewModel>();
        hostApplicationBuilder.Services.AddSingleton<ExcelDataViewModel>();
        hostApplicationBuilder.Services.AddSingleton<SemanticSearchViewModel>();
        hostApplicationBuilder.Services.AddSingleton<LlmTaskRunnerViewModel>();
        hostApplicationBuilder.Services.AddSingleton<LlmChatViewModel>();
    }

    public int Run(IHost appHost)
    {
        MainWindow = new MainWindow()
        {
            DataContext = appHost.Services.GetRequiredService<MainViewModel>()
        };
        MainWindow.Show();
        return Run();
    }
}
