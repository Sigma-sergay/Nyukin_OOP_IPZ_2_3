namespace IndependentWork2;

/// <summary>
/// Товар інтернет-магазину. Усі властивості доступні лише для читання.
/// </summary>
public class Product
{
    private readonly int _id;
    private readonly string _name;
    private readonly decimal _price;
    private readonly string _category;
    private readonly int _stockCount;

    public int Id { get { return _id; } }
    public string Name { get { return _name; } }
    public decimal Price { get { return _price; } }
    public string Category { get { return _category; } }
    public int StockCount { get { return _stockCount; } }

    // Конструктор 1 (основний): ініціалізує всі п'ять полів
    public Product(int id, string name, decimal price, string category, int stockCount)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва товару не може бути порожньою.", nameof(name));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Ціна не може бути від'ємною.");
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Категорія не може бути порожньою.", nameof(category));
        if (stockCount < 0)
            throw new ArgumentOutOfRangeException(nameof(stockCount), "Кількість на складі не може бути від'ємною.");

        _id = id;
        _name = name;
        _price = price;
        _category = category;
        _stockCount = stockCount;
    }

    // Конструктор 2 (швидке додавання): делегує основному через this(...)
    public Product(int id, string name, decimal price)
        : this(id, name, price, "Uncategorized", 0)
    {
    }

    // Конструктор 3 (копіювання): теж делегує основному, тож вся перевірка в одному місці
    public Product(Product other)
        : this(Guard(other).Id, other.Name, other.Price, other.Category, other.StockCount)
    {
    }

    // Перевірка на null до звернення до полів іншого об'єкта (аргументи this(...) обчислюються до тіла)
    private static Product Guard(Product? other)
    {
        return other ?? throw new ArgumentNullException(nameof(other));
    }

    public override string ToString()
    {
        return $"ID: {Id}, Name: {Name}, Price: {Price:C}, Category: {Category}, Stock: {StockCount}";
    }
}
