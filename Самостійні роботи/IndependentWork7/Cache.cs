namespace IndependentWork7;

/// <summary>
/// Узагальнений кеш обмеженої ємності з політикою FIFO (First In, First Out):
/// при переповненні видаляється елемент, який був доданий найпершим.
/// </summary>
/// <typeparam name="TKey">Тип ключа.</typeparam>
/// <typeparam name="TValue">Тип значення.</typeparam>
public class Cache<TKey, TValue> where TKey : notnull
{
    private readonly int _capacity;
    private readonly Dictionary<TKey, TValue> _cache;
    private readonly Queue<TKey> _keysOrder; // порядок додавання ключів: найстаріший на початку черги

    /// <summary>Поточна кількість елементів у кеші.</summary>
    public int Count => _cache.Count;

    public Cache(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Ємність кешу має бути додатною.");

        _capacity = capacity;
        _cache = new Dictionary<TKey, TValue>(capacity);
        _keysOrder = new Queue<TKey>(capacity);
    }

    /// <summary>
    /// Додає елемент. Якщо ключ уже є, значення оновлюється, а позиція в черзі витіснення не змінюється.
    /// Якщо кеш повний, спершу видаляється найстаріший елемент.
    /// </summary>
    public void Add(TKey key, TValue value)
    {
        if (_cache.ContainsKey(key))
        {
            _cache[key] = value; // порядок не змінюємо: це FIFO, а не LRU
            return;
        }

        if (_cache.Count >= _capacity)
        {
            TKey oldest = _keysOrder.Dequeue(); // найстаріший ключ
            _cache.Remove(oldest);
        }

        _cache.Add(key, value);
        _keysOrder.Enqueue(key);
    }

    /// <summary>Повертає значення за ключем або кидає KeyNotFoundException.</summary>
    public TValue Get(TKey key)
    {
        if (_cache.TryGetValue(key, out TValue? value))
            return value;

        throw new KeyNotFoundException($"Ключ '{key}' не знайдено в кеші.");
    }

    /// <summary>Перевіряє наявність ключа (не змінює порядок витіснення).</summary>
    public bool ContainsKey(TKey key)
    {
        return _cache.ContainsKey(key);
    }
}
