using System;
using InterSystems.Data.IRISClient;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;

namespace ConsoleApp
{
    class MainJob
    {
        public const int ARRAYSIZE = 1024*256;

        private String connstr = null;

        private void AccessIRIS(double[] data, int seq)
        {

            String tablename = "";

            IRISConnection IRISConnect = null;
            IRISCommand cmdInsert = null;
            DateTime dt = new DateTime(2022, 1, 1, 0, 0, 0); //2022-01-01 00:00:00


            try
            {
                IRISConnect = new IRISConnection();
                IRISConnect.ConnectionString = connstr;
                IRISConnect.Open();

                String irisjobid=IRISConnect.IRISJobID;
                var sqlInsert = new StringBuilder();
                tablename = "TestTable4"; sqlInsert.AppendLine($"INSERT INTO {tablename} VALUES (?,?,?,?,?)");



                cmdInsert = new IRISCommand(sqlInsert.ToString(), IRISConnect);
                cmdInsert.Prepare();
                var sw = new Stopwatch();

                var resp = new byte[ARRAYSIZE];
                var hr = new byte[ARRAYSIZE];

                for (int i = 0; i < ARRAYSIZE; i++)
                {
                    resp[i] = 0x10;
                    hr[i] = 0x20;
                }
                // var hr = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

                cmdInsert.Parameters.Clear();
                dt = dt.AddSeconds(seq/10);  // 10 records/sec
                cmdInsert.Parameters.Add("@ts", System.Data.SqlDbType.DateTime).Value = dt.ToString("yyyy/MM/dd HH:mm:ss");
                cmdInsert.Parameters.Add("@resp", System.Data.SqlDbType.VarBinary).Value = resp;
                cmdInsert.Parameters.Add("@hr", System.Data.SqlDbType.VarBinary).Value = hr;
                cmdInsert.Parameters.Add("@resp2", System.Data.SqlDbType.VarChar).Value = "AAA";  //resp.ToString();
                cmdInsert.Parameters.Add("@hr2", System.Data.SqlDbType.VarChar).Value = hr.ToString();
                cmdInsert.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                cmdInsert.Dispose();

                IRISConnect.Close();
                IRISConnect.Dispose();
            }
        }
            public void ExecSync(double[] data,int seq)
        {
            Task t=Task.Run(() => AccessIRIS(data,seq));
            t.Wait();
        }
        public void Exec(double[] data, int seq)
        {
            Task.Run(() => AccessIRIS(data, seq));
        }

        public MainJob(String connstr)
        {
            this.connstr = connstr;
        }
    }
}
