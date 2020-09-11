using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;

namespace DataPowerTools.Strings
{
    public static class StringUtils
    {
        /// <summary>
        /// Computes the optimal string alignment of the Damerau-Levenshtein edit distance of two strings. This is a fuzzy string matching algorithm that is both powerful and efficient. The strings are compared ordinally.
        /// </summary>
        /// <param name="str1">The first string to compare.</param>
        /// <param name="str2">The second string to compare.</param>
        /// <returns>The estimated Damerau-Levenshtein edit distance of the two strings.</returns>
        public static int DamerauLevenshteinDistance(string str1, string str2)
        {
            return DamerauLevenshteinDistance(str1, str2, new int[str1.Length + 1, str2.Length + 1]);
        }

        /// <summary>
        /// Computes the optimal string alignment of the Damerau-Levenshtein edit distance of two strings. This is a fuzzy string matching algorithm that is both powerful and efficient. The strings are compared ordinally.
        /// </summary>
        /// <param name="str1">The first string to compare.</param>
        /// <param name="str2">The second string to compare.</param>
        /// <param name="workingValues">The array used as scratch values. The first dimension of this array must be at least <c>str1.Length + 1</c>, and the second dimension of this array must be at least <c>str2.Length + 1</c>.</param>
        /// <returns>The estimated Damerau-Levenshtein edit distance of the two strings.</returns>
        public static int DamerauLevenshteinDistance(string str1, string str2, int[,] workingValues)
        {
            var length1 = str1.Length;
            var length2 = str2.Length;
            for (int i = 0; i <= length1; ++i)
            {
                workingValues[i, 0] = i;
            }

            for (int j = 1; j <= length2; ++j)
            {
                workingValues[0, j] = j;
            }

            for (int i = 1; i <= length1; ++i)
            {
                for (int j = 1; j <= length2; ++j)
                {
                    char char1 = str1[i - 1];
                    char char2 = str2[j - 1];
                    int cost = (char1 == char2) ? 0 : 1;

                    var deletion = workingValues[i - 1, j] + 1;
                    var insertion = workingValues[i, j - 1] + 1;
                    var substitution = workingValues[i - 1, j - 1] + cost;
                    workingValues[i, j] = Math.Min(Math.Min(deletion, insertion), substitution);

                    if (i > 1 && j > 1 && char1 == str2[j - 2] && str1[i - 2] == char2)
                    {
                        var transposition = workingValues[i - 2, j - 2] + cost;
                        workingValues[i, j] = Math.Min(workingValues[i, j], transposition);
                    }
                }
            }

            return workingValues[length1, length2];
        }

        /// <summary>
        /// Gets last date of the month, assuming input is formatted as "MM/yyyy" or it will try to get date.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Obsolete]
        public static DateTime GetLastDayOfMonth(string input)
        {
            if (DateTime.TryParseExact(input, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime rtn))
                return GetLastDayOfMonth(rtn);

            return GetLastDayOfMonth(DataTransforms.TransformExcelDate(input).ToString());
        }
        
        /// <summary>
        /// Gets the last day of the month. Time bits are not set.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetLastDayOfMonth(DateTime date)
        {
            var year = date.Year;
            var monthNum = date.Month;

            var lastDayOfTheMonth = DateTime.DaysInMonth(year, monthNum);

            return new DateTime(year, monthNum, lastDayOfTheMonth);
            //return DateTime.Parse($"{monthNum}-{lastDayOfTheMonth}-{year}");
        }
        
        /// <summary>
        /// Parses a date from an excel time format.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [Obsolete]
        public static DateTime FromExcelTime(string input)
        {
            try
            {
                var days = double.Parse(input);

                return DateTime.Parse("12/30/1899").AddDays(days);
            }
            catch (Exception)
            {
                throw new Exception("Could not parse date " + input);
            }
        }

        /// <summary>
        /// Do not use. Gets date from file name. Use DateFromStringProviders instead.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="ifNoDayThenEndOfMonth"></param>
        /// <returns></returns>
        [Obsolete]
        public static string GetDateFromName(string filePath, bool ifNoDayThenEndOfMonth = true)
        {
            var name = Path.GetFileNameWithoutExtension(filePath);

            return DateFromStringProviders.Default(name)?.ToString("MM/dd/yyyy");
        }

        /// <summary>
        /// Returns whether the given string is a valid path. (Not thouroughly tested).
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsValidPath(string path)
        {
            var driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            if (!driveCheck.IsMatch(path.Substring(0, 3))) return false;
            var strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
            strTheseAreInvalidFileNameChars += @":/?*" + "\"";
            var containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            if (containsABadCharacter.IsMatch(path.Substring(3, path.Length - 3)))
                return false;

            //DirectoryInfo dir = new DirectoryInfo(Path.GetFullPath(path));
            //if (!dir.Exists)
            //    dir.Create();
            return true;
        }
        
        /// <summary>
        /// Gets a database from a MSSQL connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static string GetDatabase(string connectionString)
        {
            var c = new SqlConnectionStringBuilder(connectionString);

            return c.InitialCatalog;
        }
        
        /// <summary>
        /// If string is null or whitespace, return replacement.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isFalse"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string IsNullOrWhiteSpaceThen(string str, Func<string, string> isFalse, string replacement)
        {
            return string.IsNullOrWhiteSpace(str) ? replacement : isFalse(str);
        }

        /// <summary>
        /// If string is null or empty, return replacement.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isFalse"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string IsNullOrEmptyThen(string str, Func<string, string> isFalse, string replacement)
        {
            return string.IsNullOrEmpty(str) ? replacement : isFalse(str);
        }
        
        /// <summary>
        /// If string is null or whitespace, return replacement.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isFalse"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string IsNullOrWhiteSpaceThen(string str, Func<string, string> isFalse, Func<string, string> replacement)
        {
            return string.IsNullOrWhiteSpace(str) ? replacement(str) : isFalse(str);
        }
        
        /// <summary>
        /// If string is null or empty, return replacement.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="isFalse"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string IsNullOrEmptyThen(string str, Func<string, string> isFalse, Func<string, string> replacement)
        {
            return string.IsNullOrEmpty(str) ? replacement(str) : isFalse(str);
        }

        /// <summary>
        /// If string is null or whitespace, return replacement.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string IsNullOrWhiteSpaceThen(string str, Func<string, string> replacement)
        {
            return string.IsNullOrWhiteSpace(str) ? replacement(str) : str;
        }

        /// <summary>
        /// If string is null or empty, return replacement.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string IsNullOrEmptyThen(string str, Func<string, string> replacement)
        {
            return string.IsNullOrEmpty(str) ? replacement(str) : str;
        }

        /// <summary>
        /// If string is null or whitespace, return replacement.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string IsNullOrWhiteSpaceThen(string str, string replacement)
        {
            return string.IsNullOrWhiteSpace(str) ? replacement : str;
        }
        
        /// <summary>
        /// If string is null or empty, return replacement.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string IsNullOrEmptyThen(string str, string replacement)
        {
            return string.IsNullOrEmpty(str) ? replacement : str;
        }
    }
}