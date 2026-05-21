using System;
using System.Collections.Generic;
using System.Text;

namespace KladrParser
{
    public class Code
    {
        public string OriginalCode { get => originalcode; }
        private string originalcode;
        /// <summary>
        /// код региона
        /// </summary>
        public string СС { get; set; }
        /// <summary>
        /// код района
        /// </summary>
        public string РРР { get; set; }
        /// <summary>
        /// код города
        /// </summary>
        public string ГГГ { get; set; }
        /// <summary>
        /// код населенного пункта
        /// </summary>
        public string ППП { get; set; }
        /// <summary>
        /// Признак актуальности наименования адресного объекта, может принимать следующие значения:
        ///“00” – актуальный объект(его наименование, подчиненность соответствуют состоянию на данный момент адресного пространства). 
        ///“01”-“50” – объект был переименован, в данной записи приведено одно из прежних его наименований(актуальный адресный объект присутствует в базе данных с тем же кодом, но с признаком актуальности “00”;
        ///“51” –  объект был переподчинен или влился в состав другого объекта(актуальный адресный объект определяется по базе Altnames.dbf);
        ///“52”-“98” – резервные значения признака актуальности;
        ///”99” – адресный объект не существует, т.е.нет соответствующего ему актуального адресного объекта.
        /// </summary>
        public string АА { get; set; }
        /// <summary>
        /// код улицы
        /// </summary>
        public string УУУУ { get; set; }
        /// <summary>
        /// порядковый номер позиции классификатора с обозначениями домов
        /// </summary>
        public string ДДДД { get; set; }
        /// <summary>
        /// Код-Кладр выше текущего уровня
        /// </summary>
        public string ParentKladrCode { get => GetParentCode(); }

        public int Level { get; }

        private bool isAddZeroChars;

        public Code(string code, bool isAddZeroChars = true)
        {
            this.isAddZeroChars = isAddZeroChars;
            originalcode = code;
            HandleCode();
            Level = SelectLevel(code);
        }

        private void HandleCode()
        {
            СС = GetCodebyLevel(1, false, isAddZeroChars);
            РРР = GetCodebyLevel(2, false, isAddZeroChars);
            ГГГ = GetCodebyLevel(3, false, isAddZeroChars);
            ППП = GetCodebyLevel(4, false, isAddZeroChars);
            АА = GetCodebyLevel(4, isZeroChars: false);
            УУУУ = GetCodebyLevel(5, isAddZeroChars);
            ДДДД = GetCodebyLevel(6, isAddZeroChars);

            VerifyHandler();
        }

        public void VerifyHandler()
        {
            string code = OriginalCode;
            if (code.Length == 13) //кладр;
            {
                if (СС != null && code.Substring(0, 2).Equals(OneChar('0', 2)))
                    СС = null;
                if (РРР != null && code.Substring(2, 3).Equals(OneChar('0', 3)))
                    РРР = null;
                if (ГГГ != null && code.Substring(5, 3).Equals(OneChar('0', 3)))
                    ГГГ = null;
                if (ППП != null && code.Substring(8, 3).Equals(OneChar('0', 3)))
                    ППП = null;
            }

            if (code.Length == 17 || code.Length == 19) //улицы; дома;
            {
                if (СС != null && code.Substring(0, 2).Equals(OneChar('0', 2)))
                    СС = null;
                if (РРР != null && code.Substring(2, 3).Equals(OneChar('0', 3)))
                    РРР = null;
                if (ГГГ != null && code.Substring(5, 3).Equals(OneChar('0', 3)))
                    ГГГ = null;
                if (ППП != null && code.Substring(8, 3).Equals(OneChar('0', 3)))
                    ППП = null;
                if (УУУУ != null && code.Substring(11, 4).Equals(OneChar('0', 4)))
                    УУУУ = null;
            }
        }

        public static int SelectLevel(string code)
        {
            switch (code.Length)
            {
                case 13:
                    string codewithoutaa = code.Substring(0, 11) + "00";
                    string zerochars = CharToBreakCount(codewithoutaa, '0');
                    int count = zerochars.Length;
                    if ((count == 11 || count == 12) && codewithoutaa.EndsWith(zerochars))
                        return 1;
                    else
                    if ((count >= 8 && count <= 10) && codewithoutaa.EndsWith(zerochars))
                        return 2;
                    else
                    if ((count >= 5 && count <= 7) && codewithoutaa.EndsWith(zerochars))
                        return 3;
                    else
                    if (count == 2 && code.EndsWith(zerochars))
                        return 4;
                    else return 4;
                case 17:
                    return 5;
                case 19:
                    return 6;
                default:
                    return 6;
            }
        }



        public string GetCodebyLevel(int level, bool includeAA = true, bool isZeroChars = true)
        {
            string code = OriginalCode;
            return GetCodebyLevel(code, level, includeAA, isZeroChars);

        }

        public static string GetCodebyLevel(string code, int level, bool includeAA = true, bool isZeroChars = true)
        {
            string codewithoutaa = code.Substring(0, 11);
            string aa = string.Empty;
            int kladrLength = code.Length;

            if (includeAA && kladrLength <= 13)
                aa = code.Substring(11, 2);

            //кладр;
            if (level == 1) { return codewithoutaa.Substring(0, 2) + (isZeroChars ? OneChar('0', kladrLength - 2) + aa : aa); }
            if (level == 2) { return codewithoutaa.Substring(0, 5) + (isZeroChars ? OneChar('0', kladrLength - 5) + aa : aa); }
            if (level == 3) { return codewithoutaa.Substring(0, 8) + (isZeroChars ? OneChar('0', 13 - 8) + aa : aa); }
            if (level == 4 && kladrLength == 13) { return codewithoutaa.Substring(0, 11) + (isZeroChars ? OneChar('0', kladrLength - 11) + aa : aa); }
            //улицы;
            int streetLength = 17;
            if (kladrLength == streetLength)
            {
                codewithoutaa = code.Substring(0, 15);
                if (includeAA)
                    aa = code.Substring(15, 2);
                if (level == 4) { return codewithoutaa.Substring(0, 11) + (isZeroChars ? OneChar('0', 13 - 11) : aa); }
                if (level == 5) { return codewithoutaa.Substring(0, 15) + aa; }
            }
            //дома;
            int domaLength = 19;
            if (kladrLength == domaLength)
            {
                if (level == 5) { return code.Substring(0, 15) + (isZeroChars ? OneChar('0', kladrLength - 15) : string.Empty); }
                if (level == 6) { return code.Substring(0, 19); }
            }

            return null;
        }

        private string GetParentCode()
        {
            return GetParentCode(Level);
        }
        private string GetParentCode(int level)
        {
            level--;

            if (level < 1)
                return null;

            string result = null;

            switch (level)
            {
                case 1:
                    result = СС;
                    break;
                case 2:
                    result = РРР;
                    break;
                case 3:
                    result = ГГГ;
                    break;
                case 4:
                    result = ППП;
                    break;
                case 5:
                    result = УУУУ;
                    break;
                case 6: //на случай если существует кладр-квартир
                    result = ДДДД;
                    break;
                default:
                    return null;
            }
            if (result == null || !CheckOneChar(result, '0'))
                return GetParentCode(level);
            else
                return result;

        }

        private bool CheckOneChar(string val, char digit, int? count = null)
        {
            int counter = 0;
            for (int i = 0; i < val.Length; i++)
            {
                if (val[i] != digit || (count != null && counter != (int)count))
                    return true;
                counter++;
            }

            return false;
        }

        public static string OneChar(char val, int count)
        {
            return OneChar(val.ToString(), count); ;
        }

        public static string OneChar(string val, int count)
        {
            string value = string.Empty;

            for (int i = 0; i < count; i++)
            {
                value += val;
            }

            return value;
        }

        private static string CharToBreakCount(string val, char digit)
        {
            string value = string.Empty;

            for (int i = val.Length - 1; i >= 0; i--)
            {
                if (val[i] == digit)
                    value += digit;
                else
                    break;
            }

            return value;
        }

        public override string ToString()
        {
            string code = OriginalCode;
            string result = string.Empty;

            switch (code.Length)
            {
                case 13:
                    return string.Join("/", new string[] 
                    {
                        code.Substring(0, 2),
                        code.Substring(2, 3),
                        code.Substring(5, 3),
                        code.Substring(8, 3),
                        code.Substring(11, 2)});
                case 17:
                    return string.Join("/", new string[] 
                    {
                        code.Substring(0, 2), 
                        code.Substring(2, 3), 
                        code.Substring(5, 3), 
                        code.Substring(8, 3),
                        code.Substring(11, 4),
                        code.Substring(15, 2)});
                case 19:
                    return string.Join("/", new string[] 
                    {
                        code.Substring(0, 2),
                        code.Substring(2, 3),
                        code.Substring(5, 3),
                        code.Substring(8, 3),
                        code.Substring(11, 4),
                        code.Substring(15, 4)});
                default:
                    return result;
            }
        }

        public string ToPrint()
        {
            string result = string.Empty;

            result += "Код кладр: " + OriginalCode;
            result += " Код кладр ToString: " + this.ToString();
            result += " LEVEL " + Level;
            result += СС != null ? " Код региона: " + СС : string.Empty;
            result += РРР != null ? " Код района: " + РРР : string.Empty;
            result += ГГГ != null ? " Код города: " + ГГГ : string.Empty;
            result += ППП != null ? " Код населенного пункта: " + ППП : string.Empty;
            result += АА != null ? " Код признака актуальности: " + АА : string.Empty;
            result += УУУУ != null ? " Код улицы: " + УУУУ : string.Empty;
            result += ДДДД != null ? " Порядковый номер дома: " + ДДДД : string.Empty;
            return result;
        }
    }
}
