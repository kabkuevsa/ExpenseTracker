using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using ExpenseTracker.Models;
using ExpenseTracker.Services;

namespace ExpenseTracker.Storage;

/// <summary>
/// Хранение данных в JSON-файле. Запись выполняется через временный файл <c>.tmp</c>,
/// повреждённый файл не удаляется, а переименовывается в <c>.bak_ГГГГММДД_ЧЧММСС</c>.
/// </summary>
public sealed class JsonDataStore : IDataStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // кириллица записывается читаемо, без \uXXXX
    };

    private readonly string _path;

    /// <summary>Создаёт хранилище.</summary>
    /// <param name="path">Путь к файлу данных (например, <c>expenses.json</c>).</param>
    public JsonDataStore(string path) => _path = path;

    /// <inheritdoc />
    public LoadResult Load()
    {
        if (!File.Exists(_path))
        {
            var fresh = AppData.CreateDefault();
            Save(fresh);
            return new LoadResult(fresh, "Файл данных не найден, создан новый");
        }

        try
        {
            var json = File.ReadAllText(_path, Encoding.UTF8);
            var data = JsonSerializer.Deserialize<AppData>(json, Options)
                ?? throw new InvalidDataException("Файл данных пуст.");
            EnsureValid(data);
            return new LoadResult(data, null);
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException or NotSupportedException)
        {
            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var backupPath = $"{_path}.bak_{stamp}";
            File.Move(_path, backupPath);

            var fresh = AppData.CreateDefault();
            Save(fresh);
            return new LoadResult(fresh,
                $"Файл данных повреждён. Копия сохранена: {Path.GetFileName(backupPath)}. Создан новый файл.");
        }
    }

    /// <inheritdoc />
    public void Save(AppData data)
    {
        var tempPath = _path + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(data, Options), new UTF8Encoding(false));
        File.Move(tempPath, _path, overwrite: true);
    }

    private static void EnsureValid(AppData data)
    {
        if (data.Categories is null || data.Expenses is null)
            throw new InvalidDataException("Нарушена структура файла данных.");
        if (data.NextId < 1)
            throw new InvalidDataException("Некорректное значение nextId.");
        if (data.Categories.Count > ExpenseValidator.MaxCategories)
            throw new InvalidDataException("Слишком много категорий.");

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in data.Categories)
        {
            if (ExpenseValidator.CheckCategoryName(name) is not null || !names.Add(name))
                throw new InvalidDataException("Некорректный справочник категорий.");
        }

        var ids = new HashSet<int>();
        foreach (var e in data.Expenses)
        {
            var valid = e is not null
                && e.Id >= 1 && e.Id < data.NextId && ids.Add(e.Id)
                && e.Category is not null && names.Contains(e.Category)
                && e.Date >= ExpenseValidator.MinDate
                && ExpenseValidator.CheckAmount(e.Amount) is null
                && ExpenseValidator.CheckComment(e.Comment) is null;
            if (!valid)
                throw new InvalidDataException("Некорректная запись о расходе.");
        }
    }
}
