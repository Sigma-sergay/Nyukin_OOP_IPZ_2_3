// Лабораторна робота №10. LINQ в дії: фільтри, join, агрегації. Продуктивність.
// Виконав: Нюкін Сергій, група ІПЗ-3/2, варіант 19.
// Сценарій: Управління товарами та їхніми відгуками.
//
// ЗВІТ
// 1. Класи сутностей:
//    Product (Id, Name, Price)                  - товар магазину;
//    Review  (Id, ProductId, Rating, Comment)   - відгук покупця на товар (оцінка 1..5).
//    Зв'язок: один Product -> багато Review (Review.ProductId = Product.Id).
// 2. Тестові дані: 15 товарів і 40 відгуків (метод SeedData).
//    Товар "USB-кабель" (Id = 15) навмисно без відгуків, щоб показати різницю Join і GroupJoin.
// 3. Для кожного запиту реалізовано синтаксис запитів (query syntax) і синтаксис методів
//    (method syntax), час кожного варіанту вимірюється через Stopwatch.
// 4. Висновки щодо продуктивності - в кінці файлу та виводяться в консоль.

using System.Diagnostics;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var (products, reviews) = SeedData();

Console.WriteLine($"Товарів: {products.Count}, відгуків: {reviews.Count}");
Console.WriteLine(new string('=', 70));

// ---------------------------------------------------------------------------
// Запит 1. Знайти всі продукти з середнім рейтингом > 4.0 (фільтрація + агрегація)
// ---------------------------------------------------------------------------
var q1Query = Measure("Запит 1 (query syntax)", () =>
    (from p in products
     join r in reviews on p.Id equals r.ProductId into pr
     where pr.Any()
     let avg = pr.Average(x => x.Rating)
     where avg > 4.0
     orderby avg descending
     select new { p.Name, Avg = avg }).ToList());

var q1Method = Measure("Запит 1 (method syntax)", () =>
    products
        .GroupJoin(reviews, p => p.Id, r => r.ProductId, (p, pr) => new { p.Name, Reviews = pr })
        .Where(x => x.Reviews.Any())
        .Select(x => new { x.Name, Avg = x.Reviews.Average(r => r.Rating) })
        .Where(x => x.Avg > 4.0)
        .OrderByDescending(x => x.Avg)
        .ToList());

Console.WriteLine("Продукти з середнім рейтингом > 4.0:");
foreach (var x in q1Method)
    Console.WriteLine($"  {x.Name,-25} {x.Avg:F2}");
Console.WriteLine(new string('-', 70));

// ---------------------------------------------------------------------------
// Запит 2. Об'єднати продукти з їхніми відгуками, вивести назву продукту та коментар (Join)
// ---------------------------------------------------------------------------
var q2Query = Measure("Запит 2 (query syntax)", () =>
    (from p in products
     join r in reviews on p.Id equals r.ProductId
     select new { Product = p.Name, r.Comment }).ToList());

var q2Method = Measure("Запит 2 (method syntax)", () =>
    products
        .Join(reviews, p => p.Id, r => r.ProductId, (p, r) => new { Product = p.Name, r.Comment })
        .ToList());

Console.WriteLine("Продукт - коментар (перші 10 з " + q2Method.Count + "):");
foreach (var x in q2Method.Take(10))
    Console.WriteLine($"  {x.Product,-25} \"{x.Comment}\"");
Console.WriteLine(new string('-', 70));

// ---------------------------------------------------------------------------
// Запит 3. Підрахувати середній рейтинг для кожного продукту (групування + агрегація)
// ---------------------------------------------------------------------------
var q3Query = Measure("Запит 3 (query syntax)", () =>
    (from r in reviews
     group r by r.ProductId into g
     join p in products on g.Key equals p.Id
     orderby p.Id
     select new { p.Name, Avg = g.Average(x => x.Rating), Count = g.Count() }).ToList());

var q3Method = Measure("Запит 3 (method syntax)", () =>
    reviews
        .GroupBy(r => r.ProductId)
        .Join(products, g => g.Key, p => p.Id,
              (g, p) => new { p.Id, p.Name, Avg = g.Average(x => x.Rating), Count = g.Count() })
        .OrderBy(x => x.Id)
        .Select(x => new { x.Name, x.Avg, x.Count })
        .ToList());

Console.WriteLine("Середній рейтинг кожного продукту:");
foreach (var x in q3Method)
    Console.WriteLine($"  {x.Name,-25} {x.Avg:F2}  (відгуків: {x.Count})");
Console.WriteLine(new string('-', 70));

// ---------------------------------------------------------------------------
// Запит 4. Знайти продукт з найнижчим середнім рейтингом
// ---------------------------------------------------------------------------
var q4Query = Measure("Запит 4 (query syntax)", () =>
    (from r in reviews
     group r by r.ProductId into g
     join p in products on g.Key equals p.Id
     let avg = g.Average(x => x.Rating)
     orderby avg
     select new { p.Name, Avg = avg }).First());

var q4Method = Measure("Запит 4 (method syntax, MinBy)", () =>
    reviews
        .GroupBy(r => r.ProductId)
        .Select(g => new { ProductId = g.Key, Avg = g.Average(x => x.Rating) })
        .MinBy(x => x.Avg) is { } min
            ? new { products.First(p => p.Id == min.ProductId).Name, min.Avg }
            : null);

Console.WriteLine($"Продукт з найнижчим середнім рейтингом: {q4Method!.Name} ({q4Method.Avg:F2})");
Console.WriteLine(new string('=', 70));

// Перевірка, що обидва синтаксиси дають однаковий результат
Console.WriteLine($"Результати збігаються: " +
    $"Q1={q1Query.Count == q1Method.Count}, Q2={q2Query.Count == q2Method.Count}, " +
    $"Q3={q3Query.Count == q3Method.Count}, Q4={q4Query.Name == q4Method.Name}");
Console.WriteLine();

// ---------------------------------------------------------------------------
// Аналіз продуктивності на більшому обсязі (10 000 товарів, 200 000 відгуків),
// щоб різниця між операціями була помітна, а не тонула в похибці таймера.
// ---------------------------------------------------------------------------
Console.WriteLine("Аналіз продуктивності на великому наборі (10 000 товарів / 200 000 відгуків):");
var (bigP, bigR) = GenerateBig(10_000, 200_000);

Measure("  Where (фільтр Rating == 5)", () => bigR.Where(r => r.Rating == 5).ToList());
Measure("  Join (продукт + відгук)", () =>
    bigP.Join(bigR, p => p.Id, r => r.ProductId, (p, r) => new { p.Name, r.Comment }).ToList());
Measure("  GroupBy + Average", () =>
    bigR.GroupBy(r => r.ProductId).Select(g => new { g.Key, Avg = g.Average(x => x.Rating) }).ToList());
Measure("  MinBy (після GroupBy)", () =>
    bigR.GroupBy(r => r.ProductId).Select(g => new { g.Key, Avg = g.Average(x => x.Rating) }).MinBy(x => x.Avg));
Measure("  OrderBy + First (після GroupBy)", () =>
    bigR.GroupBy(r => r.ProductId).Select(g => new { g.Key, Avg = g.Average(x => x.Rating) }).OrderBy(x => x.Avg).First());
Measure("  Вкладений Where замість Join (погано)", () =>
    bigP.Take(1000).Select(p => new { p.Name, Cnt = bigR.Count(r => r.ProductId == p.Id) }).ToList());

Console.WriteLine();
Console.WriteLine("""
ВИСНОВКИ:
 1. Синтаксис запитів і синтаксис методів компілюються в однакові виклики методів,
    тому їхній час практично однаковий; різниця в мілісекундах - це похибка і "прогрів" JIT
    (перший виміряний запит завжди повільніший).
 2. Найдешевша операція - Where: один прохід по колекції, O(n).
 3. Join працює через хеш-таблицю (Lookup) по ключу, тому O(n + m), а не O(n * m).
    Вкладений Where/Count замість Join дає O(n * m) і на великих даних у рази повільніший.
 4. Серед правильно написаних запитів найдорожчими виявились Join і GroupBy + Average:
    Join створює новий об'єкт на кожну пару (200 000 рядків результату), а GroupBy будує
    групи й проходить по кожній з них, тож обидва виділяють багато пам'яті.
 5. MinBy швидший за OrderBy + First, бо робить один прохід O(n) замість сортування O(n log n).
 6. Через відкладене виконання запит реально виконується тільки на ToList()/First()/MinBy(),
    тому Stopwatch треба зупиняти після материалізації результату, інакше виміряється 0 мс.
""");

// ===========================================================================
// Допоміжні методи
// ===========================================================================

static T Measure<T>(string title, Func<T> query)
{
    var sw = Stopwatch.StartNew();
    T result = query();            // материалізація результату всередині вимірювання
    sw.Stop();
    Console.WriteLine($"[{title}] час: {sw.Elapsed.TotalMilliseconds:F4} мс");
    return result;
}

static (List<Product>, List<Review>) SeedData()
{
    var products = new List<Product>
    {
        new(1,  "Ноутбук Lenovo IdeaPad", 24999m),
        new(2,  "Смартфон Samsung A55",   15499m),
        new(3,  "Навушники JBL Tune",      1899m),
        new(4,  "Мишка Logitech G102",      999m),
        new(5,  "Клавіатура HyperX",       2799m),
        new(6,  "Монітор Dell 24\"",       6499m),
        new(7,  "Павербанк Xiaomi",        1199m),
        new(8,  "Смарт-годинник Amazfit",  3999m),
        new(9,  "Колонка Sony SRS",        2499m),
        new(10, "Роутер TP-Link",          1599m),
        new(11, "Вебкамера Logitech",      2199m),
        new(12, "SSD Kingston 1TB",        2899m),
        new(13, "Планшет Apple iPad",     17999m),
        new(14, "Електронна книга Pocket", 5499m),
        new(15, "USB-кабель",               199m),   // без відгуків
    };

    var reviews = new List<Review>
    {
        new(1,  1, 5, "Швидкий, для навчання ідеально"),
        new(2,  1, 4, "Добрий, але гріється"),
        new(3,  1, 5, "Рекомендую"),
        new(4,  2, 4, "Гарний екран"),
        new(5,  2, 3, "Батарея могла б бути кращою"),
        new(6,  2, 4, "Нормальний за свої гроші"),
        new(7,  3, 2, "Зламались через місяць"),
        new(8,  3, 3, "Звук так собі"),
        new(9,  3, 2, "Тиснуть на вуха"),
        new(10, 4, 5, "Найкраща бюджетна мишка"),
        new(11, 4, 5, "Легка і точна"),
        new(12, 4, 4, "Підсвітка класна"),
        new(13, 5, 5, "Перемикачі приємні"),
        new(14, 5, 4, "Трохи гучна"),
        new(15, 6, 4, "Чітка картинка"),
        new(16, 6, 5, "Відмінна передача кольору"),
        new(17, 6, 4, "Підставка хитається"),
        new(18, 7, 3, "Заряджає повільно"),
        new(19, 7, 4, "Компактний"),
        new(20, 8, 5, "Батарея на два тижні"),
        new(21, 8, 4, "Зручний"),
        new(22, 8, 5, "Точний пульсометр"),
        new(23, 9, 3, "Басу мало"),
        new(24, 9, 4, "Гучна для свого розміру"),
        new(25, 10, 2, "Постійно відвалюється Wi-Fi"),
        new(26, 10, 3, "Після прошивки краще"),
        new(27, 11, 4, "Норм для дистанційки"),
        new(28, 11, 4, "Гарний мікрофон"),
        new(29, 12, 5, "Дуже швидкий"),
        new(30, 12, 5, "Система вантажиться за секунди"),
        new(31, 12, 4, "Ціна/якість"),
        new(32, 13, 5, "Ідеальний для конспектів"),
        new(33, 13, 5, "Екран супер"),
        new(34, 13, 4, "Дорогий, але вартий"),
        new(35, 14, 4, "Очі не втомлюються"),
        new(36, 14, 5, "Читаю щодня"),
        new(37, 3, 1, "Лівий навушник не працює"),
        new(38, 10, 2, "Слабкий сигнал"),
        new(39, 1, 4, "Клавіатура зручна"),
        new(40, 5, 5, "LINQ-код пишеться швидше :)"),
    };

    return (products, reviews);
}

static (List<Product>, List<Review>) GenerateBig(int productCount, int reviewCount)
{
    var rnd = new Random(42);
    var p = Enumerable.Range(1, productCount)
        .Select(i => new Product(i, $"Product {i}", rnd.Next(100, 50_000)))
        .ToList();
    var r = Enumerable.Range(1, reviewCount)
        .Select(i => new Review(i, rnd.Next(1, productCount + 1), rnd.Next(1, 6), $"Comment {i}"))
        .ToList();
    return (p, r);
}

// ===========================================================================
// Класи сутностей
// ===========================================================================

public record Product(int Id, string Name, decimal Price);

public record Review(int Id, int ProductId, int Rating, string Comment);
