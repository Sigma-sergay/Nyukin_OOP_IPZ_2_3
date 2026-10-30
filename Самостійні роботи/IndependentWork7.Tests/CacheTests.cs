using IndependentWork7;
using Xunit;

namespace IndependentWork7.Tests;

public class CacheTests
{
    // ---------- Конструктор ----------

    [Fact]
    public void Constructor_ZeroCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Cache<int, string>(0));
    }

    [Fact]
    public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Cache<int, string>(-5));
    }

    // ---------- Додавання ----------

    [Fact]
    public void Add_NewItem_IncreasesCount()
    {
        var cache = new Cache<int, string>(3);

        cache.Add(1, "one");

        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void Add_SeveralItems_AllAreStored()
    {
        var cache = new Cache<int, string>(3);

        cache.Add(1, "one");
        cache.Add(2, "two");
        cache.Add(3, "three");

        Assert.Equal(3, cache.Count);
        Assert.True(cache.ContainsKey(1));
        Assert.True(cache.ContainsKey(2));
        Assert.True(cache.ContainsKey(3));
    }

    [Fact]
    public void Add_ExistingKey_UpdatesValueWithoutChangingCount()
    {
        var cache = new Cache<string, string>(3);
        cache.Add("k", "old");

        cache.Add("k", "new");

        Assert.Equal(1, cache.Count);
        Assert.Equal("new", cache.Get("k"));
    }

    // ---------- Отримання ----------

    [Fact]
    public void Get_ExistingKey_ReturnsStoredValue()
    {
        var cache = new Cache<string, string>(2);
        cache.Add("a", "Альфа");
        cache.Add("b", "Бета");

        Assert.Equal("Альфа", cache.Get("a"));
        Assert.Equal("Бета", cache.Get("b"));
    }

    [Fact]
    public void Get_MissingKey_ThrowsKeyNotFoundException()
    {
        var cache = new Cache<string, string>(2);
        cache.Add("a", "Альфа");

        Assert.Throws<KeyNotFoundException>(() => cache.Get("missing"));
    }

    [Fact]
    public void Get_OnEmptyCache_ThrowsKeyNotFoundException()
    {
        var cache = new Cache<int, int>(2);

        Assert.Throws<KeyNotFoundException>(() => cache.Get(1));
    }

    // ---------- ContainsKey ----------

    [Fact]
    public void ContainsKey_ExistingKey_ReturnsTrue()
    {
        var cache = new Cache<int, string>(2);
        cache.Add(7, "seven");

        Assert.True(cache.ContainsKey(7));
    }

    [Fact]
    public void ContainsKey_MissingKey_ReturnsFalse()
    {
        var cache = new Cache<int, string>(2);
        cache.Add(7, "seven");

        Assert.False(cache.ContainsKey(8));
    }

    // ---------- Count ----------

    [Fact]
    public void Count_NewCache_IsZero()
    {
        var cache = new Cache<int, string>(5);

        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void Count_NeverExceedsCapacity()
    {
        var cache = new Cache<int, int>(3);

        for (int i = 1; i <= 10; i++)
        {
            cache.Add(i, i * 10);
            Assert.True(cache.Count <= 3);
        }

        Assert.Equal(3, cache.Count);
    }

    // ---------- Політика FIFO ----------

    [Fact]
    public void Add_WhenFull_EvictsOldestItem()
    {
        var cache = new Cache<int, string>(3);
        cache.Add(1, "one");
        cache.Add(2, "two");
        cache.Add(3, "three");

        cache.Add(4, "four");

        Assert.False(cache.ContainsKey(1)); // найстаріший видалено
        Assert.Throws<KeyNotFoundException>(() => cache.Get(1));
    }

    [Fact]
    public void Add_WhenFull_KeepsOtherItemsAndNewOne()
    {
        var cache = new Cache<int, string>(3);
        cache.Add(1, "one");
        cache.Add(2, "two");
        cache.Add(3, "three");

        cache.Add(4, "four");

        Assert.True(cache.ContainsKey(2));
        Assert.True(cache.ContainsKey(3));
        Assert.True(cache.ContainsKey(4));
        Assert.Equal(3, cache.Count);
    }

    [Fact]
    public void Add_ManyItems_EvictsInInsertionOrder()
    {
        var cache = new Cache<int, int>(3);

        for (int i = 1; i <= 5; i++)
            cache.Add(i, i);

        // Залишилися три останні: 3, 4, 5
        Assert.False(cache.ContainsKey(1));
        Assert.False(cache.ContainsKey(2));
        Assert.True(cache.ContainsKey(3));
        Assert.True(cache.ContainsKey(4));
        Assert.True(cache.ContainsKey(5));
    }

    [Fact]
    public void Get_DoesNotRefreshItem_BecausePolicyIsFifoNotLru()
    {
        var cache = new Cache<int, string>(2);
        cache.Add(1, "one");
        cache.Add(2, "two");

        cache.Get(1);          // читання НЕ повинно «омолоджувати» елемент
        cache.Add(3, "three"); // тому витісняється все одно 1

        Assert.False(cache.ContainsKey(1));
        Assert.True(cache.ContainsKey(2));
        Assert.True(cache.ContainsKey(3));
    }

    [Fact]
    public void Add_ExistingKeyWhenFull_DoesNotEvictAnything()
    {
        var cache = new Cache<int, string>(2);
        cache.Add(1, "one");
        cache.Add(2, "two");

        cache.Add(1, "uno"); // оновлення наявного ключа

        Assert.Equal(2, cache.Count);
        Assert.True(cache.ContainsKey(1));
        Assert.True(cache.ContainsKey(2));
        Assert.Equal("uno", cache.Get(1));
    }

    [Fact]
    public void Add_UpdatedKey_KeepsItsOriginalPositionInQueue()
    {
        var cache = new Cache<int, string>(2);
        cache.Add(1, "one");
        cache.Add(2, "two");
        cache.Add(1, "uno"); // оновлення не змінює порядок

        cache.Add(3, "three"); // має піти ключ 1: він був доданий першим

        Assert.False(cache.ContainsKey(1));
        Assert.True(cache.ContainsKey(2));
        Assert.True(cache.ContainsKey(3));
    }

    [Fact]
    public void Capacity1_EachNewItemReplacesPrevious()
    {
        var cache = new Cache<string, string>(1);

        cache.Add("a", "1");
        cache.Add("b", "2");

        Assert.Equal(1, cache.Count);
        Assert.False(cache.ContainsKey("a"));
        Assert.Equal("2", cache.Get("b"));
    }
}
