using System.Text;
using IndependentWork8;

Console.OutputEncoding = Encoding.UTF8;

// ================= Сценарій 1: FileMetadataCache (Dictionary) =================
Console.WriteLine("=== Сценарій 1: FileMetadataCache ===");
var cache = new FileMetadataCache();

cache.Add(new FileMetadata { FileName = "report.docx", Size = 48213, LastModified = new DateTime(2026, 10, 1, 9, 30, 0), Hash = "a1b2c3" });
cache.Add(new FileMetadata { FileName = "photo.jpg", Size = 2_104_882, LastModified = new DateTime(2026, 9, 15, 18, 5, 0), Hash = "d4e5f6" });
Console.WriteLine($"У кеші файлів: {cache.Count}");

// Пошук нечутливий до регістру завдяки StringComparer.OrdinalIgnoreCase
Console.WriteLine($"Get(\"REPORT.DOCX\"): {cache.Get("REPORT.DOCX")}");
Console.WriteLine($"Contains(\"missing.txt\"): {cache.Contains("missing.txt")}");
bool added = cache.Add(new FileMetadata { FileName = "Report.docx", Size = 1 });
Console.WriteLine($"Повторне додавання report.docx успішне? {added}");
Console.WriteLine($"Remove(\"photo.jpg\"): {cache.Remove("photo.jpg")}, залишилось: {cache.Count}");
Console.WriteLine();

// ================= Сценарій 2: TagManager (HashSet) =================
Console.WriteLine("=== Сценарій 2: TagManager ===");
var tags = new TagManager();
tags.AddTag("article-1", "C#");
tags.AddTag("article-1", "programming");
tags.AddTag("article-1", "c#");          // дублікат (інший регістр): ігнорується
tags.AddTag("article-1", "dotnet");
tags.AddTag("article-2", "dotnet");
tags.AddTag("article-2", "linq");
tags.AddTag("article-2", "C#");

Console.WriteLine($"Теги article-1: {string.Join(", ", tags.GetTags("article-1").Order())}");
Console.WriteLine($"Теги article-2: {string.Join(", ", tags.GetTags("article-2").Order())}");
Console.WriteLine($"Усі унікальні теги (UnionWith): {string.Join(", ", tags.GetAllUniqueTags().Order())}");
Console.WriteLine($"Спільні теги (IntersectWith): {string.Join(", ", tags.FindCommonTags("article-1", "article-2").Order())}");
Console.WriteLine($"Лише в article-1 (ExceptWith): {string.Join(", ", tags.FindTagsOnlyInFirst("article-1", "article-2").Order())}");
tags.RemoveTag("article-1", "programming");
Console.WriteLine($"Після RemoveTag: {string.Join(", ", tags.GetTags("article-1").Order())}");
Console.WriteLine();

// ================= Сценарій 3: SearchIndex (Dictionary + HashSet) =================
Console.WriteLine("=== Сценарій 3: SearchIndex ===");
var index = new SearchIndex();
index.AddDocument(1, "C# is a modern programming language");
index.AddDocument(2, "Java is a popular programming language");
index.AddDocument(3, "Python and C# are popular");
index.AddDocument(4, "LINQ makes C# collections easy");
Console.WriteLine($"Унікальних слів в індексі: {index.WordCount}");

PrintSearch("programming");
PrintSearch("popular");
PrintSearch("c#");
Console.WriteLine($"SearchMultiple(\"popular\", \"language\"): {Format(index.SearchMultiple("popular", "language"))}");
Console.WriteLine($"SearchMultiple(\"programming\", \"python\"): {Format(index.SearchMultiple("programming", "python"))}");
Console.WriteLine($"SearchMultiple(\"language\", \"is\", \"modern\"): {Format(index.SearchMultiple("language", "is", "modern"))}");

void PrintSearch(string word) =>
    Console.WriteLine($"Search(\"{word}\"): {Format(index.Search(word))}");

static string Format(HashSet<int> docs) =>
    docs.Count == 0 ? "(нічого не знайдено)" : "документи " + string.Join(", ", docs.Order());
