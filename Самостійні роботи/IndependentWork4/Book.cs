namespace IndependentWork4;

public class Book : LibraryItem
{
    private string _author;

    public string Author
    {
        get { return _author; }
    }

    // Після зміни базового класу з'явився параметр uniqueId, який передається в base(...)
    public Book(string title, int publicationYear, string author, string uniqueId)
        : base(title, publicationYear, uniqueId)
    {
        _author = author;
    }

    public override void DisplayDetails()
    {
        base.DisplayDetails();
        Console.WriteLine($"  Автор: {Author}");
    }
}
