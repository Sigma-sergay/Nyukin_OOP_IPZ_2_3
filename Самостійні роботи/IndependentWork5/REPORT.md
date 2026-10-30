# Самостійна робота №5. Приклади поліморфізму в .NET

Проєкт: `IndependentWork5`. Запуск: `dotnet run`. Тестовий файл `sample.txt` програма створює сама й видаляє після роботи.

## Вивід програми

```
=== Сценарій 1: List<object> і ToString() ===
Product    -> Product: Laptop, 35000.00 грн
Customer   -> Customer: Олена Коваль (Рівне)
Order      -> Order #1: 2 x Laptop для Олена Коваль, разом 70000.00 грн
Warehouse  -> IndependentWork5.Warehouse
Int32      -> 42
String     -> рядок
DateTime   -> 10/22/2026 00:00:00

=== Сценарій 2: MyCustomCollection і foreach ===
  яблуко
  груша
  слива
  MyCustomCollection: яблуко | груша | слива
  string[]: a | b
  List<string>: x | y | z
LINQ Where (містить 'а'): груша, слива

=== Сценарій 3: ProcessStream(Stream) ===
Тип потоку: FileStream, CanRead = True, CanSeek = True, Length = 23
  Прочитано 5 байтів: "Hello"
Тип потоку: MemoryStream, CanRead = True, CanSeek = True, Length = 25
  Прочитано 5 байтів: "Hello"
```

## 1. Поліморфізм через `System.Object`

Усі типи в C# успадковують `System.Object`, а в ньому є віртуальні методи `ToString()`, `Equals()`, `GetHashCode()`. Класи `Product`, `Customer` і `Order` перевизначають `ToString()`:

```csharp
public class Product
{
    public string Name { get; }
    public decimal Price { get; }
    public override string ToString() => $"Product: {Name}, {Price:F2} грн";
}
```

Об'єкти зібрані в `List<object>`, і цикл `foreach (object item in items) Console.WriteLine(item);` викликає `ToString()` через посилання типу `object`. **Яка реалізація виконується**, визначає реальний тип об'єкта (пізнє зв'язування):

- `Product`, `Customer`, `Order` → їхні власні `override`;
- `Warehouse` (без `override`) → успадкована реалізація з `System.Object`, яка повертає повну назву типу `IndependentWork5.Warehouse`;
- `int`, `string`, `DateTime` → їхні реалізації з бібліотеки .NET (`DateTime` форматується за поточною культурою).

**Переваги:** єдиний клієнтський код (`Console.WriteLine`, `string.Join`, інтерполяція) працює з будь-яким типом, навіть з тими, про які автор `Console.WriteLine` нічого не знав. Додати новий клас можна без змін у коді виводу.

## 2. Поліморфізм через інтерфейс `IEnumerable<T>`

`MyCustomCollection` містить `List<string>` і реалізує `IEnumerable<string>`:

```csharp
public class MyCustomCollection : IEnumerable<string>
{
    private readonly List<string> _items = new();

    public IEnumerator<string> GetEnumerator()
    {
        foreach (string item in _items)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

Цикл `foreach` не вимагає конкретного типу колекції, йому потрібен лише об'єкт з методом `GetEnumerator()`. Тому метод `PrintAll(string, IEnumerable<string>)` приймає і `MyCustomCollection`, і `string[]`, і `List<string>`. Також працює LINQ (`collection.Where(...)`), бо LINQ написаний для `IEnumerable<T>`.

**Переваги:** слабке зв'язування (код залежить від контракту, а не від реалізації), повторне використання (одна функція для всіх колекцій), можливість підставити власну структуру даних у будь-який наявний код, що працює з послідовностями.

## 3. Поліморфізм через абстрактний клас `Stream`

`Stream` є абстрактним базовим класом для `FileStream`, `MemoryStream`, `NetworkStream` та ін. Метод

```csharp
static void ProcessStream(Stream stream)
{
    byte[] buffer = new byte[5];
    int read = stream.Read(buffer, 0, buffer.Length);
    ...
}
```

приймає параметр типу `Stream`, тому викликається і з `FileStream`, і з `MemoryStream`. Абстрактний метод `Read` кожен нащадок реалізує по-своєму: `FileStream` читає з диска, `MemoryStream` бере байти з масиву в пам'яті. Властивості `CanRead`, `CanSeek`, `Length` теж поліморфні.

**Переваги:** один алгоритм обробки даних працює незалежно від джерела (файл, пам'ять, мережа, архів); код легко тестувати, підставляючи `MemoryStream` замість файлу; нові види потоків (наприклад, `GZipStream`) підключаються без змін у `ProcessStream`.

## Висновок

У всіх трьох прикладах поліморфізм дає один і той самий виграш: **клієнтський код залежить від абстракції** (`object`, `IEnumerable<T>`, `Stream`), а не від конкретного типу. Для `Object` це наслідування з віртуальним методом, для `IEnumerable<T>` це реалізація інтерфейсу, для `Stream` це абстрактний клас. Механізми різні, мета та сама: гнучкість і повторне використання коду.

## Відповіді на контрольні запитання

**1. Як поліморфізм проявляється при роботі з `List<object>`?**
У список можна класти об'єкти будь-яких типів, бо всі вони є `object`. Під час виклику `ToString()` через посилання `object` виконується метод реального типу: `Product.ToString()` для товару, `Order.ToString()` для замовлення, а для `Warehouse`, який нічого не перевизначив, `Object.ToString()`.

**2. Що потрібно реалізувати в `MyCustomCollection`, щоб її можна було перебирати через `foreach`?**
Достатньо метода `GetEnumerator()`, який повертає перелічувач. Офіційний спосіб: реалізувати інтерфейс `IEnumerable<T>` (метод `IEnumerator<T> GetEnumerator()`) і його неузагальнену частину `IEnumerable.GetEnumerator()`. У нашому прикладі перелічувач створюється через `yield return`.

**3. Чому метод `ProcessStream(Stream stream)` є прикладом поліморфної роботи з абстрактним класом?**
Параметр має абстрактний тип `Stream`, тож екземпляр самого `Stream` створити не можна, але можна передати будь-якого нащадка. Виклики `stream.Read(...)`, `stream.Length` розв'язуються під час виконання залежно від реального типу (`FileStream` або `MemoryStream`), а код методу не змінюється.

**4. Які переваги дає єдиний інтерфейс роботи для різних типів потоків і колекцій?**
Менше дублювання (один метод замість окремого для кожного типу), легша заміна реалізації (файл на пам'ять у тестах, масив на список), можливість розширення без змін наявного коду, а також готові універсальні інструменти, як-от LINQ для будь-якого `IEnumerable<T>`.
