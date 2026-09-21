using System.Text;
using ClosedXML.Excel;
using OneClickYatra.Api.Services;
using QuestPDF.Infrastructure;

namespace OneClickYatra.UnitTests.Services;

public class ReportExportServiceTests
{
    // QuestPDF requires this static, process-wide declaration before generating any document —
    // Program.cs sets it at app startup; tests that exercise real PDF generation must set it too.
    static ReportExportServiceTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private sealed class SampleRow
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    private static ReportExportService CreateSut() => new();

    private static List<string> ReadCsvLines(byte[] __csvBytes)
    {
        var text = Encoding.UTF8.GetString(__csvBytes).TrimStart('﻿').TrimEnd('\r', '\n');
        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.TrimEnd('\r')).ToList();
    }

    [Fact]
    public void ToCsv_ProducesOneDataRowPerItemPlusHeader()
    {
        var rows = new List<SampleRow>
        {
            new() { Name = "Goa Package", Amount = 12000m },
            new() { Name = "Kerala Package", Amount = 18500m }
        };

        var lines = ReadCsvLines(CreateSut().ToCsv(rows));

        Assert.Equal(3, lines.Count);
        Assert.Equal("Name,Amount", lines[0]);
        Assert.Equal("Goa Package,12000.00", lines[1]);
        Assert.Equal("Kerala Package,18500.00", lines[2]);
    }

    [Fact]
    public void ToCsv_EscapesACommaInAValue()
    {
        var rows = new List<SampleRow> { new() { Name = "Goa, Beach Package", Amount = 5000m } };

        var lines = ReadCsvLines(CreateSut().ToCsv(rows));

        Assert.Equal("\"Goa, Beach Package\",5000.00", lines[1]);
    }

    [Fact]
    public void ToCsv_EscapesAQuoteInAValue()
    {
        var rows = new List<SampleRow> { new() { Name = "The \"Best\" Package", Amount = 1000m } };

        var lines = ReadCsvLines(CreateSut().ToCsv(rows));

        Assert.Equal("\"The \"\"Best\"\" Package\",1000.00", lines[1]);
    }

    [Fact]
    public void ToCsv_NoRows_StillWritesHeaderOnly()
    {
        var lines = ReadCsvLines(CreateSut().ToCsv(new List<SampleRow>()));

        Assert.Single(lines);
        Assert.Equal("Name,Amount", lines[0]);
    }

    [Fact]
    public void ToXlsx_WritesHeaderRowAndOneRowPerItem()
    {
        var rows = new List<SampleRow>
        {
            new() { Name = "Goa Package", Amount = 12000m },
            new() { Name = "Kerala Package", Amount = 18500m }
        };

        var bytes = CreateSut().ToXlsx(rows, "Sales");

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Sales");

        Assert.Equal("Name", worksheet.Cell(1, 1).GetString());
        Assert.Equal("Amount", worksheet.Cell(1, 2).GetString());
        Assert.Equal("Goa Package", worksheet.Cell(2, 1).GetString());
        Assert.Equal(12000d, worksheet.Cell(2, 2).GetDouble());
        Assert.Equal("Kerala Package", worksheet.Cell(3, 1).GetString());
        Assert.Equal(18500d, worksheet.Cell(3, 2).GetDouble());
    }

    [Fact]
    public void ToPdf_ProducesNonEmptyPdfDocument()
    {
        var rows = new List<SampleRow> { new() { Name = "Goa Package", Amount = 12000m } };

        var bytes = CreateSut().ToPdf("Sales Report", rows);

        Assert.NotEmpty(bytes);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }
}
