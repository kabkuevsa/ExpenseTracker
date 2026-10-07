# ExpenseTracker

Консольная программа учёта личных расходов (C# 14, .NET 10) по техническому заданию КТ1.

## Требования
- .NET SDK 10
- Консоль с поддержкой UTF-8

## Сборка и запуск
```bash
dotnet build src/ExpenseTracker
dotnet run --project src/ExpenseTracker
```
Файлы `expenses.json`, `error.log` и `report_ГГГГ-ММ-ДД.csv` создаются в **текущем каталоге**, из которого запущена программа.
После сборки рядом с `ExpenseTracker.dll` (`src/ExpenseTracker/bin/Debug/net10.0/`) появляется `ExpenseTracker.xml` — XML-документация.

## Структура
```
src/ExpenseTracker/
  Models/     Expense, AppData, Report, ReportRow
  Services/   IExpenseService / ExpenseService, ICategoryService / CategoryService,
              IReportService / ReportService, IReportExporter / CsvReportExporter, ExpenseValidator
  Storage/    IDataStore / JsonDataStore, LoadResult
  UI/         ConsoleMenu
  Program.cs  точка входа, обработка необработанных исключений
docs/diagrams/  диаграммы классов и последовательности (.drawio + PNG)
```
