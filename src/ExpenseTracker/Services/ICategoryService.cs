namespace ExpenseTracker.Services;

/// <summary>Операции над справочником категорий расходов.</summary>
public interface ICategoryService
{
    /// <summary>Возвращает все категории справочника.</summary>
    /// <returns>Названия категорий в порядке добавления.</returns>
    IReadOnlyList<string> GetAll();

    /// <summary>Ищет категорию по названию без учёта регистра.</summary>
    /// <param name="name">Искомое название.</param>
    /// <returns>Название в написании справочника или <c>null</c>, если категории нет.</returns>
    string? Find(string? name);

    /// <summary>Добавляет категорию.</summary>
    /// <param name="name">Название (1–50 символов, уникальное без учёта регистра).</param>
    /// <returns>Добавленное название (без пробелов по краям).</returns>
    /// <exception cref="ArgumentException">Если название пустое или длиннее 50 символов.</exception>
    /// <exception cref="InvalidOperationException">Если такая категория уже есть или достигнут лимит в 100 категорий.</exception>
    string Add(string name);

    /// <summary>Удаляет категорию, если с ней не связано ни одной записи.</summary>
    /// <param name="name">Название категории.</param>
    /// <exception cref="KeyNotFoundException">Если категории нет в справочнике.</exception>
    /// <exception cref="InvalidOperationException">Если с категорией связаны записи о расходах.</exception>
    void Remove(string name);
}
