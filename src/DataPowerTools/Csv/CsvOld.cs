using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using DataPowerTools.Extensions;

// ReSharper disable once CheckNamespace
namespace DataPowerTools
{
    public static class CsvOld {
        /// <summary>
        /// Writes an enumerable object array to CSV.
        /// </summary>
        /// <param name="rowObjects"></param>
        /// <param name="headers"></param>
        /// <param name="outputFile"></param>
        [Obsolete("For reference only, will be removed.")]
        public static void Write(IEnumerable<object[]> rowObjects, IEnumerable<string> headers, string outputFile)
        {
            var ts = File.OpenWrite(outputFile);
            var sw = new StreamWriter(ts);
            var sb = new StringBuilder(1024);

            using (ts)
            using (sw)
            {
                foreach (var col in headers)
                    sb.Append("\"" + col + "\",");

                sb.Remove(sb.Length - 1, 1);
                sb.Append(Environment.NewLine);

                sw.Write(sb.ToString());
                sb.Clear();

                foreach (var row in rowObjects)
                {
                    //TODO: this could be sped-up by not building this string in the heap
                    var rowStr = string.Join(",", row.Select(i => @"""" + i?.ToString() + @""""));
                    sb.Append(rowStr + Environment.NewLine);
                    sw.Write(sb.ToString());
                    sb.Clear();
                }
            }
        }

        /// <summary>
        /// Writes the reader to CSV.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="outputFile"></param>
        /// <param name="writeHeaders">Whether to write the headers.</param>
        [Obsolete("For reference only, will be removed.")]
        public static void Write(IDataReader reader, string outputFile, bool writeHeaders = true)
        {
            var ts = File.OpenWrite(outputFile);
            var sw = new StreamWriter(ts);
            var sb = new StringBuilder(1024);

            var isInitialized = false;
            var fieldCount = 0;

            void Initialize()
            {
                var fieldHeaders = reader.GetFieldNames().ToArray();

                if (writeHeaders)
                {
                    foreach (var col in fieldHeaders)
                        sb.Append("\"" + col + "\",");

                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(Environment.NewLine);

                    sw.Write(sb.ToString());
                    sb.Clear();
                }

                fieldCount = reader.FieldCount;

                isInitialized = true;
            }
            using (ts)
            using (sw)
            {
                try
                {
                    while (reader.Read())
                    {
                        if (isInitialized == false)
                            Initialize();

                        var row = new object[fieldCount];
                        reader.GetValues(row);
                        var rowStr = string.Join(",", row.Select(i => @"""" + i?.ToString() + @""""));
                        sb.Append(rowStr + Environment.NewLine);
                        sw.Write(sb.ToString());
                        sb.Clear();
                    }
                }
                finally
                {
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        /// <summary>
        /// Returns a CSV string.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writeHeaders">Whether to write the headers.</param>
        [Obsolete("For reference only, will be removed.")]
        public static string WriteString(IDataReader reader, bool writeHeaders = true)
        {
            //TODO: WARNING: duplicated to above

            var sb = new StringBuilder(1024);

            var isInitialized = false;
            var fieldCount = 0;

            void Initialize()
            {
                var fieldHeaders = reader.GetFieldNames().ToArray();

                if (writeHeaders)
                {
                    foreach (var col in fieldHeaders)
                        sb.Append("\"" + col + "\",");

                    sb.Remove(sb.Length - 1, 1);
                    sb.Append(Environment.NewLine);
                }

                fieldCount = reader.FieldCount;

                isInitialized = true;
            }

            while (reader.Read())
            {
                if (isInitialized == false)
                    Initialize();

                var row = new object[fieldCount];
                reader.GetValues(row);
                var rowStr = string.Join(",", row.Select(i => @"""" + i?.ToString() + @""""));
                sb.Append(rowStr + Environment.NewLine);
            }

            return sb.ToString();
        }

        //public static void Write<T>(IEnumerable<T> rowObjects, IEnumerable<string> headers, string outputFile)
        //{
        //    var props = typeof(T).GetProperties().Select(p => p.Name).ToArray();

        //    var sb = new StringBuilder(1024 * 5);

        //    foreach (var col in headers)
        //        sb.Append("\"" + col + "\",");

        //    sb.Remove(sb.Length - 1, 1);
        //    sb.Append(Environment.NewLine);

        //    foreach (var row in rowObjects)
        //    {
        //        var rowStr = string.Join(",", row.Select(i => @"""" + i.ToString() + @""""));
        //        sb.Append(rowStr + Environment.NewLine);
        //    }

        //    File.WriteAllText(outputFile, sb.ToString());
        //}
    }
}