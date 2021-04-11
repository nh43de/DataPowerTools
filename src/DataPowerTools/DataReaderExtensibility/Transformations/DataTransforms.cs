using System;
using System.Globalization;
using DataPowerTools.Strings;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public static class DataTransforms
    {
        public static readonly DataTransform None = s => s;
        
        public static readonly DataTransform Trim = o =>
        {
            var str = o?.ToString();
            
            return str?.Trim();
        };
        
        public static readonly DataTransform TrimStart = o =>
        {
            var str = o?.ToString();

            return str?.TrimStart();
        };

        public static readonly DataTransform TrimEnd = o =>
        {
            var str = o?.ToString();
            
            return str?.TrimEnd();
        };

        /// <summary>
        /// Removes percent sign and parses. Does not divide by 100.
        /// </summary>
        public static readonly DataTransform TransformRate = o =>
        {
            var rate = o?.ToString();

            if (string.IsNullOrWhiteSpace(rate))
            {
                return null;
            }

            if (rate.EndsWith("%"))
                return double.Parse(rate.Remove(rate.Length - 1));

            return double.Parse(rate);
        };
        
        public static readonly DataTransform MultiplyBy100 = o =>
        {
            var d = (decimal?)TransformDecimal(o);

            return d * 100.0m;
        };

        public static readonly DataTransform MultiplyBy1000 = o =>
        {
            var d = (decimal?)TransformDecimal(o);

            return d * 1000.0m;
        };

        public static readonly DataTransform MultiplyBy10000 = o =>
        {
            var d = (decimal?)TransformDecimal(o);

            return d * 10000.0m;
        };

        public static readonly DataTransform DivideBy100 = o =>
        {
            var d = (decimal?)TransformDecimal(o);

            return d / 100.0m;
        };

        public static readonly DataTransform DivideBy1000 = o =>
        {
            var d = (decimal?)TransformDecimal(o);

            return d / 1000.0m;
        };

        public static readonly DataTransform DivideBy10000 = o =>
        {
            var d = (decimal?)TransformDecimal(o);

            return d / 10000.0m;
        };

        public static readonly DataTransform TransformNull = value =>
        {
            // Handle DBNull
            if (value == DBNull.Value)
                value = null;

            // Handle value type conversion of null to the values types default value
            //if (value == null && type.IsValueType)
            //    return type.GetDefaultValue(); // Extension method internally handles caching

            if (value is string v && string.IsNullOrWhiteSpace(v))
            {
                return null;
            }

            return value;
        };


        public static readonly DataTransform TransformGuid = o =>
        {
            if (string.IsNullOrWhiteSpace(o?.ToString()))
            {
                return null;
            }
            
            if (o is string s)
            {
                var r = Guid.Parse(s);
                return r;
            }
            if (o is byte[] bytes)
            {
                var r = new Guid(bytes);
                return r;
            }

            return null;

        };

        public static readonly DataTransform ToStringTransform = o => o?.ToString();

        //if destination is boolean then use this tranform
        public static readonly DataTransform TransformBoolean = o =>
        {
            var str = o?.ToString().Trim();

            if (string.IsNullOrWhiteSpace(str))
                return null;

            if (str == "0")
                return false;

            if (str == "1")
                return true;

            var strl = str.ToLower();

            if (strl == "y")
                return true;

            if (strl == "n")
                return false;

            if (strl == "false")
                return false;

            if (strl == "true")
                return true;

            if (strl == "f")
                return false;

            if (strl == "t")
                return true;

            decimal t;
            if(decimal.TryParse(strl, out t))
                if (t == 0m)
                    return true;
                else if (t == 1m) //we're not going to assume that any decimal is parsable to a boolean, only 1/0's
                    return false;

            throw new Exception("Could not parse boolean '" + o.ToString() + "'");

            //TODO: faster way
            //var isFalse = true;
            //var isEmptystring = true;
            //var isWhitespace = false;
            //Stack<char> nonWhitespaceChars;

            //int charSuml
            //foreach (var c in str)
            //{
            //    char.

            //    isEmptystring = false;
            //    isWhitespace &= char.IsWhiteSpace(c);
            //    isFalse &= c == '0';
            //}

            //return true;
        };

        public static readonly DataTransform TransformDecimal = s =>
        {
            var str = s?.ToString();

            if (string.IsNullOrWhiteSpace(str))
                return null;

            var ss = str.Trim().ToLower();

            if (ss == "-" || ss == "n/a" || ss == "na" || ss == "#n/a" || ss == "#ref!" || ss == "null" || ss == "<null>")
                return null;

            if (ss.EndsWith("%"))
                ss = ss.TrimEnd('%');

            if (ss.StartsWith("-"))
                ss = "-" + ss.TrimStart('-', ' ', '$');
            else if (ss.StartsWith("$"))
                ss = ss.TrimStart('$');

            if (ss.StartsWith("(") && ss.EndsWith(")"))
                ss = "-" + ss.Trim('$', '(', ')');

            if (ss.Contains("e"))
                return (decimal)double.Parse(ss);

            return decimal.Parse(ss);
        };

        public static readonly DataTransform TransformInt = s =>
        {
            var d = TransformDecimal(s);

            return Convert.ToInt32(d);
        };
        
        public static readonly DataTransform TransformStringIsNullOrWhiteSpace =
            s => string.IsNullOrWhiteSpace(s?.ToString()) ? null : s;

        //if destination is non string then use this tranform
        public static readonly DataTransform TransformStringIsNullOrWhiteSpaceAndTrim =
            s => string.IsNullOrWhiteSpace(s?.ToString()) ? null : s.ToString().Trim();

        //06272017
        public static readonly DataTransform MMDDYYYY_Date = o =>
        {
            var date = o?.ToString().Trim();

            if (string.IsNullOrWhiteSpace(date))
                return null;

            if (date.Length == 7)
            {
                var m = int.Parse(date.Substring(0, 1));
                var d = int.Parse(date.Substring(1, 2));
                var y = int.Parse(date.Substring(3, 4));

                if (m == 0 && d == 0 && y == 0)
                    return null;

                return new DateTime(y, m, d);
            }
            else if (date.Length == 8)
            {
                var m = int.Parse(date.Substring(0, 2));
                var d = int.Parse(date.Substring(2, 2));
                var y = int.Parse(date.Substring(4, 4));

                if (m == 0 && d == 0 && y == 0)
                    return null;

                return new DateTime(y, m, d);
            }

            throw new Exception($"Invalid MMDDYYYY date: {date}");
        };

        public static readonly DataTransform TransformExcelDate = o =>
        {
            var date = o?.ToString().Trim();

            if (string.IsNullOrWhiteSpace(date))
                return DBNull.Value;

            if (date == "--/--/--" || date == "--/--/----")
                return DBNull.Value;

            if (date == "0")
                return DBNull.Value;

            DateTime rtnDate;
            if (DateTime.TryParse(date, out rtnDate))
                return rtnDate;

            try
            {
                return DateTime.TryParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out rtnDate)
                    ? rtnDate
                    : StringUtils.FromExcelTime(date);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + " Expected yyyyMMdd format.");
            }
        };

        public static readonly DataTransform TransformScientificNotation =
            s =>
            {
                var str = s?.ToString();

                if (string.IsNullOrWhiteSpace(str))
                    return null;

                //var ss = str.Split('E');

                //if (ss.Length == 0)
                //    return null;

                //if (ss.Length > 1)
                //{
                //    var val = Math.Round(double.Parse(ss[0]), 2);
                //    var flt = double.Parse(ss[1]);

                //    return Math.Round(val*Math.Pow(10, flt), 2).ToString(CultureInfo.CurrentCulture);
                //}
                return double.Parse(str);
            };
    }
}