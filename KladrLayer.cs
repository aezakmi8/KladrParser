using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Configuration;

namespace KladrParser
{
    public class KladrLayer
    {
        /// <summary>
        /// Получить Кладр-Объект с родительскими элементами
        /// </summary>
        /// <param name="code"></param>
        /// <param name="isFromFirstParent">структура от родителей с первого уровня</param>
        /// <returns></returns>
        public Kladr GetKladrFromParent(string code, bool isFromFirstParent = false)
        {
            var list = new List<Kladr>();
            Code codeinst = new Code(code);
            if (isFromFirstParent) 
            {
                var arr = new List<string>();
                
                for (int i = 1; i <= codeinst.Level; i++)
                {
                    string somelvl = codeinst.GetCodebyLevel(i, false);
                    if (somelvl != null)
                        arr.Add(somelvl);
                }
                list = GetKladrByCode(arr.AsEnumerable().Distinct().ToArray());
            }
            else
                list = GetKladrByCode(new string[] { codeinst.ParentKladrCode, code });

            if (list == null || list.Count == 0)
                return null;

            return new StructLayer().MakeStruct(list.OrderBy(p => p.LEVEL).FirstOrDefault(), list, AsParralel: false, printDataInfo: false);
        }

        /// <summary>
        /// Добавить к Кладр-Объекту родительские элементы
        /// </summary>
        /// <param name="child"></param>
        /// <param name="isFromFirstParent">структура от родителей с первого уровня</param>
        /// <returns></returns>
        public Kladr GetKladrFromParent(Kladr child, bool isFromFirstParent = false)
        {
            var list = new List<Kladr>();
            if (isFromFirstParent)
            {
                var arr = new List<string>();

                for (int i = 1; i <= child.LEVEL; i++)
                {
                    string somelvl = child.code.GetCodebyLevel(i, false);
                    if (somelvl != null)
                        arr.Add(somelvl);
                }
                list = GetKladrByCode(arr.AsEnumerable().Distinct().ToArray());
            }
            else
                list = GetKladrByCode(new string[] { child.PARENTKLADR, child.CODE });

            if (list == null || list.Count == 0)
                return null;

            return new StructLayer().MakeStruct(list.OrderBy(p => p.LEVEL).FirstOrDefault(), list, AsParralel: false, printDataInfo: false);
        }


        /// <summary>
        /// Получить список кладр-объектов от родителя по имени
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public List<Kladr> GetKladrFromParentByName(string name)
        {
            string exp =
                "SELECT * FROM KLADR WHERE " +
                "CODE in (SELECT PARENTKLADR FROM KLADR WHERE LEVEL < 6 AND NAME like '%@name%') " +
                "UNION " +
                "SELECT* FROM KLADR WHERE LEVEL < 6 AND NAME like '%@name%'";
            exp = exp.Replace("@name", name);
            //DataTable table = WorkSQLLite.SelectQuery(exp, GetDBPath());
            //var list = ParserLayer.UniformData(table, "KladrBase");
            var list = WorkSQLLite.SelectQuery(exp, GetDBPath());
            var list2 = new List<Kladr>(list);
            var result = new List<Kladr>();
            foreach (var item in list)
            {
                result.Add(new StructLayer().MakeStruct(item, list, AsParralel: false, printDataInfo: false));
            }
            result = result.Where(p => p.KladrList.Count > 0).ToList();
            result.AddRange(list2.Where(p => p.LEVEL == 1));
            return result;
        }


        /// <summary>
        /// Получить список дочерних кладр-объектов по коду
        /// </summary>
        /// <param name="code"></param>
        /// <param name="depth">Глубина поиска по уровням, по умолчанию - оптимальное</param>
        /// <returns></returns>
        public List<Kladr> GetKladrList(string code, int depth = 2)
        {
            Code codeinst = new Code(code);
            Kladr parent = new Kladr() { CODE = code, code = codeinst, LEVEL = codeinst.Level };
            var result = GetKladrList(parent, depth);
            return result?.KladrList;
        }


        /// <summary>
        /// Получить список дочерних кладр-объектов по коду
        /// </summary>
        /// <param name="code"></param>
        /// <param name="depth">Глубина поиска по уровням, по умолчанию - оптимальное</param>
        /// <returns></returns>
        public List<Kladr> GetKladrListNoStruct(string code, string levels)
        {
            string expression = @"SELECT * FROM KLADR where PARENTKLADR LIKE '" + code + "%' and LEVEL in (" + levels + ");";
            //DataTable table = WorkSQLLite.SelectQuery(expression, GetDBPath());
            //List<Kladr> list = ParserLayer.UniformData(table, "KladrBase");
            var list = WorkSQLLite.SelectQuery(expression, GetDBPath());
            return list;
        }

        /// <summary>
        /// Заполнить Кладр-Объект дочерними элементами
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="depth">Глубина поиска по уровням, по умолчанию - оптимальное</param>
        /// <returns></returns>
        public Kladr GetKladrList(Kladr parent, int depth = 2)
        {
            string expression = @"SELECT * FROM KLADR where CODE like '" + parent.code.GetCodebyLevel(parent.code.Level, false, false) + "%' and LEVEL <=" + (parent.LEVEL + depth) + ";";
            //DataTable table = WorkSQLLite.SelectQuery(expression, GetDBPath());
            //List<Kladr> list = ParserLayer.UniformData(table, "KladrBase");
            var list = WorkSQLLite.SelectQuery(expression, GetDBPath());

            var result = new StructLayer().MakeStruct(parent, list, AsParralel: false, printDataInfo: false);
            return result;
        }

        /// <summary>
        /// Получить Кладр-Объект по коду, без дочерних
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public List<Kladr> GetKladrByCode(string code)
        {
            return GetKladrByCode(new string[] { code });
        }
        private List<Kladr> GetKladrByCode(string[] codes)
        {
            string exp = string.Empty;

            foreach (string row in codes)
            {
                if (row == null)
                    continue;
                exp = exp + "CODE = '" + row + "' or ";
            }

            if (exp.Length < 4)
                return null;
            exp = exp?.Substring(0, exp.Length - 4);
            if (exp == null || exp == string.Empty)
                return null;

            //DataTable table = WorkSQLLite.SelectQuery(@"SELECT * FROM KLADR where " + exp + " ORDER BY NAME;", GetDBPath());
            //return ParserLayer.UniformData(table, "KladrBase");
            string expression = @"SELECT * FROM KLADR where " + exp + " ORDER BY NAME;";
            return WorkSQLLite.SelectQuery(expression, GetDBPath());
        }
        /// <summary>
        /// Получить Кладр-Объекты похожие по имени, без дочерних
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<Kladr> GetKladrByName(string name)
        {
            string expression = @"SELECT * FROM KLADR where NAME like '%" + name + "%' ORDER BY NAME;";
            return WorkSQLLite.SelectQuery(expression, GetDBPath());

            //DataTable table = WorkSQLLite.SelectQuery(@"SELECT * FROM KLADR where NAME like '%" + name + "%' ORDER BY NAME;", GetDBPath());
            //return ParserLayer.UniformData(table, "KladrBase");
        }

        /// <summary>
        /// Получить список регионов
        /// </summary>
        /// <returns></returns>
        public List<Kladr> GetRegeons()
        {
            //var dbpath = GetDBPath();
            //DataTable table = WorkSQLLite.SelectQuery(@"SELECT * FROM KLADR where LEVEL = 1 ORDER BY NAME;", GetDBPath());
            //return ParserLayer.UniformData(table, "KladrBase");
            string expression = @"SELECT * FROM KLADR where LEVEL = 1 ORDER BY NAME;";
            return WorkSQLLite.SelectQuery(expression, GetDBPath());
        }

        public virtual string GetDBPath()
        {
            return ConfigurationManager.AppSettings["DBKladrSource"];
        }
    }
}
