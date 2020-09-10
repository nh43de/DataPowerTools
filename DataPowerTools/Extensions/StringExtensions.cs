using System;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using DataPowerTools.Strings;

namespace DataPowerTools.Extensions
{
    //TODO: add as sql command
    internal static class StringExtensions
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
        /// Indent each line using tabs.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="tabs"></param>
        /// <returns></returns>
        public static string Indent(this string str, int tabs = 1)
        {
            return IndentRegex.Replace(str, "".PadLeft(tabs, '\t'));
        }

        /// <summary>
        /// Indent each line using tabs.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="indentionStr"></param>
        /// <returns></returns>
        public static string Indent(this string str, string indentionStr)
        {
            return IndentRegex.Replace(str, indentionStr);
        }
    }
}