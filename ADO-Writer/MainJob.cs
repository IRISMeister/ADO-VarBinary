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

            IRISConnection IRISConnect = null;
            IRISCommand cmdInsert = null;
            DateTime dt = new DateTime(2022, 1, 1, 0, 0, 0); //2022-01-01 00:00:00

            try
            {
                IRISConnect = new IRISConnection();
                IRISConnect.ConnectionString = connstr;
                IRISConnect.Open();

                String sqlInsert="INSERT INTO TestTable VALUES (?,?,?,?,?)";

                cmdInsert = new IRISCommand(sqlInsert, IRISConnect);
                cmdInsert.Prepare();
                var sw = new Stopwatch();

                var binaryA = new byte[ARRAYSIZE];
                var binaryB = new byte[ARRAYSIZE];

                for (int i = 0; i < ARRAYSIZE; i++)
                {
                    binaryA[i] = (byte)(i%256); //0x10;
                    binaryB[i] = (byte)(i%256);  //0x20;
                }

                cmdInsert.Parameters.Clear();
                dt = dt.AddSeconds(seq/10);  // 10 records/sec
                cmdInsert.Parameters.Add("@ts", System.Data.SqlDbType.DateTime).Value = dt.ToString("yyyy/MM/dd HH:mm:ss");
                cmdInsert.Parameters.Add("@binaryA1", System.Data.SqlDbType.VarBinary).Value = binaryA;
                cmdInsert.Parameters.Add("@binaryB1", System.Data.SqlDbType.VarBinary).Value = binaryB;
                cmdInsert.Parameters.Add("@binaryA2", System.Data.SqlDbType.VarBinary).Value = binaryA;
                cmdInsert.Parameters.Add("@binaryB2", System.Data.SqlDbType.VarBinary).Value = binaryB;
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
