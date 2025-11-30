using OfficeOpenXml;
using System.Data;

namespace OllamaDemo.Shared.Common;

public class TableData
{
    private readonly List<OrderedDictionary<string, string>> rows = [];
    private List<string> columns = [];

    public IReadOnlyList<string> Columns => columns;

    public TableData() : this([]) { }

    public TableData(List<OrderedDictionary<string, string>> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        this.rows = rows;
        this.columns = rows.SelectMany(d => d.Keys).Distinct().ToList();
    }

    public int RowCount => rows.Count;
    public int ColumnsCount => columns.Count;

    public IEnumerable<IReadOnlyDictionary<string, string>> GetRows()
    {
        return rows.Select(dict => (IReadOnlyDictionary<string, string>)dict);
    }

    public string Get(string columnName, int row)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, rows.Count);
        var dict = rows[row];
        return dict.TryGetValue(columnName, out var value) ? value : string.Empty;
    }

    public string Get(int colum, int row)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(colum);
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(colum, columns.Count());
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, rows.Count);
        var dict = rows[row];
        var key = columns[colum];
        return dict.TryGetValue(key, out var value) ? value : string.Empty;
    }

    public void Set(string columnName, int row, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, rows.Count);
        if (!columns.Contains(columnName))
        {
            throw new ArgumentException($"Column '{columnName}' does not exist.");
        }
        var dict = rows[row];
        dict[columnName] = value;
    }

    public void Set(int colum, int row, string value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(colum);
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(colum, columns.Count());
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, rows.Count);
        var dict = rows[row];
        var key = columns[colum];
        dict[key] = value;
    }

    public void AddColumn(string columnName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
        if (columns.Contains(columnName)) { return; }
        foreach (var dict in rows)
        {
            dict[columnName] = string.Empty;
        }
        columns.Add(columnName);
    }

    public DataTable CreateDataTable()
    {
        var table = new DataTable();
        if (rows == null || rows.Count == 0) { return table; }
        foreach (var columnName in columns)
        {
            table.Columns.Add(columnName);
        }
        foreach (var dict in rows)
        {
            var row = table.NewRow();
            foreach (var kvp in dict)
            {
                row[kvp.Key] = kvp.Value;
            }
            table.Rows.Add(row);
        }
        return table;
    }

    public TableData Copy()
    {
        var copy = new List<OrderedDictionary<string, string>>(rows.Count);
        foreach (var dict in rows)
        {
            var newDict = new OrderedDictionary<string, string>();
            foreach (var kvp in dict)
            {
                newDict[kvp.Key] = kvp.Value;
            }
            copy.Add(newDict);
        }
        return new(copy);
    }

    public void SaveAsExcel(string filePath, string wsName = "Daten")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(wsName);
        using ExcelPackage excelPackage = new();
        var ws = excelPackage.Workbook.Worksheets.Add(wsName);
        for (int col = 0; col < columns.Count; col++)
        {
            ws.Cells[1, col + 1].Value = columns[col];
        }
        for (int row = 0; row < rows.Count; row++)
        {
            var dict = rows[row];
            for (int col = 0; col < columns.Count; col++)
            {
                var key = columns[col];
                dict.TryGetValue(key, out var value);
                ws.Cells[row + 2, col + 1].Value = value;
            }
        }
        excelPackage.SaveAs(filePath);
    }
}