namespace ExpenseTracker.Models;

/// <summary>
/// Полное состояние программы, которое целиком сохраняется в <c>expenses.json</c>:
/// счётчик идентификаторов, справочник категорий и список расходов.
/// </summary>
public sealed class AppData
{
    /// <summary>Категории, создаваемые при первом запуске программы.</summary>
    public static IReadOnlyList<string> DefaultCategories { get; } =
        ["Продукты", "Транспорт", "Жильё", "Здоровье", "Развлечения", "Прочее"];

    /// <summary>Идентификатор, который получит следующая добавленная запись.</summary>
    public required int NextId { get; set; }

    /// <summary>Справочник категорий расходов (до 100 уникальных без учёта регистра названий).</summary>
    public required List<string> Categories { get; init; }

    /// <summary>Все записи о расходах в порядке добавления.</summary>
    public required List<Expense> Expenses { get; init; }

    /// <summary>
    /// Создаёт данные «с нуля»: пустой список расходов и начальный справочник категорий.
    /// </summary>
    /// <returns>Новый экземпляр <see cref="AppData"/> с <see cref="NextId"/>, равным 1.</returns>
    public static AppData CreateDefault() => new()
    {
        NextId = 1,
        Categories = [.. DefaultCategories],
        Expenses = []
    };
}
