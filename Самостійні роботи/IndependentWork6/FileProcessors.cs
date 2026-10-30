using System.Xml.Linq;

namespace IndependentWork6;

/// <summary>
/// Сценарій 2: усі обробники мають однакові кроки (відкрити -> обробити -> закрити),
/// тому спільний алгоритм і стан живуть в абстрактному класі (шаблонний метод).
/// </summary>
public abstract class FileProcessor
{
    private StreamReader? _reader;

    public string FilePath { get; }

    protected FileProcessor(string filePath)
    {
        FilePath = filePath;
    }

    // Читач доступний нащадкам тільки між Open() і Close()
    protected StreamReader Reader =>
        _reader ?? throw new InvalidOperationException("Файл не відкрито.");

    // Шаблонний метод: порядок кроків зафіксований, змінюється лише ProcessContent
    public void Process()
    {
        Open();
        try
        {
            ProcessContent();
        }
        finally
        {
            Close(); // файл закривається навіть при помилці
        }
    }

    // Спільні кроки (не віртуальні: нащадки їх не змінюють)
    private void Open()
    {
        Console.WriteLine($"Відкриваю файл: {Path.GetFileName(FilePath)}");
        _reader = new StreamReader(FilePath);
    }

    private void Close()
    {
        _reader?.Dispose();
        _reader = null;
        Console.WriteLine($"Файл закрито: {Path.GetFileName(FilePath)}");
    }

    // Унікальна логіка: кожен тип файлу обробляє вміст по-своєму
    protected abstract void ProcessContent();
}

public class TextFileProcessor : FileProcessor
{
    public TextFileProcessor(string filePath) : base(filePath) { }

    protected override void ProcessContent()
    {
        int lines = 0, words = 0;
        string? line;
        while ((line = Reader.ReadLine()) != null)
        {
            lines++;
            words += line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        }
        Console.WriteLine($"  Текстовий файл: рядків - {lines}, слів - {words}");
    }
}

public class XmlFileProcessor : FileProcessor
{
    public XmlFileProcessor(string filePath) : base(filePath) { }

    protected override void ProcessContent()
    {
        XDocument doc = XDocument.Load(Reader);
        int elements = doc.Descendants().Count();
        string root = doc.Root?.Name.LocalName ?? "(немає)";
        Console.WriteLine($"  XML-файл: кореневий елемент <{root}>, усього елементів - {elements}");
    }
}

public class CsvFileProcessor : FileProcessor
{
    public CsvFileProcessor(string filePath) : base(filePath) { }

    protected override void ProcessContent()
    {
        string? header = Reader.ReadLine();
        int columns = header?.Split(',').Length ?? 0;
        int rows = 0;
        while (Reader.ReadLine() != null)
        {
            rows++;
        }
        Console.WriteLine($"  CSV-файл: колонок - {columns}, рядків даних - {rows}");
    }
}
