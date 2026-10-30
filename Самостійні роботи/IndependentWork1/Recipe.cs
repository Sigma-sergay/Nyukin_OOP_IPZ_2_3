namespace IndependentWork1;

/// <summary>
/// Кулінарний рецепт: назва, кількість порцій, час приготування, калорійність порції.
/// </summary>
public class Recipe
{
    private string _title;
    private int _servings;
    private int _cookTimeMinutes;
    private int _caloriesPerServing;

    public string Title
    {
        get { return _title; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Назва рецепта не може бути порожньою.");
            _title = value;
        }
    }

    public int Servings
    {
        get { return _servings; }
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Кількість порцій має бути додатною.");
            _servings = value;
        }
    }

    // Лише для читання
    public int CookTimeMinutes
    {
        get { return _cookTimeMinutes; }
    }

    public Recipe(string title, int servings, int cookTimeMinutes, int caloriesPerServing)
    {
        if (cookTimeMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(cookTimeMinutes));
        if (caloriesPerServing < 0)
            throw new ArgumentOutOfRangeException(nameof(caloriesPerServing));

        _title = string.Empty;
        Title = title;
        _servings = 1;
        Servings = servings;
        _cookTimeMinutes = cookTimeMinutes;
        _caloriesPerServing = caloriesPerServing;
    }

    // Обчислення: калорійність усієї страви
    public int GetTotalCalories()
    {
        return _servings * _caloriesPerServing;
    }

    // Перевірка стану: швидкий рецепт готується не довше 30 хвилин
    public bool IsQuick()
    {
        return _cookTimeMinutes <= 30;
    }

    // Скільки калорій у вказаній кількості порцій
    public int GetCaloriesForPortions(int portions)
    {
        if (portions < 0)
            throw new ArgumentOutOfRangeException(nameof(portions));
        return portions * _caloriesPerServing;
    }

    public void PrintInfo()
    {
        string speed = IsQuick() ? "швидкий" : "довгий";
        Console.WriteLine($"Рецепт '{Title}': {Servings} порцій, {CookTimeMinutes} хв ({speed}), всього {GetTotalCalories()} ккал");
    }
}
