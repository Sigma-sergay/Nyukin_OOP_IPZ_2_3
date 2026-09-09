using System;

namespace lab1v19
{
    class Weather
    {
        private string city;
        private string date;

        public double Temperature { get; set; }

        public string City
        {
            get { return city; }
            set { city = value; }
        }

        public string Date
        {
            get { return date; }
            set { date = value; }
        }

        public Weather(string city, string date, double temperature)
        {
            this.city = city;
            this.date = date;
            Temperature = temperature;
        }

        public void PrintForecast()
        {
            Console.WriteLine($"Прогноз погоди для м. {city} на {date}: температура {Temperature}°C");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Лабораторна робота №1 | Студент: Нюкін (Варіант 19) ===\n");

            Weather forecast1 = new Weather("Рівне", "09.09.2026", 19.5);
            Weather forecast2 = new Weather("Київ", "09.09.2026", 22.0);
            Weather forecast3 = new Weather("Львів", "09.09.2026", 17.8);

            forecast1.PrintForecast();
            forecast2.PrintForecast();
            forecast3.PrintForecast();
        }
    }
}