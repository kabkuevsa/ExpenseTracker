using System.Globalization;

namespace ExpenseTracker;

/// <summary>Единое форматирование чисел и дат (не зависит от региональных настроек ПК).</summary>
internal static class Formatting
{
    /// <summary>Сумма с двумя знаками после точки, например <c>1250.50</c>.</summary>
    public static string Money(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);

    /// <summary>Процент с одним знаком после точки, например <c>12.5</c>.</summary>
    public static string Percent(decimal value) => value.ToString("F1", CultureInfo.InvariantCulture);

    /// <summary>Дата в формате ГГГГ-ММ-ДД.</summary>
    public static string Date(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
