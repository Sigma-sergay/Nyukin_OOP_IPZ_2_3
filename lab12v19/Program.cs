// Лабораторна робота №12. Обробка IO/мережевих помилок та патерн Retry.
// Виконав: Нюкін Сергій, група ІПЗ-3/2, варіант 19.
// Сценарій: Читання даних з бази даних (імітація).
//
//   FileProcessor.ReadDatabaseDump(path)          -> FileNotFoundException перші 2 рази, потім успіх;
//   NetworkClient.QueryDatabase(connection, query) -> HttpRequestException перші 3 рази, потім успіх;
//   shouldRetry: повторювати лише для FileNotFoundException та HttpRequestException.

using System.Diagnostics;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var delay = TimeSpan.FromMilliseconds(200);   // початкова затримка; далі 200, 400, 800 мс

// ---------------------------------------------------------------------------
// Делегат shouldRetry для варіанту 19
// ---------------------------------------------------------------------------
Func<Exception, bool> shouldRetry = ex => ex is FileNotFoundException or HttpRequestException;

// ---------------------------------------------------------------------------
// Сценарій 1. FileProcessor: дамп БД "знаходиться" лише з 3-ї спроби
// ---------------------------------------------------------------------------
Console.WriteLine("=== Сценарій 1. FileProcessor.ReadDatabaseDump ===");
var fileProcessor = new FileProcessor(failuresBeforeSuccess: 2);

var dump = RetryHelper.ExecuteWithRetry(
    () => fileProcessor.ReadDatabaseDump("backup/shop_db_dump.sql"),
    retryCount: 3,
    initialDelay: delay,
    shouldRetry: shouldRetry);

Console.WriteLine($"Результат: прочитано {dump.Count} рядків дампу:");
dump.ForEach(line => Console.WriteLine("    " + line));

// ---------------------------------------------------------------------------
// Сценарій 2. NetworkClient: запит до БД проходить лише з 4-ї спроби
// ---------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("=== Сценарій 2. NetworkClient.QueryDatabase ===");
var networkClient = new NetworkClient(failuresBeforeSuccess: 3);

var rows = RetryHelper.ExecuteWithRetry(
    () => networkClient.QueryDatabase("Server=db.rfkit.local;Database=Shop", "SELECT Name FROM Customers"),
    retryCount: 3,
    initialDelay: delay,
    shouldRetry: shouldRetry);

Console.WriteLine($"Результат: отримано {rows.Count} записів:");
rows.ForEach(r => Console.WriteLine("    " + r));

// ---------------------------------------------------------------------------
// Сценарій 3. Вибіркова обробка: UnauthorizedAccessException НЕ повторюється
// (немає прав доступу — повтор нічого не змінить), виняток одразу йде вище.
// ---------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("=== Сценарій 3. shouldRetry: виняток, який не варто повторювати ===");
var lockedFile = new FileProcessor(failuresBeforeSuccess: 0, accessDenied: true);
try
{
    RetryHelper.ExecuteWithRetry(
        () => lockedFile.ReadDatabaseDump("/root/secret_dump.sql"),
        retryCount: 3,
        initialDelay: delay,
        shouldRetry: shouldRetry);
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"Перехоплено в Main: {ex.GetType().Name}: {ex.Message}");
    Console.WriteLine($"Кількість викликів ReadDatabaseDump: {lockedFile.Calls} (повторів не було)");
}

// ---------------------------------------------------------------------------
// Сценарій 4. Спроби вичерпано: мережа "лежить" довше, ніж дозволяє retryCount
// ---------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("=== Сценарій 4. Вичерпано всі спроби ===");
var deadNetwork = new NetworkClient(failuresBeforeSuccess: 10);
try
{
    RetryHelper.ExecuteWithRetry(
        () => deadNetwork.QueryDatabase("Server=offline", "SELECT 1"),
        retryCount: 2,
        initialDelay: TimeSpan.FromMilliseconds(100),
        shouldRetry: shouldRetry);
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Перехоплено в Main після {deadNetwork.Calls} спроб: {ex.Message}");
}

Console.WriteLine();
Console.WriteLine("""
ВИСНОВОК (експоненційна затримка, варіант 19):
Затримка перед повтором обчислюється як initialDelay * 2^(номер повтору - 1), тобто 200 → 400 → 800 мс.
Для мережевого запиту до БД, який падає 3 рази, сумарне очікування ≈ 1,4 с: короткі збої
проходять швидко, а при довшій недоступності сервер не "засипається" запитами, бо паузи ростуть.
""");

// ===========================================================================
// RetryHelper
// ===========================================================================
public static class RetryHelper
{
    /// <summary>
    /// Виконує operation; при винятку, для якого shouldRetry повертає true, повторює
    /// до retryCount разів з експоненційною затримкою initialDelay * 2^(attempt-1).
    /// </summary>
    /// <param name="operation">Операція, яку потрібно виконати.</param>
    /// <param name="retryCount">Максимальна кількість ПОВТОРНИХ спроб (усього спроб = retryCount + 1).</param>
    /// <param name="initialDelay">Затримка перед першим повтором.</param>
    /// <param name="shouldRetry">Чи повторювати для цього винятку; null — повторювати для будь-якого.</param>
    public static T ExecuteWithRetry<T>(
        Func<T> operation,
        int retryCount = 3,
        TimeSpan initialDelay = default,
        Func<Exception, bool>? shouldRetry = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentOutOfRangeException.ThrowIfNegative(retryCount);

        int totalAttempts = retryCount + 1;

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                Log($"Спроба {attempt}/{totalAttempts}...");
                T result = operation();
                Log($"Спроба {attempt}: успіх.");
                return result;
            }
            catch (Exception ex)
            {
                Log($"Спроба {attempt}: невдача — {ex.GetType().Name}: {ex.Message}");

                if (shouldRetry != null && !shouldRetry(ex))
                {
                    Log("shouldRetry = false: цей тип помилки не повторюємо.");
                    throw;
                }

                if (attempt >= totalAttempts)
                {
                    Log($"Вичерпано {retryCount} повторних спроб.");
                    throw;
                }

                // Експоненційна затримка: initialDelay * 2^(attempt - 1)
                var wait = TimeSpan.FromMilliseconds(initialDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
                Log($"Повтор через {wait.TotalMilliseconds:F0} мс.");
                Thread.Sleep(wait);
            }
        }
    }

    private static readonly Stopwatch Clock = Stopwatch.StartNew();

    private static void Log(string message) =>
        Console.WriteLine($"  [{Clock.Elapsed.TotalSeconds,6:F2} c] {message}");
}

// ===========================================================================
// FileProcessor — імітує читання дампу бази даних з файлу
// ===========================================================================
public class FileProcessor
{
    private readonly int _failuresBeforeSuccess;
    private readonly bool _accessDenied;

    public int Calls { get; private set; }

    public FileProcessor(int failuresBeforeSuccess, bool accessDenied = false)
    {
        _failuresBeforeSuccess = failuresBeforeSuccess;
        _accessDenied = accessDenied;
    }

    public List<string> ReadDatabaseDump(string path)
    {
        Calls++;

        if (_accessDenied)
            throw new UnauthorizedAccessException($"Немає прав на читання '{path}'.");

        if (Calls <= _failuresBeforeSuccess)
            throw new FileNotFoundException($"Файл дампу '{path}' ще не створено (бекап у процесі).", path);

        return new List<string>
        {
            "CREATE TABLE Customers (Id INT, Name NVARCHAR(100));",
            "INSERT INTO Customers VALUES (1, 'Олена');",
            "INSERT INTO Customers VALUES (2, 'Максим');",
            "INSERT INTO Customers VALUES (3, 'Ірина');",
        };
    }
}

// ===========================================================================
// NetworkClient — імітує мережевий запит до сервера бази даних
// ===========================================================================
public class NetworkClient
{
    private readonly int _failuresBeforeSuccess;

    public int Calls { get; private set; }

    public NetworkClient(int failuresBeforeSuccess) => _failuresBeforeSuccess = failuresBeforeSuccess;

    public List<string> QueryDatabase(string connectionString, string query)
    {
        Calls++;

        if (Calls <= _failuresBeforeSuccess)
            throw new HttpRequestException($"Сервер БД недоступний ({connectionString}): таймаут з'єднання.");

        return new List<string> { "Олена", "Максим", "Ірина", "Данило" };
    }
}
