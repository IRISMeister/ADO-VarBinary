#define IRIS
#define CREATETABLE
using System;
using InterSystems.Data.IRISClient;
using Npgsql;
using System.Threading;
using System.Text;
namespace ConsoleApp
{
    class Program
    {
        //public static readonly int arraysize = 1024*128;

        static void Main(string[] args)
        {

            String host = "iris";
            String port = "1972";
            String username = "SuperUser";
            String password = "SYS";
            String Namespace = "DEMO";

            int arraysize = 1024*128;
            int loopcnt = 1000;
            String errstr;

            if (args.Length >= 1) arraysize = Int32.Parse(args[0]);
            if (args.Length >= 2) host = args[1];
            if (args.Length >= 3) port = args[2];
            if (args.Length >= 4) loopcnt = Int32.Parse(args[3]);

            String sqlStatement = "DROP TABLE TestTable";
#if IRIS
            String sqlStatement1 = "CREATE TABLE TestTable (ts TIMESTAMP, binaryA1 LONGVARBINARY, binaryB1 LONGVARBINARY, binaryA2 VARBINARY("+arraysize+"), binaryB2 VARBINARY("+arraysize+"))";
            String sqlStatement2 = "CREATE INDEX idx1 ON TABLE TestTable (ts)";
            String sqlStatement3 = "select top 1 ts,binaryA1,binaryB1,binaryA2,binaryB2 from TestTable";
#else
            String sqlStatement1 = "CREATE TABLE TestTable (ts TIMESTAMP, binaryA1 bytea, binaryB1 bytea, binaryA2 bytea, binaryB2 bytea)";
            String sqlStatement2 = "CREATE INDEX idx1 ON TestTable (ts)";
            String sqlStatement3 = "select ts,binaryA1,binaryB1,binaryA2,binaryB2 from TestTable";
#endif
            String sqlStatement4 = "TUNE TABLE TestTable";
            String sqlStatement5 = "SELECT \"global\",allocatedMB,usedMB FROM bdb_sql.TableSize('TestTable')";

#if IRIS
            String ConnectionString = "Server = " + host
                + "; Port = " + port + "; Namespace = " + Namespace
                + "; Password = " + password + "; User ID = " + username + "; SharedMemory=false;pooling=true;Max Pool Size=3";
            IRISConnection con = new IRISConnection();
            IRISCommand cmd;
            IRISCommand cmd1;
            cmd = new IRISCommand(sqlStatement, con);
            cmd1 = new IRISCommand(sqlStatement1, con);
#else
            String ConnectionString = "Server = postgres" 
                + "; Port = 5432" + "; Database = demo" 
                + "; Password = postgres" + "; Username = postgres";
            NpgsqlConnection con = new NpgsqlConnection();
            NpgsqlCommand cmd;
            NpgsqlCommand cmd1;
            cmd = new NpgsqlCommand(sqlStatement, con);
            cmd1 = new NpgsqlCommand(sqlStatement1, con);
#endif
            con.ConnectionString = ConnectionString;


#if CREATETABLE
            con.Open();
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) { errstr = e.ToString(); }
            cmd1.ExecuteNonQuery();

#if IRIS
            cmd = new IRISCommand(sqlStatement2, con);
#else
            cmd = new NpgsqlCommand(sqlStatement2, con);
#endif
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            con.Close();
#endif
            Console.WriteLine("Start writing....");
            int result=0;
            MainJob mainJob = new MainJob(ConnectionString);
            for (int r = 0; r < loopcnt; r++)
            {
                result=mainJob.ExecSync(arraysize, r);
                if (result==1) {
                    Console.WriteLine("fatal error. Exiting.");
                    Environment.Exit(result);
                }
            }

            Console.WriteLine("");

            con.Open();
#if IRIS
            cmd = new IRISCommand(sqlStatement3, con);
            IRISDataReader reader;
#else
            cmd = new NpgsqlCommand(sqlStatement3, con);
            NpgsqlDataReader reader;
#endif
            reader = cmd.ExecuteReader();

            Console.WriteLine("showing the first line.");
            reader.Read();
            var binaryA1 = ((byte[])reader.GetValue(1));
            var binaryB1 = ((byte[])reader.GetValue(2));
            reader.Close();

            int str_limit=arraysize;
            if (str_limit>30) {str_limit=30;} 
            Console.WriteLine(BitConverter.ToString(binaryA1).Substring(0,str_limit)+"... "+BitConverter.ToString(binaryB1).Substring(0,str_limit)+"...");

            Console.WriteLine("arraysize:"+arraysize);
            Console.WriteLine("");

#if IRIS
            cmd = new IRISCommand(sqlStatement5, con);
            reader = cmd.ExecuteReader();

            // show global size
            while (reader.Read())
            {
                var global=reader.GetValue(0);
                var allocatedMB=reader.GetValue(1);
                var usedMB=reader.GetValue(2);
                Console.WriteLine($"global:{global} allocatedMB:{allocatedMB} usedMB:{usedMB}");
            }
            reader.Close();
            Console.WriteLine("");
            Console.WriteLine("IRIS Server Version:" + con.ServerVersion);
#else
#endif

#if IRIS
            cmd = new IRISCommand(sqlStatement4, con);
            cmd.ExecuteNonQuery();
#else
            //cmd = new NpgsqlCommand(sqlStatement4, con); no equivalent
#endif

            con.Close();

            Console.WriteLine(ConnectionString);
        }
    }
}
