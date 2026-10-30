using System.Collections;

namespace IndependentWork5;

/// <summary>
/// Власна колекція рядків. Реалізація IEnumerable&lt;string&gt; дозволяє перебирати її через foreach.
/// </summary>
public class MyCustomCollection : IEnumerable<string>
{
    private readonly List<string> _items = new();

    public int Count => _items.Count;

    public void Add(string item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    // Узагальнений варіант, який використовує foreach і LINQ
    public IEnumerator<string> GetEnumerator()
    {
        foreach (string item in _items)
        {
            yield return item;
        }
    }

    // Неузагальнений варіант, вимагається інтерфейсом IEnumerable (який успадковує IEnumerable<T>)
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
