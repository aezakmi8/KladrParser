using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using SQLite;
using XCoreWrapper.Utils;

namespace KladrParser
{
    public static class WorkSQLLite
    {
        public static void DataTableIntoSQLLite(string path, DataTable DataTable)
        {
            ArrayList list = new ArrayList();

            foreach (DataColumn dc in DataTable.Columns)
            {
                string fieldName = dc.ColumnName;
                list.Add(fieldName);
            }

            string conn = GetConnection(path);
            using (SQLiteConnection con = new SQLiteConnection(conn))
            {
                //if (con.State == ConnectionState.Closed)
                //    con.Open();

                //SQLiteCommand cmd = new SQLiteCommand();
                //cmd.Connection = con;

                int couner = 0;

                foreach (DataRow row in DataTable.Rows)
                {
                    string insertSql = "insert into " + "KLADR" + " values(";

                    Console.WriteLine(couner++);

                    for (int i = 0; i < list.Count; i++)
                    {
                        insertSql = insertSql + "'" + ReplaceEscape(row[list[i].ToString()].ToString()) + "',";
                    }

                    insertSql = insertSql.Substring(0, insertSql.Length - 1) + ")";

                    //cmd.CommandText = insertSql;

                    //cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }

        public static List<Kladr> SelectQuery(string query, string path)
        {
            string conn = GetConnection(path);
            //DataTable dt = new DataTable();
            try
            {
                using (SQLiteConnection con = new SQLiteConnection(path))
                {
                    var result = con.Query<KladrBase>(query);
                    return NS.Map<List<Kladr>>(result);

                    //con.Open();
                    //using (var ad = new SQLiteDataAdapter(query, con))
                    //    ad.Fill(dt);
                    //con.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }


            //return dt;: DataSource=C:\FlowersService\KladrBase\KLADRBASE.db; Version=3
            return null;
        }

        private static string GetConnection(string path)
        {
            return @"DataSource=" + path + "; Version=3;";
        }

        public static string ReplaceEscape(string str)
        {
            str = str.Replace("'", "''");
            return str;
        }
    }
}
