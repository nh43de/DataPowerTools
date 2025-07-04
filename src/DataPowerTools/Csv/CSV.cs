﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CsvDataReader;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions;
using SimpleCSV;

// ReSharper disable once CheckNamespace
namespace DataPowerTools
{
    public static class Csv
    {
        private static StreamWriter CreateBomAwareStreamWriter(string filePath, CSVFormat format = CSVFormat.UTF8)
        {
            return new StreamWriter(filePath, false, format.GetEncoding());
        }

        //TODO: missing this implementation args: public static void Write<T>(IEnumerable<T> rowObjects, IEnumerable<string> headers, string outputFile) { //not implemented }
        
        /// <summary>
        /// Opens a file and returns a data reader that reads the CSV.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="csvDelimiter"></param>
        /// <param name="fileHasHeaders"></param>
        /// <returns></returns>
        public static IDataReader CreateDataReader(string filePath, char csvDelimiter = ',', bool fileHasHeaders = true)
        {
            var dr = new CsvReader(new StreamReader(filePath), fileHasHeaders, csvDelimiter);
            return dr;
        }

        /// <summary>
        /// Opens a fileStream and returns a data reader that reads the CSV.
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="csvDelimiter"></param>
        /// <param name="fileHasHeaders"></param>
        /// <returns></returns>
        public static IDataReader CreateDataReader(Stream fileStream, char csvDelimiter = ',', bool fileHasHeaders = true)
        {
            var dr = new CsvReader(new StreamReader(fileStream), fileHasHeaders, csvDelimiter);
            return dr;
        }

        /// <summary>
        /// Opens a CSV file and returns a data reader.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="hasHeaders">Whether the first row in the file has headers.</param>
        /// <param name="delimiter">Field delimiter e.g. '|' or ','</param>
        /// <returns></returns>
        public static IDataReader CreateDataReader(string filePath, bool hasHeaders = true, char delimiter = ',')
        {
            var dr = new CsvReader(File.OpenText(filePath), hasHeaders, delimiter);
            return dr;
        }
        
        /// <summary>
        /// Reads a CSV file into a DataSet.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="csvDelimiter"></param>
        /// <param name="fileHasHeaders"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(string filePath, char csvDelimiter = ',', bool fileHasHeaders = true)
        {
            using var a = new CsvReader(new StreamReader(filePath), fileHasHeaders, csvDelimiter);
            return a.ToDataSet(null, a.GetFieldHeaders());
        }

        /// <summary>
        /// Gets a DataSet by reading a CSV file stream.
        /// </summary>
        /// <param name="fileStream"></param>
        /// <param name="csvDelimiter"></param>
        /// <param name="fileHasHeaders"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(Stream fileStream, char csvDelimiter = ',', bool fileHasHeaders = true)
        {
            using var a = new CsvReader(new StreamReader(fileStream), fileHasHeaders, csvDelimiter);
            return a.ToDataSet(null, a.GetFieldHeaders());
        }

        /// <summary>
        /// Writes an enumerable of object arrays into a file.
        /// 
        /// Excel Compatibility Notes:
        /// - Default UTF-8 with BOM format ensures emoji and international character support
        /// - Uses RFC 4180 compliant formatting with CRLF line endings for Windows compatibility
        /// - If Excel shows one column, use Data > From Text/CSV and select UTF-8 encoding
        /// </summary>
        /// <param name="rowObjects">The data rows to write</param>
        /// <param name="headers">Column headers</param>
        /// <param name="outputFile">Output file path</param>
        /// <param name="format">The format to use for the CSV output (UTF8 with BOM recommended for Excel)</param>
        public static void Write(IEnumerable<object[]> rowObjects, IEnumerable<string> headers, string outputFile, CSVFormat format = CSVFormat.UTF8)
        {
            using var sw = CreateBomAwareStreamWriter(outputFile, format);
            using var csvWriter = new CSVWriter(sw, CSVWriter.DefaultSeparator, CSVWriter.DefaultQuoteCharacter, CSVWriter.DefaultEscapeCharacter, CSVWriter.Rfc4180LineEnd);

            csvWriter.WriteNext(headers.ToArray(), false);

            foreach (var row in rowObjects)
            {
                var vals = row.Select(i => i?.ToString()).ToArray();
                csvWriter.WriteNext(vals, false);
            }
        }

        /// <summary>
        /// Writes an IDataReader to a CSV file onto disk (streaming operation).
        /// 
        /// Excel Compatibility Notes:
        /// - Default UTF-8 with BOM format ensures emoji and international character support
        /// - Uses RFC 4180 compliant formatting with CRLF line endings for Windows compatibility
        /// - If Excel shows one column, use Data > From Text/CSV and select UTF-8 encoding
        /// </summary>
        /// <param name="reader">The data reader to write</param>
        /// <param name="outputFile">Output file path</param>
        /// <param name="writeHeaders">Whether to include column headers</param>
        /// <param name="format">The format to use for the CSV output (UTF8 with BOM recommended for Excel)</param>
        public static void Write(IDataReader reader, string outputFile, bool writeHeaders = true, CSVFormat format = CSVFormat.UTF8)
        {
            using var sw = CreateBomAwareStreamWriter(outputFile, format);
            using var csvWriter = new CSVWriter(sw, CSVWriter.DefaultSeparator, CSVWriter.DefaultQuoteCharacter, CSVWriter.DefaultEscapeCharacter, CSVWriter.Rfc4180LineEnd);
            
            Write(reader, sw, csvWriter, writeHeaders);
        }

        /// <summary>
        /// Writes IDataReader to a TextWriter stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="sw"></param>
        /// <param name="csvWriter"></param>
        /// <param name="writeHeaders"></param>
        public static void Write(IDataReader reader, TextWriter sw, CSVWriter csvWriter, bool writeHeaders = true)
        {
            var isInitialized = false;
            var fieldCount = 0;
            
            void Initialize()
            {
                var fieldHeaders = reader.GetFieldNames().ToArray();

                if (writeHeaders)
                {
                    csvWriter.WriteNext(fieldHeaders, false);
                }

                fieldCount = reader.FieldCount;

                isInitialized = true;
            }
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

                        csvWriter.WriteNext(row.Select(o => o?.ToString()).ToArray(), false);
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
        /// Writes an IDataReader to a CSV string.
        /// 
        /// Excel Compatibility Notes:
        /// - Default UTF-8 with BOM format ensures emoji and international character support
        /// - Uses RFC 4180 compliant formatting with CRLF line endings for Windows compatibility
        /// - String output doesn't include BOM - use file output methods for Excel compatibility
        /// </summary>
        /// <param name="reader">The data reader to convert</param>
        /// <param name="writeHeaders">Whether to include column headers</param>
        /// <param name="useTabFormat">Outputs CSV in tab format (TSV)</param>
        /// <param name="format">The format to use for the CSV output (affects character encoding for file writes)</param>
        /// <returns>CSV formatted string</returns>
        public static string WriteString(IDataReader reader, bool writeHeaders = true, bool useTabFormat = false, CSVFormat format = CSVFormat.UTF8)
        {
            using var sw = new StringWriter();

            using var csvWriter = useTabFormat 
                ? new CSVWriter(sw, '\t', CSVWriter.NoQuoteCharacter, CSVWriter.DefaultEscapeCharacter, CSVWriter.Rfc4180LineEnd) 
                : new CSVWriter(sw, CSVWriter.DefaultSeparator, CSVWriter.DefaultQuoteCharacter, CSVWriter.DefaultEscapeCharacter, CSVWriter.Rfc4180LineEnd);

            Write(reader, sw, csvWriter, writeHeaders);

            return sw.ToString();
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
            var dr = new CsvReader(new StringReader(data), hasHeaders, delimiter);
            return dr;
        }
        
        /// <summary>
        ///     CSV-parses a string. Semi-obsolete.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="delimiter"></param>
        /// <param name="qualifier"></param>
        /// <returns></returns>
        [Obsolete("Parse handling is poor - shouldn't really use this.")]
        public static IEnumerable<string[]> Parse(string content, char delimiter, char qualifier)
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
        [Obsolete("Parse handling is poor - shouldn't really use this - to be replaced with a better implementation.")]
        public static IEnumerable<string[]> Parse(TextReader reader, char delimiter, char qualifier)
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
                            yield return record.ToArray();

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
                yield return record.ToArray();
        }
    }
}