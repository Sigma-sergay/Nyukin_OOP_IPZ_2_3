namespace IndependentWork11;

/// <summary>
/// LINQ-пайплайни аналітики. Кожен метод повертає матеріалізований результат (ToList),
/// щоб вимірювання часу охоплювало реальне виконання, а не лише побудову запиту.
/// </summary>
public static class Pipelines
{
    public record CategoryTop(string Category, List<(string Product, int Quantity)> Top);
    public record CityRevenue(string City, decimal Revenue);
    public record MonthStats(int Year, int Month, int Units, decimal Revenue);
    public record CustomerSummary(string Name, string City, decimal TotalSpent, string FavoriteCategory);

    /// <summary>
    /// Пайплайн 1: топ-3 найпопулярніших продукти (за кількістю) у кожній категорії.
    /// Оператори: GroupBy -> Select(Sum) -> Join -> GroupBy -> OrderByDescending/Take.
    /// </summary>
    public static List<CategoryTop> TopProductsByCategory(
        List<Sale> sales, List<Product> products)
    {
        return sales
            .GroupBy(s => s.ProductId)                                   // продажі за продуктами
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(s => s.Quantity) })
            .Join(products,                                              // додаємо назву й категорію
                  t => t.ProductId, p => p.Id,
                  (t, p) => new { p.Name, p.Category, t.Quantity })
            .GroupBy(x => x.Category)                                    // групуємо за категоріями
            .Select(g => new CategoryTop(
                g.Key,
                g.OrderByDescending(x => x.Quantity)                     // найпопулярніші зверху
                 .Take(3)
                 .Select(x => (x.Name, x.Quantity))
                 .ToList()))
            .OrderBy(c => c.Category)
            .ToList();
    }

    /// <summary>
    /// Пайплайн 2: загальний дохід за містами клієнтів.
    /// Оператори: Join + Join -> Select (Quantity * UnitPrice) -> GroupBy -> Sum -> OrderByDescending.
    /// </summary>
    public static List<CityRevenue> RevenueByCity(
        List<Sale> sales, List<Product> products, List<Customer> customers)
    {
        return sales
            .Join(products, s => s.ProductId, p => p.Id,
                  (s, p) => new { s.CustomerId, Revenue = s.Quantity * p.UnitPrice })
            .Join(customers, x => x.CustomerId, c => c.Id,
                  (x, c) => new { c.City, x.Revenue })
            .GroupBy(x => x.City)
            .Select(g => new CityRevenue(g.Key, g.Sum(x => x.Revenue)))
            .OrderByDescending(r => r.Revenue)
            .ToList();
    }

    /// <summary>
    /// Пайплайн 3: продажі за роком і місяцем (кількість одиниць та дохід).
    /// Оператори: Join -> GroupBy (складений ключ) -> Select (Sum) -> OrderBy/ThenBy.
    /// </summary>
    public static List<MonthStats> SalesByMonth(
        List<Sale> sales, List<Product> products)
    {
        return sales
            .Join(products, s => s.ProductId, p => p.Id,
                  (s, p) => new { s.SaleDate, s.Quantity, Revenue = s.Quantity * p.UnitPrice })
            .GroupBy(x => new { x.SaleDate.Year, x.SaleDate.Month })
            .Select(g => new MonthStats(
                g.Key.Year, g.Key.Month,
                g.Sum(x => x.Quantity),
                g.Sum(x => x.Revenue)))
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
    }

    /// <summary>
    /// Пайплайн 4 (власний): топ-5 клієнтів за витратами і їхня улюблена категорія.
    /// Оператори: Join + Join -> GroupBy -> Select із вкладеним GroupBy/OrderByDescending/First -> OrderByDescending/Take.
    /// </summary>
    public static List<CustomerSummary> TopCustomersWithFavoriteCategory(
        List<Sale> sales, List<Product> products, List<Customer> customers)
    {
        return sales
            .Join(products, s => s.ProductId, p => p.Id,
                  (s, p) => new { s.CustomerId, p.Category, s.Quantity, Revenue = s.Quantity * p.UnitPrice })
            .GroupBy(x => x.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                TotalSpent = g.Sum(x => x.Revenue),
                // улюблена категорія: та, де клієнт купив найбільше одиниць
                FavoriteCategory = g.GroupBy(x => x.Category)
                                    .OrderByDescending(cg => cg.Sum(x => x.Quantity))
                                    .ThenBy(cg => cg.Key)
                                    .First().Key
            })
            .Join(customers, t => t.CustomerId, c => c.Id,
                  (t, c) => new CustomerSummary(c.Name, c.City, t.TotalSpent, t.FavoriteCategory))
            .OrderByDescending(s => s.TotalSpent)
            .Take(5)
            .ToList();
    }
}
