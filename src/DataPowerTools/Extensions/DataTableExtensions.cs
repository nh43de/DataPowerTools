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
        /// </summary>
        public static async Task CreateTableFor(this DataTable dataTable, DbConnection connection, string tableName, int? rowLimit = null, CancellationToken token = default(CancellationToken))
        {
            var sql = dataTable.ToDataReader().FitToCreateTableSql(tableName, rowLimit);

            await connection.ExecuteSqlAsync(sql, null, token);
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