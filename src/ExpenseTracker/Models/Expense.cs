namespace ExpenseTracker.Models;

/// <summary>
/// Запись об одном расходе пользователя.
/// </summary>
/// <remarks>
/// Объект хранится в файле <c>expenses.json</c>. Идентификатор назначается
/// последовательно и после удаления записи повторно не используется.
/// </remarks>
public sealed class Expense
{
    /// <summary>Уникальный целочисленный идентификатор записи (начиная с 1). Не меняется при редактировании.</summary>
    public required int Id { get; init; }

    /// <summary>Дата расхода. Допустимый диапазон: от 2000-01-01 до текущей даты ПК.</summary>
    public required DateOnly Date { get; set; }

    /// <summary>Название категории из справочника (в написании, принятом в справочнике).</summary>
    public required string Category { get; set; }

    /// <summary>Сумма расхода: больше 0, не более 1 000 000 000,00, не более двух знаков после запятой.</summary>
    public required decimal Amount { get; set; }

    /// <summary>Необязательный комментарий длиной до 200 символов; <c>null</c>, если комментария нет.</summary>
    public string? Comment { get; set; }
}
