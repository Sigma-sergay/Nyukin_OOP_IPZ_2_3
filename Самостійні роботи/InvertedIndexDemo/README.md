# InvertedIndexDemo: інвертований індекс

Консольний застосунок з власною реалізацією інвертованого індексу для повнотекстового пошуку.

- Звіт з таблицею вимірювань і висновками: [REPORT.md](REPORT.md)
- Інтерактивний пошук: `dotnet run`
- Вимірювання продуктивності: `dotnet run -c Release -- bench`

## Структура

| Файл | Призначення |
|---|---|
| `Document.cs` | клас документа (`Id`, `Title`, `Content`) |
| `TextProcessor.cs` | `Tokenize` (нормалізація тексту) і `RemoveStopWords` |
| `InvertedIndex.cs` | індекс `Dictionary<string, HashSet<int>>`, пошук AND (`IntersectWith`), OR (`UnionWith`) |
| `PerformanceTest.cs` | порівняння лінійного пошуку з індексом, генерація документів |
| `Program.cs` | інтерактивний пошук та режим `bench` |

## Приклад виводу програми

```
Loaded 5 documents
Index contains 37 unique words

Enter search query (or 'exit'): programming language
--- Search Results (AND) ---
[1] Introduction to C#
  C# is a modern object-oriented programming language developed by Microsoft....
[2] Java Basics
  Java is a popular programming language known for its portability and performance....
[3] Python Overview
  Python is a high-level programming language with simple syntax....

Found 3 documents

Enter search query (or 'exit'): c# async
--- Search Results (AND) ---
[4] C# Advanced Features
  C# supports LINQ, async/await, and many modern programming paradigms....

Found 1 documents

Enter search query (or 'exit'): exit
```
