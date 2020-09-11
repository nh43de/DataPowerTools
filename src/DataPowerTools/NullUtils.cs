using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;

namespace DataPowerTools
{
    /// <summary>
    /// Helpers for doing null check and replacements.
    /// </summary>
    public static class NullUtils
    {
        /// <summary>
        /// If null then return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="notNullFunc"></param>
        /// <param name="nullReplacement"></param>
        /// <returns></returns>
        public static T IsNullThen<T>(T obj, Func<T, T> notNullFunc, T nullReplacement) 
        {
            return obj == null ? nullReplacement : notNullFunc(obj);
        }

        /// <summary>
        /// If null then return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Object to test.</param>
        /// <param name="notNullReplacement">If not null, return this value.</param>
        /// <param name="nullReplacement">If null, return this value.</param>
        /// <returns></returns>
        public static T IsNullThen<T>(T obj, T notNullReplacement, T nullReplacement) 
        {
            return obj == null ? nullReplacement : notNullReplacement;
        }
        
        /// <summary>
        /// If null then return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Value to test.</param>
        /// <param name="replacement">Return value if null.</param>
        /// <returns></returns>
        public static T IsNullThen<T>(T obj, T replacement)
        {
            return obj == null ? replacement : obj;
        }
    }
}