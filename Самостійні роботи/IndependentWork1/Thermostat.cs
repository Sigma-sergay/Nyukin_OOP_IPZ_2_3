namespace IndependentWork1;

/// <summary>
/// Термостат: підтримує задану температуру і вирішує, чи вмикати обігрів.
/// </summary>
public class Thermostat
{
    private double _targetTemperature;
    private double _currentTemperature;
    private bool _isHeating;

    // Цільова температура обмежена розумним діапазоном
    public double TargetTemperature
    {
        get { return _targetTemperature; }
        set
        {
            if (value < 5 || value > 35)
                throw new ArgumentOutOfRangeException(nameof(value), "Температура має бути в діапазоні 5-35 °C.");
            _targetTemperature = value;
        }
    }

    // Лише для читання: поточну температуру можна змінити тільки через UpdateReading
    public double CurrentTemperature
    {
        get { return _currentTemperature; }
    }

    public bool IsHeating
    {
        get { return _isHeating; }
    }

    public Thermostat(double targetTemperature, double currentTemperature)
    {
        _targetTemperature = 20;
        TargetTemperature = targetTemperature;
        _currentTemperature = currentTemperature;
        _isHeating = NeedsHeating();
    }

    // Перевірка стану: чи потрібно обігрівати (з гістерезисом 0.5 °C)
    public bool NeedsHeating()
    {
        return _currentTemperature < _targetTemperature - 0.5;
    }

    // Приймає нове показання датчика й оновлює стан обігріву
    public void UpdateReading(double measuredTemperature)
    {
        _currentTemperature = measuredTemperature;
        _isHeating = NeedsHeating();
    }

    public void PrintInfo()
    {
        string state = IsHeating ? "обігрів увімкнено" : "обігрів вимкнено";
        Console.WriteLine($"Термостат: ціль {TargetTemperature:F1} °C, зараз {CurrentTemperature:F1} °C, {state}");
    }
}
