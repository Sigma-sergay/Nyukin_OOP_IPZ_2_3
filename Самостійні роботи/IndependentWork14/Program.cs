// Самостійна робота №20. Реалізація exponential backoff (проєкт IndependentWork14).
// Виконав: Нюкін Сергій, ІПЗ-3/2.
//
// Обрані параметри: maxRetries = 4, initialDelay = 150 мс, jitterFactor = 0.2 (±20%).
// Сценарій: синхронізація кошика інтернет-магазину з сервером складу, який після
// перезапуску кілька секунд відповідає помилкою 503 (Service Unavailable).

using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var policy = new RetryPolicy(maxRetries: 4, initialDelay: TimeSpan.FromMilliseconds(150), jitterFactor: 0.2);

// Тимчасові помилки повторюємо, помилки в даних запиту — ні
Func<Exception, bool> isTransient = ex => ex is HttpRequestException or TimeoutException;

// ---------------------------------------------------------------------------
// Сценарій 1: склад "прокидається" — 3 невдачі, потім успіх
// ---------------------------------------------------------------------------
Console.WriteLine("=== Сценарій 1. Успіх після кількох спроб ===");
var warehouse = new WarehouseApi(failuresBeforeSuccess: 3);
int reserved = policy.Execute(() => warehouse.ReserveItems(cartId: 1042, items: 3), isTransient);
Console.WriteLine($"Результат: зарезервовано {reserved} товари(ів) для кошика #1042.\n");

// ---------------------------------------------------------------------------
// Сценарій 2: склад лежить повністю — політика вичерпує всі спроби
// ---------------------------------------------------------------------------
Console.WriteLine("=== Сценарій 2. Остаточна невдача після maxRetries ===");
var deadWarehouse = new WarehouseApi(failuresBeforeSuccess: int.MaxValue);
try
{
    policy.Execute(() => deadWarehouse.ReserveItems(cartId: 2077, items: 1), isTransient);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Main перехопив виняток після {deadWarehouse.Calls} викликів: {ex.Message}\n");
}

// ---------------------------------------------------------------------------
// Сценарій 3: shouldRetry = false — некоректний запит не повторюється
// ---------------------------------------------------------------------------
Console.WriteLine("=== Сценарій 3. shouldRetry відхиляє неперехідну помилку ===");
var api = new WarehouseApi(failuresBeforeSuccess: 0);
try
{
    policy.Execute(() => api.ReserveItems(cartId: 3001, items: -5), isTransient);
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine($"Main перехопив: {ex.GetType().Name}. Викликів API: {api.Calls} (повторів не було)\n");
}

// ---------------------------------------------------------------------------
// Сценарій 4: узагальненість — той самий RetryPolicy для операції з іншим типом T
// ---------------------------------------------------------------------------
Console.WriteLine("=== Сценарій 4. Execute<string> для іншої операції ===");
int attempts = 0;
string token = policy.Execute(() =>
{
    attempts++;
    if (attempts < 2) throw new TimeoutException("Auth-сервер не відповів за 2 с.");
    return "token-7f3a";
}, isTransient);
Console.WriteLine($"Результат: отримано токен '{token}'.\n");

Console.WriteLine("""
ВИСНОВОК:
Exponential backoff з jitter добре підходить для тимчасових збоїв (перезапуск сервісу, 503, таймаут):
паузи 150 → 300 → 600 → 1200 мс дають сервісу час відновитися, а випадкові ±20% розводять у часі
повтори від багатьох клієнтів. Межі патерну: він не допомагає при постійних помилках (некоректний
запит, немає прав), збільшує загальний час відповіді користувачу і небезпечний для неідемпотентних
операцій (повторне списання грошей) — тому shouldRetry має пропускати лише перехідні помилки.
""");

// ===========================================================================
// RetryPolicy
// ===========================================================================
public class RetryPolicy
{
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;
    private readonly double _jitterFactor;
    private readonly Random _random = new();

    /// <param name="maxRetries">Максимальна кількість ПОВТОРНИХ спроб (усього викликів — maxRetries + 1).</param>
    /// <param name="initialDelay">Початкова затримка для експоненційного backoff.</param>
    /// <param name="jitterFactor">Частка випадкового відхилення: 0.2 означає ±20% від розрахованої затримки.</param>
    public RetryPolicy(int maxRetries, TimeSpan initialDelay, double jitterFactor = 0.2)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxRetries);
        ArgumentOutOfRangeException.ThrowIfLessThan(initialDelay, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfNegative(jitterFactor);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(jitterFactor, 1.0);

        _maxRetries = maxRetries;
        _initialDelay = initialDelay;
        _jitterFactor = jitterFactor;
    }

    public T Execute<T>(Func<T> operation, Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentNullException.ThrowIfNull(operation);

        for (int retryAttempt = 0; ; retryAttempt++)
        {
            try
            {
                Console.WriteLine($"  Спроба {retryAttempt + 1}/{_maxRetries + 1}");
                T result = operation();
                Console.WriteLine($"  ✓ Успіх на спробі {retryAttempt + 1}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ {ex.GetType().Name}: {ex.Message}");

                if (shouldRetry != null && !shouldRetry(ex))
                {
                    Console.WriteLine("  shouldRetry = false → виняток передається далі без повторів");
                    throw;
                }

                if (retryAttempt >= _maxRetries)
                {
                    Console.WriteLine($"  Вичерпано {_maxRetries} повторних спроб → виняток передається далі");
                    throw;
                }

                TimeSpan delay = CalculateDelay(retryAttempt);
                Console.WriteLine($"  Затримка перед повтором: {delay.TotalMilliseconds:F0} мс " +
                                  $"(база {_initialDelay.TotalMilliseconds * Math.Pow(2, retryAttempt):F0} мс)");
                Thread.Sleep(delay);
            }
        }
    }

    /// <summary>
    /// delay = initialDelay * 2^retryAttempt * (1 + random.NextDouble() * jitterFactor * 2 - jitterFactor)
    /// </summary>
    private TimeSpan CalculateDelay(int retryAttempt)
    {
        double jitter = 1 + _random.NextDouble() * _jitterFactor * 2 - _jitterFactor;
        double ms = _initialDelay.TotalMilliseconds * Math.Pow(2, retryAttempt) * jitter;
        return TimeSpan.FromMilliseconds(ms);
    }
}

// ===========================================================================
// Імітація зовнішнього сервісу складу
// ===========================================================================
public class WarehouseApi
{
    private readonly int _failuresBeforeSuccess;

    public int Calls { get; private set; }

    public WarehouseApi(int failuresBeforeSuccess) => _failuresBeforeSuccess = failuresBeforeSuccess;

    public int ReserveItems(int cartId, int items)
    {
        Calls++;

        if (items <= 0)
            throw new ArgumentOutOfRangeException(nameof(items), "Кількість товарів має бути додатною.");

        if (Calls <= _failuresBeforeSuccess)
            throw new HttpRequestException($"503 Service Unavailable: склад перезапускається (кошик #{cartId}).");

        return items;
    }
}
