using ExpenseTracker.Models;

namespace ExpenseTracker.Services;

/// <summary>Операции над записями о расходах.</summary>
public interface IExpenseService
{
    /// <summary>Добавляет новую запись.</summary>
    /// <param name="amount">Сумма расхода.</param>
    /// <param name="date">Дата расхода.</param>
    /// <param name="category">Название категории из справочника (регистр не важен).</param>
    /// <param name="comment">Комментарий или <c>null</c>.</param>
    /// <returns>Созданная запись с присвоенным ID.</returns>
    /// <exception cref="ArgumentException">Если значение нарушает ограничения раздела 4.1 ТЗ.</exception>
    Expense Add(decimal amount, DateOnly date, string category, string? comment);

    /// <summary>Ищет запись по ID.</summary>
    /// <param name="id">Идентификатор записи.</param>
    /// <returns>Найденная запись или <c>null</c>.</returns>
    Expense? GetById(int id);

    /// <summary>Заменяет значения полей записи; ID не меняется.</summary>
    /// <param name="id">Идентификатор изменяемой записи.</param>
    /// <param name="amount">Новая сумма.</param>
    /// <param name="date">Новая дата.</param>
    /// <param name="category">Новая категория.</param>
    /// <param name="comment">Новый комментарий или <c>null</c>.</param>
    /// <exception cref="KeyNotFoundException">Если записи с таким ID нет.</exception>
    /// <exception cref="ArgumentException">Если новые значения нарушают ограничения.</exception>
    void Update(int id, decimal amount, DateOnly date, string category, string? comment);

    /// <summary>Удаляет запись по ID. Освободившийся ID повторно не назначается.</summary>
    /// <param name="id">Идентификатор записи.</param>
    /// <exception cref="KeyNotFoundException">Если записи с таким ID нет.</exception>
    void Delete(int id);

    /// <summary>Возвращает все записи: по дате по убыванию, при равных датах — по ID по убыванию.</summary>
    /// <returns>Отсортированный список записей.</returns>
    IReadOnlyList<Expense> GetAll();

    /// <summary>Отбирает записи за период и, при необходимости, по категории.</summary>
    /// <param name="start">Начальная дата (включительно).</param>
    /// <param name="end">Конечная дата (включительно).</param>
    /// <param name="category">Категория или <c>null</c>, если категория не важна.</param>
    /// <returns>Отобранные записи в порядке, как у <see cref="GetAll"/>.</returns>
    /// <exception cref="ArgumentException">Если начальная дата позже конечной или категории нет в справочнике.</exception>
    IReadOnlyList<Expense> Filter(DateOnly start, DateOnly end, string? category);
}
