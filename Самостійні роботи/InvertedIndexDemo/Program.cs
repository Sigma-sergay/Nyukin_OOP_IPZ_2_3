using System.Text;
using InvertedIndexDemo;

Console.OutputEncoding = Encoding.UTF8;

List<Document> documents = LoadSampleDocuments();

// Режим вимірювання: dotnet run -c Release -- bench
if (args.Contains("bench"))
{
    RunBenchmarks(documents);
    return;
}

var index = new InvertedIndex();
foreach (Document doc in documents)
    index.AddDocument(doc);

Console.WriteLine($"Loaded {index.DocumentCount} documents");
Console.WriteLine($"Index contains {index.UniqueWords} unique words");
Console.WriteLine();

while (true)
{
    Console.Write("Enter search query (or 'exit'): ");
    string? query = Console.ReadLine();

    // null означає кінець вводу (Ctrl+D / Ctrl+Z або перенаправлений ввід)
    if (query == null || query == "exit")
        break;

    string[] words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    Console.WriteLine("\n--- Search Results (AND) ---");
    List<Document> results = index.SearchMultipleWords(words);
    foreach (Document doc in results)
    {
        Console.WriteLine(doc);
        Console.WriteLine($"  {doc.Content.Substring(0, Math.Min(100, doc.Content.Length))}...");
    }
    Console.WriteLine($"\nFound {results.Count} documents\n");
}

static List<Document> LoadSampleDocuments()
{
    return new List<Document>
    {
        new Document
        {
            Id = 1,
            Title = "Introduction to C#",
            Content = "C# is a modern object-oriented programming language developed by Microsoft."
        },
        new Document
        {
            Id = 2,
            Title = "Java Basics",
            Content = "Java is a popular programming language known for its portability and performance."
        },
        new Document
        {
            Id = 3,
            Title = "Python Overview",
            Content = "Python is a high-level programming language with simple syntax."
        },
        new Document
        {
            Id = 4,
            Title = "C# Advanced Features",
            Content = "C# supports LINQ, async/await, and many modern programming paradigms."
        },
        new Document
        {
            Id = 5,
            Title = "Comparing Languages",
            Content = "C#, Java, and Python are among the most popular programming languages today."
        }
    };
}

static void RunBenchmarks(List<Document> baseDocuments)
{
    Console.WriteLine("=== Порівняння з методички (один виклик, тіки) ===");
    foreach (int size in new[] { 10, 100, 1000, 10000 })
    {
        Console.WriteLine($"\n-- {size} документів, слово 'programming' --");
        PerformanceTest.CompareSearchMethods(PerformanceTest.GenerateDocuments(size, baseDocuments), "programming");
    }

    Console.WriteLine();
    Console.WriteLine("=== Усереднені вимірювання (багато повторів) ===");
    PerformanceTest.BenchmarkTable(baseDocuments, new[] { 10, 100, 1000, 10000 }, new[] { "programming", "term42" });
}
