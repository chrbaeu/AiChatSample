using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OllamaDemo.Shared.Common;
using OllamaDemo.StructuredData.Common;
using System.Collections.ObjectModel;
using System.Data;

namespace OllamaDemo.StructuredData.ViewModels;

public sealed partial class StructuredDataViewModel(
    IDialogService dialogService,
    AppSettings appSettings) : ObservableObject
{
    private TableData tableData = new();

    [ObservableProperty]
    public partial string SystemPrompt { get; set; } = "Du bist ein hilfsbereiter Assistent und lieferst strukturierte Daten im JSON-Format.";

    [ObservableProperty]
    public partial string UserPrompt { get; set; } = "Erstelle eine Tabelle mit den Spalten A, B und C für XYZ.";

    [ObservableProperty]
    public partial ObservableCollection<string> AvailableModels { get; set; } = new(appSettings.ChatModels);

    [ObservableProperty]
    public partial string SelectedModel { get; set; } = appSettings.ChatModels.FirstOrDefault() ?? string.Empty;

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    [ObservableProperty]
    public partial DataView DataView { get; private set; } = new();

    [RelayCommand]
    private async Task Run()
    {
        if (string.IsNullOrWhiteSpace(UserPrompt) || string.IsNullOrWhiteSpace(SelectedModel)) { return; }

        IsRunning = true;
        try
        {
            using var structuredDataService = new StructuredDataService(SelectedModel, appSettings.OllamaEndpointUri);
            tableData = await structuredDataService.RunAsync(SystemPrompt, UserPrompt);
            DataView = tableData.CreateDataTable().DefaultView;
        }
        catch (Exception ex)
        {
            dialogService.ShowMessageBox(null, "Fehler", $"Fehler beim Erzeugen strukturierter Daten: {ex.Message}");
        }
        finally
        {
            IsRunning = false;
        }
    }

    [RelayCommand]
    private void ExportToExcel()
    {
        var filePath = dialogService.ShowSaveFileDialog(null, "Excel Dateien (*.xlsx)|*.xlsx", null, "StrukturierteDaten.xlsx");
        if (string.IsNullOrEmpty(filePath)) { return; }

        try
        {
            tableData.SaveAsExcel(filePath, "Strukturierte Daten");
        }
        catch (Exception ex)
        {
            dialogService.ShowMessageBox(null, "Fehler", $"Fehler beim Export der Exceldatei: {ex.Message}");
        }
    }
}
