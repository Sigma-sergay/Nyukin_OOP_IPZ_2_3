// Самостійна робота №19. Дизайн винятків і політики retry: теорія та кейси Polly.
// Виконав: Нюкін Сергій, ІПЗ-3/2.
// Предметна область: інтернет-магазин (склад, курси валют, платежі, каталог).
// Звіт — у REPORT.md.

using System.Diagnostics;
using System.Text;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;

Console.OutputEncoding = Encoding.UTF8;
var clock = Stopwatch.StartNew();
void Log(string msg) => Console.WriteLine($"  [{clock.Elapsed.TotalSeconds,5:F2} c] {msg}");

// ===========================================================================
// Частина 1. Власний тип винятку InsufficientStockException
// ===========================================================================
Console.WriteLine("=== 1. Власний виняток InsufficientStockException ===");
var warehouse = new Warehouse(new Dictionary<string, int> { ["SKU-KB-01"] = 5, ["SKU-MS-02"] = 0 });

try
{
    warehouse.Reserve("SKU-KB-01", 2);
    Log("Зарезервовано 2 шт. SKU-KB-01");
    warehouse.Reserve("SKU-KB-01", 10);          // на складі лишилось 3 → виняток
}
catch (InsufficientStockException ex)
{
    Log($"Перехоплено {nameof(InsufficientStockException)}: {ex.Message}");
    Log($"  Sku={ex.Sku}, RequestedQuantity={ex.RequestedQuantity}, AvailableQuantity={ex.AvailableQuantity}");
}
finally
{
    Log("finally: блок завершено (тут звільняли б ресурси, наприклад транзакцію)");
}

// Обгортання низькорівневої помилки у власний виняток через innerException
try
{
    warehouse.ReserveFromLegacySystem("SKU-MS-02", 1);
}
catch (InsufficientStockException ex) when (ex.InnerException is KeyNotFoundException)
{
    Log($"Перехоплено з InnerException: {ex.Message} ← {ex.InnerException.GetType().Name}");
}

// Альтернатива: очікуваний неуспіх повертаємо як Result<T>, а не винятком
var check = warehouse.TryReserve("SKU-MS-02", 1);
Log(check.IsSuccess ? $"Зарезервовано: {check.Value}" : $"Result.Failure: {check.Error}");

// ===========================================================================
// Частина 2. Сценарій A — Retry: API курсів валют тимчасово недоступний
// ===========================================================================
Console.WriteLine();
Console.WriteLine("=== 2A. Retry (exponential backoff + jitter): API курсів валют ===");

ResiliencePipeline<decimal> ratePipeline = new ResiliencePipelineBuilder<decimal>()
    .AddRetry(new RetryStrategyOptions<decimal>
    {
        ShouldHandle = new PredicateBuilder<decimal>().Handle<HttpRequestException>(),
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(200),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        OnRetry = args =>
        {
            Log($"Retry #{args.AttemptNumber + 1} через {args.RetryDelay.TotalMilliseconds:F0} мс " +
                $"(причина: {args.Outcome.Exception?.Message})");
            return default;
        }
    })
    .Build();

var rates = new ExchangeRateApi(failuresBeforeSuccess: 2);
decimal usdRate = await ratePipeline.ExecuteAsync(async ct => await rates.GetUsdRateAsync(ct));
Log($"Курс USD отримано: {usdRate} грн (викликів API: {rates.Calls})");

// ===========================================================================
// Частина 2. Сценарій B — Circuit Breaker: платіжний шлюз «ліг»
// ===========================================================================
Console.WriteLine();
Console.WriteLine("=== 2B. Circuit Breaker: платіжний шлюз ===");

var gateway = new PaymentGateway(failuresBeforeRecovery: 3);

ResiliencePipeline<string> paymentPipeline = new ResiliencePipelineBuilder<string>()
    .AddCircuitBreaker(new CircuitBreakerStrategyOptions<string>
    {
        ShouldHandle = new PredicateBuilder<string>().Handle<HttpRequestException>(),
        FailureRatio = 0.5,                          // ≥50% невдач…
        MinimumThroughput = 3,                       // …серед щонайменше 3 викликів…
        SamplingDuration = TimeSpan.FromSeconds(10), // …за останні 10 с → розімкнути
        BreakDuration = TimeSpan.FromSeconds(1),     // скільки тримати ланцюг розімкнутим
        OnOpened = args => { Log($"⚡ Ланцюг РОЗІМКНЕНО на {args.BreakDuration.TotalSeconds:F0} c"); return default; },
        OnHalfOpened = _ => { Log("◐ Ланцюг напіввідкритий: пробний виклик"); return default; },
        OnClosed = _ => { Log("● Ланцюг ЗАМКНЕНО: шлюз знову працює"); return default; },
    })
    .Build();

for (int order = 1; order <= 7; order++)
{
    if (order == 6)
    {
        Log("…чекаємо 1.2 c, поки мине BreakDuration…");
        await Task.Delay(1200);
    }

    try
    {
        string receipt = await paymentPipeline.ExecuteAsync(async ct => await gateway.ChargeAsync(order, 499m, ct));
        Log($"Замовлення #{order}: оплачено, чек {receipt}");
    }
    catch (BrokenCircuitException)
    {
        Log($"Замовлення #{order}: шлюз НЕ викликався (fail-fast), запропонувати оплату пізніше");
    }
    catch (HttpRequestException ex)
    {
        Log($"Замовлення #{order}: помилка шлюзу — {ex.Message}");
    }
}
Log($"Реальних викликів шлюзу: {gateway.Calls} з 7 замовлень");

// ===========================================================================
// Частина 2. Сценарій C — Timeout + Fallback: повільний запит до каталогу
// ===========================================================================
Console.WriteLine();
Console.WriteLine("=== 2C. Timeout + Fallback: каталог товарів ===");

var cachedCatalog = new List<string> { "Клавіатура (кеш)", "Мишка (кеш)", "Монітор (кеш)" };

ResiliencePipeline<List<string>> catalogPipeline = new ResiliencePipelineBuilder<List<string>>()
    .AddFallback(new FallbackStrategyOptions<List<string>>
    {
        ShouldHandle = new PredicateBuilder<List<string>>().Handle<TimeoutRejectedException>(),
        FallbackAction = _ => Outcome.FromResultAsValueTask(cachedCatalog),
        OnFallback = _ => { Log("↩ Fallback: повертаємо каталог із кешу"); return default; }
    })
    .AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromMilliseconds(500),
        OnTimeout = args => { Log($"⏱ Запит перевищив {args.Timeout.TotalMilliseconds:F0} мс — скасовано"); return default; }
    })
    .Build();

var catalogDb = new CatalogDatabase();

var fast = await catalogPipeline.ExecuteAsync(async ct => await catalogDb.GetProductsAsync(TimeSpan.FromMilliseconds(100), ct));
Log("Швидкий запит: " + string.Join(", ", fast));

var slow = await catalogPipeline.ExecuteAsync(async ct => await catalogDb.GetProductsAsync(TimeSpan.FromSeconds(3), ct));
Log("Повільний запит: " + string.Join(", ", slow));

Console.WriteLine();
Console.WriteLine("Готово. Опис сценаріїв, обґрунтування стратегій і висновки — у REPORT.md.");

// ===========================================================================
// Власний виняток
// ===========================================================================
public class InsufficientStockException : Exception
{
    public string? Sku { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public InsufficientStockException() { }

    public InsufficientStockException(string message) : base(message) { }

    public InsufficientStockException(string message, Exception innerException) : base(message, innerException) { }

    public InsufficientStockException(string sku, int requested, int available)
        : base($"Недостатньо товару {sku}: запитано {requested}, доступно {available}.")
    {
        Sku = sku;
        RequestedQuantity = requested;
        AvailableQuantity = available;
    }
}

// ===========================================================================
// Result<T> для очікуваних неуспіхів
// ===========================================================================
public readonly record struct Result<T>(bool IsSuccess, T? Value, string? Error)
{
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

// ===========================================================================
// Імітації зовнішніх систем
// ===========================================================================
public class Warehouse(Dictionary<string, int> stock)
{
    public void Reserve(string sku, int quantity)
    {
        int available = stock.GetValueOrDefault(sku);
        if (quantity > available)
            throw new InsufficientStockException(sku, quantity, available);
        stock[sku] = available - quantity;
    }

    public void ReserveFromLegacySystem(string sku, int quantity)
    {
        try
        {
            var legacy = new Dictionary<string, int>();     // стара система не знає цього SKU
            _ = legacy[sku];
        }
        catch (KeyNotFoundException ex)
        {
            throw new InsufficientStockException($"Товар {sku} відсутній у старій складській системі.", ex);
        }
    }

    public Result<int> TryReserve(string sku, int quantity)
    {
        int available = stock.GetValueOrDefault(sku);
        if (quantity > available)
            return Result<int>.Failure($"товару {sku} немає в наявності (доступно {available})");
        stock[sku] = available - quantity;
        return Result<int>.Success(quantity);
    }
}

public class ExchangeRateApi(int failuresBeforeSuccess)
{
    public int Calls { get; private set; }

    public async Task<decimal> GetUsdRateAsync(CancellationToken ct)
    {
        Calls++;
        await Task.Delay(20, ct);
        if (Calls <= failuresBeforeSuccess)
            throw new HttpRequestException("503 від api.bank.example: сервіс перевантажений");
        return 41.37m;
    }
}

public class PaymentGateway(int failuresBeforeRecovery)
{
    public int Calls { get; private set; }

    public async Task<string> ChargeAsync(int orderId, decimal amount, CancellationToken ct)
    {
        Calls++;
        await Task.Delay(20, ct);
        if (Calls <= failuresBeforeRecovery)
            throw new HttpRequestException("502 Bad Gateway від платіжного провайдера");
        return $"PAY-{orderId:D4}-{amount:F0}";
    }
}

public class CatalogDatabase
{
    public async Task<List<string>> GetProductsAsync(TimeSpan queryDuration, CancellationToken ct)
    {
        await Task.Delay(queryDuration, ct);   // ct скасовує «запит» при таймауті
        return ["Клавіатура", "Мишка", "Монітор", "Навушники"];
    }
}
