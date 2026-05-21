using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KladrParser
{
    public class ParserLayer
    {
        public static string[] ContentTypes { get; } = new string[] { "region", "district", "city", "street", "building" };
        public static string[] TableTypes = new string[] { "REGEONS", "CITYS", "DOMA", "STREET", "SOCRBASE", "KladrBase" };
        public List<Kladr> UnitedData = new List<Kladr>();

        public void AddTable(DataTable sourse)
        {
            AddTable(sourse, sourse.TableName);
        }
        public void AddTable(DataTable sourse, string tableName)
        {
            if (tableName == "KLADR")
            {
                UniteData(sourse.AsEnumerable().Where(row => row.Field<string>("CODE").Contains("00000000000")).CopyToDataTable(), "REGEONS");
                UniteData(sourse.AsEnumerable().Where(row => !row.Field<string>("CODE").Contains("00000000000")).CopyToDataTable(), "CITYS");
            }
            else
                UniteData(sourse, tableName);
        }

        private void UniteData(DataTable sourse, string tableName)
        {
            UnitedData.AddRange(UniformData(sourse, tableName));
        }

        public static List<Kladr> UniformData(DataTable sourse, string tableName)
        {
            if (TableTypes.FirstOrDefault(p => p == tableName) == null)
                return null;

            ConcurrentBag<Kladr> data = new ConcurrentBag<Kladr>();

            Parallel.ForEach(sourse.Rows.Cast<DataRow>(), row =>
            {
                Code code = null;
                code = new Code(row.Field<string>("CODE"));
                var kladrobj = new Kladr()
                {
                    CODE = row.Field<string>("CODE"),
                    code = code,
                    NAME = row.Field<string>("NAME"),
                    LEVEL = code.Level,
                    PARENTKLADR = code.ParentKladrCode,
                    SOCR = row.Field<string>("SOCR"),
                    SOCRNAME = row.Field<string>("SOCRNAME"),
                    INDEX = row.Field<string>("INDEX"),
                    GNINMB = row.Field<string>("GNINMB"),
                    UNO = row.Field<string>("UNO"),
                    OCATD = row.Field<string>("OCATD")

                };
                if (sourse.Columns.Contains("STATUS"))
                    kladrobj.STATUS = row["STATUS"].ToString();
                data.Add(kladrobj);
            });
            return data.ToList();
        }

        public bool VerifyData(DataTable SOCRBASE)
        {
            try
            {
                if (SOCRBASE == null || SOCRBASE.Rows.Count == 0)
                    return false;

                //проставляю полное имя, от сокращения
                UnitedData.AsParallel().ForAll(item =>
                {
                    DataRow bb = SOCRBASE.AsEnumerable().FirstOrDefault(row => item.SOCR.Equals(row.Field<string>("SCNAME").Trim()) && item.LEVEL == Convert.ToInt32(row.Field<string>("LEVEL").Trim()));
                    if (bb != null && bb["SOCRNAME"] != DBNull.Value)
                        item.SOCRNAME = bb.Field<string>("SOCRNAME");
                });

                //обработка для особенных уровней, которые не сопоставляются по шаблону, в основном это Level 5
                foreach (Kladr kladrobj in UnitedData.Where(p => p.SOCRNAME == null && p.code.АА.EndsWith("00")))
                {
                    DataRow sokritem = SOCRBASE.AsEnumerable().FirstOrDefault(i => kladrobj.SOCR.Equals(i.Field<string>("SCNAME").Trim()));
                    if (sokritem != null)
                    {
                        kladrobj.SOCRNAME = sokritem["SOCRNAME"] != DBNull.Value ? sokritem.Field<string>("SOCRNAME") : null;
                        if (sokritem["LEVEL"] != DBNull.Value)
                            kladrobj.LEVEL = Convert.ToInt32(sokritem.Field<string>("LEVEL"));
                        else
                            kladrobj.LEVEL = null;
                    }
                }

                //удалённые, пропавшие, забытые и тп.
                List<Kladr> rAA = UnitedData.Where(p => p.SOCRNAME == null && !p.code.АА.EndsWith("00")).ToList();
                //оставщийся хлам, которого нет в базе сокращений и/или несуществующие дома
                List<Kladr> sht = UnitedData.Where(p => p.code.АА == null || !p.code.АА.EndsWith("00")).ToList();
                List<Kladr> sht1 = UnitedData.Where(p => p.SOCRNAME == null && p.code.АА.EndsWith("00")).ToList();

                UnitedData = UnitedData.Where(p => p.code.АА == null || p.code.АА.EndsWith("00")).ToList();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
}