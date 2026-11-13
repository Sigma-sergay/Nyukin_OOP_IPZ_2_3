// Лабораторна робота №11. LINQ: SelectMany, пагінація й custom extensions.
// Виконав: Нюкін Сергій, група ІПЗ-3/2, варіант 19.
// Сценарій: Компанії та їхні відділи з працівниками.
//
// ЗВІТ
// 1. Класи сутностей (з вкладеними колекціями):
//    Company    (Id, Name, Departments: List<Department>)
//    Department (Id, Name, Employees:   List<Employee>)
//    Employee   (Id, Name, Salary)
//    Ієрархія: Company -> багато Department -> багато Employee.
// 2. Тестові дані: 3 компанії, 10 відділів, 27 працівників (метод SeedData).
//    Відділ "Стажування" навмисно без працівників, щоб перевірити крайовий випадок
//    для методу розширення.
// 3. Запити варіанту:
//    1) SelectMany — отримати всіх працівників усіх відділів усіх компаній одним списком;
//    2) Пагінація — вивести всі відділи по 3 на сторінку (Skip/Take);
//    3) Custom Extension — AverageDepartmentSalary() для Department.
// 4. Відповіді на контрольні питання — у README.md.

using System.Text;

Console.OutputEncoding = Encoding.UTF8;

List<Company> companies = SeedData();

// ---------------------------------------------------------------------------
// 1. SelectMany: усі працівники всіх відділів усіх компаній в одному списку
// ---------------------------------------------------------------------------
Console.WriteLine("=== 1. SelectMany: усі працівники всіх компаній ===");

// Синтаксис методів: два рівні вкладеності "розгортаються" двома SelectMany
var allEmployees = companies
    .SelectMany(c => c.Departments)
    .SelectMany(d => d.Employees)
    .ToList();

// Те саме в синтаксисі запитів (кілька from = SelectMany)
var allEmployeesQuery =
    (from c in companies
     from d in c.Departments
     from e in d.Employees
     select e).ToList();

Console.WriteLine($"Усього працівників: {allEmployees.Count} (query syntax: {allEmployeesQuery.Count})");
foreach (var e in allEmployees)
    Console.WriteLine($"  #{e.Id,-3} {e.Name,-22} {e.Salary,8:N0} грн");

// SelectMany з результуючим селектором: зберігаємо інформацію, звідки працівник
Console.WriteLine();
Console.WriteLine("Працівники з компанією та відділом (перші 8):");
var withContext = companies
    .SelectMany(c => c.Departments, (c, d) => new { Company = c.Name, Dept = d })
    .SelectMany(x => x.Dept.Employees,
                (x, e) => new { x.Company, Department = x.Dept.Name, e.Name, e.Salary });
foreach (var x in withContext.Take(8))
    Console.WriteLine($"  {x.Company,-12} | {x.Department,-14} | {x.Name}");

// Порівняння з Select: Select дає колекцію колекцій, а не плаский список
var nested = companies.Select(c => c.Departments).ToList();     // List<List<Department>>
var flat   = companies.SelectMany(c => c.Departments).ToList(); // List<Department>
Console.WriteLine();
Console.WriteLine($"Select:     {nested.Count} елементи (кожен — список відділів компанії)");
Console.WriteLine($"SelectMany: {flat.Count} елементів (усі відділи одним списком)");

// ---------------------------------------------------------------------------
// 2. Пагінація: усі відділи по 3 на сторінку
// ---------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("=== 2. Пагінація: відділи по 3 на сторінку ===");

const int pageSize = 3;
var allDepartments = companies
    .SelectMany(c => c.Departments, (c, d) => new { Company = c.Name, Department = d })
    .ToList();

int totalPages = (int)Math.Ceiling(allDepartments.Count / (double)pageSize);
for (int page = 1; page <= totalPages; page++)
{
    var items = allDepartments.Page(page, pageSize);   // власний метод розширення Skip/Take
    Console.WriteLine($"--- Сторінка {page} з {totalPages} ---");
    foreach (var x in items)
        Console.WriteLine($"  {x.Department.Name,-14} ({x.Company}), працівників: {x.Department.Employees.Count}");
}

// Сторінка за межами діапазону — порожня, без винятку
Console.WriteLine($"Сторінка {totalPages + 1}: елементів {allDepartments.Page(totalPages + 1, pageSize).Count()}");

// ---------------------------------------------------------------------------
// 3. Custom Extension: AverageDepartmentSalary() для Department
// ---------------------------------------------------------------------------
Console.WriteLine();
Console.WriteLine("=== 3. Метод розширення AverageDepartmentSalary() ===");

foreach (var c in companies)
{
    Console.WriteLine(c.Name + ":");
    foreach (var d in c.Departments)
        Console.WriteLine($"  {d.Name,-14} середня зарплата: {d.AverageDepartmentSalary(),10:N2} грн");
}

// Використання методу розширення всередині LINQ-запиту
var bestPaid = companies
    .SelectMany(c => c.Departments)
    .Where(d => d.Employees.Count > 0)
    .MaxBy(d => d.AverageDepartmentSalary())!;
Console.WriteLine();
Console.WriteLine($"Відділ з найвищою середньою зарплатою: {bestPaid.Name} ({bestPaid.AverageDepartmentSalary():N2} грн)");

var above40k = companies
    .SelectMany(c => c.Departments)
    .Where(d => d.AverageDepartmentSalary() > 40_000)
    .Select(d => d.Name);
Console.WriteLine("Відділи із середньою зарплатою > 40 000: " + string.Join(", ", above40k));

// ---------------------------------------------------------------------------
// Тестові дані
// ---------------------------------------------------------------------------
static List<Company> SeedData()
{
    int empId = 1;
    Employee E(string name, decimal salary) => new(empId++, name, salary);

    return new List<Company>
    {
        new(1, "SoftServe", new()
        {
            new(1, "Backend", new() { E("Іван Петренко", 52000), E("Олена Коваль", 48000), E("Андрій Мельник", 61000), E("Марія Шевчук", 45000) }),
            new(2, "Frontend", new() { E("Дмитро Бойко", 42000), E("Катерина Ткач", 39000), E("Олег Кравець", 44000) }),
            new(3, "QA", new() { E("Наталія Лисенко", 33000), E("Сергій Руденко", 35000), E("Юлія Мороз", 31000) }),
            new(4, "Стажування", new()),                       // відділ без працівників
        }),
        new(2, "EPAM", new()
        {
            new(5, "DevOps", new() { E("Віктор Гончар", 67000), E("Тарас Савчук", 58000), E("Ірина Павленко", 63000) }),
            new(6, "Data Science", new() { E("Оксана Литвин", 71000), E("Максим Поліщук", 66000) }),
            new(7, "Support", new() { E("Анна Кузьменко", 26000), E("Роман Ковальчук", 28000), E("Софія Олійник", 25000) }),
        }),
        new(3, "Rivne IT Hub", new()
        {
            new(8, "Mobile", new() { E("Богдан Сидоренко", 47000), E("Христина Марченко", 43000), E("Денис Захарченко", 39000) }),
            new(9, "Design", new() { E("Аліна Козак", 36000), E("Владислав Остапчук", 34000) }),
            new(10, "HR", new() { E("Людмила Іваненко", 29000), E("Григорій Білик", 27000), E("Вікторія Гнатюк", 30000), E("Павло Ярмоленко", 26000) }),
        }),
    };
}

// ---------------------------------------------------------------------------
// Класи сутностей
// ---------------------------------------------------------------------------
public record Employee(int Id, string Name, decimal Salary);

public record Department(int Id, string Name, List<Employee> Employees);

public record Company(int Id, string Name, List<Department> Departments);

// ---------------------------------------------------------------------------
// Методи розширення
// ---------------------------------------------------------------------------
public static class DepartmentExtensions
{
    /// <summary>
    /// Середня зарплата працівників відділу. Для відділу без працівників повертає 0,
    /// а не кидає InvalidOperationException, як стандартний Average() на порожній колекції.
    /// </summary>
    public static decimal AverageDepartmentSalary(this Department department)
    {
        ArgumentNullException.ThrowIfNull(department);
        return department.Employees.Count == 0
            ? 0m
            : department.Employees.Average(e => e.Salary);
    }
}

public static class PagingExtensions
{
    /// <summary>
    /// Повертає сторінку з номером pageNumber (з 1) розміром pageSize через Skip/Take.
    /// </summary>
    public static IEnumerable<T> Page<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }
}
