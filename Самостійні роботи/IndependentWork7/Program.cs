using System.Text;
using IndependentWork7;

Console.OutputEncoding = Encoding.UTF8;

var cache = new Cache<string, string>(3);
Console.WriteLine("Створено кеш ємністю 3 (політика FIFO)");

cache.Add("a", "Альфа");
cache.Add("b", "Бета");
cache.Add("c", "Гамма");
Console.WriteLine($"Додано a, b, c. Count = {cache.Count}");
Console.WriteLine($"Get(\"a\") = {cache.Get("a")}");

Console.WriteLine("\nДодаємо ключ d: кеш повний, найстаріший (a) має зникнути");
cache.Add("d", "Дельта");
Console.WriteLine($"Count = {cache.Count}");
foreach (string key in new[] { "a", "b", "c", "d" })
{
    Console.WriteLine($"  ContainsKey(\"{key}\") = {cache.ContainsKey(key)}");
}

Console.WriteLine("\nДодаємо ключ e: тепер має зникнути b");
cache.Add("e", "Епсилон");
foreach (string key in new[] { "b", "c", "d", "e" })
{
    Console.WriteLine($"  ContainsKey(\"{key}\") = {cache.ContainsKey(key)}");
}

Console.WriteLine("\nСпроба отримати видалений ключ:");
try
{
    cache.Get("a");
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine($"  KeyNotFoundException: {ex.Message}");
}
