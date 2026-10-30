# Самостійна робота №8: Dictionary і HashSet

Проєкт `IndependentWork8` демонструє три сценарії: кеш метаданих файлів (`Dictionary`), теги статей (`HashSet`) і пошуковий індекс (`Dictionary` + `HashSet`).

- Звіт з аналізом open-source прикладів, порівняльною таблицею й висновками: [REPORT.md](REPORT.md)
- Запуск: `dotnet run`

## Приклад виводу програми

```
=== Сценарій 1: FileMetadataCache ===
У кеші файлів: 2
Get("REPORT.DOCX"): report.docx: 48213 байт, змінено 2026-10-01 09:30, хеш a1b2c3
Contains("missing.txt"): False
Повторне додавання report.docx успішне? False
Remove("photo.jpg"): True, залишилось: 1

=== Сценарій 2: TagManager ===
Теги article-1: C#, dotnet, programming
Теги article-2: C#, dotnet, linq
Усі унікальні теги (UnionWith): C#, dotnet, linq, programming
Спільні теги (IntersectWith): C#, dotnet
Лише в article-1 (ExceptWith): programming
Після RemoveTag: C#, dotnet

=== Сценарій 3: SearchIndex ===
Унікальних слів в індексі: 15
Search("programming"): документи 1, 2
Search("popular"): документи 2, 3
Search("c#"): документи 1, 3, 4
SearchMultiple("popular", "language"): документи 2
SearchMultiple("programming", "python"): (нічого не знайдено)
SearchMultiple("language", "is", "modern"): документи 1
```
