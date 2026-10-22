namespace lab12v19;

/// <summary>
/// Узагальнений репозиторій. Обмеження where T : IEntity гарантує,
/// що кожен елемент має цілочисельний Id, за яким його можна знайти.
/// </summary>
/// <typeparam name="T">Тип сутності.</typeparam>
public interface IRepository<T> where T : IEntity
{
    /// <summary>Повертає елемент за Id або default (null), якщо його не знайдено.</summary>
    T? GetById(int id);

    /// <summary>Повертає копію списку всіх елементів.</summary>
    IEnumerable<T> GetAll();

    /// <summary>Додає елемент. Якщо Id == 0, репозиторій сам призначає новий Id.</summary>
    void Add(T item);

    /// <summary>Замінює наявний елемент з таким самим Id.</summary>
    void Update(T item);

    /// <summary>Видаляє елемент за Id.</summary>
    void Delete(int id);
}
