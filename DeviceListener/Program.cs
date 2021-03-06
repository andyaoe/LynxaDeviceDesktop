﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Lynxa;
using System.IO;

namespace DeviceListener
{
    class Program
    {
        static void Main(string[] args)
        {
            SerialPort _serialPort = new SerialPort("COM2", 9600, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
            _serialPort.Open();
            MessageHandler my_message_handler = new MessageHandler();
            LynxaMessageInfo lynxa_message_info = new LynxaMessageInfo();

            const string fileName = "LynxaMessage.dat";
            BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create));

            while (true)
            {
                try
                {
                    int data = _serialPort.ReadByte();
                    if (data != -1)
                    {
                        writer.Write((byte)data);
                        lynxa_message_info = my_message_handler.ParsePacket((byte)data);
                        if (lynxa_message_info != null)
                        {

                            Console.WriteLine("------------------------------------");
                            LynxaMessageId lynxa_message_id = (LynxaMessageId)lynxa_message_info.messageId;
                            switch (lynxa_message_id)
                            {
                                case LynxaMessageId.GnggaMessage100Id:
                                    GnggaMessage_100 nmeaRecord_100 = GnggaMessage_100.Parser.ParseFrom(lynxa_message_info.payloadBuffer);
                                    Console.WriteLine("Nmea Record Received:");
                                    Console.WriteLine($"epochTime:{nmeaRecord_100.EpochTime}");
                                    Console.WriteLine($"LatitudeMinutes:{nmeaRecord_100.LatitudeMinutes}");
                                    Console.WriteLine($"LatitudeDegrees:{nmeaRecord_100.LatitudeDegrees}");
                                    Console.WriteLine($"LatitudeCardinalAscii:{nmeaRecord_100.LatitudeCardinalAscii}");
                                    Console.WriteLine($"longitudeMinutes:{nmeaRecord_100.LongitudeMinutes}");
                                    Console.WriteLine($"longitudeDegrees:{nmeaRecord_100.LongitudeDegrees}");
                                    Console.WriteLine($"longitudeCardinalAscii:{nmeaRecord_100.LongitudeCardinalAscii}");
                                    Console.WriteLine($"numberOfSatellitesInUse:{nmeaRecord_100.NumberOfSatellitesInUse}");
                                    break;
                                case LynxaMessageId.WifiStationList102Id:
                                    WifiStationList_102 wifiStationList_102 = WifiStationList_102.Parser.ParseFrom(lynxa_message_info.payloadBuffer);
                                    Console.WriteLine($"epochTime:{wifiStationList_102.EpochTime}");
                                    Console.WriteLine($"Number of Wifi Stations:{wifiStationList_102.NumberStationsFound}");
                                    for (int i = 0; i < wifiStationList_102.NumberStationsFound; i++)
                                    {
                                        Console.Write($"BSSID:");
                                        Console.Write($"{wifiStationList_102.WifiStations[i].Bssid[0]}:");
                                        Console.Write($"{wifiStationList_102.WifiStations[i].Bssid[1]}:");
                                        Console.Write($"{wifiStationList_102.WifiStations[i].Bssid[2]}:");
                                        Console.Write($"{wifiStationList_102.WifiStations[i].Bssid[3]}:");
                                        Console.Write($"{wifiStationList_102.WifiStations[i].Bssid[4]}:");
                                        Console.Write($"{wifiStationList_102.WifiStations[i].Bssid[5]}\r\n");
                                        Console.WriteLine($"RSSI:{wifiStationList_102.WifiStations[i].Rssi}");
                                    }
                                    break;
                                case LynxaMessageId.ModemParameters103Id:
                                    ModemParameters_103 modemParameters_103 = ModemParameters_103.Parser.ParseFrom(lynxa_message_info.payloadBuffer);
                                    Console.WriteLine("ModemParameters Received:");
                                    Console.WriteLine($"epochTime:{modemParameters_103.EpochTime}");
                                    Console.WriteLine($"CellId:{modemParameters_103.CellId}");
                                    Console.WriteLine($"PLMN:{modemParameters_103.Plmn}");
                                    Console.WriteLine($"TAC:{modemParameters_103.Tac}");

                                    var result = LynxaDeviceMessageTranslator.TranslateModemParameters(modemParameters_103);
                                    

                                    Console.WriteLine($"MCC:{result.MCC}");
                                    Console.WriteLine($"MNC:{result.MNC}");
                                    Console.WriteLine($"LAC:{result.LAC}");
                                    break;
                            }
                        }
                    }
                }
                catch (TimeoutException te)
                {
                    //do nothing
                    if (Console.KeyAvailable)
                    {
                        //save file and exit
                        writer.Close();
                        break;
                    }
                }
            }
        }
    }
}
