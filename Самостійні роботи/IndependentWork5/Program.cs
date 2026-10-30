using System.Text;
using IndependentWork5;

Console.OutputEncoding = Encoding.UTF8;

// ======================= Сценарій 1: System.Object та ToString() =======================
Console.WriteLine("=== Сценарій 1: List<object> і ToString() ===");

var laptop = new Product("Laptop", 35000m);
var olena = new Customer("Олена Коваль", "Рівне");
var order = new Order(1, olena, laptop, 2);

List<object> items = new()
{
    laptop,
    olena,
    order,
    new Warehouse(),   // без override: виклик піде в System.Object.ToString()
    42,                // вбудовані типи теж є object
    "рядок",
    DateTime.Parse("2026-10-22")
};

foreach (object item in items)
{
    // Тип змінної - object, але виконується ToString() реального типу
    Console.WriteLine($"{item.GetType().Name,-10} -> {item}");
}
Console.WriteLine();

// ======================= Сценарій 2: IEnumerable<string> =======================
Console.WriteLine("=== Сценарій 2: MyCustomCollection і foreach ===");

var collection = new MyCustomCollection();
collection.Add("яблуко");
collection.Add("груша");
collection.Add("слива");

foreach (string fruit in collection)
{
    Console.WriteLine($"  {fruit}");
}

// Один і той самий метод працює з будь-якою реалізацією IEnumerable<string>
string[] array = { "a", "b" };
List<string> list = new() { "x", "y", "z" };
PrintAll("MyCustomCollection", collection);
PrintAll("string[]", array);
PrintAll("List<string>", list);

// Бонус: LINQ працює з нашою колекцією, бо вона реалізує IEnumerable<string>
var withA = collection.Where(s => s.Contains('а')).ToList();
Console.WriteLine($"LINQ Where (містить 'а'): {string.Join(", ", withA)}");
Console.WriteLine();

// ======================= Сценарій 3: абстрактний клас Stream =======================
Console.WriteLine("=== Сценарій 3: ProcessStream(Stream) ===");

// Тестовий файл створюємо прямо в коді, щоб розв'язок був самодостатнім
const string fileName = "sample.txt";
File.WriteAllText(fileName, "Hello from file stream!");

using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
{
    ProcessStream(fileStream);
}

byte[] data = Encoding.UTF8.GetBytes("Hello from memory stream!");
using (var memoryStream = new MemoryStream(data))
{
    ProcessStream(memoryStream);
}

File.Delete(fileName);

// ---------------------------------------------------------------------------------------

static void PrintAll(string title, IEnumerable<string> source)
{
    Console.WriteLine($"  {title}: {string.Join(" | ", source)}");
}

// Приймає абстрактний базовий тип Stream: не знає, що саме за ним - файл чи пам'ять
static void ProcessStream(Stream stream)
{
    Console.WriteLine($"Тип потоку: {stream.GetType().Name}, CanRead = {stream.CanRead}, CanSeek = {stream.CanSeek}, Length = {stream.Length}");

    byte[] buffer = new byte[5];
    int read = stream.Read(buffer, 0, buffer.Length);
    Console.WriteLine($"  Прочитано {read} байтів: \"{Encoding.UTF8.GetString(buffer, 0, read)}\"");
}
