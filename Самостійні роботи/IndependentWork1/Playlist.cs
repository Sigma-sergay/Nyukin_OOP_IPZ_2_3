namespace IndependentWork1;

/// <summary>
/// Плейлист: назва, кількість треків і загальна тривалість.
/// </summary>
public class Playlist
{
    // Приватні поля
    private string _name;
    private int _trackCount;
    private double _totalMinutes;

    // Властивість get/set з перевіркою значення
    public string Name
    {
        get { return _name; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Назва плейлиста не може бути порожньою.");
            _name = value;
        }
    }

    // Властивості "тільки для читання": змінити їх можна лише через методи класу
    public int TrackCount
    {
        get { return _trackCount; }
    }

    public double TotalMinutes
    {
        get { return _totalMinutes; }
    }

    // Конструктор ініціалізує всі поля
    public Playlist(string name, int trackCount, double totalMinutes)
    {
        if (trackCount < 0)
            throw new ArgumentOutOfRangeException(nameof(trackCount), "Кількість треків не може бути від'ємною.");
        if (totalMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(totalMinutes), "Тривалість не може бути від'ємною.");

        _name = string.Empty;
        Name = name; // через властивість, щоб спрацювала перевірка
        _trackCount = trackCount;
        _totalMinutes = totalMinutes;
    }

    // Просте обчислення: середня тривалість одного треку
    public double GetAverageTrackLength()
    {
        if (_trackCount == 0)
            return 0;
        return _totalMinutes / _trackCount;
    }

    // Перевірка стану: чи довший плейлист за вказану кількість хвилин
    public bool IsLongerThan(double minutes)
    {
        return _totalMinutes > minutes;
    }

    // Змінює стан об'єкта
    public void AddTrack(double lengthMinutes)
    {
        if (lengthMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(lengthMinutes), "Тривалість треку має бути додатною.");

        _trackCount++;
        _totalMinutes += lengthMinutes;
    }

    public void PrintInfo()
    {
        Console.WriteLine($"Плейлист '{Name}': {TrackCount} треків, {TotalMinutes:F1} хв");
    }
}
