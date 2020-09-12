using System.Data;
using System.IO;
using CsvDataReader;
using DataPowerTools.Extensions;

namespace DataPowerTools.Connectivity
{
    public static class Csv
    {
        public static IDataReader GetDataReader(string filePath, char csvDelimiter = ',', bool fileHasHeaders = true) // int headerOffsetRows = 1)
        {
            return new CsvReader(new StreamReader(filePath), fileHasHeaders, csvDelimiter);
        }

        public static IDataReader GetDataReader(Stream fileStream, char csvDelimiter = ',', bool fileHasHeaders = true) // int headerOffsetRows = 1)
        {
            return new CsvReader(new StreamReader(fileStream), fileHasHeaders, csvDelimiter);
        }

        public static DataSet GetDataSet(string filePath, char csvDelimiter = ',', bool fileHasHeaders = true) // int headerOffsetRows = 1)
        {
            var a = new CsvReader(new StreamReader(filePath), fileHasHeaders, csvDelimiter);

            return a.ToDataSet(null, a.GetFieldHeaders());
        }
        public static DataSet GetDataSet(Stream fileStream, char csvDelimiter = ',', bool fileHasHeaders = true) // int headerOffsetRows = 1)
        {
            var a = new CsvReader(new StreamReader(fileStream), fileHasHeaders, csvDelimiter);

            return a.ToDataSet(null, a.GetFieldHeaders());
        }
    }
}