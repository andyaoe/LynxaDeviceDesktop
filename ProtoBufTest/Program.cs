using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using System.IO;

namespace ProtoBufTest
{
    class Program
    {
        static void Main(string[] args)
        {
            GnggaMessage_100 gngga_message_100 = new GnggaMessage_100
            {
                LatitudeDegrees = 100,
                LongitudeDegrees = 200
            };

            using (var output = File.Create("nmea_serialized.dat"))
            {
                gngga_message_100.WriteTo(output);
            }

            using (var input = File.OpenRead("nmea_serialized.dat"))
            {
                gngga_message_100 = GnggaMessage_100.Parser.ParseFrom(input);
            }

            Console.WriteLine("Nmea Record Contents");
            Console.WriteLine("Latitude Degrees=" + gngga_message_100.LatitudeDegrees);
            Console.WriteLine("Longitude Degrees=" + gngga_message_100.LongitudeDegrees);
            Console.WriteLine("COMPLETE!");
        }
    }
}
