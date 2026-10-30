namespace IndependentWork11;

/// <summary>
/// Генерує тестові дані "Продажі". Фіксований seed робить результати відтворюваними.
/// </summary>
public static class DataGenerator
{
    private static readonly string[] Categories =
        { "Електроніка", "Побутова техніка", "Книги", "Одяг", "Спорт", "Іграшки", "Меблі", "Продукти" };

    private static readonly string[] Cities =
        { "Рівне", "Київ", "Львів", "Одеса", "Харків", "Дніпро", "Луцьк", "Тернопіль", "Житомир", "Вінниця" };

    private static readonly string[] FirstNames =
        { "Олена", "Іван", "Марія", "Андрій", "Софія", "Дмитро", "Анна", "Богдан", "Наталя", "Юрій" };

    private static readonly string[] LastNames =
        { "Коваль", "Шевченко", "Бондаренко", "Мельник", "Ткачук", "Кравченко", "Лисенко", "Мороз" };

    public static List<Product> GenerateProducts(Random random, int count = 100)
    {
        var products = new List<Product>(count);
        for (int i = 1; i <= count; i++)
        {
            string category = Categories[(i - 1) % Categories.Length];
            products.Add(new Product
            {
                Id = i,
                Name = $"{category} #{i}",
                Category = category,
                UnitPrice = Math.Round((decimal)(random.NextDouble() * 20000 + 50), 2)
            });
        }
        return products;
    }

    public static List<Customer> GenerateCustomers(Random random, int count = 80)
    {
        var customers = new List<Customer>(count);
        for (int i = 1; i <= count; i++)
        {
            customers.Add(new Customer
            {
                Id = i,
                Name = $"{FirstNames[random.Next(FirstNames.Length)]} {LastNames[random.Next(LastNames.Length)]}",
                City = Cities[random.Next(Cities.Length)]
            });
        }
        return customers;
    }

    public static List<Sale> GenerateSales(Random random, int productCount, int customerCount, int count = 1000)
    {
        var start = new DateTime(2024, 1, 1);
        int days = (new DateTime(2026, 10, 1) - start).Days;

        var sales = new List<Sale>(count);
        for (int i = 1; i <= count; i++)
        {
            sales.Add(new Sale
            {
                Id = i,
                ProductId = random.Next(1, productCount + 1),
                CustomerId = random.Next(1, customerCount + 1),
                Quantity = random.Next(1, 11),
                SaleDate = start.AddDays(random.Next(days))
            });
        }
        return sales;
    }
}
