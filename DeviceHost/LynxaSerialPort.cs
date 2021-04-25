using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using Google.Protobuf;

namespace Lynxa
{
    public class LynxaSerialPort
    {
        SerialPort _serialPort;
        string _portName;
        int _baudRate;
        MessageHandler _myMessageHandler = new MessageHandler();
        //public event EventHandler<Type, object> LynxaPacketReceivedEvent;
        public event EventHandler<object> LynxaPacketReceivedEvent;

        public LynxaSerialPort(string portName, int baudRate)
        {
            _portName = portName;
            _baudRate = baudRate;
        }

        public void Open()
        {
            _serialPort = new SerialPort(_portName, _baudRate, Parity.None, 8, StopBits.One);
            _serialPort.Handshake = Handshake.None;
            _serialPort.ReadTimeout = 1000;
            _serialPort.WriteTimeout = 1000;
            _serialPort.Open();

            //start thread
            Thread readThread = new Thread(Read);
            readThread.Start();

        }

        public void Close()
        {
            _serialPort.Close();
        }

        public void SendDevicePropertyMessage(string type, string name, string argument)
        {
            DeviceProperty_10 deviceProperty = null;

            //if (Enum.TryParse(type, out DeviceProperty_10.Types.DevicePropertyType device_property_type))
            if (Enum.TryParse(type, true, out DeviceProperty_10.Types.DevicePropertyType device_property_type))
            {
                if (Enum.TryParse(name, true, out DeviceProperty_10.Types.DevicePropertyName device_property_name))
                {
                    deviceProperty = new DeviceProperty_10();
                    deviceProperty.Type = device_property_type;
                    deviceProperty.Name = device_property_name;
                    deviceProperty.Argument = argument;
                }
            }

            if (deviceProperty != null)
            {
                LynxaMessageInfo lynxaMessageInfo = new LynxaMessageInfo();
                lynxaMessageInfo.deviceUid = 0xFFFFFFFF;
                lynxaMessageInfo.messageId = 10;
                lynxaMessageInfo.payloadBuffer = deviceProperty.ToByteArray();
                lynxaMessageInfo.payloadSize = Convert.ToUInt16(lynxaMessageInfo.payloadBuffer.Length);

                byte[] output = MessageHandler.ConstructPacket(lynxaMessageInfo);
                _serialPort.Write(output, 0, output.Length);
            }

        }

        public void Read()
        {
            while (_serialPort.IsOpen)
            {
                try
                {
                    int data = _serialPort.ReadByte();
                    if (data != -1)
                    {
                        LynxaMessageInfo lynxa_message_info = _myMessageHandler.ParsePacket((byte)data);
                        if (lynxa_message_info != null)
                        {
                            switch (lynxa_message_info.messageId)
                            {
                                case 100:
                                    GnggaMessage_100 nmeaRecord_100 = GnggaMessage_100.Parser.ParseFrom(lynxa_message_info.payloadBuffer);
                                    LynxaPacketReceivedEvent?.Invoke(this, nmeaRecord_100);
                                    break;
                                case 102:
                                    WifiStationList_102 wifiStationList_102 = WifiStationList_102.Parser.ParseFrom(lynxa_message_info.payloadBuffer);
                                    LynxaPacketReceivedEvent?.Invoke(this, wifiStationList_102);
                                    break;
                                case 103:
                                    ModemParameters_103 modemParameters_103 = ModemParameters_103.Parser.ParseFrom(lynxa_message_info.payloadBuffer);
                                    LynxaPacketReceivedEvent?.Invoke(this, modemParameters_103);
                                    break;
                            }
                        }
                    }
                }
                catch (TimeoutException te)
                {
                    //comes here if there was a timeout
                }
            }
        }
    }
}