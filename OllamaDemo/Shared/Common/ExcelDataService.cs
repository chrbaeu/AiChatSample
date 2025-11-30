using CommunityToolkit.Mvvm.Messaging;
using OfficeOpenXml;

namespace OllamaDemo.Shared.Common;

public class ExcelDataService(IMessenger messenger)
{
    public TableData ExcelData { get; private set; } = new([]);

    public void LoadExcel(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        using var excelPackage = new ExcelPackage(filePath);
        var ws = excelPackage.Workbook.Worksheets.First();
        ExcelData = ConvertWorksheetToExcelData(ws);
        messenger.Send($"{nameof(ExcelDataService)}.{nameof(ExcelData)}");
    }

    private static TableData ConvertWorksheetToExcelData(ExcelWorksheet ws)
    {
        var result = new List<OrderedDictionary<string, string>>();
        var headers = new List<string>();
        for (int col = 1; col <= ws.Dimension.End.Column; col++)
        {
            if (ws.Column(col).Hidden) { continue; }
            var title = ws.Cells[1, col].Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                title = $"Spalte {col}";
            }
            headers.Add(title);
        }
        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            if (ws.Row(row).Hidden) { continue; }
            var dict = new OrderedDictionary<string, string>();
            for (int col = 1; col <= ws.Dimension.End.Column; col++)
            {
                if (ws.Column(col).Hidden) { continue; }
                var key = headers[col - 1];
                var value = ws.Cells[row, col].Text.Trim();
                dict[key] = value;
            }
            result.Add(dict);
        }
        return new(result);
    }

}
