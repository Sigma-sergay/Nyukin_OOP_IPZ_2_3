using System.Diagnostics;

namespace InvertedIndexDemo;

public static class PerformanceTest
{
    /// <summary>
    /// Порівняння з методички: один запуск лінійного пошуку та пошуку через індекс.
    /// Для одного виклику значення в тіках сильно залежать від JIT-компіляції, тому
    /// надійніші результати дає BenchmarkTable.
    /// </summary>
    public static void CompareSearchMethods(List<Document> documents, string searchWord)
    {
        var sw = new Stopwatch();

        // 1. Лінійний пошук (та сама нормалізація, що й в індексі)
        sw.Start();
        var normalizedWord = searchWord.ToLowerInvariant();
        var linearResults = documents.Where(d =>
            TextProcessor.Tokenize(d.Content).Contains(normalizedWord)).ToList();
        sw.Stop();
        Console.WriteLine($"Linear search: {sw.ElapsedTicks} ticks");

        // 2. Інвертований індекс
        var index = new InvertedIndex();
        foreach (var doc in documents)
            index.AddDocument(doc);

        sw.Restart();
        var indexResults = index.SearchSingleWord(searchWord);
        sw.Stop();
        Console.WriteLine($"Inverted Index: {sw.ElapsedTicks} ticks");

        // Перевірка коректності: порівняння має сенс, лише якщо результати збігаються
        Console.WriteLine($"Linear found: {linearResults.Count}, Index found: {indexResults.Count}");
    }

    /// <summary>
    /// Генерує документи комбінуванням п'яти базових текстів і випадкових слів зі словника.
    /// Фіксований seed робить результати відтворюваними.
    /// </summary>
    public static List<Document> GenerateDocuments(int count, List<Document> baseDocuments)
    {
        var random = new Random(42);
        var vocabulary = new List<string>
        {
            "compiler", "runtime", "framework", "library", "network", "database", "thread", "memory",
            "garbage", "collection", "generic", "interface", "delegate", "lambda", "query", "index"
        };
        for (int i = 0; i < 200; i++)
            vocabulary.Add($"term{i}");

        var result = new List<Document>(count);
        for (int i = 1; i <= count; i++)
        {
            Document template = baseDocuments[(i - 1) % baseDocuments.Count];
            var extra = new List<string>();
            int extraCount = random.Next(10, 21);
            for (int j = 0; j < extraCount; j++)
                extra.Add(vocabulary[random.Next(vocabulary.Count)]);

            result.Add(new Document
            {
                Id = i,
                Title = $"{template.Title} #{i}",
                Content = template.Content + " " + string.Join(' ', extra)
            });
        }
        return result;
    }

    /// <summary>
    /// Таблиця вимірювань для різної кількості документів: час побудови індексу,
    /// середній час одного пошуку (лінійний та через індекс) і кількість пошуків, після якої індекс окупається.
    /// </summary>
    public static void BenchmarkTable(List<Document> baseDocuments, int[] sizes, string[] words)
    {
        Console.WriteLine($"{"Документів",10} | {"Слово",-12} | {"Знайдено",8} | {"Побудова, мс",12} | {"Лінійний, мкс",13} | {"Індекс, мкс",11} | {"Прискорення",11} | {"Окупність, пошуків",18}");
        Console.WriteLine(new string('-', 120));

        foreach (int size in sizes)
        {
            List<Document> documents = GenerateDocuments(size, baseDocuments);

            // Побудова індексу
            long buildStart = Stopwatch.GetTimestamp();
            var index = new InvertedIndex();
            foreach (var doc in documents)
                index.AddDocument(doc);
            double buildMs = Stopwatch.GetElapsedTime(buildStart).TotalMilliseconds;

            foreach (string word in words)
            {
                string normalized = word.ToLowerInvariant();

                // Прогрів (JIT) та перевірка однакової кількості результатів
                int linearCount = documents.Count(d => TextProcessor.Tokenize(d.Content).Contains(normalized));
                int indexCount = index.SearchSingleWord(word).Count;
                if (linearCount != indexCount)
                    throw new InvalidOperationException($"Результати не збігаються: {linearCount} проти {indexCount}");

                // Скільки повторів робити, щоб загальний час був вимірюваним
                int linearRuns = Math.Max(3, 20000 / Math.Max(size, 1));
                int indexRuns = 20000;

                long t0 = Stopwatch.GetTimestamp();
                for (int i = 0; i < linearRuns; i++)
                    _ = documents.Where(d => TextProcessor.Tokenize(d.Content).Contains(normalized)).ToList();
                double linearUs = Stopwatch.GetElapsedTime(t0).TotalMilliseconds * 1000 / linearRuns;

                long t1 = Stopwatch.GetTimestamp();
                for (int i = 0; i < indexRuns; i++)
                    _ = index.SearchSingleWord(word);
                double indexUs = Stopwatch.GetElapsedTime(t1).TotalMilliseconds * 1000 / indexRuns;

                double speedup = linearUs / indexUs;
                double savingPerSearchMs = (linearUs - indexUs) / 1000.0;
                string payback = savingPerSearchMs > 0 ? Math.Ceiling(buildMs / savingPerSearchMs).ToString("F0") : "-";

                Console.WriteLine($"{size,10} | {word,-12} | {indexCount,8} | {buildMs,12:F2} | {linearUs,13:F1} | {indexUs,11:F2} | {speedup,10:F0}x | {payback,18}");
            }
        }
    }
}
