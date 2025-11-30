using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OllamaDemo.Shared.Common;
using System.Data;
using System.IO;

namespace OllamaDemo.Shared.ViewModels;

public partial class ExcelDataViewModel(IDialogService dialogService, ExcelDataService excelDataService) : ObservableObject
{
    [ObservableProperty]
    public partial DataView ExcelDataView { get; private set; } = new();

    [RelayCommand]
    private void LoadExcel()
    {
        var filePath = dialogService.ShowOpenFileDialog(null, "Excel Dateien (*.xlsx)|*.xlsx");
        if (!File.Exists(filePath)) { return; }
        try
        {
            excelDataService.LoadExcel(filePath);
            ExcelDataView = excelDataService.ExcelData.CreateDataTable().DefaultView;
        }
        catch (Exception ex)
        {
            dialogService.ShowMessageBox(null, "Fehler", $"Fehler beim Einlesen der Exceldatei: {ex.Message}");
        }
    }

}
