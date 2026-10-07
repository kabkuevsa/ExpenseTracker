namespace ExpenseTracker.Models;

/// <summary>Строка отчёта: итог по одной категории.</summary>
/// <param name="Category">Название категории.</param>
/// <param name="Amount">Сумма расходов по категории за период.</param>
/// <param name="SharePercent">Доля категории в общей сумме, %, с точностью до 0,1.</param>
public sealed record ReportRow(string Category, decimal Amount, decimal SharePercent);

/// <summary>Отчёт о расходах по категориям за период.</summary>
/// <param name="Start">Начало периода (включительно).</param>
/// <param name="End">Конец периода (включительно).</param>
/// <param name="Rows">Строки отчёта, упорядоченные по сумме по убыванию; только категории с записями.</param>
/// <param name="Total">Общая сумма расходов за период.</param>
public sealed record Report(DateOnly Start, DateOnly End, IReadOnlyList<ReportRow> Rows, decimal Total);
