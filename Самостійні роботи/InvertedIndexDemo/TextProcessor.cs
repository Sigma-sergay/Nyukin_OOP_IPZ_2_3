using System.Text.RegularExpressions;

namespace InvertedIndexDemo;

public static class TextProcessor
{
    // Слово: літери й цифри, з можливими # або + наприкінці (щоб "C#" і "C++" лишались словами)
    private static readonly Regex WordRegex = new(@"[\p{L}\p{N}]+[#+]*", RegexOptions.Compiled);

    private static readonly HashSet<string> StopWords = new()
    {
        "a", "an", "the", "is", "are", "was", "were",
        "in", "on", "at", "to", "for", "of", "and", "or"
    };

    /// <summary>
    /// Розбиває текст на слова, прибирає пунктуацію й переводить у нижній регістр.
    /// Приклад: "Hello, world!" -> ["hello", "world"]
    /// </summary>
    public static IEnumerable<string> Tokenize(string text)
    {
        if (string.IsNullOrEmpty(text))
            yield break;

        foreach (Match match in WordRegex.Matches(text.ToLowerInvariant()))
        {
            yield return match.Value;
        }
    }

    /// <summary>
    /// Видаляє стоп-слова (a, an, the, is, are, ...).
    /// </summary>
    public static IEnumerable<string> RemoveStopWords(IEnumerable<string> words)
    {
        return words.Where(w => !StopWords.Contains(w));
    }
}
