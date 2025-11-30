using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using System.Windows;

namespace OllamaDemo;

public static class Program
{
    public static IHost? AppHost { get; private set; }

    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder();
            ExcelPackage.License.SetNonCommercialPersonal("Christian Baeumlisberger");
            var app = new App(builder);
            AppHost = builder.Build();
            app.Exit += (object sender, ExitEventArgs e) => AppHost?.StopAsync();
            AppHost.Start();
            app.InitializeComponent();
            return app.Run(AppHost);
        }
        finally
        {
            AppHost?.Dispose();
        }
    }
}
