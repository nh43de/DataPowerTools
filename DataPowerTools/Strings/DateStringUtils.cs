using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DataPowerTools.Strings
{
    /// <summary>
    /// Utilities for dealing with date strings.
    /// </summary>
    public static class DateStringUtils
    {
        /// <summary>
        /// Use groups to match month, year, day, etc. E.g. (?&lt;month&gt;[a-zA-Z]+)[- _]+(?&lt;year&gt;[0-9]+)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regex"></param>
        /// <param name="ifNoDayThenEndOfMonth">Otherwise will return the first day of the month.</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static bool TryGetDateFromRegex(string str, string regex, out DateTime dt,
            bool ifNoDayThenEndOfMonth = true)
        {
            try
            {
                dt = GetDateFromRegex(str, regex, ifNoDayThenEndOfMonth);
                return true;
            }
            catch (Exception)
            {
                dt = DateTime.MinValue;
                return false;
            }
        }
        
        /// <summary>
        /// Use groups to match month, year, day, etc. E.g. (?&lt;month&gt;[a-zA-Z]+)[- _]+(?&lt;year&gt;[0-9]+)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regex"></param>
        /// <param name="ifNoDayThenEndOfMonth">Otherwise will return the first day of the month.</param>
        /// <returns></returns>
        [DebuggerHidden]
        public static DateTime GetDateFromRegex(string str, string regex, bool ifNoDayThenEndOfMonth = true)
        {
            var m = new Regex(regex, RegexOptions.IgnoreCase).Match(str);

            int dayInt;
            int monthInt;
            int yearInt;
            {
                var day = m.Groups["day"]?.Value.Trim();
                var month = m.Groups["month"]?.Value.Trim();
                var year = m.Groups["year"]?.Value.Trim();

                var hasDay = (m.Groups["day"]?.Success ?? false) && !string.IsNullOrWhiteSpace(day);
                var hasMonth = (m.Groups["month"]?.Success ?? false) && !string.IsNullOrWhiteSpace(month);
                var hasYear = (m.Groups["year"]?.Success ?? false) && !string.IsNullOrWhiteSpace(year);

                if (hasYear)
                {
                    var isYearInt = int.TryParse(year, out yearInt);
                    if (isYearInt)
                    {
                        if (year.Length == 2)
                            year = "20" + year;
                        yearInt = int.Parse(year);
                    }
                    else
                    {
                        throw new Exception("Year cannot be non-numeric.");
                    }
                }
                else
                {
                    throw new Exception("Year was not specified or not found.");
                }

                if (hasMonth)
                {
                    var isMonthInt = int.TryParse(month, out monthInt);
                    if (!isMonthInt)
                    {
                        DateTime monthDt;
                        if (DateTime.TryParseExact(month, "MMMM", CultureInfo.InvariantCulture,
                            //try long month abbrevs (i.e. "August")
                            DateTimeStyles.AssumeLocal, out monthDt))
                        {
                            monthInt = monthDt.Month;
                        }
                        else if (DateTime.TryParseExact(month, "MMM", CultureInfo.InvariantCulture,
                            //try short month abbrevs (i.e. "Aug")
                            DateTimeStyles.AssumeLocal, out monthDt))
                        {
                            monthInt = monthDt.Month;
                        }
                        else //try custom month abbrevs
                        {
                            var abbreviatedMonths = new CultureInfo("en-US")
                            {
                                DateTimeFormat =
                                {
                                    AbbreviatedMonthNames =
                                        new[]
                                        {
                                            "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sept", "Oct",
                                            "Nov",
                                            "Dec", ""
                                        }
                                }
                            };

                            if (DateTime.TryParseExact(month, "MMM", abbreviatedMonths, DateTimeStyles.AssumeLocal,
                                out monthDt))
                                monthInt = monthDt.Month;
                            else
                                throw new Exception("Could not parse month.");
                        }
                    }
                }
                else
                {
                    throw new Exception("Month was not found.");
                }

                if (hasDay)
                {
                    var isDayInt = int.TryParse(day, out dayInt);
                    if (isDayInt == false)
                        throw new Exception("Day cannot be non-numeric.");
                }
                else
                {
                    dayInt = ifNoDayThenEndOfMonth ? DateTime.DaysInMonth(yearInt, monthInt) : 1;
                }
            }

            return new DateTime(yearInt, monthInt, dayInt);
        }

        /// <summary>
        /// Tries to get the date from multiple regexes. If there is a successful match, then it will return that match.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regexes"></param>
        /// <param name="ifNoDayThenEndOfMonth"></param>
        /// <returns></returns>
        public static DateTime? GetDateFromRegexes(string str, IEnumerable<string> regexes,
            bool ifNoDayThenEndOfMonth = true)
        {
            foreach (var regex in regexes)
            {
                DateTime d;
                if (TryGetDateFromRegex(str, regex, out d, ifNoDayThenEndOfMonth))
                    return d;
            }

            return null;
        }

        /// <summary>
        /// Returns a short date string from regex matches.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regexes"></param>
        /// <param name="ifNoDayThenEndOfMonth"></param>
        /// <returns></returns>
        public static string GetShortDateStringFromRegexes(string str, IEnumerable<string> regexes,
            bool ifNoDayThenEndOfMonth = true)
        {
            return GetDateFromRegexes(str, regexes, ifNoDayThenEndOfMonth)?.ToString("MM/dd/yyyy");
        }
    }
}