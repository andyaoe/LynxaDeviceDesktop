using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Lynxa;


namespace DeviceListener
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPort _serialPort = new SerialPort("COM4", 115200, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
            _serialPort.Open();

            while (true)
            {
                try
                {
                    int data = _serialPort.ReadByte();
                    if (data != -1)
                    {
                        MessageHandler.ParsePacket((byte)data);
                    }
                }
                catch (TimeoutException te)
                {
                    //do nothing
                }
            }
        }
    }
}
