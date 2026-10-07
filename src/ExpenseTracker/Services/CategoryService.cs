using ExpenseTracker.Models;
using ExpenseTracker.Storage;

namespace ExpenseTracker.Services;

/// <summary>Реализация <see cref="ICategoryService"/> поверх общего состояния <see cref="AppData"/>.</summary>
public sealed class CategoryService : ICategoryService
{
    private readonly IDataStore _store;
    private readonly AppData _data;

    /// <summary>Создаёт сервис справочника категорий.</summary>
    /// <param name="store">Хранилище, в которое сохраняются изменения.</param>
    /// <param name="data">Загруженное состояние программы (общее с другими сервисами).</param>
    public CategoryService(IDataStore store, AppData data)
    {
        _store = store;
        _data = data;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAll() => _data.Categories.AsReadOnly();

    /// <inheritdoc />
    public string? Find(string? name) =>
        _data.Categories.FirstOrDefault(c => string.Equals(c, name?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public string Add(string name)
    {
        var error = ExpenseValidator.CheckCategoryName(name);
        if (error is not null)
            throw new ArgumentException(error);

        var trimmed = name.Trim();
        if (Find(trimmed) is not null)
            throw new InvalidOperationException($"Категория «{trimmed}» уже существует.");
        if (_data.Categories.Count >= ExpenseValidator.MaxCategories)
            throw new InvalidOperationException($"Достигнут лимит категорий ({ExpenseValidator.MaxCategories}).");

        _data.Categories.Add(trimmed);
        _store.Save(_data);
        return trimmed;
    }

    /// <inheritdoc />
    public void Remove(string name)
    {
        var canonical = Find(name) ?? throw new KeyNotFoundException($"Категория «{name}» не найдена.");
        var linked = _data.Expenses.Count(e => string.Equals(e.Category, canonical, StringComparison.OrdinalIgnoreCase));
        if (linked > 0)
            throw new InvalidOperationException(
                $"Нельзя удалить категорию «{canonical}»: с ней связано записей — {linked}.");

        _data.Categories.Remove(canonical);
        _store.Save(_data);
    }
}
