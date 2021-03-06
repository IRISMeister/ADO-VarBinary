#define IRIS
using System;
using InterSystems.Data.IRISClient;
using Npgsql;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using MathNet.Numerics.Statistics;
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
            String Namespace = "DEMO";

            if (args.Length >= 1) host = args[0];
            if (args.Length >= 2) port = args[1];

            List<double> stat_data = new List<double>();

#if IRIS
            String ConnectionString = "Server = " + host
                + "; Port = " + port + "; Namespace = " + Namespace
                + "; Password = " + password + "; User ID = " + username + "; SharedMemory=false;pooling=true;";

            IRISConnection con = new IRISConnection();
#else
            String ConnectionString = "Server = postgres" 
                + "; Port = 5432" + "; Database = demo" 
                + "; Password = postgres" + "; Username = postgres";
            NpgsqlConnection con = new NpgsqlConnection();
#endif
            con.ConnectionString = ConnectionString;
            con.Open();

            String fromStatement = "from TestTable where ts<'2022-01-01 00:01:39' and ts>='2022-01-01 00:01:09'";
            String sqlStatement1 = "select ts,binaryA1,binaryB1 " + fromStatement;
            String sqlStatement2 = "select ts,binaryA2,binaryB2 " + fromStatement;

            Console.WriteLine("LONGVARBINARY");
            stat_data.Clear();
            for (int i=0; i<10; i++ ) RunQuery(stat_data,con,sqlStatement1);
            ShowStats(stat_data);

            Console.WriteLine("VARBINARY");
            stat_data.Clear();
            for (int i=0; i<10; i++ ) RunQuery(stat_data,con,sqlStatement2);
            ShowStats(stat_data);

            Console.WriteLine("IRIS Server Version:" + con.ServerVersion);
            Console.WriteLine(ConnectionString);
            con.Close();
        }
        static void ShowStats(List<double> stat_data) {
            double [] data = stat_data.ToArray();
            Console.WriteLine("Iterate#:{0},Mean:{1},Median:{2},StandardDeviation:{3},Minimum:{4},Maximum:{5}", data.Length,data.Mean(),data.Median(),data.StandardDeviation(),data.Minimum(),data.Maximum());
        }

#if IRIS
        static void RunQuery(List<double> stat_data,IRISConnection con, String sql) {
#else
        static void RunQuery(List<double> stat_data,NpgsqlConnection con, String sql) {
#endif
            Stopwatch sw;
            double ms;
            int cnt = 0;
            int total_size = 0;
            byte[] binaryA;
            byte[] binaryB;
            int str_limit=0;
            int binary_size=0;
            String buff=null;

#if IRIS
            IRISCommand cmd = new IRISCommand(sql, con);
            IRISDataReader reader;
#else
            NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            NpgsqlDataReader reader;
#endif
            // Read through once to keep them in global buffer (which is set to 2GB via merge.cpf )
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                binaryA = ((byte[])reader.GetValue(1));
                binaryB = ((byte[])reader.GetValue(2));
            }
            reader.Close();

            reader = cmd.ExecuteReader();
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

                if (cnt==0) {  // check it just once.
                    binary_size=binaryA.Length;  // it's all the same.
                    str_limit=binary_size;
                    if (str_limit>30) {str_limit=30;} 
                    buff = BitConverter.ToString(binaryA).Substring(0,str_limit);
                    if (buff != "00-01-02-03-04-05-06-07-08-09-".Substring(0,str_limit)) {
                        Console.WriteLine($" !!! binaryA ({buff}) is not what we expected !!!");
                    }
                    buff = BitConverter.ToString(binaryB).Substring(0,str_limit);
                    if (buff != "00-01-02-03-04-05-06-07-08-09-".Substring(0,str_limit)) {
                        Console.WriteLine($" !!! binaryB ({buff}) is not what we expected !!!");
                    }
                    //Console.WriteLine(BitConverter.ToString(binaryA).Substring(0,str_limit)+"... "+BitConverter.ToString(binaryB).Substring(0,str_limit)+"...");
                }

                cnt++;
            }
            reader.Close();

            sw.Stop();
            ms = 1000.0 * sw.ElapsedTicks / Stopwatch.Frequency;
            stat_data.Add(ms);
            Console.WriteLine(ms+ " reccnt:"+cnt + " ms/rec:"+ms/cnt+ " total_size:"+ total_size);
            if (total_size != binary_size*cnt*2 ) { 
                Console.WriteLine($" !!! total_size of binary fileds ({total_size}) is not what we expected ({binary_size*cnt*2}) !!!");
            }

        }
    }
}
