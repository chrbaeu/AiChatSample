using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OllamaDemo.LlmTaskRunner.Common;
using OllamaDemo.Shared.Common;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;

namespace OllamaDemo.LlmTaskRunner.ViewModels;

public partial class LlmTaskRunnerViewModel(
    IDialogService dialogService,
    ExcelDataService excelDataService,
    AppSettings appSettings
    ) : ObservableObject
{
    private TableData tableData = new();

    [ObservableProperty]
    public partial string SystemPrompt { get; set; } = "Du bist ein hilfsbereiter Assistent.";

    [ObservableProperty]
    public partial string UserPrompt { get; set; } = "{1}";

    [ObservableProperty]
    public partial ObservableCollection<string> AvailableModels { get; set; } = new(appSettings.ChatModels);

    [ObservableProperty]
    public partial string SelectedModel { get; set; } = appSettings.ChatModels.First();

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    [ObservableProperty]
    public partial float Progress { get; set; }

    [ObservableProperty]
    public partial DataView DataView { get; private set; } = new();


    [RelayCommand]
    private async Task Run()
    {
        if (string.IsNullOrWhiteSpace(SystemPrompt) || string.IsNullOrWhiteSpace(UserPrompt)) { return; }
        Progress = 0;
        IsRunning = true;
        using var llmTaskService = new LlmTaskService(SelectedModel, appSettings.OllamaEndpointUri);
        tableData = excelDataService.ExcelData.Copy();
        tableData.AddColumn("Antwort des LLM");
        var dataTable = tableData.CreateDataTable();
        DataView = dataTable.DefaultView;
        try
        {
            await Task.Run(async () =>
            {
                int row = 0, resultColumn = tableData.Columns.Count - 1;
                foreach (var item in tableData.GetRows())
                {
                    var result = await llmTaskService.RunTaskAsync(SystemPrompt, UserPrompt, item);
                    tableData.Set("Antwort des LLM", row, result);
                    dataTable.Rows[row][resultColumn] = result;
                    row++;
                    Progress = (float)row / tableData.RowCount * 100;
                }
            });
        }
        finally
        {
            llmTaskService.Dispose();
            Progress = 100;
            IsRunning = false;
        }
    }

    [RelayCommand]
    private async Task ExportToExcel()
    {
        var filePath = dialogService.ShowSaveFileDialog(null, "Excel Dateien (*.xlsx)|*.xlsx", null, "Export.xlsx");
        if (!File.Exists(filePath)) { return; }
        try
        {
            tableData.SaveAsExcel(filePath);
        }
        catch (Exception ex)
        {
            dialogService.ShowMessageBox(null, "Fehler", $"Fehler beim Export der Exceldatei: {ex.Message}");
        }
    }

}
