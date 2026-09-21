using System.Globalization;
using System.Reflection;
using System.Text;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OneClickYatra.Api.Services;

/// <summary>
/// Builds CSV/XLSX/PDF exports purely by reflecting over T's public instance properties, so this
/// one implementation works for every report's row type without a per-report mapping.
/// </summary>
public sealed class ReportExportService : IReportExportService
{
    public byte[] ToCsv<T>(IEnumerable<T> __rows)
    {
        var properties = GetProperties<T>();
        var builder = new StringBuilder();

        builder.AppendLine(string.Join(",", properties.Select(p => EscapeCsvField(p.Name))));
        foreach (var row in __rows)
        {
            var values = properties.Select(p => EscapeCsvField(FormatValue(p.GetValue(row))));
            builder.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
    }

    public byte[] ToXlsx<T>(IEnumerable<T> __rows, string __sheetName)
    {
        var properties = GetProperties<T>();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(string.IsNullOrWhiteSpace(__sheetName) ? "Report" : __sheetName);

        for (var column = 0; column < properties.Count; column++)
        {
            var headerCell = worksheet.Cell(1, column + 1);
            headerCell.Value = properties[column].Name;
            headerCell.Style.Font.Bold = true;
        }

        var rowIndex = 2;
        foreach (var row in __rows)
        {
            for (var column = 0; column < properties.Count; column++)
            {
                SetCellValue(worksheet.Cell(rowIndex, column + 1), properties[column].GetValue(row));
            }
            rowIndex++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ToPdf<T>(string __title, IEnumerable<T> __rows)
    {
        var properties = GetProperties<T>();
        var rows = __rows.ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(style => style.FontSize(8));

                page.Header().Column(column =>
                {
                    column.Item().Text(__title).FontSize(16).Bold();
                    column.Item().Text($"Generated {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC — {rows.Count} row(s)")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var _ in properties)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    foreach (var property in properties)
                    {
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text(property.Name).Bold();
                    }

                    foreach (var row in rows)
                    {
                        foreach (var property in properties)
                        {
                            table.Cell().Padding(3).Text(FormatValue(property.GetValue(row)));
                        }
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static List<PropertyInfo> GetProperties<T>()
        => typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();

    private static void SetCellValue(IXLCell __cell, object? __value)
    {
        switch (__value)
        {
            case null:
                __cell.Value = string.Empty;
                break;
            case string s:
                __cell.Value = s;
                break;
            case bool b:
                __cell.Value = b;
                break;
            case DateTime dt:
                __cell.Value = dt;
                break;
            case DateOnly d:
                __cell.Value = d.ToDateTime(TimeOnly.MinValue);
                break;
            case decimal or double or float or int or long or short:
                __cell.Value = Convert.ToDouble(__value, CultureInfo.InvariantCulture);
                break;
            default:
                __cell.Value = __value.ToString() ?? string.Empty;
                break;
        }
    }

    private static string FormatValue(object? __value) => __value switch
    {
        null => string.Empty,
        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        decimal dec => dec.ToString("0.00", CultureInfo.InvariantCulture),
        double or float => Convert.ToDouble(__value, CultureInfo.InvariantCulture).ToString("0.00", CultureInfo.InvariantCulture),
        _ => __value.ToString() ?? string.Empty
    };

    private static string EscapeCsvField(string __field)
    {
        if (__field.IndexOfAny(['"', ',', '\n', '\r']) < 0)
        {
            return __field;
        }

        return $"\"{__field.Replace("\"", "\"\"")}\"";
    }
}
