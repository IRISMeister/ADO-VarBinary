#define CREATETABLE
using System;
using InterSystems.Data.IRISClient;
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

            String ConnectionString = "Server = " + host
                + "; Port = " + port + "; Namespace = " + Namespace
                + "; Password = " + password + "; User ID = " + username + "; SharedMemory=false;pooling=true;Max Pool Size=3";
            IRISConnection IRISConnect = new IRISConnection();
            IRISConnect.ConnectionString = ConnectionString;

            String sqlStatement = "DROP TABLE TestTable";
            String sqlStatement1 = "CREATE TABLE TestTable (ts TIMESTAMP, binaryA1 LONGVARBINARY, binaryB1 LONGVARBINARY, binaryA2 VARBINARY("+arraysize+"), binaryB2 VARBINARY("+arraysize+"))";
            String sqlStatement2 = "CREATE INDEX idx1 ON TABLE TestTable (ts)";
            String sqlStatement3 = "select top 1 ts,binaryA1,binaryB1,binaryA2,binaryB2 from TestTable";
            String sqlStatement4 = "TUNE TABLE TestTable";
            String sqlStatement5 = "SELECT \"global\",allocatedMB,usedMB FROM bdb_sql.TableSize('TestTable')";

            IRISCommand cmd;
            IRISCommand cmd1;
#if CREATETABLE
            IRISConnect.Open();
            cmd = new IRISCommand(sqlStatement, IRISConnect);
            cmd1 = new IRISCommand(sqlStatement1, IRISConnect);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) { errstr = e.ToString(); }
            cmd1.ExecuteNonQuery();

            cmd = new IRISCommand(sqlStatement2, IRISConnect);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            IRISConnect.Close();
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

            IRISDataReader reader;
            IRISConnect.Open();
            cmd = new IRISCommand(sqlStatement3, IRISConnect);
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

            cmd = new IRISCommand(sqlStatement5, IRISConnect);
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
            Console.WriteLine("IRIS Server Version:" + IRISConnect.ServerVersion);

            IRISCommand cmd4 = new IRISCommand(sqlStatement4, IRISConnect);
            cmd4.ExecuteNonQuery();

            IRISConnect.Close();

            Console.WriteLine(ConnectionString);
        }
    }
}
