namespace lab6v19;

/// <summary>
/// Базовий клас ієрархії: звичайний обліковий запис.
/// </summary>
public class Account
{
    // Приватні поля
    private string _username;
    private string _passwordHash;

    // Публічні властивості
    public string Username
    {
        get => _username;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Ім'я користувача не може бути порожнім.");
            _username = value;
        }
    }

    public string PasswordHash
    {
        get => _passwordHash;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Хеш пароля не може бути порожнім.");
            _passwordHash = value;
        }
    }

    // Конструктор базового класу
    public Account(string username, string passwordHash)
    {
        // Присвоюємо через властивості, щоб спрацювала перевірка
        _username = string.Empty;
        _passwordHash = string.Empty;
        Username = username;
        PasswordHash = passwordHash;
    }

    // virtual: похідні класи МОЖУТЬ перевизначити цей метод (override)
    public virtual void Login()
    {
        Console.WriteLine($"[Account] Користувач '{Username}' виконав вхід.");
    }

    // НЕ virtual: похідний клас може лише сховати його через new
    public string GetAccountType()
    {
        return "Базовий акаунт";
    }
}
