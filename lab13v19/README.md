# Лабораторна робота №13 — Власні винятки та патерн Result (варіант 19)

**Виконав:** Нюкін Сергій, ІПЗ-3/2

## Мета роботи

Навчитися створювати власні типи винятків для специфічних помилок і реалізовувати патерн `Result<T>` для контрольованої обробки очікуваних невдач без викидання винятків.

## Варіант 19: Обробка даних користувача

| Елемент | Реалізація |
|---|---|
| Власний виняток | `InvalidUserDataException(string Field, string Value)` |
| Додатковий виняток | `UserNotFoundException(int UserId)` |
| Сервіс | `UserProfileService` |
| Метод з винятком | `void UpdateProfile(UserProfile profile)` — кидає `InvalidUserDataException`, якщо поле профілю невалідне |
| Метод з Result | `Result<bool> ChangeEmail(int userId, string newEmail)` — `Failure`, якщо email уже використовується (а також якщо формат невірний або користувача немає) |
| Третій метод | `UserProfile GetProfile(int userId)` — кидає `UserNotFoundException` |

## Структура

```
lab13v19/
├── lab13v19.csproj
├── Exceptions.cs          # InvalidUserDataException, UserNotFoundException
├── Result.cs              # Result<T>: IsSuccess, Value, Error, Success / Failure(string) / Failure(Exception)
├── UserProfileService.cs  # UserProfile, UserProfileService
├── Program.cs             # демонстрація в Main
├── output.txt             # консольний вивід
└── README.md
```

## Запуск

```
dotnet run --project lab13v19
```

## Власні винятки

Обидва винятки успадковані від `System.Exception` і мають стандартний набір конструкторів:
- без параметрів;
- `(string message)`;
- `(string message, Exception innerException)`;
- конструктор з даними предметної області: `(string field, string value, string reason)` / `(int userId)`;
- `protected`-конструктор серіалізації `(SerializationInfo, StreamingContext)` і перевизначений `GetObjectData` — для сумісності (у .NET 8+ formatter-серіалізація позначена застарілою, тому код обгорнуто в `#pragma warning disable SYSLIB0051`).

Властивості `Field`, `Value`, `UserId` дають обробнику структуровану інформацію: наприклад, UI може підсвітити саме те поле, яке не пройшло перевірку.

## Result<T>

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }
    private Result(T value, bool isSuccess, string error) { ... }
    public static Result<T> Success(T value);
    public static Result<T> Failure(string error);
    public static Result<T> Failure(Exception ex);
}
```

Конструктор приватний — створити `Result` можна лише через фабричні методи, тож неможливий стан «успіх з помилкою».

## Перевірки в UserProfileService

| Поле | Правило |
|---|---|
| `UserName` | 3–30 символів, починається з літери; літери, цифри, `_`, `.`, `-` |
| `Email` | формат `name@domain.tld` |
| `Age` | 14–120 |
| `Phone` | `+380XXXXXXXXX` |

## Консольний вивід

```
=== Лабораторна 13, варіант 19: обробка даних користувача ===

--- UpdateProfile / GetProfile: власні винятки + try-catch ---
[OK]   Коректне оновлення: профіль #1 оновлено
[FAIL] Порожнє ім'я: InvalidUserDataException (Field = UserName, Value = '')
       Поле 'UserName' має невалідне значення '': 3–30 символів, починається з літери, лише літери, цифри, '_', '.', '-'
[FAIL] Email без домену: InvalidUserDataException (Field = Email, Value = 'olena@example')
       Поле 'Email' має невалідне значення 'olena@example': не відповідає формату name@domain.tld
[FAIL] Вік 7: InvalidUserDataException (Field = Age, Value = '7')
       Поле 'Age' має невалідне значення '7': допустимий вік 14–120
[FAIL] Телефон не в форматі: InvalidUserDataException (Field = Phone, Value = '0933334455')
       Поле 'Phone' має невалідне значення '0933334455': очікується формат +380XXXXXXXXX
[FAIL] Неіснуючий користувач: UserNotFoundException (UserId = 42) — Користувача з Id = 42 не знайдено.

--- ChangeEmail: Result<bool> через IsSuccess / Error ---
[OK]   user 1 → serhii.n@example.com: email змінено (Value = True)
[FAIL] user 2 → maksym@example.com: Email 'maksym@example.com' вже використовується користувачем 'maksym'.
[FAIL] user 3 → maksym@example.com: Новий email збігається з поточним.
[FAIL] user 3 → not-an-email: Email 'not-an-email' має невірний формат.
[FAIL] user 99 → someone@example.com: Користувача з Id = 99 не знайдено.

--- Result<T>.Failure(Exception) ---
Помилка: UserNotFoundException: Користувача з Id = 7 не знайдено.
Знайдено: #1 serhii_n, 18 р., serhii.n@example.com, +380671112233

--- Підсумковий стан профілів ---
#1 serhii_n, 18 р., serhii.n@example.com, +380671112233
#2 olena, 19 р., olena@example.com, +380502223344
#3 maksym, 20 р., maksym@example.com, +380933334455
```

## Чому для кожного методу обрано саме такий підхід

| Метод | Підхід | Чому |
|---|---|---|
| `UpdateProfile` | виняток `InvalidUserDataException` | Профіль приходить із форми, яка вже мала перевірити дані; невалідне поле на рівні сервісу — порушення контракту, і код, що викликає, не має продовжувати так, ніби все гаразд. Виняток зупиняє операцію й несе `Field`/`Value`. |
| `GetProfile` | виняток `UserNotFoundException` | `userId` береться із системи (сесія, посилання), тож відсутній користувач — справді виняткова ситуація. |
| `ChangeEmail` | `Result<bool>` | Зайнятий email — звичайний сценарій, що трапляється часто й очікувано. Немає сенсу платити за створення винятку й stack trace; простіше повернути `Failure` з поясненням, яке одразу показується користувачу. |
| `SafeGetProfile` у `Main` | `Result<T>.Failure(Exception)` | Показує, як на межі шарів перетворити виняток нижнього шару на `Result` для верхнього. |

## Відповіді на контрольні запитання

1. **Навіщо власні винятки?** Вони точно називають помилку предметної області (`InvalidUserDataException` замість загального `ArgumentException`), дозволяють ловити саме її окремим `catch` і передають додаткові дані (`Field`, `Value`, `UserId`).
2. **Стандартні конструктори:** без параметрів, `(string message)`, `(string message, Exception inner)`; раніше також `protected (SerializationInfo, StreamingContext)` для серіалізації.
3. **Патерн Result:** метод повертає об'єкт, що містить або значення, або опис помилки; викликач зобов'язаний перевірити `IsSuccess`. Доцільний для очікуваних невдач: валідація вводу, «не знайдено» при пошуку, зайняте ім'я / email.
4. **Винятки vs Result:** винятки — для непередбачених або фатальних ситуацій і порушення контракту (немає з'єднання, пошкоджені дані, неіснуючий Id з системи); Result — для бізнес-сценаріїв, які є частиною нормальної роботи програми.

## Висновок

Для варіанту 19 обидва підходи доповнюють один одного. Метод `UpdateProfile` отримує вже зібраний профіль, тому невалідне поле є помилкою контракту — `InvalidUserDataException` із властивостями `Field` і `Value` зупиняє оновлення й точно вказує, що не так. Натомість у `ChangeEmail` зайнятий email — нормальна ситуація під час роботи користувача, тому `Result<bool>` повертає зрозумілий текст помилки без накладних витрат на виняток, і код у `Main` обробляє її простим `if (result.IsSuccess)`.
