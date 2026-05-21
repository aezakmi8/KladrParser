using System;
using System.Data;

namespace KladrParser
{
    public class WorkDBF
    {
        private System.Data.Odbc.OdbcConnection Conn = null;
        public DataTable Execute(string Command, string tableName)
        {
            DataTable dt = null;
            if (Conn != null)
            {
                try
                {
                    if(Conn.State == ConnectionState.Closed)
                        Conn.Open();
                    dt = new DataTable() { TableName = tableName };
                    System.Data.Odbc.OdbcCommand oCmd = Conn.CreateCommand();
                    oCmd.CommandText = Command;
                    dt.Load(oCmd.ExecuteReader());
                    Conn.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return dt;
        }
        public DataTable GetAll(string DB_path)
        {
            return Execute("SELECT * FROM " + DB_path, DB_path);
        }

        public WorkDBF(string DefaultDir = null)
        {
            if(DefaultDir == null)
                DefaultDir = "C:\\;";
            this.Conn = new System.Data.Odbc.OdbcConnection();
            Conn.ConnectionString =/* @"Driver={Microsoft Access dBASE Driver (.dbf, .ndx, *.mdx)};" +*/
                @"DSN=dBASE Files;" +
                   "SourceType=DBF;Exclusive=No;" +
                   "DefaultDir=" + DefaultDir + ';' +
                   "Collate=Machine;NULL=NO;DELETED=NO;" +
                   "BACKGROUNDFETCH=NO;";
        }
    }
}
