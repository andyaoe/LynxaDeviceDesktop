﻿using System;
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
    
    public class LynxaMessageInfo
    {
        public UInt16 messageId { get; set; }
        public UInt16 totalMessageSize { get; set; }
        public UInt16 payloadSize { get; set; }
        public UInt16 payloadPointer { get; set; }
        public UInt16 messagePointer { get; set; }
    }

    public class MessageHandler
    {
        public int _parserState = 0;
        private byte _messageIdLsb;
        private byte _messageIdMsb;
        private UInt16 _messageId;
        private byte _payloadSizeLsb;
        private byte _payloadSizeMsb;
        private UInt16 _payloadSize;
        private byte _checksumLsb;
        private byte _checksumMsb;
        private UInt16 _checksum;
        private byte[] _packetBuffer = new byte[1024];
        private UInt16 _packetCounter = 0;
        private UInt16 _payloadCounter = 0;
        private UInt16 _checksumPayloadIndex = 0;

        private void ResetStateMachine()
        {
            _parserState = 0;
            _packetCounter = 0;
            _payloadCounter = 0;
        }

        public MessageHandler()
        {
            ResetStateMachine();
        }

        public void ParsePacket(byte value)
        {
            bool full_packet_received = false;
            bool reset_state_machine = false;
            UInt16 calculated_checksum = 0;

            //Console.Write($"{value:X} ");
            _packetBuffer[_packetCounter++] = value;

            switch (_parserState)
            {
                case 0: //look for the first magic byte '#'
                    if (value == '#')
                    {
                        _parserState++;
                    }
                    else
                    {
                        reset_state_machine = true;
                    }
                    break;
                case 1: //look for the second magic byte '#'
                    if (value == '#')
                    {
                        _parserState++;
                    }
                    else
                    {
                        reset_state_machine = true;
                    }
                    break;
                case 2: //save the message id (MSB)
                    _messageIdMsb = value;
                    _parserState++;
                    break;
                case 3: //save the message id (LSB)
                    _messageIdLsb = value;
                    _messageId = (UInt16)((_messageIdMsb << 8) + _messageIdLsb);
                    _parserState++;
                    break;
                case 4: //save the payload size (MSB)
                    _payloadSizeMsb = value;
                    _parserState++;
                    break;
                case 5: //save the payload size (LSB)
                    _payloadSizeLsb = value;
                    _payloadSize = (UInt16)((_payloadSizeMsb << 8) + _payloadSizeLsb);
                    _parserState++;
                    break;
                case 6: //save each byte of the payload
                    _payloadCounter++;
                    if (_payloadCounter == _payloadSize)
                    {
                        _checksumPayloadIndex = _packetCounter;
                        _parserState++;
                    }
                    break;
                case 7: //save the checksum (MSB)
                    _checksumMsb = value;
                    _parserState++;
                    break;
                case 8: //save the checksum (LSB)
                    _checksumLsb = value;
                    _checksum = (UInt16)((_checksumMsb << 8) + _checksumLsb);
                    _parserState++;
                    break;
                case 9: //verify checksum
                    if (value == 0)
                    {
                        for (int i = 0; i < _checksumPayloadIndex; i++)
                        {
                            calculated_checksum += _packetBuffer[i];
                        }

                        if (_checksum == calculated_checksum)
                        {
                            //packet is valid
                            full_packet_received = true;
                        }
                    }
                    reset_state_machine = true;
                    break;
            }

            if (full_packet_received)
            {
                Console.WriteLine("_messageId=" + _messageId);
                Console.WriteLine("_payloadSize=" + _payloadSize);
                Console.WriteLine("_checksum=" + _checksum);
                Console.WriteLine("_calculated_checksum=" + calculated_checksum);

                byte[] payload_buffer = new byte[_payloadSize];
                Array.Copy(_packetBuffer, 6, payload_buffer, 0, _payloadSize);

                GnggaMessage_100 nmeaRecord_100 = GnggaMessage_100.Parser.ParseFrom(payload_buffer);
                Console.WriteLine("Nmea Record Received:");
                Console.WriteLine($"Latitude:{nmeaRecord_100.LatitudeDegrees}");
                Console.WriteLine($"Longitude:{nmeaRecord_100.LongitudeDegrees}");
            }

            if (reset_state_machine)
            {
                ResetStateMachine();
            }
        }
    }
}
