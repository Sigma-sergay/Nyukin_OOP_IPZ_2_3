# Лабораторна робота №11 — LINQ: SelectMany, пагінація й custom extensions (варіант 19)

**Виконав:** Нюкін Сергій, ІПЗ-3/2

## Мета роботи

Навчитися використовувати оператор `SelectMany` для роботи з вкладеними колекціями, реалізовувати пагінацію даних за допомогою LINQ, а також створювати власні методи розширення для `IEnumerable<T>` або ключових доменних сутностей.

## Варіант 19: Компанії та їхні відділи з працівниками

**Сутності:**

| Клас | Поля | Вкладена колекція |
|------|------|-------------------|
| `Company` | `Id`, `Name` | `List<Department> Departments` |
| `Department` | `Id`, `Name` | `List<Employee> Employees` |
| `Employee` | `Id`, `Name`, `Salary` | — |

**Тестові дані:** 3 компанії (SoftServe, EPAM, Rivne IT Hub), 10 відділів, 27 працівників. Відділ «Стажування» навмисно залишено порожнім, щоб перевірити крайовий випадок методу розширення.

**Завдання:**

1. `SelectMany`: отримати всіх працівників усіх відділів усіх компаній в одному списку.
2. Пагінація: вивести всі відділи по 3 відділи на сторінку.
3. Custom Extension: метод розширення `AverageDepartmentSalary()` для `Department`, що обчислює середню зарплату у відділі.

## Структура

```
lab11v19/
├── lab11v19.csproj
├── Program.cs     # сутності, тестові дані, SelectMany, пагінація, методи розширення
└── README.md
```

## Запуск

```
dotnet run --project lab11v19
```

## Реалізація

### 1. SelectMany

Дані мають два рівні вкладеності (компанія → відділи → працівники), тому їх розгортають двома `SelectMany`:

```csharp
var allEmployees = companies
    .SelectMany(c => c.Departments)
    .SelectMany(d => d.Employees)
    .ToList();
```

Той самий запит у синтаксисі запитів записується кількома `from`:

```csharp
from c in companies
from d in c.Departments
from e in d.Employees
select e
```

Результат: **27 працівників** одним плоским списком. Окремо показано перевантаження `SelectMany` із результуючим селектором, яке зберігає назву компанії й відділу для кожного працівника.

### 2. Пагінація (Skip/Take)

Для пагінації написано узагальнений метод розширення `Page<T>()`:

```csharp
public static IEnumerable<T> Page<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    => source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
```

10 відділів по 3 на сторінку дають 4 сторінки (3 + 3 + 3 + 1). Запит сторінки за межами діапазону повертає порожню послідовність, а не кидає виняток.

### 3. Метод розширення `AverageDepartmentSalary()`

```csharp
public static decimal AverageDepartmentSalary(this Department department)
    => department.Employees.Count == 0 ? 0m : department.Employees.Average(e => e.Salary);
```

Стандартний `Average()` на порожній колекції кидає `InvalidOperationException`, тому для відділу без працівників метод повертає 0. Метод викликається як звичайний метод відділу (`d.AverageDepartmentSalary()`) і використовується всередині LINQ-запитів:

- `MaxBy(d => d.AverageDepartmentSalary())` знаходить відділ із найвищою середньою зарплатою: **Data Science (68 500 грн)**;
- `Where(d => d.AverageDepartmentSalary() > 40_000)` повертає Backend, Frontend, DevOps, Data Science, Mobile.

## Відповіді на контрольні питання

### 1. Для чого використовується `SelectMany`? Відмінність від `Select`

`Select` перетворює **кожен** елемент на **один** новий елемент. Якщо цей новий елемент сам є колекцією, отримуємо «колекцію колекцій». `SelectMany` перетворює кожен елемент на послідовність, а потім **розгортає (flatten)** усі ці послідовності в одну плоску.

Приклад із роботи:

```csharp
companies.Select(c => c.Departments)      // IEnumerable<List<Department>>: 3 елементи (по списку на компанію)
companies.SelectMany(c => c.Departments)  // IEnumerable<Department>: 10 відділів одним списком
```

`SelectMany` потрібен, коли дані вкладені (компанії → відділи → працівники, замовлення → позиції, блоги → пости) і треба працювати з елементами нижнього рівня як з однією колекцією. У синтаксисі запитів йому відповідає кілька `from` поспіль.

### 2. Як реалізувати пагінацію за допомогою LINQ? Які методи використовуються

Пагінація будується на двох методах:

- `Skip(n)` пропускає перші `n` елементів;
- `Take(k)` бере наступні `k` елементів.

Для сторінки з номером `page` (нумерація з 1) розміром `size`:

```csharp
source.Skip((page - 1) * size).Take(size);
```

Загальна кількість сторінок: `Math.Ceiling(count / (double)size)`. Для стабільного результату колекцію бажано відсортувати (`OrderBy`) перед пагінацією, особливо коли дані надходять із бази. Також у .NET є `Chunk(size)`, який одразу ділить послідовність на масиви-сторінки.

### 3. Концепція методів розширення. Як вони інтегруються з LINQ

Метод розширення — це **статичний** метод у **статичному** класі, перший параметр якого позначено `this`. Він дозволяє «додати» метод до наявного типу без зміни його коду та без успадкування. Викликається такий метод як звичайний метод екземпляра: `department.AverageDepartmentSalary()`, хоча компілятор перетворює це на `DepartmentExtensions.AverageDepartmentSalary(department)`.

Увесь LINQ to Objects побудований на методах розширення: `Where`, `Select`, `SelectMany`, `Skip`, `Take` тощо — це методи розширення для `IEnumerable<T>` у класі `System.Linq.Enumerable`. Тому власні методи розширення для `IEnumerable<T>` (як `Page<T>()` у цій роботі) вбудовуються в ланцюжки LINQ нарівні зі стандартними, а методи розширення для доменних сутностей (як `AverageDepartmentSalary()`) можна викликати всередині лямбд у `Where`, `OrderBy`, `MaxBy`.

### 4. Приклад власного методу розширення для `IEnumerable<T>` та його призначення

```csharp
public static IEnumerable<T> Page<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
{
    ArgumentNullException.ThrowIfNull(source);
    ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
    ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
    return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
}
```

**Призначення:** сховати формулу `Skip((page - 1) * size).Take(size)` за зрозумілою назвою та перевірити аргументи. Метод узагальнений, тому працює для будь-якої колекції (відділів, працівників, компаній) і вбудовується в ланцюжок LINQ: `departments.Where(...).Page(2, 3)`. Завдяки відкладеному виконанню він нічого не обчислює, доки результат не почнуть перебирати.
