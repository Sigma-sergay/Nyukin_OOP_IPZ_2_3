using System.Runtime.Serialization;

namespace lab13v19;

/// <summary>
/// Невалідне значення поля профілю користувача.
/// Field — назва поля, Value — значення, яке не пройшло перевірку.
/// </summary>
[Serializable]
public class InvalidUserDataException : Exception
{
    public string Field { get; } = string.Empty;
    public string Value { get; } = string.Empty;

    public InvalidUserDataException()
        : base("Невалідні дані користувача.") { }

    public InvalidUserDataException(string message)
        : base(message) { }

    public InvalidUserDataException(string message, Exception innerException)
        : base(message, innerException) { }

    public InvalidUserDataException(string field, string value, string reason)
        : base($"Поле '{field}' має невалідне значення '{value}': {reason}")
    {
        Field = field;
        Value = value;
    }

#pragma warning disable SYSLIB0051 // конструктор серіалізації застарів у .NET 8+, лишаємо для сумісності
    protected InvalidUserDataException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        Field = info.GetString(nameof(Field)) ?? string.Empty;
        Value = info.GetString(nameof(Value)) ?? string.Empty;
    }

    [Obsolete("Formatter-based serialization is obsolete (SYSLIB0051).")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(Field), Field);
        info.AddValue(nameof(Value), Value);
    }
#pragma warning restore SYSLIB0051
}

/// <summary>
/// Профіль з указаним ідентифікатором не існує.
/// </summary>
[Serializable]
public class UserNotFoundException : Exception
{
    public int UserId { get; }

    public UserNotFoundException()
        : base("Користувача не знайдено.") { }

    public UserNotFoundException(string message)
        : base(message) { }

    public UserNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }

    public UserNotFoundException(int userId)
        : base($"Користувача з Id = {userId} не знайдено.")
    {
        UserId = userId;
    }

#pragma warning disable SYSLIB0051
    protected UserNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        UserId = info.GetInt32(nameof(UserId));
    }

    [Obsolete("Formatter-based serialization is obsolete (SYSLIB0051).")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(UserId), UserId);
    }
#pragma warning restore SYSLIB0051
}
