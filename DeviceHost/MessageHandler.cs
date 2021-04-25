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
    public class LynxaMessageInfo
    {
        public byte modifier { get; set; }
        public UInt32 deviceUid { get; set; }
        public UInt16 messageId { get; set; }
        public UInt16 totalMessageSize { get; set; }
        public UInt16 payloadSize { get; set; }
        public byte[] payloadBuffer { get; set; }
        public byte[] messageBuffer { get; set; }
    }

    public class MessageHandler
    {
        public int _parserState = 0;
        private byte _modifier;
        private UInt32 _deviceUid;
        private UInt16 _messageId;
        private UInt16 _payloadSize;
        private UInt16 _checksum;
        private byte[] _packetBuffer = new byte[1024];
        private UInt16 _packetCounter = 0;
        private UInt16 _payloadCounter = 0;
        private UInt16 _checksumPayloadIndex = 0;
        private UInt16 _payloadIndex = 0;

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

        public static byte[] ConstructPacket(LynxaMessageInfo messageInfo)
        {
            UInt16 message_write_offset = 0;
            UInt16 checksum = 0;
            byte[] buffer = new byte[1024];

            //write magic bytes
            buffer[message_write_offset++] = Convert.ToByte('#');
            buffer[message_write_offset++] = Convert.ToByte('#');
            //write modifier
            buffer[message_write_offset++] = 0xFF;
            //write device UID
            buffer[message_write_offset++] = (byte)(messageInfo.deviceUid >> 24);
            buffer[message_write_offset++] = (byte)(messageInfo.deviceUid >> 16);
            buffer[message_write_offset++] = (byte)(messageInfo.deviceUid >> 8);
            buffer[message_write_offset++] = (byte)(messageInfo.deviceUid >> 0);
            //write message ID
            buffer[message_write_offset++] = (byte)(messageInfo.messageId >> 8);
            buffer[message_write_offset++] = (byte)(messageInfo.messageId >> 0);
            //write payload size
            buffer[message_write_offset++] = (byte)(messageInfo.payloadSize >> 8);
            buffer[message_write_offset++] = (byte)(messageInfo.payloadSize >> 0);
            //write payload
            Array.Copy(messageInfo.payloadBuffer, 0, buffer, message_write_offset, messageInfo.payloadSize);
            message_write_offset += messageInfo.payloadSize;

            //calculate context->checksum
            for (UInt16 i = 0; i < message_write_offset; i++)
            {
                checksum += buffer[i];
            }

            //write context->checksum
            buffer[message_write_offset++] = (byte)(checksum >> 8);
            buffer[message_write_offset++] = (byte)(checksum >> 0);

            byte[] output_array = new byte[message_write_offset];
            Array.Copy(buffer, output_array, output_array.Length);
            return output_array;
        }

        public LynxaMessageInfo ParsePacket(byte value)
        {
            bool full_packet_received = false;
            bool reset_state_machine = false;
            UInt16 calculated_checksum = 0;
            LynxaMessageInfo lynxa_message_info = null;

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
                case 2: //save the modifier
                    _modifier = value;
                    _parserState++;
                    break;
                case 3: //save the device UID (MSB)
                    _deviceUid = (UInt32)(value << 24);
                    _parserState++;
                    break;
                case 4: //save the device UID
                    _deviceUid |= (UInt32)(value << 16);
                    _parserState++;
                    break;
                case 5: //save the device UID
                    _deviceUid |= (UInt32)(value << 8);
                    _parserState++;
                    break;
                case 6: //save the device UID (LSB)
                    _deviceUid |= (UInt32)(value << 0);
                    _parserState++;
                    //Console.WriteLine($"_deviceUid={_deviceUid}");
                    break;
                case 7: //save the message id (MSB)
                    _messageId = (UInt16)(value << 8);
                    _parserState++;
                    break;
                case 8: //save the message id (LSB)
                    _messageId |= (UInt16)(value << 0);
                    _parserState++;
                    //Console.WriteLine($"_messageId={_messageId}");
                    break;
                case 9: //save the payload size (MSB)
                    _payloadSize = (UInt16)(value << 8);
                    _parserState++;
                    break;
                case 10: //save the payload size (LSB)
                    _payloadSize |= (UInt16)(value << 0);
                    _parserState++;
                    _payloadIndex = _packetCounter;
                    //Console.WriteLine($"_payloadSize={_payloadSize}");
                    break;
                case 11: //save each byte of the payload
                    _payloadCounter++;
                    if (_payloadCounter == _payloadSize)
                    {
                        _checksumPayloadIndex = _packetCounter;
                        _parserState++;
                    }
                    break;
                case 12: //save the checksum (MSB)
                    _checksum = (UInt16)(value << 8);
                    _parserState++;
                    break;
                case 13: //save the checksum (LSB)
                    _checksum |= (UInt16)(value << 0);
                    _parserState++;
                    //Console.WriteLine($"_checksum={_checksum}");

                    //verify checksum
                    for (int i = 0; i < _checksumPayloadIndex; i++)
                    {
                        calculated_checksum += _packetBuffer[i];
                    }

                    //Console.WriteLine($"calculated_checksum={calculated_checksum}");

                    if (_checksum == calculated_checksum)
                    {
                        //packet is valid
                        full_packet_received = true;
                    }
                    reset_state_machine = true;
                    break;
            }

            if (full_packet_received)
            {
                lynxa_message_info = new LynxaMessageInfo();
                lynxa_message_info.modifier = _modifier;
                lynxa_message_info.deviceUid = _deviceUid;
                lynxa_message_info.messageId = _messageId;
                lynxa_message_info.payloadSize = _payloadSize;
                lynxa_message_info.totalMessageSize = _packetCounter;
                lynxa_message_info.messageBuffer = new byte[_packetCounter];
                lynxa_message_info.payloadBuffer = new byte[_payloadSize];

                Array.Copy(_packetBuffer, 0, lynxa_message_info.messageBuffer, 0, _packetCounter);
                Array.Copy(_packetBuffer, _payloadIndex, lynxa_message_info.payloadBuffer, 0, _payloadSize);
            }

            if (reset_state_machine)
            {
                ResetStateMachine();
            }

            return lynxa_message_info;
        }
    }
}
