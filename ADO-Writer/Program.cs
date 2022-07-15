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
            String Namespace = "USER";

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

            IRISConnect.Open();

            String sqlStatement = "DROP TABLE TestTable";
            String sqlStatement2 = "CREATE TABLE TestTable (ts TIMESTAMP, binaryA1 LONGVARBINARY, binaryB1 LONGVARBINARY, binaryA2 VARBINARY("+arraysize+"), binaryB2 VARBINARY("+arraysize+"))";
            String sqlStatement1 = "CREATE INDEX idx1 ON TABLE TestTable (ts)";
            String sqlStatement3 = "select top 1 ts,binaryA1,binaryB1,binaryA2,binaryB2 from TestTable";
            String sqlStatement4 = "TUNE TABLE TestTable";

            IRISCommand cmd = new IRISCommand(sqlStatement, IRISConnect);
            IRISCommand cmd2 = new IRISCommand(sqlStatement2, IRISConnect);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) { errstr = e.ToString(); }
            cmd2.ExecuteNonQuery();

            cmd2 = new IRISCommand(sqlStatement1, IRISConnect);
            cmd2.ExecuteNonQuery();

            cmd.Dispose();
            cmd2.Dispose();
            IRISConnect.Close();

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

            //wait last job to finish
            Console.WriteLine("");

            IRISConnect.Open();
            IRISCommand cmd3 = new IRISCommand(sqlStatement3, IRISConnect);
            IRISDataReader reader = cmd3.ExecuteReader();

            Console.WriteLine("showing the first line.");
            reader.Read();
            var binaryA1 = ((byte[])reader.GetValue(1));
            var binaryB1 = ((byte[])reader.GetValue(2));

            int str_limit=arraysize;
            if (str_limit>30) {str_limit=30;} 
            Console.WriteLine(BitConverter.ToString(binaryA1).Substring(0,str_limit)+"... "+BitConverter.ToString(binaryB1).Substring(0,str_limit)+"...");

            Console.WriteLine("arraysize:"+arraysize);
            Console.WriteLine("IRIS Server Version:" + IRISConnect.ServerVersion);
            reader.Close();

            IRISCommand cmd4 = new IRISCommand(sqlStatement4, IRISConnect);
            cmd4.ExecuteNonQuery();

            IRISConnect.Close();

            Console.WriteLine(ConnectionString);
        }
    }
}
