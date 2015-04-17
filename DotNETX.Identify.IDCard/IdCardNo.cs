using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNETX.Identify.IDCard
{
    public class IdCardNo
    {
        /// <summary>
        /// 县级
        /// </summary>
        public string County { get; set; }

        /// <summary>
        /// 地级
        /// </summary>
        public string Prefecture { get; set; }

        /// <summary>
        /// 省级
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime Birthday { get; set; }

        /// <summary>
        /// 性别：M=Male|F=Female
        /// </summary>
        public string Gender { get; set; }

        public static IdCardNo Parse(string id)
        {
            string error;
            string region_code;
            string str_birthday;
            int gender_code;

            if (!TryCheck(id, out error))
            {
                throw new ArgumentException(error, "id");
            }

            if (id.Length == 15)
            {
                region_code = id.Substring(0, 6);
                str_birthday = "19" + id.Substring(6, 6);
                gender_code = id[14]-'0';
            }
            else
            {
                region_code = id.Substring(0, 6);
                str_birthday = id.Substring(6, 8);
                gender_code = id[16]- '0';
            }

            string province, prefecture, contry, gender;
            DateTime birthday;
            contry = s_County[region_code];
            prefecture = s_Prefecture[region_code.Substring(0, 4)];
            province = s_Province[region_code.Substring(0, 2)];
            birthday = DateTime.ParseExact(str_birthday, "yyyyMMdd", null);
            gender = gender_code % 2 == 0 ? "F" : "M";
            IdCardNo idCardNo = new IdCardNo();
            idCardNo.County = contry;
            idCardNo.Prefecture = prefecture;
            idCardNo.Province = province;
            idCardNo.Birthday = birthday;
            idCardNo.Gender = gender;
            return idCardNo;
        }

        public static bool TryParse(string id, out IdCardNo idCardNo)
        {
            try
            {
                idCardNo = Parse(id);
                return true;
            }
            catch
            {
                idCardNo = null;
                return false;
            }
        }

        public static bool TryCheck(string id, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(id)) error = "空身份证号";
            if (!Regex.IsMatch(id, @"^\d+$")) error = "格式错误";

            if (error != null) return false;

            string region_code;
            string str_birthday;
            if (id.Length == 15)
            {
                region_code = id.Substring(0, 6);
                str_birthday = "19" + id.Substring(6, 6);
            }
            else if (id.Length == 18)
            {
                if (!Check18(id)) error = "校验位错误";
                region_code = id.Substring(0, 6);
                str_birthday = id.Substring(6, 8);
            }
            else
            {
                error = "号码位数错误";
                return false;
            }

            if (!s_County.ContainsKey(region_code))
            {
                error = "区域码错误";
                return false;
            }

            DateTime birthday;
            if (!DateTime.TryParseExact(str_birthday, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out birthday)
                || birthday.ToString("yyyyMMdd") != str_birthday)
            {
                error = "出生日期错误";
                return false;
            }
            return true;
        }

        public static string ParseRegion(string regionCode)
        {
            string code;
            if (string.IsNullOrEmpty(regionCode) || regionCode.Length < 2) return null;
            else if (regionCode.Length < 4)
            {
                code = regionCode.Substring(0, 2);
                if (s_Province.ContainsKey(code))
                {
                    return s_Province[code];
                }
            }
            else if (regionCode.Length < 6)
            {
                code = regionCode.Substring(0, 4);
                if (s_Prefecture.ContainsKey(code))
                {
                    return s_Prefecture[code];
                }
            }
            else
            {
                code = regionCode.Substring(0, 6);
                if (s_County.ContainsKey(code))
                {
                    return s_County[code];
                }
            }
            return null;
        }

        public static DateTime ParseBirthday(string id)
        {
            string error;
            if (!TryCheck(id, out error))
            {
                throw new ArgumentException(error, "id");
            }
            if (id.Length == 15)
            {
                return DateTime.ParseExact("19" + id.Substring(6, 6), "yyyyMMdd", null);
            }
            else
            {
                return DateTime.ParseExact(id.Substring(6, 8), "yyyyMMdd", null);
            }
        }

        public static bool TryParseBirthday(string id, out DateTime birthday)
        {
            string error;
            if (!TryCheck(id, out error))
            {
                birthday = new DateTime();
                return false;
            }
            if (id.Length == 15)
            {
                return DateTime.TryParseExact("19" + id.Substring(6, 6), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out birthday);
            }
            else
            {
                return DateTime.TryParseExact(id.Substring(6, 8), "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out birthday);
            }
        }

        static IdCardNo()
        {
            ReadDataFromResource();
        }

        private static Dictionary<string, string> s_Province = new Dictionary<string, string>();
        private static Dictionary<string, string> s_Prefecture = new Dictionary<string, string>();
        private static Dictionary<string, string> s_County = new Dictionary<string, string>();

        public static void ReadDataFromResource()
        {
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(IdCardNo), "中国大陆身份证地区码.txt"))
            {
                var sr = new System.IO.StreamReader(stream, Encoding.GetEncoding("GB2312"), true);
                while (!sr.EndOfStream)
                {
                    var row = sr.ReadLine().Split(new char[] { '\t', ' ', ',', '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (row.Length < 2) continue;
                    switch (row[0].Length)
                    {
                        case 2:
                            s_Province[row[0]] = row[1];
                            break;
                        case 4:
                            s_Prefecture[row[0]] = row[1];
                            break;
                        case 6:
                            s_County[row[0]] = row[1];
                            break;
                    }
                }
            }
        }

        private static bool Check18(string id)
        {
            int[] weights = new int[] { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
            string checks = "10X98765432";
            int val = 0;
            for (var i = 0; i < 17; i++)
            {
                val += (id[i] - '0') * weights[i];
            }
            return id[17] == checks[val % 11];
        }
    }
}
