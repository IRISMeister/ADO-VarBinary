#define IRIS
using System;
using InterSystems.Data.IRISClient;
using Npgsql;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;

namespace ConsoleApp
{
    class MainJob
    {
        private String connstr = null;

        private int AccessIRIS(int arraysize, int seq)
        {

#if IRIS
            IRISConnection con = null;
            IRISCommand cmdInsert = null;
#else
            NpgsqlConnection con = null;
            NpgsqlCommand cmdInsert = null;
#endif

            int result=0;
            DateTime dt = new DateTime(2022, 1, 1, 0, 0, 0); //2022-01-01 00:00:00

            try
            {
#if IRIS
                con = new IRISConnection();
                String sqlInsert="INSERT INTO TestTable VALUES (?,?,?,?,?)";
#else
                con = new NpgsqlConnection();
                String sqlInsert="INSERT INTO TestTable (ts,binaryA1,binaryB1,binaryA2,binaryB2) VALUES (@ts,@binaryA1,@binaryB1,@binaryA2,@binaryB2)";
#endif
                con.ConnectionString = connstr;
                con.Open();


#if IRIS
                cmdInsert = new IRISCommand(sqlInsert, con);
#else
                cmdInsert = new NpgsqlCommand(sqlInsert, con);
#endif

                var binaryA = new byte[arraysize];
                var binaryB = new byte[arraysize];

                for (int i = 0; i < arraysize; i++)
                {
                    binaryA[i] = (byte)(i%256); //0x10;
                    binaryB[i] = (byte)(i%256);  //0x20;
                }

                cmdInsert.Parameters.Clear();
                dt = dt.AddSeconds(seq/10);  // 10 records/sec
#if IRIS
                cmdInsert.Parameters.Add("@ts", System.Data.SqlDbType.DateTime).Value = dt;
                cmdInsert.Parameters.Add("@binaryA1", System.Data.SqlDbType.VarBinary).Value = binaryA;
                cmdInsert.Parameters.Add("@binaryB1", System.Data.SqlDbType.VarBinary).Value = binaryB;
                cmdInsert.Parameters.Add("@binaryA2", System.Data.SqlDbType.VarBinary).Value = binaryA;
                cmdInsert.Parameters.Add("@binaryB2", System.Data.SqlDbType.VarBinary).Value = binaryB;
#else
                cmdInsert.Parameters.Add("@ts", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = dt;
                cmdInsert.Parameters.Add("@binaryA1", NpgsqlTypes.NpgsqlDbType.Bytea).Value = binaryA;
                cmdInsert.Parameters.Add("@binaryB1", NpgsqlTypes.NpgsqlDbType.Bytea).Value = binaryB;
                cmdInsert.Parameters.Add("@binaryA2", NpgsqlTypes.NpgsqlDbType.Bytea).Value = binaryA;
                cmdInsert.Parameters.Add("@binaryB2", NpgsqlTypes.NpgsqlDbType.Bytea).Value = binaryB;

#endif                
                cmdInsert.Prepare();  // IRIS can prepare before adding parameters. PG cannot.
                cmdInsert.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                result=1;
            }
            finally
            {
                cmdInsert.Dispose();

                con.Close();
                con.Dispose();
            }
            return result;
        }
        public int ExecSync(int arraysize, int seq)
        {
            return AccessIRIS(arraysize,seq);
        }

        public MainJob(String connstr)
        {
            this.connstr = connstr;
        }
    }
}
