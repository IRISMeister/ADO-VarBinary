using System;
using InterSystems.Data.IRISClient;
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

            String errstr;
            int loopcnt = 1000;

            if (args.Length >= 1) {}
            if (args.Length >= 2) host = args[1];
            if (args.Length >= 3) port = args[2];
            if (args.Length >= 4) loopcnt = Int32.Parse(args[3]);

            double[] data = new double[loopcnt];


            String ConnectionString = "Server = " + host
                + "; Port = " + port + "; Namespace = " + Namespace
                + "; Password = " + password + "; User ID = " + username + "; SharedMemory=false;pooling=true;Max Pool Size=3";
            IRISConnection IRISConnect = new IRISConnection();
            IRISConnect.ConnectionString = ConnectionString;

            IRISConnect.Open();

            String sqlStatement = "DROP TABLE TestTable";
            String sqlStatement2 = "CREATE TABLE TestTable (ts TIMESTAMP, binaryA1 LONGVARBINARY, binaryB1 LONGVARBINARY, binaryA2 VARBINARY(262144), binaryB2 VARBINARY(262144))";
            String sqlStatement1 = "CREATE INDEX idx1 ON TABLE TestTable (ts)";
            String sqlStatement3 = "select top 1 ts,binaryA1,binaryB1,binaryA2,binaryB2 from TestTable";

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
            // This case makes async calls to access IRIS.
            MainJob mainJob = new MainJob(ConnectionString);
            for (int r = 0; r < loopcnt; r++)
            {
                mainJob.ExecSync(data, r);
            }

            //wait last job to finish
            Console.WriteLine("");

            IRISConnect.Open();
            IRISCommand cmd3 = new IRISCommand(sqlStatement3.ToString(), IRISConnect);
            IRISDataReader reader = cmd3.ExecuteReader();

            Console.WriteLine("showing the first line.");
            reader.Read();
            var timestamp = DateTime.SpecifyKind((DateTime)reader.GetValue(0), DateTimeKind.Utc);
            var binaryA1 = ((byte[])reader.GetValue(1));
            var binaryB1 = ((byte[])reader.GetValue(2));
            Console.WriteLine(BitConverter.ToString(binaryA1) + BitConverter.ToString(binaryB1));

            Console.WriteLine("IRIS Server Version:" + IRISConnect.ServerVersion);
            reader.Close();
            IRISConnect.Close();

            Console.WriteLine(ConnectionString);
        }
    }
}
