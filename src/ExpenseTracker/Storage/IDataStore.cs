using ExpenseTracker.Models;

namespace ExpenseTracker.Storage;

/// <summary>Результат загрузки данных.</summary>
/// <param name="Data">Загруженное (или заново созданное) состояние программы.</param>
/// <param name="Notice">Сообщение для пользователя (файл создан или был повреждён); <c>null</c>, если всё штатно.</param>
public sealed record LoadResult(AppData Data, string? Notice);

/// <summary>Постоянное хранилище состояния программы.</summary>
public interface IDataStore
{
    /// <summary>Загружает данные; при отсутствии или повреждении файла создаёт новые.</summary>
    /// <returns>Данные и необязательное сообщение для пользователя.</returns>
    LoadResult Load();

    /// <summary>Сохраняет данные целиком.</summary>
    /// <param name="data">Состояние программы.</param>
    /// <exception cref="IOException">Если записать данные не удалось.</exception>
    void Save(AppData data);
}
