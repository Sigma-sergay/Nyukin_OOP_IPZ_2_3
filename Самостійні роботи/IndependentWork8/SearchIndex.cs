using System.Text.RegularExpressions;

namespace IndependentWork8;

/// <summary>
/// Сценарій 3: простий пошуковий індекс. Слово -> множина документів, де воно зустрічається.
/// </summary>
public class SearchIndex
{
    private readonly Dictionary<string, HashSet<int>> _invertedIndex =
        new(StringComparer.OrdinalIgnoreCase);

    public int WordCount => _invertedIndex.Count;

    public void AddDocument(int docId, string content)
    {
        // Слова: літери й цифри, з можливими # або + наприкінці (щоб "C#" і "C++" лишались словами)
        foreach (Match match in Regex.Matches(content, @"[\p{L}\p{N}]+[#+]*"))
        {
            string word = match.Value;

            if (!_invertedIndex.TryGetValue(word, out HashSet<int>? docs))
            {
                docs = new HashSet<int>();
                _invertedIndex[word] = docs;
            }

            docs.Add(docId); // HashSet не дозволить записати один документ двічі
        }
    }

    public HashSet<int> Search(string word)
    {
        return _invertedIndex.TryGetValue(word, out HashSet<int>? docs)
            ? new HashSet<int>(docs) // копія
            : new HashSet<int>();
    }

    /// <summary>Документи, що містять ВСІ слова (перетин множин).</summary>
    public HashSet<int> SearchMultiple(params string[] words)
    {
        if (words.Length == 0)
            return new HashSet<int>();

        HashSet<int> result = Search(words[0]);
        foreach (string word in words.Skip(1))
        {
            if (result.Count == 0)
                break; // далі шукати марно

            result.IntersectWith(Search(word));
        }
        return result;
    }
}
