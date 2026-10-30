namespace IndependentWork4;

public class Journal : LibraryItem
{
    private int _issueNumber;

    public int IssueNumber
    {
        get { return _issueNumber; }
    }

    public Journal(string title, int publicationYear, int issueNumber, string uniqueId)
        : base(title, publicationYear, uniqueId)
    {
        _issueNumber = issueNumber;
    }

    public override void DisplayDetails()
    {
        base.DisplayDetails();
        Console.WriteLine($"  Випуск №: {IssueNumber}");
    }
}
