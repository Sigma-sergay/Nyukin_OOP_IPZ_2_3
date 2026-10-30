namespace InvertedIndexDemo;

/// <summary>
/// Інвертований індекс: слово -> множина ідентифікаторів документів, де воно зустрічається.
/// </summary>
public class InvertedIndex
{
    // Слово -> множина ID документів
    private readonly Dictionary<string, HashSet<int>> _index;

    // ID -> документ (для швидкого отримання за ідентифікатором)
    private readonly Dictionary<int, Document> _documents;

    private readonly bool _removeStopWords;

    /// <param name="removeStopWords">
    /// Чи прибирати стоп-слова під час індексації. За замовчуванням вимкнено,
    /// щоб нормалізація збігалась із лінійним пошуком у PerformanceTest.
    /// </param>
    public InvertedIndex(bool removeStopWords = false)
    {
        _index = new Dictionary<string, HashSet<int>>();
        _documents = new Dictionary<int, Document>();
        _removeStopWords = removeStopWords;
    }

    public int DocumentCount => _documents.Count;
    public int UniqueWords => _index.Keys.Count;

    /// <summary>
    /// Додає документ до індексу.
    /// </summary>
    public void AddDocument(Document doc)
    {
        ArgumentNullException.ThrowIfNull(doc);

        // 1. Зберігаємо документ
        if (!_documents.TryAdd(doc.Id, doc))
            throw new ArgumentException($"Документ з Id = {doc.Id} уже є в індексі.", nameof(doc));

        // 2. Розбиваємо Content на слова (нижній регістр, без пунктуації)
        IEnumerable<string> words = TextProcessor.Tokenize(doc.Content);
        if (_removeStopWords)
            words = TextProcessor.RemoveStopWords(words);

        // 3. Для кожного слова додаємо doc.Id до _index[word]
        foreach (string word in words)
        {
            if (!_index.TryGetValue(word, out HashSet<int>? ids))
            {
                ids = new HashSet<int>();
                _index[word] = ids;
            }
            ids.Add(doc.Id); // HashSet ігнорує повторне додавання того самого Id
        }
    }

    /// <summary>
    /// Шукає документи, що містять слово.
    /// </summary>
    public List<Document> SearchSingleWord(string word)
    {
        // Нормалізуємо запит тими самими правилами, що й текст документів
        return SearchMultipleWords(word);
    }

    /// <summary>
    /// Шукає документи, що містять ВСІ слова (AND): перетин множин.
    /// </summary>
    public List<Document> SearchMultipleWords(params string[] words)
    {
        List<string> tokens = NormalizeQuery(words);
        if (tokens.Count == 0)
            return new List<Document>();

        HashSet<int> result = new(GetPostings(tokens[0]));
        foreach (string token in tokens.Skip(1))
        {
            result.IntersectWith(GetPostings(token));
            if (result.Count == 0)
                break; // перетин уже порожній: далі шукати марно
        }

        return ToDocuments(result);
    }

    /// <summary>
    /// Шукає документи, що містять БУДЬ-ЯКЕ слово (OR): об'єднання множин.
    /// </summary>
    public List<Document> SearchAnyWord(params string[] words)
    {
        List<string> tokens = NormalizeQuery(words);

        HashSet<int> result = new();
        foreach (string token in tokens)
        {
            result.UnionWith(GetPostings(token));
        }

        return ToDocuments(result);
    }

    private static List<string> NormalizeQuery(string[] words)
    {
        return words.SelectMany(TextProcessor.Tokenize).ToList();
    }

    private HashSet<int> GetPostings(string token)
    {
        return _index.TryGetValue(token, out HashSet<int>? ids) ? ids : new HashSet<int>();
    }

    private List<Document> ToDocuments(HashSet<int> ids)
    {
        return ids.OrderBy(id => id).Select(id => _documents[id]).ToList();
    }
}
