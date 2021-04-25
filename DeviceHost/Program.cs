using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Lynxa;
using System.IO;

namespace Lynxa
{
    class Program
    {
        static void Main(string[] args)
        {
            LynxaSerialPort lynxaSerialPort = new LynxaSerialPort("COM2", 9600);
            lynxaSerialPort.LynxaPacketReceivedEvent += LynxaSerialPort_LynxaPacketReceivedEvent;
            lynxaSerialPort.Open();

            CommandLineInterface command_line_interface = new CommandLineInterface(lynxaSerialPort);

            while (true)
            {
                //exit if a key is pressed
                //if (Console.KeyAvailable)
                //{
                //    //save file and exit
                //    break;
                //}

                string input = Console.ReadLine();
                Console.WriteLine($"echo: { input} ");
                command_line_interface.Process(input);
            }
        }

        private static void LynxaSerialPort_LynxaPacketReceivedEvent(object sender, object e)
        {
            Console.WriteLine($"Received Packet: { e.GetType() }");

            switch (e)
            {
                case DeviceProperty_10 devicePropertyMessage:
                    Console.WriteLine($"device property type:{devicePropertyMessage.Type.ToString()}");
                    Console.WriteLine($"device property argument:{devicePropertyMessage.Argument}");
                    break;
                case GnggaMessage_100 gnggaMessage:
                    Console.WriteLine($"epochTime:{gnggaMessage.EpochTime}");
                    Console.WriteLine($"LatitudeMinutes:{gnggaMessage.LatitudeMinutes}");
                    Console.WriteLine($"LatitudeDegrees:{gnggaMessage.LatitudeDegrees}");
                    Console.WriteLine($"LatitudeCardinalAscii:{gnggaMessage.LatitudeCardinalAscii}");
                    Console.WriteLine($"longitudeMinutes:{gnggaMessage.LongitudeMinutes}");
                    Console.WriteLine($"longitudeDegrees:{gnggaMessage.LongitudeDegrees}");
                    Console.WriteLine($"longitudeCardinalAscii:{gnggaMessage.LongitudeCardinalAscii}");
                    Console.WriteLine($"numberOfSatellitesInUse:{gnggaMessage.NumberOfSatellitesInUse}");
                    break;
                case WifiStationList_102 wifiStationList:
                    Console.WriteLine($"epochTime:{wifiStationList.EpochTime}");
                    Console.WriteLine($"Number of Wifi Stations:{wifiStationList.NumberStationsFound}");
                    for (int i = 0; i < wifiStationList.NumberStationsFound; i++)
                    {
                        Console.Write($"BSSID:");
                        Console.Write($"{wifiStationList.WifiStations[i].Bssid[0]}:");
                        Console.Write($"{wifiStationList.WifiStations[i].Bssid[1]}:");
                        Console.Write($"{wifiStationList.WifiStations[i].Bssid[2]}:");
                        Console.Write($"{wifiStationList.WifiStations[i].Bssid[3]}:");
                        Console.Write($"{wifiStationList.WifiStations[i].Bssid[4]}:");
                        Console.Write($"{wifiStationList.WifiStations[i].Bssid[5]}\r\n");
                        Console.WriteLine($"RSSI:{wifiStationList.WifiStations[i].Rssi}");
                    }
                    break;
                case ModemParameters_103 modemParameters:
                    Console.WriteLine("ModemParameters Received:");
                    Console.WriteLine($"epochTime:{modemParameters.EpochTime}");
                    Console.WriteLine($"CellId:{modemParameters.CellId}");
                    Console.WriteLine($"PLMN:{modemParameters.Plmn}");
                    Console.WriteLine($"TAC:{modemParameters.Tac}");
                    break;
            }
        }
    }
}
