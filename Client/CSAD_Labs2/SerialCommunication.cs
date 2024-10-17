using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAD_Labs2
{
    public class SerialCommunication
    {
        private readonly ISerialPort _serialPort;

        public SerialCommunication(ISerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public void Start()
        {
            _serialPort.Open();
            Console.WriteLine("Connected to Arduino. Enter message (Enter 'exit' for exit).");

            while (true)
            {
                Console.Write("Enter message: ");
                string message = Console.ReadLine();

                if (message.ToLower() == "exit")
                {
                    break;
                }

                _serialPort.WriteLine(message);
                Console.WriteLine("Sent to Arduino: " + message);

                System.Threading.Thread.Sleep(300);

                string response = _serialPort.ReadLine();
                Console.WriteLine("Received from Arduino: " + response);

                System.Threading.Thread.Sleep(500);
            }

            _serialPort.Close();
            Console.WriteLine("The connection is closed.");
        }
    }
}
