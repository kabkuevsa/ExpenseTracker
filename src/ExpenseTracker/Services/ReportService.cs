using ExpenseTracker.Models;

namespace ExpenseTracker.Services;

/// <summary>Реализация <see cref="IReportService"/>: группирует отобранные записи по категориям.</summary>
public sealed class ReportService : IReportService
{
    private readonly IExpenseService _expenses;

    /// <summary>Создаёт сервис отчётов.</summary>
    /// <param name="expenses">Сервис, из которого берутся записи за период.</param>
    public ReportService(IExpenseService expenses) => _expenses = expenses;

    /// <inheritdoc />
    public Report Build(DateOnly start, DateOnly end)
    {
        var items = _expenses.Filter(start, end, null);
        var total = items.Sum(e => e.Amount);

        var rows = items
            .GroupBy(e => e.Category)
            .Select(g =>
            {
                var sum = g.Sum(e => e.Amount);
                var share = total == 0 ? 0m : Math.Round(sum / total * 100m, 1, MidpointRounding.AwayFromZero);
                return new ReportRow(g.Key, sum, share);
            })
            .OrderByDescending(r => r.Amount)
            .ThenBy(r => r.Category, StringComparer.Ordinal)
            .ToList();

        return new Report(start, end, rows, total);
    }
}
