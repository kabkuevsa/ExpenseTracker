using ExpenseTracker.Models;

namespace ExpenseTracker.Services;

/// <summary>Формирование отчёта о расходах по категориям.</summary>
public interface IReportService
{
    /// <summary>Строит отчёт за период: сумма и доля по каждой категории, имеющей записи.</summary>
    /// <param name="start">Начальная дата (включительно).</param>
    /// <param name="end">Конечная дата (включительно).</param>
    /// <returns>Отчёт со строками по убыванию суммы и общей суммой за период.</returns>
    /// <exception cref="ArgumentException">Если начальная дата позже конечной.</exception>
    Report Build(DateOnly start, DateOnly end);
}
