namespace IndependentWork6;

/// <summary>
/// Сценарій 1: контракт «вміти відправляти повідомлення». Реалізацію диктує канал.
/// </summary>
public interface INotifier
{
    void Send(string message);
}

/// <summary>
/// Додатковий незалежний контракт: клас може реалізувати кілька інтерфейсів одночасно.
/// </summary>
public interface ILoggable
{
    string GetLog();
}

public class EmailNotifier : INotifier, ILoggable
{
    private readonly string _address;
    private int _sent;

    public EmailNotifier(string address)
    {
        _address = address;
    }

    public void Send(string message)
    {
        _sent++;
        Console.WriteLine($"[Email -> {_address}] {message}");
    }

    public string GetLog() => $"EmailNotifier: надіслано листів - {_sent}";
}

public class SmsNotifier : INotifier
{
    private readonly string _phone;

    public SmsNotifier(string phone)
    {
        _phone = phone;
    }

    public void Send(string message)
    {
        // SMS обмежені за довжиною, тому логіка відправки відрізняється
        string text = message.Length > 40 ? message[..37] + "..." : message;
        Console.WriteLine($"[SMS -> {_phone}] {text}");
    }
}

public class PushNotifier : INotifier
{
    private readonly string _deviceToken;

    public PushNotifier(string deviceToken)
    {
        _deviceToken = deviceToken;
    }

    public void Send(string message)
    {
        Console.WriteLine($"[Push -> device {_deviceToken}] {message}");
    }
}
