using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OfficeOpenXml;
using OllamaDemo.SemanticSearch.Common;
using OllamaDemo.Shared.Common;
using System.Collections.ObjectModel;

namespace OllamaDemo.SemanticSearch.ViewModels;

public partial class SemanticSearchViewModel(
    IDialogService dialogService,
    ExcelDataService excelDataService,
    AppSettings appSettings
    ) : ObservableObject, IDisposable
{
    private SemanticSearchService? searchService;

    public ObservableCollection<ResultViewModel> Results { get; } = [];

    [ObservableProperty]
    public partial ObservableCollection<string> AvailableModels { get; set; } = new(appSettings.EmbeddingModels);

    [ObservableProperty]
    public partial string SelectedModel { get; set; } = appSettings.EmbeddingModels.First();

    [ObservableProperty]
    public partial string KeyColumns { get; set; } = "{1}";

    [ObservableProperty]
    public partial string TextColumns { get; set; } = "{2}";

    [ObservableProperty]
    public partial string Query { get; set; } = "";

    [ObservableProperty]
    public partial int Top { get; set; } = 10;

    [ObservableProperty]
    public partial double Threshold { get; set; } = -1;

    [ObservableProperty]
    public partial bool IsIndexed { get; set; }

    [ObservableProperty]
    public partial float Progress { get; set; }

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    public void Dispose()
    {
        searchService?.Dispose();
    }

    [RelayCommand]
    private async Task IndexAsync()
    {
        Progress = 0;
        IsRunning = true;
        try
        {
            searchService?.Dispose();
            searchService = new SemanticSearchService(SelectedModel, appSettings.OllamaEndpointUri);
            await Task.Run(async () =>
            {
                var items = excelDataService.ExcelData.GetRows()
                .Select(x => x.Select((value, index) => (value, index)).ToDictionary(x => (x.index + 1).ToString(), x => x.value.Value))
                .Select(x => new DataItem
                {
                    Key = KeyColumns.ReplacePlaceholders(x),
                    Text = TextColumns.ReplacePlaceholders(x),
                }).ToList();
                await searchService.LoadDataAsync(items, x =>
                {
                    Progress = (float)x / items.Count * 100;
                });
            });
            IsIndexed = true;
        }
        finally
        {
            Progress = 100;
            IsRunning = false;
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(Query) || !IsIndexed || searchService is null) { return; }
        Progress = 0;
        IsRunning = true;
        try
        {
            Results.Clear();
            await foreach (var result in searchService.SearchAsync(Query, Threshold, Top))
            {
                Results.Add(new ResultViewModel
                {
                    Item = result.Record,
                    Score = result.Score ?? 0
                });
            }
        }
        finally
        {
            Progress = 100;
            IsRunning = false;
        }
    }

    [RelayCommand]
    private async Task ExportToExcelAsync()
    {
        var filePath = dialogService.ShowSaveFileDialog(null, "Excel Dateien (*.xlsx)|*.xlsx", null, "Suchergebnis.xlsx");
        if (string.IsNullOrEmpty(filePath)) { return; }
        try
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Suchergebnis");
            worksheet.Cells[1, 1].Value = "Score";
            worksheet.Cells[1, 2].Value = "Id";
            worksheet.Cells[1, 3].Value = "Text";
            int row = 2;
            foreach (var result in Results)
            {
                worksheet.Cells[row, 1].Value = result.Score;
                worksheet.Cells[row, 2].Value = result.Item.Key;
                worksheet.Cells[row, 3].Value = result.Item.Text;
                row++;
            }
            await package.SaveAsAsync(filePath);
        }
        catch (Exception ex)
        {
            dialogService.ShowMessageBox(null, "Fehler", $"Fehler beim Export der Exceldatei: {ex.Message}");
        }
    }


    public partial class ResultViewModel : ObservableObject
    {
        [ObservableProperty]
        public required partial DataItem Item { get; set; }

        [ObservableProperty]
        public partial double Score { get; set; }
    }

}
