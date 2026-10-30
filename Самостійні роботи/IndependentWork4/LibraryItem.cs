namespace IndependentWork4;

/// <summary>
/// Базовий бібліотечний елемент.
/// </summary>
public class LibraryItem
{
    private string _title;
    private int _publicationYear;
    private string _uniqueId; // додано на кроці 4

    public string Title
    {
        get { return _title; }
    }

    public int PublicationYear
    {
        get { return _publicationYear; }
    }

    // Лише для читання: ідентифікатор призначається один раз у конструкторі
    public string UniqueId
    {
        get { return _uniqueId; }
    }

    public LibraryItem(string title, int publicationYear, string uniqueId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Назва не може бути порожньою.", nameof(title));
        if (string.IsNullOrWhiteSpace(uniqueId))
            throw new ArgumentException("Унікальний ідентифікатор не може бути порожнім.", nameof(uniqueId));

        _title = title;
        _publicationYear = publicationYear;
        _uniqueId = uniqueId;
    }

    public virtual void DisplayDetails()
    {
        Console.WriteLine($"[{UniqueId}] {Title} ({PublicationYear})");
    }
}
