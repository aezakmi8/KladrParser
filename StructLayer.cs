using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KladrParser
{
    public class StructLayer
    {
        private int printcounter = 1;
        private bool printDataInfo;

        public List<Kladr> MakeStruct(string parentCode, List<Kladr> collection, bool AsParralel, bool printDataInfo = false)
        {
            var code = new Code(parentCode);
            var parent = new Kladr() { CODE = parentCode, code = code, LEVEL = code.Level };

            parent = MakeStruct(parent, collection, AsParralel, printDataInfo);
            return parent?.KladrList;
        }

        public Kladr MakeStruct(Kladr parent, List<Kladr> collection, bool AsParralel, bool printDataInfo = false)
        {
            if (collection == null || collection.Count == 0)
                return null;
            this.printDataInfo = printDataInfo;
            printcounter = 1;
            MakeStruct(parent, collection.Where(p => p.LEVEL > parent.LEVEL).OrderBy(p => p.CODE).ToList());
            return parent;
        }

        private void MakeStruct(Kladr parent, List<Kladr> collection)
        {
            if (printDataInfo)
                Console.WriteLine("Итого: " + printcounter++ + " {0} {1} {2} {3}", parent.code.ToString(), parent.LEVEL, parent.SOCRNAME, parent.NAME);

            string parentCodeforFilter = parent.code.GetCodebyLevel((int)parent.LEVEL, false, false);

            if (parentCodeforFilter == null)
                return;

            List<Kladr> childcollection = collection.Where(p => p.CODE != parent.CODE && p.CODE.StartsWith(parentCodeforFilter)).ToList();
            var forlist = childcollection.Where(p => p.PARENTKLADR.StartsWith(parent.CODE)).ToList();

            //forlist.AsParallel().ForAll(item =>
            //{
            //    parent.KladrList.Add(item);
            //    MakeStruct(item, childcollection);
            //});

            foreach (Kladr item in forlist)
            {
                parent.KladrList.Add(item);
                MakeStruct(item, childcollection);
            }
        }

    }
}
