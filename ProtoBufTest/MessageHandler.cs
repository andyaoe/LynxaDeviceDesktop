using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace VioluxTestApp
{
    public enum MessageId
    {
        BallastControl = 1,
        BallastChannelCurrent = 2,
        LedBrightness = 3,
        PlayAudio = 4,
        AudioVolume = 5,
        Ota = 7,
        FirmwareVersion = 8,
        DoorStatus = 10,
        DeviceModel = 11,
        HardwareVersion = 12,
        ButtonPush = 13,
        LampStatus = 14,
        SystemMode = 15,
        LedStatus = 16,
        ThingyNameIndex = 17,
        BallastStatus = 18,
    }

    public enum CommandId
    {
        Get = 1,
        Set = 2,
        Current = 3,
    }

    public enum DoorStatusArg0
    {
        DoorStatusClosed = 0,
        DoorStatusOpen = 1,
    }

    public enum LampChannel
    {
        Zero,
        One,
        Two,
        Three,
    }

    public enum SystemModeArg0
    {
        NormalMode = 0,
        HardwareSimulationMode = 1,
        ManufacturingMode = 2,
    }

    public class MessageFields
    {
        public MessageId messageId { get; set; }
        public CommandId commandId { get; set; }
        public byte argument0 { get; set; }
        public byte argument1 { get; set; }

        public MessageFields Copy()
        {
            MessageFields result = new MessageFields();
            result.messageId = this.messageId;
            result.commandId = this.commandId;
            result.argument0 = this.argument0;
            result.argument1 = this.argument1;
            return result;
        }
    }

    public static class MessageHandler
    {
       public static int parserState = 0;
       public static MessageFields parserMessageFields = new MessageFields();
       public static byte[] BuildPacket(MessageId messageId, CommandId commandId, Byte arg0, Byte arg1)
        {
            byte[] output = new byte[7];
            output[0] = Convert.ToByte('#');
            output[1] = Convert.ToByte('#');
            output[2] = Convert.ToByte(messageId);
            output[3] = Convert.ToByte(commandId);
            output[4] = Convert.ToByte(arg0);
            output[5] = Convert.ToByte(arg1);
            output[6] = Convert.ToByte(CalculateCheckSum(output));

            return output;
        }

        public static byte CalculateCheckSum(byte[] buffer)
        {
            byte iChkSum = 0;
            for (int i = 2; i < 6; i++)
            {
                iChkSum += buffer[i];
            }

            return iChkSum;
        }

        public static byte CalculateCheckSum(MessageFields messageFields)
        {
            byte iChkSum = 0;

            iChkSum += (byte)messageFields.messageId;
            iChkSum += (byte)messageFields.commandId;
            iChkSum += (byte)messageFields.argument0;
            iChkSum += (byte)messageFields.argument1;

            return iChkSum;
        }

        public static MessageFields ParsePacket(byte value)
        {
            MessageFields output = null;

            switch (parserState)
            {
                case 0: //look for the first magic byte '#'
                    if (value == '#')
                    {
                        parserState = 1;
                    }
                    break;
                case 1: //look for the second magic byte '#'
                    if (value == '#')
                    {
                        parserState = 2;
                    }
                    else
                    {
                        parserState = 0;
                    }
                    break;
                case 2: //save the message id
                    parserMessageFields.messageId = (MessageId)value; ;
                    parserState = 3;
                    break;
                case 3: //save the command id
                    parserMessageFields.commandId = (CommandId)value; ;
                    parserState = 4;
                    break;
                case 4: //save arg0
                    parserMessageFields.argument0 = value;
                    parserState = 5;
                    break;
                case 5: //save arg1
                    parserMessageFields.argument1 = value; ;
                    parserState = 6;
                    break;
                case 6: //Check the checksum
                    if (CalculateCheckSum(parserMessageFields) == value)
                    {
                        output = parserMessageFields.Copy();
                    }
                    parserState = 0;
                    break;
            }

            return output;

        }
    }
}
