using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Google.Protobuf;
using System.IO;

namespace Lynxa
{
    public enum MessageId
    {
        Nmea_100 = 100,
    }
    
    public class MessageFields
    {
        public MessageId messageId { get; set; }
        public UInt16 payloadSize { get; set; }
    }

    public static class MessageHandler
    {
        public static int _parserState = 0;
        public static MessageFields _parserMessageFields = new MessageFields();
        private static byte _messageIdLsb;
        private static byte _messageIdMsb;
        private static byte _payloadSizeLsb;
        private static byte _payloadSizeMsb;
        private static byte _checksumLsb;
        private static byte _checksumMsb;
        private static UInt16 _messageId;
        private static UInt16 _payloadSize;
        private static UInt16 _payloadCounter;
        private static UInt16 _checksum;
        private static UInt16 _runningChecksum;
        private static byte[] _payloadBuffer = new byte[1024];

        public static void ParsePacket(byte value)
        {
            bool full_packet_received = false;

            //Console.Write($"{value:X} ");

            switch (_parserState)
            {
                case 0: //look for the first magic byte '#'
                    if (value == '#')
                    {
                        _parserState++;
                    }
                    break;
                case 1: //look for the second magic byte '#'
                    if (value == '#')
                    {
                        _parserState++;
                        _runningChecksum = 0;
                    }
                    else
                    {
                        _parserState = 0;
                    }
                    break;
                case 2: //save the message id (LSB)
                    _messageIdLsb = value;
                    _runningChecksum += value;
                    _parserState++;
                    break;
                case 3: //save the message id (MSB)
                    _messageIdMsb = value;
                    _runningChecksum += value;
                    _messageId = (UInt16)((_messageIdMsb << 8) + _messageIdLsb);
                    _parserState++;
                    break;
                case 4: //save the payload size (LSB)
                    _payloadSizeLsb = value;
                    _runningChecksum += value;
                    _parserState++;
                    break;
                case 5: //save the payload size (MSB)
                    _payloadSizeMsb = value;
                    _runningChecksum += value;
                    _payloadSize = (UInt16)((_payloadSizeMsb << 8) + _payloadSizeLsb);
                    _payloadCounter = 0;
                    _parserState++;
                    break;
                case 6: //save each byte of the payload
                    _payloadBuffer[_payloadCounter++] = value;
                    _runningChecksum += value;

                    if (_payloadCounter == _payloadSize)
                    {
                        _parserState++;
                    }
                    break;
                case 7: //save the checksum (LSB)
                    _checksumLsb = value;
                    _parserState++;
                    break;
                case 8: //save the checksum (MSB)
                    _checksumMsb = value;
                    _checksum = (UInt16)((_checksumMsb << 8) + _checksumLsb);
                    _parserState = 0;
                    full_packet_received = true;
                    break;
            }

            if (full_packet_received)
            {
                Console.WriteLine("_messageId=" + _messageId);
                Console.WriteLine("_payloadSize=" + _payloadSize);
                Console.WriteLine("_checksum=" + _checksum);
                Console.WriteLine("_runningChecksum=" + _runningChecksum);

                byte[] payload_buffer_sized = new byte[_payloadSize];
                Array.Copy(_payloadBuffer, payload_buffer_sized, _payloadSize);

                NmeaRecord_100 nmeaRecord_100 = NmeaRecord_100.Parser.ParseFrom(payload_buffer_sized);
                Console.WriteLine("Nmea Record Received:");
                Console.WriteLine($"Timestamp:{nmeaRecord_100.Timestamp}");
                Console.WriteLine($"Latitude:{nmeaRecord_100.Latitude}");
                Console.WriteLine($"Longitude:{nmeaRecord_100.Longitude}");


            }
        }
    }
}
