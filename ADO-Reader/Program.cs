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

            String sqlStatement1 = "select ts,binaryA1,binaryB1 from TestTable where ts<'2022-01-01 00:01:39' and ts>='2022-01-01 00:01:09'";
            String sqlStatement2 = "select ts,binaryA2,binaryB2 from TestTable where ts<'2022-01-01 00:01:39' and ts>='2022-01-01 00:01:09'";

            // LONGVARBINARY
            Console.WriteLine("LONGVARBINARY");
            for (int i=0; i<10; i++ ) RunQuery(IRISConnect,sqlStatement1);

            // VARBINARY
            Console.WriteLine("VARBINARY");
            for (int i=0; i<10; i++ ) RunQuery(IRISConnect,sqlStatement2);

            Console.WriteLine("IRIS Server Version:" + IRISConnect.ServerVersion);
            Console.WriteLine(ConnectionString);
            IRISConnect.Close();
        }

        static void RunQuery(IRISConnection IRISConnect, String sql) {
            Stopwatch sw;
            double ms;
            int cnt = 0;
            int total_size = 0;
            byte[] binaryA;
            byte[] binaryB;

            IRISCommand cmd = new IRISCommand(sql, IRISConnect);
            IRISDataReader reader = cmd.ExecuteReader();

            sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            cnt = 0;
            total_size = 0;

            while (reader.Read())
            {
                binaryA = ((byte[])reader.GetValue(1));
                binaryB = ((byte[])reader.GetValue(2));
                total_size += binaryA.Length + binaryB.Length;
                cnt++;
                //Console.WriteLine(BitConverter.ToString(binaryA)+" "+BitConverter.ToString(binaryB));
            }

            sw.Stop();
            ms = 1000.0 * sw.ElapsedTicks / Stopwatch.Frequency;
            Console.WriteLine(sw.ElapsedTicks + " " + Stopwatch.Frequency + " " + ms+ " reccnt:"+cnt + " ms/rec:"+ms/cnt+ " total_size:"+ total_size);

        }
    }
}
