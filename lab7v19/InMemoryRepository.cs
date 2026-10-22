namespace lab12v19;

/// <summary>
/// Реалізація IRepository&lt;T&gt;, що зберігає дані в пам'яті (List&lt;T&gt;).
/// </summary>
public class InMemoryRepository<T> : IRepository<T> where T : IEntity
{
    private readonly List<T> _items = new();
    private int _nextId = 1;

    public T? GetById(int id)
    {
        // FirstOrDefault повертає default(T), якщо елемента немає
        return _items.FirstOrDefault(x => x.Id == id);
    }

    public IEnumerable<T> GetAll()
    {
        // Повертаємо копію, щоб зовнішній код не міг змінити внутрішній список
        return _items.ToList();
    }

    public void Add(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (item.Id == 0)
        {
            // Автоматично призначаємо вільний Id
            while (_items.Any(x => x.Id == _nextId))
                _nextId++;
            item.Id = _nextId++;
        }
        else if (_items.Any(x => x.Id == item.Id))
        {
            throw new ArgumentException($"Елемент з Id = {item.Id} уже існує.", nameof(item));
        }

        _items.Add(item);
    }

    public void Update(T item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        int index = _items.FindIndex(x => x.Id == item.Id);
        if (index < 0)
            throw new KeyNotFoundException($"Елемент з Id = {item.Id} не знайдено.");

        _items[index] = item;
    }

    public void Delete(int id)
    {
        int index = _items.FindIndex(x => x.Id == id);
        if (index < 0)
            throw new KeyNotFoundException($"Елемент з Id = {id} не знайдено.");

        _items.RemoveAt(index);
    }
}
