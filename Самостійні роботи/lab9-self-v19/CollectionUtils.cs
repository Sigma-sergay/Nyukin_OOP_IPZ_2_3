namespace lab9_self_v19;

/// <summary>
/// Власні узагальнені утиліти для колекцій на основі делегатів.
/// </summary>
public static class CollectionUtils
{
    /// <summary>
    /// Перетворює кожен елемент колекції за допомогою mapper (аналог LINQ Select).
    /// </summary>
    public static IEnumerable<TResult> Map<TSource, TResult>(
        IEnumerable<TSource> collection,
        Func<TSource, TResult> mapper)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(mapper);

        return MapIterator(collection, mapper);
    }

    // Перевірка аргументів у зовнішньому методі виконується одразу,
    // а сам ітератор (yield) працює відкладено, при переборі результату
    private static IEnumerable<TResult> MapIterator<TSource, TResult>(
        IEnumerable<TSource> collection,
        Func<TSource, TResult> mapper)
    {
        foreach (TSource item in collection)
        {
            yield return mapper(item);
        }
    }

    /// <summary>
    /// Знаходить елемент з максимальним значенням ключа (аналог LINQ MaxBy).
    /// Для порожньої колекції кидає InvalidOperationException.
    /// </summary>
    public static T MaxBy<T, TKey>(
        IEnumerable<T> collection,
        Func<T, TKey> keySelector)
        where TKey : IComparable<TKey>
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(keySelector);

        bool first = true;
        T maxItem = default!;
        TKey maxKey = default!;

        foreach (T item in collection)
        {
            TKey key = keySelector(item);
            if (first || key.CompareTo(maxKey) > 0)
            {
                maxItem = item;
                maxKey = key;
                first = false;
            }
        }

        if (first)
            throw new InvalidOperationException("Колекція не містить жодного елемента.");

        return maxItem;
    }

    /// <summary>
    /// Повертає лише ті елементи, що задовольняють умову (аналог LINQ Where).
    /// </summary>
    public static IEnumerable<T> Filter<T>(
        IEnumerable<T> collection,
        Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(predicate);

        return FilterIterator(collection, predicate);
    }

    private static IEnumerable<T> FilterIterator<T>(
        IEnumerable<T> collection,
        Predicate<T> predicate)
    {
        foreach (T item in collection)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Виконує дію для кожного елемента колекції (у LINQ немає, але є List&lt;T&gt;.ForEach).
    /// </summary>
    public static void ForEach<T>(
        IEnumerable<T> collection,
        Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(action);

        foreach (T item in collection)
        {
            action(item);
        }
    }
}
