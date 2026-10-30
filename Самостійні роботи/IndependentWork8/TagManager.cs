namespace IndependentWork8;

/// <summary>
/// Сценарій 2: теги статей. HashSet гарантує унікальність тегів і дає операції над множинами.
/// </summary>
public class TagManager
{
    // ArticleId -> множина тегів статті
    private readonly Dictionary<string, HashSet<string>> _articleTags = new();

    private static HashSet<string> NewTagSet() => new(StringComparer.OrdinalIgnoreCase);

    public void AddTag(string articleId, string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Тег не може бути порожнім.", nameof(tag));

        if (!_articleTags.TryGetValue(articleId, out HashSet<string>? tags))
        {
            tags = NewTagSet();
            _articleTags[articleId] = tags;
        }

        tags.Add(tag.Trim()); // повторне додавання "C#" і "c#" нічого не змінює
    }

    public void RemoveTag(string articleId, string tag)
    {
        if (_articleTags.TryGetValue(articleId, out HashSet<string>? tags))
        {
            tags.Remove(tag.Trim());
        }
    }

    public HashSet<string> GetTags(string articleId)
    {
        // Повертаємо копію, щоб зовнішній код не змінював внутрішній стан
        return _articleTags.TryGetValue(articleId, out HashSet<string>? tags)
            ? new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase)
            : NewTagSet();
    }

    /// <summary>Усі унікальні теги по всіх статтях (об'єднання множин).</summary>
    public HashSet<string> GetAllUniqueTags()
    {
        HashSet<string> result = NewTagSet();
        foreach (HashSet<string> tags in _articleTags.Values)
        {
            result.UnionWith(tags);
        }
        return result;
    }

    /// <summary>Теги, спільні для двох статей (перетин множин).</summary>
    public HashSet<string> FindCommonTags(string articleId1, string articleId2)
    {
        HashSet<string> result = GetTags(articleId1); // копія першої множини
        result.IntersectWith(GetTags(articleId2));
        return result;
    }

    /// <summary>Теги першої статті, яких немає в другій (різниця множин).</summary>
    public HashSet<string> FindTagsOnlyInFirst(string articleId1, string articleId2)
    {
        HashSet<string> result = GetTags(articleId1);
        result.ExceptWith(GetTags(articleId2));
        return result;
    }
}
