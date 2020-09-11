using System;
using System.Text.RegularExpressions;

namespace DataPowerTools.Strings
{
    //TODO: needs to cache compiled regexes!!!

    /// <summary>
    /// Example date from string providers. Remember that a date from string provider is a delegate that returns a DateTime for any given string.
    /// </summary>
    public static class DateFromStringProviders
    {
        /// <summary>
        /// Used for Alliant's data only.
        /// </summary>
        public static DateFromStringProvider Alliant = fileName =>
        {
            DateTime dt;

            var fileNameRegexes = new[]
            {
                @"(?<month>[0-9]{1,2})-(?<year>[0-9]{2}).csv", //e.g. 5-16
                @"(?<month>[0-9]{1,2}) - (?<year>[0-9]{2}).csv", //e.g. 5 - 16
                @"(?<month>[0-9]{2})-(?<year>[0-9]{4})" //e.g. 01-2011
            };

            return DateStringUtils.GetDateFromRegexes(fileName, fileNameRegexes);
        };

        /// <summary>
        /// Default looks for things like "Dec- 2011", "2011-12-021","2011-12-21", "11-12-21", "11-03" (year-month).
        /// </summary>
        public static DateFromStringProvider Default = str =>
        {
            DateTime dt;

            var dateRegexes = new[]
            {
                @"(?<month>[a-zA-Z]+)[- _]+(?<year>[0-9]{2,4})", //e.g. "Dec- 2011"
                @"(?<year>[0-9]{4})-?(?<month>[0-9]{2})-?(?<day>[0-9]{3})", //e.g. "2011-12-021"
                @"(?<year>[0-9]{4})-?(?<month>[0-9]{2})-?(?<day>[0-9]{2})", //e.g. "2011-12-21"
                @"(?<year>[0-9]{2})-?(?<month>[0-9]{2})-?(?<day>[0-9]{2})", //e.g. "11-12-21"
                @"(?<year>[0-9]{4})-?(?<month>[0-9]{2}))" //e.g. 2011-03
            };

            return DateStringUtils.GetDateFromRegexes(str, dateRegexes);
        };

        private static void DoNothing() //this is just to show resharper colorizing for regex exps
        {
            var a = new Regex(@"(?<month>[0-9]{1,2})-(?<year>[0-9]{2})");
            var b = new Regex(@"(?<month>[0-9]{1,2})-(?<year>[0-9]{2})");
        }
    }
}