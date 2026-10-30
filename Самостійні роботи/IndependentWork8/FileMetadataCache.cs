namespace IndependentWork8;

/// <summary>
/// Сценарій 1: кеш метаданих файлів. Dictionary дає пошук за ім'ям файлу за O(1).
/// </summary>
public class FileMetadataCache
{
    // Імена файлів у Windows нечутливі до регістру, тому використовуємо відповідний comparer
    private readonly Dictionary<string, FileMetadata> _cache =
        new(StringComparer.OrdinalIgnoreCase);

    public int Count => _cache.Count;

    /// <summary>Додає метадані. Повертає false, якщо файл з такою назвою вже є в кеші.</summary>
    public bool Add(FileMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        return _cache.TryAdd(metadata.FileName, metadata);
    }

    /// <summary>Повертає метадані або null, якщо файлу немає в кеші.</summary>
    public FileMetadata? Get(string fileName)
    {
        return _cache.TryGetValue(fileName, out FileMetadata? metadata) ? metadata : null;
    }

    public bool Remove(string fileName)
    {
        return _cache.Remove(fileName);
    }

    public bool Contains(string fileName)
    {
        return _cache.ContainsKey(fileName);
    }
}
