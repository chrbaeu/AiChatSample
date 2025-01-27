using DevExpress.AIIntegration;
using Microsoft.Extensions.AI;
using System;
using System.Globalization;
using System.Threading;

namespace WpfDxAI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    public App()
    {
        CultureInfo culture = CultureInfo.CreateSpecificCulture("de-DE");
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        IChatClient asChatClient = new OllamaChatClient(new Uri("http://localhost:11434/"), "llama3.1:8b");
        AIExtensionsContainerDesktop.Default.RegisterChatClient(asChatClient);
    }

}
