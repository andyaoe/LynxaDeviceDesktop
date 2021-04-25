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
    public enum CommandType
    {
        CommandType_undefined,
        CommandType_get,
        CommandType_set,
        CommandType_current,
    }
    public class CommandFields
    {
        public string type;
        public string name;
        public string arg;

        public CommandFields(string type, string name, string arg)
        {
            this.type = type;
            this.name = name;
            this.arg = arg;
        }
    }
    public class CommandLineInterface
    {
        private CommandFields receivedCommand = null;
        private LynxaSerialPort _serialPort;

        public CommandLineInterface(LynxaSerialPort serialPort)
        {
            _serialPort = serialPort;
        }

        public void Process(string input)
        {
            CommandFields input_command;
            string[] split_input = input.Split();
            bool command_found = false;

            if (split_input.Length == 3)
            {
                input_command = new CommandFields(
                    split_input[0].ToLower(),
                    split_input[1].ToLower(),
                    split_input[2].ToLower());

                if (input_command.type != "set")
                {
                    Console.WriteLine("Only SET command can have 3 arguments");
                    return;
                }
            }
            else if (split_input.Length == 2)
            {
                input_command = new CommandFields(
                    split_input[0].ToLower(),
                    split_input[1].ToLower(),
                    "");

                if (input_command.type != "get")
                {
                    Console.WriteLine("Only GET command can have 2 arguments");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Not enough arguments");
                return;
            }

            string[] command_name_options = Enum.GetNames(typeof(DeviceProperty_10.Types.DevicePropertyName));
            foreach (string s in command_name_options)
            {
                //find a command name match
                if (s.ToLower() == input_command.name)
                {
                    if (input_command.type == "get")
                    {
                        receivedCommand = null;
                        _serialPort.SendDevicePropertyMessage(input_command.type, input_command.name, input_command.arg);
                        command_found = true;
                    }
                    else if (input_command.type == "set")
                    {
                        receivedCommand = null;
                        _serialPort.SendDevicePropertyMessage(input_command.type, input_command.name, input_command.arg);
                        command_found = true;
                    }
                }
            }

            if (command_found)
            {
                DateTime timeout = DateTime.Now.AddSeconds(1);
                //wait for up to 1 second for return command
                while (DateTime.Now < timeout)
                {
                    if (receivedCommand != null)
                    {
                        //if command is set, compare contents and if they match
                        if (input_command.type == "set")
                        {
                            if ((input_command.name == receivedCommand.name) &&
                                (input_command.arg == receivedCommand.arg))
                            {
                                Console.WriteLine($"SET { input_command.name } = { input_command.arg } SUCCESS");
                            }
                            else
                            {
                                Console.WriteLine($"SET { input_command.name } = { input_command.arg } FAIL");
                            }
                        }

                        Console.WriteLine($"Received { receivedCommand.name } = { receivedCommand.arg }");
                    }
                }

                if (receivedCommand == null)
                {
                    Console.WriteLine("error: no message received from device");
                }
            }
        }

        public void LynxaSerialPort_LynxaPacketReceivedEvent(object sender, object e)
        {
            Console.WriteLine($"Received Packet: { e.GetType() }");

            switch (e)
            {
                //case DeviceParameter
                case DeviceProperty_10 deviceProperty:
                    receivedCommand = new CommandFields(
                        deviceProperty.Type.ToString().ToLower(),
                        deviceProperty.Name.ToString().ToLower(),
                        deviceProperty.Argument.ToLower());
                    break;
            }
        }
    }
}