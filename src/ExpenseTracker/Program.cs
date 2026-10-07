using System.Text;
using ExpenseTracker;
using ExpenseTracker.Services;
using ExpenseTracker.Storage;
using ExpenseTracker.UI;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

try
{
    var store = new JsonDataStore(Path.Combine(Directory.GetCurrentDirectory(), "expenses.json"));
    var loaded = store.Load();
    if (loaded.Notice is not null)
        Console.WriteLine(loaded.Notice);

    // Все сервисы работают с одним и тем же объектом данных.
    IExpenseService expenses = new ExpenseService(store, loaded.Data);
    ICategoryService categories = new CategoryService(store, loaded.Data);
    IReportService reports = new ReportService(expenses);
    IReportExporter exporter = new CsvReportExporter();

    new ConsoleMenu(expenses, categories, reports, exporter).Run();
    return 0;
}
catch (Exception ex)
{
    ErrorLogger.Write(ex);
    Console.WriteLine("Произошла внутренняя ошибка. Подробности в файле error.log");
    return 1;
}
