using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine($"Логічних процесорів: {Environment.ProcessorCount}");
Console.WriteLine();

// ============================== Частина 1. Продуктивність ==============================
Console.WriteLine("=== Частина 1. LINQ проти PLINQ ===");

int[] sizes = { 1_000_000, 5_000_000, 10_000_000 };

// Прогрів: щоб JIT-компіляція не потрапила у вимірювання
{
    List<int> warm = GenerateData(100_000);
    _ = warm.Where(IsPrime).Select(n => (long)n * 2).Sum();
    _ = warm.AsParallel().Where(IsPrime).Select(n => (long)n * 2).Sum();
}

Console.WriteLine($"{"Операція",-34} | {"Елементів",10} | {"LINQ, мс",9} | {"PLINQ, мс",10} | {"Прискорення",11} | Результати збігаються");
Console.WriteLine(new string('-', 105));

foreach (int size in sizes)
{
    List<int> data = GenerateData(size);

    // Операція 1: ДОРОГА для кожного елемента (перевірка на просте число перебором дільників)
    Compare("Where(IsPrime).Select(x*2).Sum", size, data,
        d => d.Where(IsPrime).Select(n => (long)n * 2).Sum(),
        d => d.AsParallel().Where(IsPrime).Select(n => (long)n * 2).Sum());

    // Операція 2: ДЕШЕВА для кожного елемента (проста арифметика)
    Compare("Where(x%3==0).Select(sqrt).Sum", size, data,
        d => d.Where(n => n % 3 == 0).Select(n => (long)Math.Sqrt(n)).Sum(),
        d => d.AsParallel().Where(n => n % 3 == 0).Select(n => (long)Math.Sqrt(n)).Sum());
}

// ============================== Частина 2. Побічні ефекти ==============================
Console.WriteLine();
Console.WriteLine("=== Частина 2. Побічні ефекти й потокобезпечність ===");

List<int> sample = GenerateData(2_000_000);
int expectedEven = sample.Count(n => n % 2 == 0);
Console.WriteLine($"Правильна кількість парних чисел: {expectedEven}");

// Явно задаємо 4 потоки, щоб потоки змагалися за змінну навіть на машині з одним ядром
const int threads = 4;

// --- Сценарій А: спільний лічильник без синхронізації ---
Console.WriteLine("\n-- А. counter++ без синхронізації (5 запусків) --");
for (int run = 1; run <= 5; run++)
{
    int counter = 0;
    sample.AsParallel().WithDegreeOfParallelism(threads).ForAll(n =>
    {
        if (n % 2 == 0)
            counter++; // НЕпотокобезпечно: читання-зміна-запис не атомарні
    });
    Console.WriteLine($"   запуск {run}: {counter}  {(counter == expectedEven ? "" : $"(втрачено {expectedEven - counter})")}");
}

// --- Сценарій А': те саме, але "читання - зміна - запис" розтягнуте в часі ---
// На багатоядерному процесорі гонка в counter++ проявляється сама. На одноядерній машині потоки
// рідко переривають один одного саме всередині counter++, тому тут вікно гонки розширено явно:
// Thread.Yield() віддає процесор іншому потоку між читанням і записом.
List<int> smallSample = sample.Take(200_000).ToList();
int expectedSmall = smallSample.Count(n => n % 2 == 0);
Console.WriteLine($"\n-- А'. Розширене вікно гонки (200 000 елементів, очікується {expectedSmall}, 3 запуски) --");
for (int run = 1; run <= 3; run++)
{
    int counter = 0;
    smallSample.AsParallel().WithDegreeOfParallelism(threads).ForAll(n =>
    {
        if (n % 2 == 0)
        {
            int current = counter;   // читання
            Thread.Yield();          // інший потік може змінити counter саме тут
            counter = current + 1;   // запис: перезаписує чужі зміни
        }
    });
    Console.WriteLine($"   запуск {run}: {counter}  (втрачено {expectedSmall - counter})");
}

// --- Виправлення А1: Interlocked ---
Console.WriteLine("\n-- А1. Виправлення: Interlocked.Increment (5 запусків) --");
for (int run = 1; run <= 5; run++)
{
    int counter = 0;
    sample.AsParallel().WithDegreeOfParallelism(threads).ForAll(n =>
    {
        if (n % 2 == 0)
            Interlocked.Increment(ref counter);
    });
    Console.WriteLine($"   запуск {run}: {counter}  {(counter == expectedEven ? "правильно" : "ПОМИЛКА")}");
}

// --- Виправлення А2: lock ---
Console.WriteLine("\n-- А2. Виправлення: lock (5 запусків) --");
var gate = new object();
for (int run = 1; run <= 5; run++)
{
    int counter = 0;
    sample.AsParallel().WithDegreeOfParallelism(threads).ForAll(n =>
    {
        if (n % 2 == 0)
        {
            lock (gate)
            {
                counter++;
            }
        }
    });
    Console.WriteLine($"   запуск {run}: {counter}  {(counter == expectedEven ? "правильно" : "ПОМИЛКА")}");
}

// --- Найкраще виправлення: без спільного стану взагалі ---
int pure = sample.AsParallel().WithDegreeOfParallelism(threads).Count(n => n % 2 == 0);
Console.WriteLine($"\n-- А3. Найкраще: без побічних ефектів, Count(...) = {pure}  {(pure == expectedEven ? "правильно" : "ПОМИЛКА")}");

// --- Сценарій Б: додавання у звичайний List<int> з кількох потоків ---
Console.WriteLine("\n-- Б. List<int>.Add з кількох потоків (5 запусків) --");
for (int run = 1; run <= 5; run++)
{
    var results = new List<int>();
    try
    {
        sample.AsParallel().WithDegreeOfParallelism(threads).Where(n => n % 2 == 0).ForAll(n => results.Add(n));
        Console.WriteLine($"   запуск {run}: у списку {results.Count}, очікувалось {expectedEven}  {(results.Count == expectedEven ? "" : $"(втрачено {expectedEven - results.Count})")}");
    }
    catch (AggregateException ex)
    {
        Console.WriteLine($"   запуск {run}: виняток {ex.InnerException?.GetType().Name} (список зіпсовано)");
    }
}

Console.WriteLine("\n-- Б1. Виправлення: ConcurrentBag<int> (5 запусків) --");
for (int run = 1; run <= 5; run++)
{
    var bag = new ConcurrentBag<int>();
    sample.AsParallel().WithDegreeOfParallelism(threads).Where(n => n % 2 == 0).ForAll(n => bag.Add(n));
    Console.WriteLine($"   запуск {run}: у колекції {bag.Count}  {(bag.Count == expectedEven ? "правильно" : "ПОМИЛКА")}");
}

// --- Сценарій В: порядок результатів ---
Console.WriteLine("\n-- В. Порядок результатів --");
int[] small = Enumerable.Range(1, 20).ToArray();
var unordered = small.AsParallel().WithDegreeOfParallelism(threads).Select(n => n * 10).ToList();
var ordered = small.AsParallel().AsOrdered().WithDegreeOfParallelism(threads).Select(n => n * 10).ToList();
Console.WriteLine($"   AsParallel():            {string.Join(" ", unordered)}");
Console.WriteLine($"   AsParallel().AsOrdered(): {string.Join(" ", ordered)}");
Console.WriteLine($"   Порядок збережено без AsOrdered: {unordered.SequenceEqual(small.Select(n => n * 10))}");

// ============================== Допоміжні методи ==============================

static List<int> GenerateData(int count)
{
    var random = new Random(42);
    var list = new List<int>(count);
    for (int i = 0; i < count; i++)
        list.Add(random.Next(2, 1_000_000));
    return list;
}

// Обчислювально дорога перевірка: перебір дільників до кореня
static bool IsPrime(int n)
{
    if (n < 2) return false;
    if (n % 2 == 0) return n == 2;
    for (int i = 3; (long)i * i <= n; i += 2)
    {
        if (n % i == 0) return false;
    }
    return true;
}

static void Compare(string name, int size, List<int> data, Func<List<int>, long> linq, Func<List<int>, long> plinq)
{
    var sw1 = Stopwatch.StartNew();
    long linqResult = linq(data);
    sw1.Stop();

    var sw2 = Stopwatch.StartNew();
    long plinqResult = plinq(data);
    sw2.Stop();

    double speedup = (double)sw1.ElapsedMilliseconds / Math.Max(1, sw2.ElapsedMilliseconds);
    Console.WriteLine($"{name,-34} | {size,10:N0} | {sw1.ElapsedMilliseconds,9} | {sw2.ElapsedMilliseconds,10} | {speedup,10:F2}x | {(linqResult == plinqResult ? "так" : "НІ")}");
}
