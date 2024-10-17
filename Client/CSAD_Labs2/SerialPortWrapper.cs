using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAD_Labs2
{
    public class SerialPortWrapper : ISerialPort
    {
        private readonly SerialPort _serialPort;

        public SerialPortWrapper(string portName, int baudRate)
        {
            _serialPort = new SerialPort(portName, baudRate);
        }

        public bool IsOpen => _serialPort.IsOpen;

        public void Open()
        {
            _serialPort.Open();
        }

        public void Close()
        {
            _serialPort.Close();
        }

        public void WriteLine(string text)
        {
            _serialPort.WriteLine(text);
        }

        public string ReadLine()
        {
            return _serialPort.ReadLine();
        }
    }
}
