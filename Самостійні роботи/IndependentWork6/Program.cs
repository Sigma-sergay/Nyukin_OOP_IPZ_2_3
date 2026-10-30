using System.Text;
using IndependentWork6;

Console.OutputEncoding = Encoding.UTF8;

// ============ Сценарій 1: система сповіщень (інтерфейс) ============
Console.WriteLine("=== Сценарій 1: INotifier ===");

List<INotifier> notifiers = new()
{
    new EmailNotifier("olena@example.com"),
    new SmsNotifier("+380501234567"),
    new PushNotifier("a1b2c3")
};

foreach (INotifier notifier in notifiers)
{
    // Однаковий виклик, різна поведінка залежно від каналу
    notifier.Send("Ваше замовлення №1024 відправлено та буде доставлене завтра.");
}

// Клас може реалізовувати ще один контракт, що неможливо з єдиним базовим класом
foreach (INotifier notifier in notifiers)
{
    if (notifier is ILoggable loggable)
        Console.WriteLine(loggable.GetLog());
}
Console.WriteLine();

// ============ Сценарій 2: обробка файлів (абстрактний клас) ============
Console.WriteLine("=== Сценарій 2: FileProcessor ===");

string dir = Path.Combine(Path.GetTempPath(), "IndependentWork6_" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(dir);

string txtPath = Path.Combine(dir, "notes.txt");
string xmlPath = Path.Combine(dir, "books.xml");
string csvPath = Path.Combine(dir, "students.csv");

File.WriteAllText(txtPath, "Перший рядок тексту\nДругий рядок із трьома словами\n");
File.WriteAllText(xmlPath, "<library><book id=\"1\"><title>Кобзар</title></book><book id=\"2\"><title>Лісова пісня</title></book></library>");
File.WriteAllText(csvPath, "name,group,grade\nОлена,ІП-21,5\nІвано,ІП-21,4\nМарія,ІП-22,5\n");

List<FileProcessor> processors = new()
{
    new TextFileProcessor(txtPath),
    new XmlFileProcessor(xmlPath),
    new CsvFileProcessor(csvPath)
};

foreach (FileProcessor processor in processors)
{
    processor.Process(); // відкриття і закриття виконує базовий клас
    Console.WriteLine();
}

Directory.Delete(dir, recursive: true);
