namespace OneClickYatra.Api.Services;

/// <summary>Generic, reflection-based tabular export used by every report — one implementation
/// works for every report's row type since it only ever reflects over T's public properties.</summary>
public interface IReportExportService
{
    byte[] ToCsv<T>(IEnumerable<T> __rows);
    byte[] ToXlsx<T>(IEnumerable<T> __rows, string __sheetName);
    byte[] ToPdf<T>(string __title, IEnumerable<T> __rows);
}
