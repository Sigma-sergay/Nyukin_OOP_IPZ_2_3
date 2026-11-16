using System.Text.RegularExpressions;

namespace lab13v19;

public class UserProfile
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Phone { get; set; } = string.Empty;

    public override string ToString() => $"#{Id} {UserName}, {Age} р., {Email}, {Phone}";
}

/// <summary>
/// Сервіс профілів користувачів (сховище в пам'яті).
/// </summary>
public class UserProfileService
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^\+380\d{9}$", RegexOptions.Compiled);
    private static readonly Regex NameRegex = new(@"^[\p{L}][\p{L}\d_.-]{2,29}$", RegexOptions.Compiled);

    private readonly Dictionary<int, UserProfile> _profiles = new();

    public UserProfileService()
    {
        _profiles[1] = new UserProfile { Id = 1, UserName = "serhii", Email = "serhii@example.com", Age = 18, Phone = "+380671112233" };
        _profiles[2] = new UserProfile { Id = 2, UserName = "olena", Email = "olena@example.com", Age = 19, Phone = "+380502223344" };
        _profiles[3] = new UserProfile { Id = 3, UserName = "maksym", Email = "maksym@example.com", Age = 20, Phone = "+380933334455" };
    }

    /// <summary>
    /// Повертає профіль. Відсутній користувач — помилка виклику (Id мав прийти з системи), тому виняток.
    /// </summary>
    public UserProfile GetProfile(int userId)
    {
        if (!_profiles.TryGetValue(userId, out var profile))
            throw new UserNotFoundException(userId);
        return profile;
    }

    /// <summary>
    /// Оновлює профіль. Невалідне поле — порушення контракту методу, тому InvalidUserDataException.
    /// </summary>
    public void UpdateProfile(UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var current = GetProfile(profile.Id);

        if (string.IsNullOrWhiteSpace(profile.UserName) || !NameRegex.IsMatch(profile.UserName))
            throw new InvalidUserDataException(nameof(profile.UserName), profile.UserName,
                "3–30 символів, починається з літери, лише літери, цифри, '_', '.', '-'");

        if (!EmailRegex.IsMatch(profile.Email ?? string.Empty))
            throw new InvalidUserDataException(nameof(profile.Email), profile.Email ?? "null", "не відповідає формату name@domain.tld");

        if (profile.Age < 14 || profile.Age > 120)
            throw new InvalidUserDataException(nameof(profile.Age), profile.Age.ToString(), "допустимий вік 14–120");

        if (!PhoneRegex.IsMatch(profile.Phone ?? string.Empty))
            throw new InvalidUserDataException(nameof(profile.Phone), profile.Phone ?? "null", "очікується формат +380XXXXXXXXX");

        current.UserName = profile.UserName;
        current.Email = profile.Email!;
        current.Age = profile.Age;
        current.Phone = profile.Phone!;
    }

    /// <summary>
    /// Змінює email. Зайнятий email або неправильний ввід — очікувані ситуації, тому Result, а не виняток.
    /// </summary>
    public Result<bool> ChangeEmail(int userId, string newEmail)
    {
        if (!_profiles.TryGetValue(userId, out var profile))
            return Result<bool>.Failure($"Користувача з Id = {userId} не знайдено.");

        if (string.IsNullOrWhiteSpace(newEmail) || !EmailRegex.IsMatch(newEmail))
            return Result<bool>.Failure($"Email '{newEmail}' має невірний формат.");

        if (string.Equals(profile.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            return Result<bool>.Failure("Новий email збігається з поточним.");

        var owner = _profiles.Values.FirstOrDefault(p =>
            p.Id != userId && string.Equals(p.Email, newEmail, StringComparison.OrdinalIgnoreCase));
        if (owner is not null)
            return Result<bool>.Failure($"Email '{newEmail}' вже використовується користувачем '{owner.UserName}'.");

        profile.Email = newEmail;
        return Result<bool>.Success(true);
    }
}
