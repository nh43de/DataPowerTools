using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataPowerTools.DataConnectivity;

namespace DataPowerTools.Extensions
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// Changes DataTable column ordinals so that columns are in alphabetical order.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="columnNames"></param>
        private static void SortColumns(this DataTable dataTable, IEnumerable<string> columnNames)
        {
            var columns = columnNames.ToList();
            foreach (var columnName in columns)
            {
                var ordinal = columns.IndexOf(columnName);
                dataTable.Columns[columnName].SetOrdinal(ordinal);
            }
        }

        /// <summary>
        /// Creates a DataReader from a datatable.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static IDataReader ToDataReader(this DataTable dataTable)
        {
            return dataTable.CreateDataReader();
        }

        /// <summary>
        /// Removes columns that are not present in the column name enumerable.
        /// </summary>
        /// <param name="dataTable">The datatable to remove columns from.</param>
        /// <param name="columnNames">The column names that you wish to keep.</param>
        private static void RemoveNotExistingColumns(this DataTable dataTable, IEnumerable<string> columnNames)
        {
            var enumerable = columnNames as string[] ?? columnNames.ToArray();

            for (var i = dataTable.Columns.Count - 1; i >= 0; i--)
            {
                var dataColumn = dataTable.Columns[i];
                var removeColumn = !enumerable.Contains(dataColumn.ColumnName);
                if (removeColumn)
                    dataTable.Columns.Remove(dataColumn.ColumnName);
            }
        }

        /// <summary>
        /// Writes a DataTable to CSV.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="outputFile"></param>
        public static void WriteCsv(this DataTable dt, string outputFile)
        {
            var headers = dt.Columns.OfType<DataColumn>().Select(p => p.ColumnName).ToArray();

            var objects = dt.Rows.OfType<DataRow>().Select(r => r.ItemArray);

            DataConnectivity.Csv.Write(objects, headers, outputFile);
        }
        
        /// <summary>
        /// Prints all data in a DataTable.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string PrintData(this DataTable table)
        {
            var sb = new StringBuilder();
            foreach (DataRow rowItem in table.Rows)
                sb.AppendLine(rowItem.PrintRow());
            return sb.ToString();
        }

        /// <summary>
        /// Gets the DataColumns of the DataTable. Shorthand for dataTable.Columns.OfType().ToArray();
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static DataColumn[] GetDataColumns(this DataTable dataTable)
        {
            return dataTable.Columns.OfType<DataColumn>().ToArray();
        }
    }
}