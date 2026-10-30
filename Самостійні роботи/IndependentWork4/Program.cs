using System.Text;
using IndependentWork4;

Console.OutputEncoding = Encoding.UTF8;

// 1. Початкова ієрархія: Book і Journal
Console.WriteLine("=== LibraryItem -> Book, Journal ===");
var book = new Book("Кобзар", 1840, "Тарас Шевченко", "BK-001");
var journal = new Journal("Дзвін", 2023, 5, "JR-001");

book.DisplayDetails();
journal.DisplayDetails();
Console.WriteLine();

// 2. Розширення ієрархії: AudioBook успадковує Book
Console.WriteLine("=== AudioBook (нащадок Book) ===");
var audioBook = new AudioBook("Лісова пісня", 1911, "Леся Українка", "Оксана Стасюк", "AB-001");
audioBook.DisplayDetails();
Console.WriteLine();

// 3. Поліморфний виклик: усі об'єкти через посилання базового типу
Console.WriteLine("=== Поліморфізм: List<LibraryItem> ===");
List<LibraryItem> catalog = new() { book, journal, audioBook };
foreach (LibraryItem item in catalog)
{
    item.DisplayDetails();
}
