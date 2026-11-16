using System.Text;
using lab13v19;

Console.OutputEncoding = Encoding.UTF8;
var service = new UserProfileService();

Console.WriteLine("=== Лабораторна 13, варіант 19: обробка даних користувача ===");

// ---------- 1. Методи, що кидають власні винятки ----------
Console.WriteLine("\n--- UpdateProfile / GetProfile: власні винятки + try-catch ---");

TryUpdate("Коректне оновлення", new UserProfile
{ Id = 1, UserName = "serhii_n", Email = "serhii@example.com", Age = 18, Phone = "+380671112233" });

TryUpdate("Порожнє ім'я", new UserProfile
{ Id = 2, UserName = "", Email = "olena@example.com", Age = 19, Phone = "+380502223344" });

TryUpdate("Email без домену", new UserProfile
{ Id = 2, UserName = "olena", Email = "olena@example", Age = 19, Phone = "+380502223344" });

TryUpdate("Вік 7", new UserProfile
{ Id = 3, UserName = "maksym", Email = "maksym@example.com", Age = 7, Phone = "+380933334455" });

TryUpdate("Телефон не в форматі", new UserProfile
{ Id = 3, UserName = "maksym", Email = "maksym@example.com", Age = 20, Phone = "0933334455" });

TryUpdate("Неіснуючий користувач", new UserProfile
{ Id = 42, UserName = "ghost", Email = "ghost@example.com", Age = 30, Phone = "+380991112233" });

// ---------- 2. Метод, що повертає Result<T> ----------
Console.WriteLine("\n--- ChangeEmail: Result<bool> через IsSuccess / Error ---");

ShowChange(1, "serhii.n@example.com");
ShowChange(2, "maksym@example.com");
ShowChange(3, "maksym@example.com");
ShowChange(3, "not-an-email");
ShowChange(99, "someone@example.com");

// Failure(Exception): перетворюємо виняток з іншого шару на Result
Console.WriteLine("\n--- Result<T>.Failure(Exception) ---");
Result<UserProfile> lookup = SafeGetProfile(service, 7);
Console.WriteLine(lookup.IsSuccess ? $"Знайдено: {lookup.Value}" : $"Помилка: {lookup.Error}");
lookup = SafeGetProfile(service, 1);
Console.WriteLine(lookup.IsSuccess ? $"Знайдено: {lookup.Value}" : $"Помилка: {lookup.Error}");

Console.WriteLine("\n--- Підсумковий стан профілів ---");
for (int id = 1; id <= 3; id++)
    Console.WriteLine(service.GetProfile(id));

void TryUpdate(string title, UserProfile profile)
{
    try
    {
        service.UpdateProfile(profile);
        Console.WriteLine($"[OK]   {title}: профіль #{profile.Id} оновлено");
    }
    catch (InvalidUserDataException ex)
    {
        Console.WriteLine($"[FAIL] {title}: InvalidUserDataException (Field = {ex.Field}, Value = '{ex.Value}')");
        Console.WriteLine($"       {ex.Message}");
    }
    catch (UserNotFoundException ex)
    {
        Console.WriteLine($"[FAIL] {title}: UserNotFoundException (UserId = {ex.UserId}) — {ex.Message}");
    }
}

void ShowChange(int userId, string email)
{
    Result<bool> result = service.ChangeEmail(userId, email);
    if (result.IsSuccess)
        Console.WriteLine($"[OK]   user {userId} → {email}: email змінено (Value = {result.Value})");
    else
        Console.WriteLine($"[FAIL] user {userId} → {email}: {result.Error}");
}

static Result<UserProfile> SafeGetProfile(UserProfileService svc, int id)
{
    try
    {
        return Result<UserProfile>.Success(svc.GetProfile(id));
    }
    catch (UserNotFoundException ex)
    {
        return Result<UserProfile>.Failure(ex);
    }
}
