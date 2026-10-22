namespace lab6v19;

/// <summary>
/// Звичайний користувач: має електронну пошту та вміє змінювати пароль.
/// </summary>
public class UserAccount : Account
{
    private string _email;

    public string Email
    {
        get => _email;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
                throw new ArgumentException("Некоректна електронна пошта.");
            _email = value;
        }
    }

    // base(...) викликає конструктор базового класу Account
    public UserAccount(string username, string passwordHash, string email)
        : base(username, passwordHash)
    {
        _email = string.Empty;
        Email = email;
    }

    // override: надаємо власну реалізацію віртуального методу
    public override void Login()
    {
        base.Login(); // спочатку виконуємо логіку базового класу
        Console.WriteLine($"[UserAccount] Надіслано сповіщення про вхід на {Email}.");
    }

    // Власний унікальний метод
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        Console.WriteLine($"[UserAccount] Пароль користувача '{Username}' змінено.");
    }

    // new: ПРИХОВУЄ метод базового класу (він не virtual, тому override неможливий).
    // Який саме метод викличеться, залежить від ТИПУ ПОСИЛАННЯ, а не від типу об'єкта.
    public new string GetAccountType()
    {
        return "Акаунт користувача";
    }
}
