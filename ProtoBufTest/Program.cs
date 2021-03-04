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
            NmeaRecord_100 nmea_record_100 = new NmeaRecord_100
            {
                Timestamp = 12345,
                Latitude = 1.0f,
                Longitude = 1.0f
            };

            using (var output = File.Create("nmea_serialized.dat"))
            {
                nmea_record_100.WriteTo(output);
            }

            using (var input = File.OpenRead("nmea_serialized.dat"))
            {
                nmea_record_100 = NmeaRecord_100.Parser.ParseFrom(input);
            }

            Console.WriteLine("Nmea Record Contents");
            Console.WriteLine("TimeStamp=" + nmea_record_100.Timestamp);
            Console.WriteLine("Latitude=" + nmea_record_100.Latitude);
            Console.WriteLine("Longitude=" + nmea_record_100.Longitude);
            Console.WriteLine("COMPLETE!");
        }
    }
}
