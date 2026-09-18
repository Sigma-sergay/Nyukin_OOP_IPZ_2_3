using System;

namespace Lab3
{
    // Варіант 19: Клас EventLogger з реалізацією патерну Dispose
    public class EventLogger : IDisposable
    {
        // Поля класу
        private string _logName;
        private bool _isLogging; // Поле для імітації некерованого ресурсу
        private bool _disposed = false; // Прапорець, що вказує, чи було вже звільнено ресурси

        // Публічні властивості
        public string LogName => _logName;
        public bool IsLogging => _isLogging;

        // Конструктор: "виділяє ресурс"
        public EventLogger(string logName)
        {
            _logName = logName;
            _isLogging = true;
            Console.WriteLine($"[Створено] EventLogger для '{_logName}'. Логування розпочато.");
        }

        // Метод для роботи з ресурсом
        public void LogEvent(string eventName)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(_logName, "Неможливо записати подію: об'єкт EventLogger вже знищено/закрыто.");
            }

            if (_isLogging)
            {
                Console.WriteLine($"  [Запис] Подія '{eventName}' успішно записана у лог '{_logName}'.");
            }
        }

        // Захищений віртуальний метод Dispose(bool disposing) — серце патерну Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 1. Звільнення керованих (managed) ресурсів
                    Console.WriteLine($"[Dispose(true)] Звільнення керованих ресурсів для '{_logName}'...");
                }

                // 2. Звільнення некерованих (unmanaged) ресурсів
                if (_isLogging)
                {
                    Console.WriteLine($"[Dispose] Зупинка логування та закриття файлу/сесії логу '{_logName}'.");
                    _isLogging = false;
                }

                _disposed = true;
            }
        }

        // Публічний метод Dispose (виклики ззовні)
        public void Dispose()
        {
            Dispose(true);
            // Повідомляємо GC, що фіналізатор (деструктор) викликати не потрібно,
            // оскільки ресурси вже звільнені вручну
            GC.SuppressFinalize(this);
        }

        // Деструктор (фіналізатор)
        ~EventLogger()
        {
            Console.WriteLine($"[~EventLogger] Деструктор викликано збирачем сміття (GC) для '{_logName}'!");
            Dispose(false);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("=== СЦЕНАРІЙ 1: Використання блоку using ===");
            using (EventLogger logger1 = new EventLogger("SystemEvents.log"))
            {
                logger1.LogEvent("Користувач увійшов у систему");
            } // ТУТ автоматично викликається Dispose()
            Console.WriteLine("Блок using завершено.\n");


            Console.WriteLine("=== СЦЕНАРІЙ 2: Явний виклик Dispose() ===");
            EventLogger logger2 = new EventLogger("ApplicationErrors.log");
            logger2.LogEvent("Помилка підключення до мережі");
            logger2.Dispose(); // Явний виклик
            Console.WriteLine("Dispose() викликано явно.\n");


            Console.WriteLine("=== СЦЕНАРІЙ 3: Об'єкт без Dispose() + GC.Collect() ===");
            CreateUnusedLogger();

            Console.WriteLine("Примусовий виклик Garbage Collector...");
            GC.Collect();
            GC.WaitForPendingFinalizers(); // Очікування завершення фіналізаторів

            Console.WriteLine("\nПрограму завершено успішно.");
        }

        // Окремий метод для Сценарію 3, щоб об'єкт втратив посилання
        static void CreateUnusedLogger()
        {
            EventLogger logger3 = new EventLogger("UnmanagedAudit.log");
            logger3.LogEvent("Тестова подія без явного закриття");
            // Dispose() НЕ викликається, виходимо з методу — об'єкт стає недоступним
        }
    }
}