namespace IndependentWork5;

public class Product
{
    public string Name { get; }
    public decimal Price { get; }

    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    // Перевизначаємо віртуальний метод System.Object.ToString()
    public override string ToString() => $"Product: {Name}, {Price:F2} грн";
}

public class Customer
{
    public string FullName { get; }
    public string City { get; }

    public Customer(string fullName, string city)
    {
        FullName = fullName;
        City = city;
    }

    public override string ToString() => $"Customer: {FullName} ({City})";
}

public class Order
{
    public int Number { get; }
    public Customer Customer { get; }
    public Product Product { get; }
    public int Quantity { get; }

    public Order(int number, Customer customer, Product product, int quantity)
    {
        Number = number;
        Customer = customer;
        Product = product;
        Quantity = quantity;
    }

    public decimal Total => Product.Price * Quantity;

    public override string ToString() =>
        $"Order #{Number}: {Quantity} x {Product.Name} для {Customer.FullName}, разом {Total:F2} грн";
}

/// <summary>
/// Клас БЕЗ перевизначення ToString(): для порівняння показує реалізацію з System.Object.
/// </summary>
public class Warehouse
{
    public string Location { get; set; } = "Рівне";
}
