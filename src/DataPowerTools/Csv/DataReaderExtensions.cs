using System.Data;

namespace DataPowerTools.Csv
{
    public static class DataReaderExtensions
    {
        /// <summary>
        /// Writes datareader to CSV.
        /// </summary>
        public static void WriteCsv(this IDataReader reader, string outputFile)
        {
            DataConnectivity.Csv.Write(reader, outputFile);
        }

        /// <summary>
        /// Writes datareader to CSV.
        /// </summary>
        public static string AsCsv(this IDataReader reader, bool writeHeaders = true)
        {
            return DataConnectivity.Csv.WriteString(reader);
        }
    }
}