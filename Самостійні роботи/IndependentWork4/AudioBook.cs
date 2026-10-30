namespace IndependentWork4;

/// <summary>
/// Аудіокнига: різновид книги з оповідачем.
/// </summary>
public class AudioBook : Book
{
    private string _narrator;

    public string Narrator
    {
        get { return _narrator; }
    }

    public AudioBook(string title, int publicationYear, string author, string narrator, string uniqueId)
        : base(title, publicationYear, author, uniqueId)
    {
        _narrator = narrator;
    }

    public override void DisplayDetails()
    {
        base.DisplayDetails(); // виводить назву, рік, ID та автора (з Book)
        Console.WriteLine($"  Оповідач: {Narrator}");
    }
}
