using System.Collections.Generic;
using System;

namespace KladrParser
{
    public class Kladr : KladrBase
    {
        public Code code { get; set; }
        public List<Kladr> KladrList { get; set; } = new List<Kladr>();

    }

    public class KladrBase
    {
        public virtual string CODE { get; set; }
        public virtual string PARENTKLADR { get; set; }
        public string NAME { get; set; }
        public virtual int? LEVEL { get; set; }
        public string KORP { get; set; }
        public string SOCR { get; set; }
        public virtual string SOCRNAME { get; set; }
        public string INDEX { get; set; }
        public string GNINMB { get; set; }
        public string UNO { get; set; }
        public string OCATD { get; set; }
        public string STATUS { get; set; }
    }
}
