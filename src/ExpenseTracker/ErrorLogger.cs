using System.Globalization;
using System.Text;

namespace ExpenseTracker;

/// <summary>Запись необработанных исключений в файл <c>error.log</c>.</summary>
internal static class ErrorLogger
{
    /// <summary>Дописывает в журнал время, тип исключения и трассировку стека.</summary>
    /// <param name="exception">Исключение, которое нужно зафиксировать.</param>
    /// <param name="path">Путь к файлу журнала.</param>
    /// <remarks>Ошибки самой записи журнала подавляются, чтобы не маскировать исходное исключение.</remarks>
    public static void Write(Exception exception, string path = "error.log")
    {
        try
        {
            var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var text = $"[{time}] {exception.GetType().FullName}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}";
            File.AppendAllText(path, text, new UTF8Encoding(false));
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
