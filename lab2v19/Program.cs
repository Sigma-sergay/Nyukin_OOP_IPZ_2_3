using System;

namespace lab2v19
{
    // Клас за варіантом 19 (Weather)
    class Weather
    {
        // Приватні поля
        private string _city;
        private DateTime _date;
        private double _temperatureCelsius;

        // Публічні властивості
        public string City
        {
            get => _city;
            set => _city = value;
        }

        public DateTime Date
        {
            get => _date;
            set => _date = value;
        }

        public double TemperatureCelsius
        {
            get => _temperatureCelsius;
            set => _temperatureCelsius = value;
        }

        // 1. Повноцінний параметризований конструктор
        public Weather(string city, DateTime date, double temperature)
        {
            _city = city;
            _date = date;
            _temperatureCelsius = temperature;
            Console.WriteLine($"[Конструктор 1] Створено об'єкт погоди для м. {_city}");
        }

        // 2. Конструктор за замовчуванням (ланцюговий виклик через : this())
        public Weather() 
            : this("Kyiv", DateTime.Now, 10.0)
        {
            Console.WriteLine("[Конструктор 2] Викликано конструктор за замовчуванням (Kyiv, поточна дата, 10.0°C)");
        }

        // Деструктор (Фіналізатор)
        ~Weather()
        {
            Console.WriteLine($"[Деструктор] Об'єкт погоди для м. \"{_city}\" знищено збирачем сміття (GC).");
        }

        // Метод PrintForecast
        public void PrintForecast()
        {
            Console.WriteLine($"Прогноз погоди для м. {_city} на {_date:dd.MM.yyyy HH:mm}: {_temperatureCelsius}°C");
        }
    }

    class Program
    {
        // Метод для виходу об'єктів із зони видимості
        static void CreateAndUseObjects()
        {
            Console.WriteLine("--- Створення об'єктів ---");
            
            // Створення об'єкта конструктором за замовчуванням
            Weather defaultWeather = new Weather();

            // Створення об'єкта параметризованим конструктором
            Weather customWeather = new Weather("Рівне", new DateTime(2026, 9, 9), 19.5);

            Console.WriteLine("\n--- Виклик методів ---");
            defaultWeather.PrintForecast();
            customWeather.PrintForecast();
            
            Console.WriteLine("\n--- Завершення роботи з об'єктами ---");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=== Лабораторна робота №2 | Студент: Нюкін (Варіант 19) ===\n");

            CreateAndUseObjects();

            Console.WriteLine("\nОб'єкти більше не використовуються. Готуємося до виклику GC...");
            
            // Примусовий виклик GC для демонстрації роботоздатності деструктора
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine("\n=== Роботу програми завершено ===");
        }
    }
}