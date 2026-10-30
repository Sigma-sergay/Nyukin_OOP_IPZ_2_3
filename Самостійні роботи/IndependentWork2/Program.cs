using System.Globalization;
using System.Text;
using IndependentWork2;

Console.OutputEncoding = Encoding.UTF8;

// Українська культура, щоб формат :C показував ціну у гривнях
CultureInfo.CurrentCulture = new CultureInfo("uk-UA");

Console.WriteLine("Створення товарів");

// Основний конструктор
var laptop = new Product(101, "Laptop", 35000m, "Electronics", 15);
Console.WriteLine($"Товар 1 (основний конструктор): {laptop}");

// Скорочений конструктор: категорія і залишок беруться за замовчуванням
var mouse = new Product(102, "Mouse", 800m);
Console.WriteLine($"Товар 2 (скорочений конструктор): {mouse}");

// Конструктор копіювання: спочатку оригінал, потім копія
var laptopCopy = new Product(laptop);
Console.WriteLine($"Товар 3 (конструктор копіювання): {laptopCopy}");

Console.WriteLine();
Console.WriteLine($"Копія - це окремий об'єкт: {!ReferenceEquals(laptop, laptopCopy)}");
