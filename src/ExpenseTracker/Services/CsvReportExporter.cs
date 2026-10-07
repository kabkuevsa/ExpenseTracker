using System.Globalization;
using System.Text;
using ExpenseTracker.Models;

namespace ExpenseTracker.Services;

/// <summary>
/// Экспорт отчёта в CSV: кодировка UTF-8, разделитель «;», заголовок «Категория;Сумма;Доля, %»,
/// имя файла <c>report_ГГГГ-ММ-ДД.csv</c> (дата — текущая).
/// </summary>
public sealed class CsvReportExporter : IReportExporter
{
    private readonly string _directory;
    private readonly TimeProvider _time;

    /// <summary>Создаёт экспортёр.</summary>
    /// <param name="directory">Каталог для файла; по умолчанию текущий каталог.</param>
    /// <param name="timeProvider">Источник текущей даты; по умолчанию системные часы.</param>
    public CsvReportExporter(string? directory = null, TimeProvider? timeProvider = null)
    {
        _directory = directory ?? Directory.GetCurrentDirectory();
        _time = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    /// <exception cref="IOException">Если файл не удалось записать.</exception>
    public string Export(Report report)
    {
        var today = DateOnly.FromDateTime(_time.GetLocalNow().DateTime);
        var path = Path.GetFullPath(Path.Combine(_directory, $"report_{Formatting.Date(today)}.csv"));

        var sb = new StringBuilder();
        sb.AppendLine("Категория;Сумма;Доля, %");
        foreach (var row in report.Rows)
            sb.AppendLine(string.Join(';', Escape(row.Category), Formatting.Money(row.Amount), Formatting.Percent(row.SharePercent)));
        sb.AppendLine(string.Join(';', "Итого", Formatting.Money(report.Total), report.Total > 0 ? "100.0" : "0.0"));

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        return path;
    }

    private static string Escape(string value) =>
        value.IndexOfAny([';', '"', '\n', '\r']) >= 0
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
}
