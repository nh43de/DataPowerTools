using System;
using System.Data;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using DataPowerTools.Strings;
using Newtonsoft.Json.Linq;

namespace DataPowerTools.Extensions
{
    //TODO: add as sql command
    public static class StringExtensions
    {
        public static readonly Regex IndentRegex = new Regex("^", RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>
        /// Opens the file path specified as a CSV.
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="csvDelimiter"></param>
        /// <param name="fileHasHeaders"></param>
        /// <returns></returns>
        public static IDataReader OpenCsvFile(this string strPath, char csvDelimiter = ',', bool fileHasHeaders = true)
        {
            return Csv.CreateDataReader(strPath, csvDelimiter, fileHasHeaders);
        }
        
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
        /// Reads a CSV string into an IDataReader
        /// </summary>
        /// <param name="data"></param>
        /// <param name="csvDelimiter"></param>
        /// <param name="hasHeaders"></param>
        /// <returns></returns>
        public static IDataReader ReadCsvString(this string data, char csvDelimiter = ',', bool hasHeaders = true)
        {
            return Csv.ReadString(data, hasHeaders, csvDelimiter);
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

        public static bool IsValidJson(this string obj)
        {
            try
            {
                JObject.Parse(obj);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}