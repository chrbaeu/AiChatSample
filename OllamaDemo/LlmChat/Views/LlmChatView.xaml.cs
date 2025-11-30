using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using OllamaDemo.LlmChat.Common;
using OllamaDemo.Shared.Common;
using OllamaSharp;
using Radzen;
using System.Windows.Controls;

namespace OllamaDemo.LlmChat.Views;

/// <summary>
/// Interaction logic for LlmChatView.xaml
/// </summary>
public partial class LlmChatView : UserControl
{
    public LlmChatView()
    {
        InitializeComponent();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddWpfBlazorWebView();
#if DEBUG
        serviceCollection.AddBlazorWebViewDeveloperTools();
#endif
        serviceCollection.AddRadzenComponents();
        serviceCollection.AddSingleton<AppSettings>(Program.AppHost!.Services.GetRequiredService<AppSettings>());
        serviceCollection.AddSingleton<RagService>(Program.AppHost!.Services.GetRequiredService<RagService>());
        serviceCollection.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        serviceCollection.AddSingleton<IChatClient>(sp =>
        {
            IChatClient client = new OllamaApiClient(new Uri("http://localhost:11434"));
            client = ChatClientBuilderChatClientExtensions.AsBuilder(client).UseFunctionInvocation().Build();
            return client;
        });
        serviceCollection.AddScoped<AiChatTools>();
        serviceCollection.AddScoped<IAIChatService, AiChatService>();
        Resources.Add("services", serviceCollection.BuildServiceProvider());
        Microsoft.AspNetCore.Components.WebView.Wpf.RootComponent rh = new()
        {
            ComponentType = typeof(HeadOutlet),
            Selector = "head::after"
        };
        blazorWebView.RootComponents.Add(rh);
    }

    private void OnBlazorWebViewInitializing(object? sender, BlazorWebViewInitializingEventArgs e)
    {
        e.EnvironmentOptions ??= new CoreWebView2EnvironmentOptions();
        e.EnvironmentOptions.Language = "de-DE";
    }
}
