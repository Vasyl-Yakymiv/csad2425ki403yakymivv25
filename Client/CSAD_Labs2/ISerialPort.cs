using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSAD_Labs2
{
    public interface ISerialPort
    {
        void Open();
        void Close();
        void WriteLine(string text);
        string ReadLine();
        bool IsOpen { get; }
    }
}
