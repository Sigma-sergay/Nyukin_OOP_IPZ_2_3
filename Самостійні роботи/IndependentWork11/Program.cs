using System.Diagnostics;
using System.Globalization;
using System.Text;
using IndependentWork11;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture; // однаковий формат чисел на будь-якій машині

// ---------- Підготовка даних (фіксований seed: результати відтворювані) ----------
var random = new Random(42);
List<Product> products = DataGenerator.GenerateProducts(random);
List<Customer> customers = DataGenerator.GenerateCustomers(random);
List<Sale> sales = DataGenerator.GenerateSales(random, products.Count, customers.Count);

Console.WriteLine($"Дані: {products.Count} продуктів, {customers.Count} клієнтів, {sales.Count} продажів");
Console.WriteLine();

// ---------- Пайплайн 1 ----------
Console.WriteLine("=== Пайплайн 1: найпопулярніші продукти за категоріями (топ-3) ===");
var top = Measure(() => Pipelines.TopProductsByCategory(sales, products), out TimeInfo t1);
foreach (var category in top.Take(3))
{
    Console.WriteLine($"{category.Category}:");
    foreach (var (name, qty) in category.Top)
        Console.WriteLine($"   {name}: {qty} шт.");
}
Console.WriteLine($"   ... усього категорій: {top.Count}");
PrintTime(t1);

// ---------- Пайплайн 2 ----------
Console.WriteLine("\n=== Пайплайн 2: загальний дохід за містами клієнтів ===");
var revenue = Measure(() => Pipelines.RevenueByCity(sales, products, customers), out TimeInfo t2);
foreach (var r in revenue)
    Console.WriteLine($"   {r.City,-12} {r.Revenue,14:N2} грн");
PrintTime(t2);

// ---------- Пайплайн 3 ----------
Console.WriteLine("\n=== Пайплайн 3: продажі за роками та місяцями ===");
var months = Measure(() => Pipelines.SalesByMonth(sales, products), out TimeInfo t3);
foreach (var m in months.Take(4))
    Console.WriteLine($"   {m.Year}-{m.Month:00}: {m.Units,4} шт., {m.Revenue,14:N2} грн");
Console.WriteLine("   ...");
foreach (var m in months.TakeLast(2))
    Console.WriteLine($"   {m.Year}-{m.Month:00}: {m.Units,4} шт., {m.Revenue,14:N2} грн");
Console.WriteLine($"   усього місяців: {months.Count}");
PrintTime(t3);

// ---------- Пайплайн 4 ----------
Console.WriteLine("\n=== Пайплайн 4 (власний): топ-5 клієнтів за витратами та їхня улюблена категорія ===");
var customersTop = Measure(() => Pipelines.TopCustomersWithFavoriteCategory(sales, products, customers), out TimeInfo t4);
foreach (var c in customersTop)
    Console.WriteLine($"   {c.Name,-20} {c.City,-10} {c.TotalSpent,12:N2} грн  улюблена категорія: {c.FavoriteCategory}");
PrintTime(t4);

// ---------- Перевірка коректності пайплайнів ручним підрахунком ----------
Console.WriteLine("\n=== Перевірка коректності (ручні цикли) ===");
decimal manualRevenue = 0;
int manualUnits = 0;
foreach (Sale sale in sales)
{
    foreach (Product product in products)
    {
        if (product.Id == sale.ProductId)
        {
            manualRevenue += sale.Quantity * product.UnitPrice;
            break;
        }
    }
    manualUnits += sale.Quantity;
}
Console.WriteLine($"Сума доходу за містами збігається з ручним підрахунком: {revenue.Sum(r => r.Revenue) == manualRevenue}");
Console.WriteLine($"Сума доходу за місяцями збігається з ручним підрахунком: {months.Sum(m => m.Revenue) == manualRevenue}");
Console.WriteLine($"Сума одиниць за місяцями збігається з ручним підрахунком: {months.Sum(m => m.Units) == manualUnits}");

// ---------- Допоміжні методи ----------

// Для кожного пайплайну створюється НОВИЙ Stopwatch.
// Перший виклик ("холодний") включає JIT-компіляцію, тому додатково міряємо "прогріте" виконання.
static T Measure<T>(Func<T> pipeline, out TimeInfo time)
{
    var coldWatch = Stopwatch.StartNew();
    T result = pipeline();
    coldWatch.Stop();

    const int runs = 200;
    var warmWatch = Stopwatch.StartNew();
    for (int i = 0; i < runs; i++)
        _ = pipeline();
    warmWatch.Stop();

    time = new TimeInfo(coldWatch.Elapsed.TotalMilliseconds, warmWatch.Elapsed.TotalMilliseconds / runs);
    return result;
}

static void PrintTime(TimeInfo t) =>
    Console.WriteLine($"   Час: перший запуск {t.ColdMs:F2} мс, у середньому після прогріву {t.WarmMs:F3} мс");

record TimeInfo(double ColdMs, double WarmMs);
