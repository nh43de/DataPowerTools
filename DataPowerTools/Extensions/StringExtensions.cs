using System.Text;
using System.Text.RegularExpressions;
using DataPowerTools.Strings;

namespace DataPowerTools.Extensions
{
    public static class StringExtensions
    {
        public static readonly Regex IndentRegex = new Regex("^", RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>
        /// Uses default date from string provider to parse a date from a string. Returns a short date string mm/dd/yy
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetDateString(this string str)
        {
            return StringUtils.GetDateFromName(str);
        }
        
        /// <summary>
        /// Crops a string using the left and right offset.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="marginLeft"></param>
        /// <param name="marginRight"></param>
        /// <returns></returns>
        public static string Crop(this string str, int marginLeft, int marginRight)
        {
            return str.Substring(marginLeft, str.Length - marginRight - marginLeft);
        }

        /// <summary>
        /// Crops a string using the same left and right offset.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="margin"></param>
        /// <returns></returns>
        public static string Crop(this string str, int margin)
        {
            return str.Crop(margin, margin);
        }

        /// <summary>
        /// Indent using tabs.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="tabs"></param>
        /// <returns></returns>
        public static string Indent(this string str, int tabs = 1)
        {
            return IndentRegex.Replace(str, "".PadLeft(tabs, '\t'));
        }

        /// <summary>
        /// Indent using string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="indentionStr"></param>
        /// <returns></returns>
        public static string Indent(this string str, string indentionStr)
        {
            return IndentRegex.Replace(str, indentionStr);
        }

        /// <summary>
        /// Returns a string that is the source string repeated n times. For example, "0".Repeat(1) = "0" and "0".Repeat(3) = "000".
        /// </summary>
        /// <param name="str"></param>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public static string Repeat(this string str, int repeatCount)
        {
            var sb = new StringBuilder(str.Length * repeatCount);
            for (var i = 0; i < repeatCount; i++)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }

        /// <summary>
        /// If string is null or whitespace, return true. Watch for null reference exceptions.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool StrIsEmptyOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// If string is null or empty, return true. Watch for null reference exceptions.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool StrIsEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Appends new lines to the string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="appendStr"></param>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public static string Append(this string str, string appendStr, int repeatCount = 1)
        {
            var d = appendStr.Repeat(repeatCount);

            return str + d;
        }

        /// <summary>
        /// Appends new lines to the string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="newlineCount"></param>
        /// <returns></returns>
        public static string AppendNewLine(this string str, int newlineCount = 1)
        {
            return str.Append("\r\n", newlineCount);
        }
    }
}