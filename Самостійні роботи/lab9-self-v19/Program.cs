using System.Text;
using lab9_self_v19;

Console.OutputEncoding = Encoding.UTF8;

// ============ Колекція 1: List<int> ============
var numbers = new List<int> { 3, -7, 12, 5, -20, 8, 1 };
Console.WriteLine("=== List<int>: " + string.Join(", ", numbers) + " ===");

var squares = CollectionUtils.Map(numbers, n => n * n);
Console.WriteLine("Map (квадрати): " + string.Join(", ", squares));

int largestByAbs = CollectionUtils.MaxBy(numbers, n => Math.Abs(n));
Console.WriteLine($"MaxBy (найбільший за модулем): {largestByAbs}");

var positiveEven = CollectionUtils.Filter(numbers, n => n > 0 && n % 2 == 0);
Console.WriteLine("Filter (додатні парні): " + string.Join(", ", positiveEven));

Console.Write("ForEach (кожне число + 1): ");
CollectionUtils.ForEach(numbers, n => Console.Write($"{n + 1} "));
Console.WriteLine();
Console.WriteLine();

// ============ Колекція 2: List<string> ============
var words = new List<string> { "delegate", "lambda", "LINQ", "predicate", "action", "func" };
Console.WriteLine("=== List<string>: " + string.Join(", ", words) + " ===");

var lengths = CollectionUtils.Map(words, w => $"{w}={w.Length}");
Console.WriteLine("Map (слово=довжина): " + string.Join(", ", lengths));

string longest = CollectionUtils.MaxBy(words, w => w.Length);
Console.WriteLine($"MaxBy (найдовше слово): {longest}");

var startWithA = CollectionUtils.Filter(words, w => w.StartsWith("a", StringComparison.OrdinalIgnoreCase));
Console.WriteLine("Filter (починаються з 'a'): " + string.Join(", ", startWithA));

Console.WriteLine("ForEach (у верхньому регістрі):");
CollectionUtils.ForEach(words, w => Console.WriteLine($"  {w.ToUpperInvariant()}"));
Console.WriteLine();

// ============ Колекція 3: List<Product> ============
var products = new List<Product>
{
    new Product { Name = "Laptop", Price = 25000 },
    new Product { Name = "Mouse", Price = 500 },
    new Product { Name = "Keyboard", Price = 1500 },
    new Product { Name = "Monitor", Price = 8200 }
};
Console.WriteLine("=== List<Product> ===");

var names = CollectionUtils.Map(products, p => p.Name);
Console.WriteLine("Names: " + string.Join(", ", names));

var mostExpensive = CollectionUtils.MaxBy(products, p => p.Price);
Console.WriteLine($"Most expensive: {mostExpensive.Name} - {mostExpensive.Price}");

var expensive = CollectionUtils.Filter(products, p => p.Price > 1000);
Console.WriteLine("Expensive products (> 1000):");
CollectionUtils.ForEach(expensive, p => Console.WriteLine($"  - {p.Name}"));

// Map з перетворенням типу: Product -> рядок зі знижкою 10%
var discounted = CollectionUtils.Map(products, p => $"{p.Name}: {p.Price * 0.9m:F2} грн");
Console.WriteLine("Зі знижкою 10%:");
CollectionUtils.ForEach(discounted, line => Console.WriteLine($"  {line}"));
Console.WriteLine();

// ============ Порівняння з LINQ ============
Console.WriteLine("=== Порівняння з LINQ ===");
Console.WriteLine($"Map == Select:   {CollectionUtils.Map(numbers, n => n * n).SequenceEqual(numbers.Select(n => n * n))}");
Console.WriteLine($"Filter == Where: {CollectionUtils.Filter(numbers, n => n > 0).SequenceEqual(numbers.Where(n => n > 0))}");
Console.WriteLine($"MaxBy == LINQ MaxBy: {ReferenceEquals(CollectionUtils.MaxBy(products, p => p.Price), products.MaxBy(p => p.Price))}");

// Відкладене виконання: Map не рахує нічого, доки результат не переберуть
int calls = 0;
var lazy = CollectionUtils.Map(numbers, n => { calls++; return n; });
Console.WriteLine($"Викликів лямбди одразу після Map: {calls}");
_ = lazy.ToList();
Console.WriteLine($"Викликів лямбди після перебору:   {calls}");

// Порожня колекція
try
{
    CollectionUtils.MaxBy(new List<int>(), n => n);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"MaxBy для порожньої колекції: {ex.Message}");
}
