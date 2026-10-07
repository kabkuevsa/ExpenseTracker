using System.Globalization;

namespace ExpenseTracker.Services;

/// <summary>
/// Правила проверки и разбора вводимых значений (раздел 4.1 ТЗ).
/// Методы <c>Check*</c> возвращают текст ошибки или <c>null</c>, если значение допустимо.
/// </summary>
public static class ExpenseValidator
{
    /// <summary>Максимально допустимая сумма расхода.</summary>
    public const decimal MaxAmount = 1_000_000_000.00m;

    /// <summary>Максимальная длина комментария.</summary>
    public const int MaxCommentLength = 200;

    /// <summary>Максимальная длина названия категории.</summary>
    public const int MaxCategoryNameLength = 50;

    /// <summary>Максимальное число категорий в справочнике.</summary>
    public const int MaxCategories = 100;

    /// <summary>Самая ранняя допустимая дата расхода.</summary>
    public static readonly DateOnly MinDate = new(2000, 1, 1);

    /// <summary>Проверяет сумму расхода.</summary>
    /// <param name="amount">Проверяемая сумма.</param>
    /// <returns>Текст ошибки либо <c>null</c>, если сумма допустима.</returns>
    public static string? CheckAmount(decimal amount)
    {
        if (amount <= 0 || amount > MaxAmount)
            return "Сумма должна быть больше 0 и не более 1 000 000 000.00.";
        if (decimal.Round(amount, 2) != amount)
            return "Допускается не более двух знаков после запятой.";
        return null;
    }

    /// <summary>Проверяет дату расхода.</summary>
    /// <param name="date">Проверяемая дата.</param>
    /// <param name="today">Текущая дата ПК (верхняя граница).</param>
    /// <returns>Текст ошибки либо <c>null</c>, если дата допустима.</returns>
    public static string? CheckDate(DateOnly date, DateOnly today)
    {
        if (date < MinDate)
            return $"Дата не может быть ранее {Formatting.Date(MinDate)}.";
        if (date > today)
            return $"Дата не может быть позже текущей ({Formatting.Date(today)}).";
        return null;
    }

    /// <summary>Проверяет комментарий.</summary>
    /// <param name="comment">Комментарий или <c>null</c>.</param>
    /// <returns>Текст ошибки либо <c>null</c>, если комментарий допустим.</returns>
    public static string? CheckComment(string? comment) =>
        comment is { Length: > MaxCommentLength }
            ? $"Комментарий не должен превышать {MaxCommentLength} символов."
            : null;

    /// <summary>Проверяет название категории (длина от 1 до 50 символов, не пустое).</summary>
    /// <param name="name">Название категории.</param>
    /// <returns>Текст ошибки либо <c>null</c>, если название допустимо.</returns>
    public static string? CheckCategoryName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Название категории не может быть пустым.";
        if (name.Trim().Length > MaxCategoryNameLength)
            return $"Название категории не должно превышать {MaxCategoryNameLength} символов.";
        return null;
    }

    /// <summary>Разбирает сумму из строки; допускает и точку, и запятую как разделитель.</summary>
    /// <param name="input">Введённая строка.</param>
    /// <param name="amount">Результат разбора.</param>
    /// <returns><c>true</c>, если строка является числом.</returns>
    public static bool TryParseAmount(string? input, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(input))
            return false;
        var normalized = input.Trim().Replace(" ", "").Replace(',', '.');
        return decimal.TryParse(normalized,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out amount);
    }

    /// <summary>Разбирает дату строго в формате ГГГГ-ММ-ДД.</summary>
    /// <param name="input">Введённая строка.</param>
    /// <param name="date">Результат разбора.</param>
    /// <returns><c>true</c>, если строка соответствует формату и является существующей датой.</returns>
    public static bool TryParseDate(string? input, out DateOnly date) =>
        DateOnly.TryParseExact(input?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out date);
}
