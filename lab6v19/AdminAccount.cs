namespace lab6v19;

/// <summary>
/// Адміністратор: має рівень доступу та може видавати права.
/// </summary>
public class AdminAccount : Account
{
    private int _accessLevel;

    public int AccessLevel
    {
        get => _accessLevel;
        set
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value), "Рівень доступу має бути не менше 1.");
            _accessLevel = value;
        }
    }

    public AdminAccount(string username, string passwordHash, int accessLevel)
        : base(username, passwordHash)
    {
        AccessLevel = accessLevel;
    }

    public override void Login()
    {
        base.Login();
        Console.WriteLine($"[AdminAccount] Вхід адміністратора з рівнем доступу {AccessLevel}. Дію записано в журнал аудиту.");
    }

    // Власний унікальний метод
    public void GrantPermissions(Account target, string permission)
    {
        Console.WriteLine(
            $"[AdminAccount] '{Username}' надав право '{permission}' користувачу '{target.Username}'.");
    }
}
