using ExpenseTracker.Models;
using ExpenseTracker.Storage;

namespace ExpenseTracker.Services;

/// <summary>
/// Реализация <see cref="IExpenseService"/>: хранит записи в памяти и после каждого
/// изменения сохраняет данные через <see cref="IDataStore"/>.
/// </summary>
public sealed class ExpenseService : IExpenseService
{
    private readonly IDataStore _store;
    private readonly AppData _data;
    private readonly TimeProvider _time;

    /// <summary>Создаёт сервис поверх уже загруженных данных.</summary>
    /// <param name="store">Хранилище, в которое сохраняются изменения.</param>
    /// <param name="data">Загруженное состояние программы (общее с другими сервисами).</param>
    /// <param name="timeProvider">Источник текущей даты; по умолчанию системные часы.</param>
    public ExpenseService(IDataStore store, AppData data, TimeProvider? timeProvider = null)
    {
        _store = store;
        _data = data;
        _time = timeProvider ?? TimeProvider.System;
    }

    private DateOnly Today => DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

    /// <inheritdoc />
    /// <example>
    /// <code>
    /// var expense = service.Add(350.50m, new DateOnly(2026, 10, 1), "Продукты", "Молоко и хлеб");
    /// Console.WriteLine(expense.Id); // 1
    /// </code>
    /// </example>
    public Expense Add(decimal amount, DateOnly date, string category, string? comment)
    {
        var (canonical, normalizedComment) = Validate(amount, date, category, comment);
        var expense = new Expense
        {
            Id = _data.NextId,
            Date = date,
            Category = canonical,
            Amount = amount,
            Comment = normalizedComment
        };
        _data.Expenses.Add(expense);
        _data.NextId++;
        _store.Save(_data);
        return expense;
    }

    /// <inheritdoc />
    public Expense? GetById(int id) => _data.Expenses.FirstOrDefault(e => e.Id == id);

    /// <inheritdoc />
    public void Update(int id, decimal amount, DateOnly date, string category, string? comment)
    {
        var expense = GetById(id) ?? throw new KeyNotFoundException($"Запись с ID {id} не найдена");
        var (canonical, normalizedComment) = Validate(amount, date, category, comment);
        expense.Amount = amount;
        expense.Date = date;
        expense.Category = canonical;
        expense.Comment = normalizedComment;
        _store.Save(_data);
    }

    /// <inheritdoc />
    public void Delete(int id)
    {
        var expense = GetById(id) ?? throw new KeyNotFoundException($"Запись с ID {id} не найдена");
        _data.Expenses.Remove(expense);
        _store.Save(_data);
    }

    /// <inheritdoc />
    public IReadOnlyList<Expense> GetAll() => Sort(_data.Expenses);

    /// <inheritdoc />
    public IReadOnlyList<Expense> Filter(DateOnly start, DateOnly end, string? category)
    {
        if (start > end)
            throw new ArgumentException("Начальная дата не может быть позже конечной.");

        var items = _data.Expenses.Where(e => e.Date >= start && e.Date <= end);
        if (category is not null)
        {
            var canonical = FindCategory(category)
                ?? throw new ArgumentException($"Категория «{category}» отсутствует в справочнике.");
            items = items.Where(e => string.Equals(e.Category, canonical, StringComparison.OrdinalIgnoreCase));
        }
        return Sort(items);
    }

    private string? FindCategory(string? name) =>
        _data.Categories.FirstOrDefault(c => string.Equals(c, name?.Trim(), StringComparison.OrdinalIgnoreCase));

    private (string Category, string? Comment) Validate(decimal amount, DateOnly date, string? category, string? comment)
    {
        var error = ExpenseValidator.CheckAmount(amount)
            ?? ExpenseValidator.CheckDate(date, Today)
            ?? ExpenseValidator.CheckComment(comment?.Trim());
        if (error is not null)
            throw new ArgumentException(error);

        var canonical = FindCategory(category)
            ?? throw new ArgumentException($"Категория «{category}» отсутствует в справочнике.");
        var normalizedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        return (canonical, normalizedComment);
    }

    private static List<Expense> Sort(IEnumerable<Expense> items) =>
        items.OrderByDescending(e => e.Date).ThenByDescending(e => e.Id).ToList();
}
