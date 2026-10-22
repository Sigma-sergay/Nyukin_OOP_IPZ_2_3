using System.Text;
using lab12v19;

Console.OutputEncoding = Encoding.UTF8;

// Працюємо через інтерфейс: код не залежить від конкретного сховища
IRepository<Notification> repo = new InMemoryRepository<Notification>();

Console.WriteLine("=== Add ===");
repo.Add(new Notification { Message = "Ласкаво просимо!", RecipientId = 1 });
repo.Add(new Notification { Message = "Нове повідомлення в чаті", RecipientId = 2 });
repo.Add(new Notification { Message = "Пароль змінено", RecipientId = 1 });
PrintAll(repo);

Console.WriteLine("\n=== GetById ===");
Console.WriteLine($"Id = 2: {repo.GetById(2)}");
Console.WriteLine($"Id = 99: {(repo.GetById(99) is null ? "не знайдено (null)" : "знайдено")}");

Console.WriteLine("\n=== Update ===");
var toUpdate = repo.GetById(2)!;
toUpdate.IsRead = true;
repo.Update(toUpdate);
Console.WriteLine($"Після оновлення: {repo.GetById(2)}");

Console.WriteLine("\n=== Delete ===");
repo.Delete(1);
PrintAll(repo);

Console.WriteLine("\n=== Неіснуючий елемент ===");
try
{
    repo.Delete(99);
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine($"Помилка: {ex.Message}");
}

static void PrintAll(IRepository<Notification> repo)
{
    foreach (var n in repo.GetAll())
        Console.WriteLine(n);
}
