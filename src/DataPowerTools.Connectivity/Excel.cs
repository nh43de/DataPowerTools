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

#if NETSTANDARD2_0
        static Excel()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }
#endif

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
        /// <param name="headerConfig"></param>
        /// <returns></returns>
        public static IDataReader GetDataReader(string filePath, HeaderReaderConfiguration headerConfig = null)
        {
            var fs = new FileStream(filePath, FileMode.Open);

            var r = (IDataReader)ExcelReaderFactory.CreateReader(fs);

            if (headerConfig != null)
            {
                r = r.ApplyHeaders(headerConfig);
            }

            return r;
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