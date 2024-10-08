using System;
using System.IO.Ports;
using System.Threading; // Для використання затримки

class Program
{
    static void Main(string[] args)
    {
        // Встановлюємо послідовний порт
        SerialPort port = new SerialPort("COM6", 9600);
        port.Open();

        Console.WriteLine("Підключено до Arduino. Введіть повідомлення для відправки (напишіть 'exit' для виходу).");

        while (true)
        {
            // Введення повідомлення користувачем
            Console.Write("Введіть повідомлення: ");
            string message = Console.ReadLine();

            // Перевірка на команду виходу
            if (message.ToLower() == "exit")
            {
                break;
            }

            // Відправка повідомлення на Arduino
            port.WriteLine(message);
            Console.WriteLine("Надіслано до Arduino: " + message);

            // Додаємо більшу затримку перед читанням відповіді
            Thread.Sleep(300); // Збільшуємо затримку до 300 мс

            // Отримання відповіді від Arduino
            string response = port.ReadLine();
            Console.WriteLine("Отримано від Arduino: " + response);

            // Додаємо затримку перед наступним надсиланням
            Thread.Sleep(500); // 500 мс, щоб дати час Arduino на обробку
        }

        port.Close();
        Console.WriteLine("З'єднання закрите.");
    }
}