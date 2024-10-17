using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAD_Labs2
{
class Program
    {
        static void Main(string[] args)
        {
            ISerialPort serialPort = new SerialPortWrapper("COM6", 9600);

            // Створення об'єкту SerialCommunication
            SerialCommunication serialCommunication = new SerialCommunication(serialPort);

            // Запуск серійної комунікації
            serialCommunication.Start();
        }
    }
}
