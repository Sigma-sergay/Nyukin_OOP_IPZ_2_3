using System.Text;
using lab6v19;

Console.OutputEncoding = Encoding.UTF8;

// Об'єкти базового та похідних класів
var basic = new Account("guest", "hash_guest");
var user = new UserAccount("olena", "hash_olena", "olena@example.com");
var admin = new AdminAccount("root", "hash_root", 5);

// 1. Поліморфізм: виклик override-методу через посилання БАЗОВОГО типу
Console.WriteLine("=== 1. Поліморфізм (virtual / override) ===");
List<Account> accounts = new() { basic, user, admin };

foreach (Account acc in accounts)
{
    // Тип посилання — Account, але виконується метод РЕАЛЬНОГО типу об'єкта
    acc.Login();
    Console.WriteLine();
}

// 2. Власні методи похідних класів
Console.WriteLine("=== 2. Унікальні методи похідних класів ===");
user.ChangePassword("new_hash_olena");
admin.GrantPermissions(user, "редагування контенту");
Console.WriteLine();

// 3. Різниця між override та new
Console.WriteLine("=== 3. Різниця між override та new ===");

// Один і той самий об'єкт UserAccount, але два різних посилання
UserAccount userRef = user;
Account baseRef = user;

Console.WriteLine("-- Метод GetAccountType() (приховування через new) --");
Console.WriteLine($"Через UserAccount-посилання: {userRef.GetAccountType()}");
Console.WriteLine($"Через Account-посилання:     {baseRef.GetAccountType()}");
Console.WriteLine("Результати РІЗНІ: при new метод обирається за типом посилання.");
Console.WriteLine();

Console.WriteLine("-- Метод Login() (перевизначення через override) --");
Console.WriteLine("Через UserAccount-посилання:");
userRef.Login();
Console.WriteLine("Через Account-посилання:");
baseRef.Login();
Console.WriteLine("Результати ОДНАКОВІ: при override метод обирається за типом об'єкта.");
Console.WriteLine();

// AdminAccount не ховає GetAccountType(), тому там завжди виклик версії базового класу
Console.WriteLine("-- AdminAccount не використовує new --");
Console.WriteLine($"admin.GetAccountType(): {admin.GetAccountType()}");
