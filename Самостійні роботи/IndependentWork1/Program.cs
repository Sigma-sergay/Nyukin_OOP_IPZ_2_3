using System.Text;
using IndependentWork1;

Console.OutputEncoding = Encoding.UTF8;

// 1. Playlist
Console.WriteLine("=== Playlist ===");
var playlist = new Playlist("Ранкова пробіжка", 12, 46.5);
playlist.PrintInfo();
Console.WriteLine($"Середня тривалість треку: {playlist.GetAverageTrackLength():F2} хв");
Console.WriteLine($"Довший за 40 хв? {playlist.IsLongerThan(40)}");
playlist.AddTrack(3.5);
Console.Write("Після додавання треку: ");
playlist.PrintInfo();
playlist.Name = "Вечірня пробіжка";
Console.WriteLine($"Нова назва: {playlist.Name}");
Console.WriteLine();

// 2. Recipe
Console.WriteLine("=== Recipe ===");
var recipe = new Recipe("Омлет з овочами", 2, 15, 280);
recipe.PrintInfo();
Console.WriteLine($"Калорій у 1 порції: {recipe.GetCaloriesForPortions(1)} ккал");
recipe.Servings = 4;
Console.Write("Після збільшення порцій: ");
recipe.PrintInfo();
Console.WriteLine();

// 3. Thermostat
Console.WriteLine("=== Thermostat ===");
var thermostat = new Thermostat(22, 18.5);
thermostat.PrintInfo();
Console.WriteLine($"Потрібен обігрів? {thermostat.NeedsHeating()}");
thermostat.UpdateReading(18.5);
thermostat.PrintInfo();
thermostat.UpdateReading(22.3);
thermostat.PrintInfo();
