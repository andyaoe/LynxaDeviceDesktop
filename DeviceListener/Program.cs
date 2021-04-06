﻿using System;
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
            MessageHandler my_message_handler = new MessageHandler();
            LynxaMessageInfo lynxa_message_info = new LynxaMessageInfo();

            while (true)
            {
                try
                {
                    int data = _serialPort.ReadByte();
                    if (data != -1)
                    {
                        lynxa_message_info = my_message_handler.ParsePacket((byte)data);
                        if (lynxa_message_info != null)
                        {
                            switch (lynxa_message_info.messageId)
                            {
                                case 100:
                                    GnggaMessage_100 nmeaRecord_100 = GnggaMessage_100.Parser.ParseFrom(lynxa_message_info.payloadBuffer);
                                    Console.WriteLine("Nmea Record Received:");
                                    Console.WriteLine($"LatitudeDegrees:{nmeaRecord_100.LatitudeDegrees}");
                                    Console.WriteLine($"LongitudeDegrees:{nmeaRecord_100.LongitudeDegrees}");
                                    break;
                                case 102:
                                    WifiStationList_102 wifiStationList_102 = WifiStationList_102.Parser.ParseFrom(lynxa_message_info.payloadBuffer);
                                    Console.WriteLine($"Number of Wifi Stations:{wifiStationList_102.NumberStationsFound}");
                                    break;
                                case 103:
                                    ModemParameters_103 modemParameters_103 = ModemParameters_103.Parser.ParseFrom(lynxa_message_info.payloadBuffer);
                                    Console.WriteLine("ModemParameters Received:");
                                    Console.WriteLine($"CellId:{modemParameters_103.CellId}");
                                    break;
                            }
                        }
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
