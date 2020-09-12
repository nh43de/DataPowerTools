using System;
using System.Collections;
using System.Data;
using System.IO;
using CsvDataReader;
using System.Collections.Generic;
using System.Linq;
using DataPowerTools.Extensions;
using ExcelDataReader;

namespace DataPowerTools.Connectivity
{
    public static partial class DataReaderFactories
    {
        /// <summary>
        /// Gets a data reader for the specified type using conventions.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="csvDelimiter"></param>
        /// <param name="fileHasHeaders"></param>
        /// <returns></returns>
        public static IDataReader Default(string filePath, bool fileHasHeaders = true, char csvDelimiter = ',')
        {
            using var fs = new FileStream(filePath, FileMode.Open);

            return Default(Path.GetFileName(filePath), fs, fileHasHeaders, csvDelimiter);
        }

        /// <summary>
        /// Entry point for when source is a stream (i.e. web applications).
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <param name="fileHasHeaders"></param>
        /// <param name="csvDelimiter"></param>
        /// <param name="headerOffsetRows"></param>
        /// <returns></returns>
        public static IDataReader Default(string fileName, Stream fileStream, bool fileHasHeaders = true, char csvDelimiter = ',')
        {
            IDataReader reader;

            switch (Path.GetExtension(fileName))
            {
                case ".xls":
                case ".xlsx":
                    reader = Excel.GetDataReader(fileStream, fileName);
                    break;

                default: //csv etc
                    var del = csvDelimiter;
                    reader = new CsvReader(new StreamReader(fileStream), fileHasHeaders, del, '"', '"', '#', ValueTrimmingOptions.UnquotedOnly);

                    break;
            }

            return reader;
        }

    }
}
