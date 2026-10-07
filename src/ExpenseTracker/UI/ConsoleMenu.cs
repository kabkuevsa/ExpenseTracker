using System.Globalization;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Spectre.Console;

namespace ExpenseTracker.UI;

/// <summary>
/// Текстовый диалог с пользователем в консоли: главное меню, ввод и проверка значений,
/// вывод таблиц (Spectre.Console). Бизнес-логики здесь нет — она в сервисах.
/// </summary>
public sealed class ConsoleMenu
{
    private const int PageSize = 20;

    private readonly IExpenseService _expenses;
    private readonly ICategoryService _categories;
    private readonly IReportService _reports;
    private readonly IReportExporter _exporter;
    private readonly TimeProvider _time;

    /// <summary>Создаёт меню.</summary>
    /// <param name="expenses">Сервис записей о расходах.</param>
    /// <param name="categories">Сервис справочника категорий.</param>
    /// <param name="reports">Сервис отчётов.</param>
    /// <param name="exporter">Экспортёр отчёта в файл.</param>
    /// <param name="timeProvider">Источник текущей даты; по умолчанию системные часы.</param>
    public ConsoleMenu(IExpenseService expenses, ICategoryService categories,
        IReportService reports, IReportExporter exporter, TimeProvider? timeProvider = null)
    {
        _expenses = expenses;
        _categories = categories;
        _reports = reports;
        _exporter = exporter;
        _time = timeProvider ?? TimeProvider.System;
    }

    private DateOnly Today => DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

    /// <summary>
    /// Запускает главный цикл меню; возвращает управление при выборе пункта «0» или при закрытии ввода.
    /// </summary>
    /// <remarks>
    /// Ошибки бизнес-правил (<see cref="ArgumentException"/>, <see cref="KeyNotFoundException"/>,
    /// <see cref="InvalidOperationException"/>) выводятся пользователю. Остальные исключения
    /// передаются вызывающему коду и обрабатываются как внутренняя ошибка.
    /// </remarks>
    public void Run()
    {
        try
        {
            while (true)
            {
                PrintMainMenu();
                var choice = ReadLine("Выберите пункт: ").Trim();
                if (choice == "0")
                    return;
                SafeRun(() => Dispatch(choice));
            }
        }
        catch (EndOfStreamException)
        {
            // Ввод закрыт (Ctrl+D / Ctrl+Z) — штатное завершение.
        }
    }

    private static void PrintMainMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== ExpenseTracker: учёт личных расходов ===");
        Console.WriteLine("1. Добавить расход");
        Console.WriteLine("2. Список расходов");
        Console.WriteLine("3. Изменить расход");
        Console.WriteLine("4. Удалить расход");
        Console.WriteLine("5. Категории");
        Console.WriteLine("6. Отбор по периоду и категории");
        Console.WriteLine("7. Отчёт по категориям за период");
        Console.WriteLine("8. Экспорт отчёта в CSV");
        Console.WriteLine("0. Выход");
    }

    private void Dispatch(string choice)
    {
        switch (choice)
        {
            case "1": AddExpense(); break;
            case "2": PrintPaged(_expenses.GetAll()); break;
            case "3": EditExpense(); break;
            case "4": DeleteExpense(); break;
            case "5": CategoriesMenu(); break;
            case "6": FilterExpenses(); break;
            case "7": ShowReport(); break;
            case "8": ExportReport(); break;
            default: Console.WriteLine("Неизвестный пункт меню. Введите число от 0 до 8."); break;
        }
    }

    private static void SafeRun(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or InvalidOperationException)
        {
            Console.WriteLine(ex.Message);
        }
    }

    // ---------- расходы ----------

    private void AddExpense()
    {
        var amount = ReadAmount("Сумма: ");
        var date = ReadDate("Дата (ГГГГ-ММ-ДД): ");
        var category = ReadCategory("Категория (номер или название): ");
        var comment = ReadComment("Комментарий (необязательно): ", editing: false, current: null);

        var expense = _expenses.Add(amount, date, category, comment);
        Console.WriteLine($"Расход добавлен. ID: {expense.Id}");
    }

    private void EditExpense()
    {
        var id = ReadInt("ID записи: ");
        var expense = _expenses.GetById(id);
        if (expense is null)
        {
            Console.WriteLine($"Запись с ID {id} не найдена");
            return;
        }

        Console.WriteLine("Enter — оставить текущее значение; в комментарии «-» очищает его.");
        var amount = ReadAmount($"Сумма [{Formatting.Money(expense.Amount)}]: ", expense.Amount);
        var date = ReadDate($"Дата [{Formatting.Date(expense.Date)}]: ", expense.Date);
        var category = ReadCategory($"Категория [{expense.Category}]: ", expense.Category);
        var comment = ReadComment($"Комментарий [{expense.Comment ?? "нет"}]: ", editing: true, current: expense.Comment);

        _expenses.Update(id, amount, date, category, comment);
        Console.WriteLine("Расход изменён");
    }

    private void DeleteExpense()
    {
        var id = ReadInt("ID записи: ");
        if (_expenses.GetById(id) is null)
        {
            Console.WriteLine($"Запись с ID {id} не найдена");
            return;
        }

        var answer = ReadLine("Удалить запись? (да/нет): ").Trim();
        if (answer.Equals("да", StringComparison.OrdinalIgnoreCase))
        {
            _expenses.Delete(id);
            Console.WriteLine("Расход удалён");
        }
        else
        {
            Console.WriteLine("Удаление отменено");
        }
    }

    private void FilterExpenses()
    {
        var (start, end) = ReadPeriod();
        var category = ReadOptionalCategory();

        var items = _expenses.Filter(start, end, category);
        PrintPaged(items);
        Console.WriteLine($"Итого: {Formatting.Money(items.Sum(e => e.Amount))}");
    }

    // ---------- категории ----------

    private void CategoriesMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Категории: 1 — список, 2 — добавить, 3 — удалить, 0 — назад");
            var choice = ReadLine("Выберите пункт: ").Trim();
            if (choice == "0")
                return;
            SafeRun(() => CategoriesDispatch(choice));
        }
    }

    private void CategoriesDispatch(string choice)
    {
        switch (choice)
        {
            case "1":
                PrintCategories();
                break;
            case "2":
                var name = ReadCategoryName();
                Console.WriteLine($"Категория «{_categories.Add(name)}» добавлена");
                break;
            case "3":
                _categories.Remove(ReadLine("Название удаляемой категории: "));
                Console.WriteLine("Категория удалена");
                break;
            default:
                Console.WriteLine("Неизвестный пункт. Введите число от 0 до 3.");
                break;
        }
    }

    private void PrintCategories()
    {
        var all = _categories.GetAll();
        for (var i = 0; i < all.Count; i++)
            Console.WriteLine($"{i + 1}. {all[i]}");
    }

    // ---------- отчёт ----------

    private void ShowReport()
    {
        var (start, end) = ReadPeriod();
        var report = _reports.Build(start, end);

        if (report.Rows.Count == 0)
        {
            Console.WriteLine("Записей нет");
        }
        else
        {
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Категория");
            table.AddColumn(new TableColumn("Сумма").RightAligned());
            table.AddColumn(new TableColumn("Доля, %").RightAligned());
            foreach (var row in report.Rows)
                table.AddRow(Markup.Escape(row.Category), Formatting.Money(row.Amount), Formatting.Percent(row.SharePercent));
            AnsiConsole.Write(table);
        }
        Console.WriteLine($"Общая сумма за период: {Formatting.Money(report.Total)}");
    }

    private void ExportReport()
    {
        var (start, end) = ReadPeriod();
        var report = _reports.Build(start, end);
        var path = _exporter.Export(report);
        Console.WriteLine($"Отчёт сохранён: {path}");
    }

    // ---------- вывод таблицы ----------

    private static void PrintPaged(IReadOnlyList<Expense> items)
    {
        if (items.Count == 0)
        {
            Console.WriteLine("Записей нет");
            return;
        }

        for (var offset = 0; offset < items.Count; offset += PageSize)
        {
            RenderTable(items.Skip(offset).Take(PageSize));
            var shown = Math.Min(offset + PageSize, items.Count);
            Console.WriteLine($"Показано {shown} из {items.Count}");

            if (shown < items.Count)
            {
                var answer = ReadLine("Enter — следующая страница, q — прервать: ").Trim();
                if (answer.Equals("q", StringComparison.OrdinalIgnoreCase))
                    break;
            }
        }
    }

    private static void RenderTable(IEnumerable<Expense> page)
    {
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("ID");
        table.AddColumn("Дата");
        table.AddColumn("Категория");
        table.AddColumn(new TableColumn("Сумма").RightAligned());
        table.AddColumn("Комментарий");
        foreach (var e in page)
        {
            table.AddRow(
                e.Id.ToString(CultureInfo.InvariantCulture),
                Formatting.Date(e.Date),
                Markup.Escape(e.Category),
                Formatting.Money(e.Amount),
                Markup.Escape(e.Comment ?? ""));
        }
        AnsiConsole.Write(table);
    }

    // ---------- ввод с повторным запросом ----------

    private static string ReadLine(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? throw new EndOfStreamException();
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            var input = ReadLine(prompt).Trim();
            if (int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out var value) && value > 0)
                return value;
            Console.WriteLine("Введите целое положительное число, например 5.");
        }
    }

    private static decimal ReadAmount(string prompt, decimal? current = null)
    {
        while (true)
        {
            var input = ReadLine(prompt);
            if (current is not null && input.Trim().Length == 0)
                return current.Value;

            if (!ExpenseValidator.TryParseAmount(input, out var amount))
            {
                Console.WriteLine("Введите число, например 1250.50.");
                continue;
            }
            var error = ExpenseValidator.CheckAmount(amount);
            if (error is null)
                return amount;
            Console.WriteLine(error);
        }
    }

    private DateOnly ReadDate(string prompt, DateOnly? current = null, bool enforceRules = true)
    {
        while (true)
        {
            var input = ReadLine(prompt);
            if (current is not null && input.Trim().Length == 0)
                return current.Value;

            if (!ExpenseValidator.TryParseDate(input, out var date))
            {
                Console.WriteLine("Неверный формат даты. Используйте ГГГГ-ММ-ДД, например 2026-10-01.");
                continue;
            }
            var error = enforceRules ? ExpenseValidator.CheckDate(date, Today) : null;
            if (error is null)
                return date;
            Console.WriteLine(error);
        }
    }

    private string ReadCategory(string prompt, string? current = null)
    {
        PrintCategories();
        while (true)
        {
            var input = ReadLine(prompt).Trim();
            if (current is not null && input.Length == 0)
                return current;

            var all = _categories.GetAll();
            if (int.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out var number)
                && number >= 1 && number <= all.Count)
                return all[number - 1];

            var found = _categories.Find(input);
            if (found is not null)
                return found;

            Console.WriteLine($"Категория «{input}» не найдена. Введите номер от 1 до {all.Count} или название из списка.");
        }
    }

    private string? ReadOptionalCategory()
    {
        while (true)
        {
            var input = ReadLine("Категория (Enter — все): ").Trim();
            if (input.Length == 0)
                return null;

            var found = _categories.Find(input);
            if (found is not null)
                return found;
            Console.WriteLine($"Категория «{input}» не найдена в справочнике.");
        }
    }

    private static string? ReadComment(string prompt, bool editing, string? current)
    {
        while (true)
        {
            var input = ReadLine(prompt);
            if (editing && input.Length == 0)
                return current;
            if (editing && input.Trim() == "-")
                return null;

            var trimmed = input.Trim();
            var error = ExpenseValidator.CheckComment(trimmed);
            if (error is null)
                return trimmed.Length == 0 ? null : trimmed;
            Console.WriteLine(error);
        }
    }

    private static string ReadCategoryName()
    {
        while (true)
        {
            var input = ReadLine("Название новой категории: ");
            var error = ExpenseValidator.CheckCategoryName(input);
            if (error is null)
                return input.Trim();
            Console.WriteLine(error);
        }
    }

    private (DateOnly Start, DateOnly End) ReadPeriod()
    {
        while (true)
        {
            var start = ReadDate("Начальная дата (ГГГГ-ММ-ДД): ", enforceRules: false);
            var end = ReadDate("Конечная дата (ГГГГ-ММ-ДД): ", enforceRules: false);
            if (start <= end)
                return (start, end);
            Console.WriteLine("Начальная дата не может быть позже конечной. Введите период заново.");
        }
    }
}
