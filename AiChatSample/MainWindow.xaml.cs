using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AiChatSample;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel mainWindowViewModel, ChatService chatService, ThemeService themeService)
    {
        DataContext = mainWindowViewModel;
        ServiceCollection serviceCollection = new();
        serviceCollection.AddWpfBlazorWebView();
        serviceCollection.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        serviceCollection.AddSingleton<ChatService>(chatService);
        serviceCollection.AddSingleton<ThemeService>(themeService);
        Resources.Add("services", serviceCollection.BuildServiceProvider());
        InitializeComponent();
    }

}
