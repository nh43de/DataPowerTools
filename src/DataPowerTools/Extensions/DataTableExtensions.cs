using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            return dataTable.CreateDataReader().CountRows();
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
        /// Writes a DataTable to CSV file with Excel-friendly formatting.
        /// 
        /// Excel Compatibility:
        /// - Uses UTF-8 with BOM by default for emoji and international character support
        /// - RFC 4180 compliant with CRLF line endings
        /// - If Excel shows one column, use Data > From Text/CSV and select UTF-8 encoding
        /// </summary>
        /// <param name="dataTable">The DataTable to export</param>
        /// <param name="outputFile">The output file path</param>
        /// <param name="format">The format to use for the CSV output (UTF8 with BOM recommended for Excel)</param>
        public static void WriteCsv(this DataTable dataTable, string outputFile, CSVFormat format = CSVFormat.UTF8)
        {
            using (var reader = dataTable.CreateDataReader())
            {
                reader.WriteCsv(outputFile, format);
            }
        }
        
        /// <summary>
        /// Prints all data in a DataTable.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string PrintData(this DataTable table)
        {
            var sb = new StringBuilder();

            sb.Append("[");

            foreach (DataRow rowItem in table.Rows)
            {
                sb.AppendLine(rowItem.PrintRow() + ",");
            }

            sb.Remove(sb.Length - 3, 3);

            sb.Append("]");

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

        public static string[] GetPrimaryKeys(this DataTable schema)
        {
            var keys = new List<string>();

            foreach (DataRow column in schema.Rows)
                if (schema.Columns.Contains("IsKey") && (bool) column["IsKey"])
                    keys.Add(column["ColumnName"].ToString());

            return keys.ToArray();
        }

        /// <summary>
        /// Generates a table for the enumerable by fitting the data to a best fit table and executing it against the connection.
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        /// </summary>
        public static async Task CreateTableFor(this DataTable dataTable, DbConnection connection, string tableName, int? rowLimit = null, Action<DbCommand> commandBuilder = null, CancellationToken token = default(CancellationToken))
        {
            var sql = dataTable.ToDataReader().FitToCreateTableSql(tableName, rowLimit);

            await connection.ExecuteSqlAsync(sql, null, commandBuilder, token);
        }

        /// <summary>
        /// Generates a create table statement for the enumerable by fitting the data to a best fit table.
        /// </summary>
        public static string FitToCreateTableSql(this DataTable dataTable, string tableName, int? rowLimit = null)
        {
            var sql = dataTable.ToDataReader().FitToCreateTableSql(tableName, rowLimit);

            return sql;
        }
    }
}