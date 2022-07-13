#define USELONGVARBINARY
using System;
using InterSystems.Data.IRISClient;
using System.Diagnostics;
using System.Threading;
using System.Text;
namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            String host = "iris";
            String port = "1972";
            String username = "SuperUser";
            String password = "SYS";
            String Namespace = "USER";

            int loopcnt = 5000;

            if (args.Length >= 1) {}
            if (args.Length >= 2) host = args[1];
            if (args.Length >= 3) port = args[2];
            if (args.Length >= 4) loopcnt = Int32.Parse(args[3]);

            double[] data = new double[loopcnt];


            String ConnectionString = "Server = " + host
                + "; Port = " + port + "; Namespace = " + Namespace
                + "; Password = " + password + "; User ID = " + username + "; SharedMemory=false;pooling=true;";
            IRISConnection IRISConnect = new IRISConnection();
            IRISConnect.ConnectionString = ConnectionString;


            IRISConnect.Open();

#if USELONGVARBINARY
            Console.WriteLine("Using LONGVARBINARY.");
            String sqlStatement3 = "select ts,binaryA1,binaryB1 from TestTable where ts<'2022-01-01 00:01:39' and ts>='2022-01-01 00:01:09'";
#else
            Console.WriteLine("Using VARBINARY.");
            String sqlStatement3 = "select ts,binaryA2,binaryB2 from TestTable where ts<'2022-01-01 00:01:39' and ts>='2022-01-01 00:01:09'";
#endif
            IRISCommand cmd3 = new IRISCommand(sqlStatement3.ToString(), IRISConnect);
            IRISDataReader reader = cmd3.ExecuteReader();

            var sw = new Stopwatch();
            double ms;
            sw.Reset();
            sw.Start();
            int cnt = 0;
            int total_size = 0;

            while (reader.Read())
            {
                var timestamp = DateTime.SpecifyKind((DateTime)reader.GetValue(0), DateTimeKind.Utc);
#if USELONGVARBINARY
                var binaryA = ((byte[])reader.GetValue(1));
                var binaryB = ((byte[])reader.GetValue(2));
#else
                var binaryA = ((byte[])reader.GetValue(1));
                var binaryB = ((byte[])reader.GetValue(2));
#endif
                total_size += binaryA.Length + binaryB.Length;
                cnt++;
                // Console.WriteLine(BitConverter.ToString(binaryA)+ BitConverter.ToString(binaryB));
            }

            sw.Stop();
            ms = 1000.0 * sw.ElapsedTicks / Stopwatch.Frequency;
            Console.WriteLine(sw.ElapsedTicks + " " + Stopwatch.Frequency + " " + ms+ " reccnt:"+cnt + " ms/rec:"+ms/cnt+ " total_size:"+ total_size);


            Console.WriteLine("IRIS Server Version:" + IRISConnect.ServerVersion);
            reader.Close();
            IRISConnect.Close();

            Console.WriteLine(ConnectionString);
        }
    }
}
