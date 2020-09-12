using System;
using System.Data;
using System.IO;
using DataPowerTools.Connectivity.Helpers;
using DataPowerTools.Extensions;
using ExcelDataReader;

namespace DataPowerTools.Connectivity
{
    public static class Excel
    {
        /// <summary>
        /// Gets a datareader from an excel stream (can be xls or xlsx).
        /// </summary>
        /// <param name="fileName">Used to determine whether it's an xls or xlsx</param>
        /// <param name="fileStream"></param>
        /// <param name="headerConfig">Header finding config.</param>
        /// <returns></returns>
        public static IDataReader GetDataReader(Stream fileStream, string fileName, HeaderReaderConfiguration headerConfig)
        {
            var reader = ExcelReaderFactory.CreateReader(fileStream).ApplyHeaders(headerConfig);

            if (reader == null)
                throw new Exception("Error creating excel reader");

            return reader;
        }

        /// <summary>
        /// Gets a datareader from an excel file (can be xls or xlsx).
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static IDataReader GetDataReader(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open);

            return ExcelReaderFactory.CreateReader(fs);
        }

        /// <summary>
        /// Returns a DataSet from an excel file.
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(Stream fs)
        {
            using var wb = ExcelReaderFactory.CreateReader(fs);

            var dd = wb.AsDataSet();

            return dd;
        }
    }
}