using ExpenseTracker.Models;

namespace ExpenseTracker.Services;

/// <summary>Экспорт отчёта во внешний файл.</summary>
public interface IReportExporter
{
    /// <summary>Сохраняет отчёт в файл.</summary>
    /// <param name="report">Отчёт для экспорта.</param>
    /// <returns>Полный путь созданного файла.</returns>
    string Export(Report report);
}
