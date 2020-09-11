using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CsvDataReader;
using DataPowerTools.Extensions;

namespace DataPowerTools.DataConnectivity
{
    public static class Csv
    {
        /// <summary>
        /// Writes an enumerable object array to CSV.
        /// </summary>
        /// <param name="rowObjects"></param>
        /// <param name="headers"></param>
        /// <param name="outputFile"></param>
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
        /// Opens a CSV file and returns a data reader.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="hasHeaders">Whether the first row in the file has headers.</param>
        /// <param name="delimiter">Field delimiter e.g. '|' or ','</param>
        /// <returns></returns>
        public static IDataReader Read(string filePath, bool hasHeaders = true, char delimiter = ',')
        {
            return new CsvReader(File.OpenText(filePath), true, delimiter);
        }

        /// <summary>
        /// Opens a CSV string and returns a data reader for it.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="hasHeaders">Whether the first row in the file has headers.</param>
        /// <param name="delimiter">Field delimiter e.g. '|' or ','</param>
        /// <returns></returns>
        public static IDataReader ReadString(string data, bool hasHeaders = true, char delimiter = ',')
        {
            return new CsvReader(new StringReader(data), true, delimiter);
        }

        /// <summary>
        /// Writes the reader to CSV.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="outputFile"></param>
        /// <param name="writeHeaders">Whether to write the headers.</param>
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
        }

        /// <summary>
        /// Writes the reader to CSV.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writeHeaders">Whether to write the headers.</param>
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

        //TODO: needs to be support streaming to disk


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

        /// <summary>
        ///     CSV-parses a string. Semi-obsolete.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="delimiter"></param>
        /// <param name="qualifier"></param>
        /// <returns></returns>
        [Obsolete]
        public static IEnumerable<IList<string>> Parse(string content, char delimiter, char qualifier)
        {
            using (var reader = new StringReader(content))
            {
                return Parse(reader, delimiter, qualifier);
            }
        }

        /// <summary>
        ///     Returns a parsed list of columns. I.e. use rtn[row][col]. Semi-obsolete - use CsvReader in DataPowerTools.Connectivity instead. TODO: i'm not sure if this supports escaped strings.
        ///     However, 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="delimiter"></param>
        /// <param name="qualifier"></param>
        /// <returns></returns>
        [Obsolete]
        public static IEnumerable<IList<string>> Parse(TextReader reader, char delimiter, char qualifier)
        {
            var inQuote = false;
            var record = new List<string>();
            var sb = new StringBuilder();

            while (reader.Peek() != -1)
            {
                var readChar = (char) reader.Read();

                if ((readChar == '\n') || ((readChar == '\r') && ((char) reader.Peek() == '\n')))
                {
                    // If it's a \r\n combo consume the \n part and throw it away.
                    if (readChar == '\r')
                        reader.Read();

                    if (inQuote)
                    {
                        if (readChar == '\r')
                            sb.Append('\r');
                        sb.Append('\n');
                    }
                    else
                    {
                        if ((record.Count > 0) || (sb.Length > 0))
                        {
                            record.Add(sb.ToString());
                            sb.Clear();
                        }

                        if (record.Count > 0)
                            yield return record;

                        record = new List<string>(record.Count);
                    }
                }
                else if ((sb.Length == 0) && !inQuote)
                {
                    if (readChar == qualifier)
                        inQuote = true;
                    else if (readChar == delimiter)
                    {
                        record.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (char.IsWhiteSpace(readChar))
                    {
                        // Ignore leading whitespace
                    }
                    else
                        sb.Append(readChar);
                }
                else if (readChar == delimiter)
                {
                    if (inQuote)
                        sb.Append(delimiter);
                    else
                    {
                        record.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (readChar == qualifier)
                {
                    if (inQuote)
                        if ((char) reader.Peek() == qualifier)
                        {
                            reader.Read();
                            sb.Append(qualifier);
                        }
                        else
                            inQuote = false;
                    else
                        sb.Append(readChar);
                }
                else
                    sb.Append(readChar);
            }

            if ((record.Count > 0) || (sb.Length > 0))
                record.Add(sb.ToString());

            if (record.Count > 0)
                yield return record;
        }
    }
}