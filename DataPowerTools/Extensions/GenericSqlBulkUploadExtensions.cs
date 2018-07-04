using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataPowerTools.DataConnectivity.Sql;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.PowerTools;
using Sqlite.Extensions;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// This generates inserts statements. Methods that assist in bulk uploading to the databases. These need to be cleaned up and unified as there is a lot of duplicated code.
    /// </summary>
    public static class GenericSqlBulkUploadExtensions
    {
        /// <summary>
        /// Bulk upload enumerable by generating insert statements. If using sql server, use SqlServer extension methods instead.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="dbConnection"></param>
        /// <param name="destinationTable"></param>
        /// <param name="databaseEngine"></param>
        /// <param name="bulkInsertOptions"></param>
        public static async Task BulkInsert<T>(
            this IEnumerable<T> items,
            DbConnection dbConnection,
            string destinationTable,
            DatabaseEngine databaseEngine,
            GenericBulkCopyOptions bulkInsertOptions = null)
        {
            var d = new GenericBulkCopy(dbConnection, bulkInsertOptions);

            await d.WriteToServer(items.ToDataReader(), destinationTable, databaseEngine);
        }

        /// <summary>
        /// Bulk insert data table. If using sql server, use SqlServer extension methods instead.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="connection"></param>
        /// <param name="databaseEngine"></param>
        /// <param name="bulkInsertOptions"></param>
        public static async Task BulkInsert(
            this DataTable data,
            DbConnection connection,
            string destinationTableName,
            DatabaseEngine databaseEngine,
            GenericBulkCopyOptions bulkInsertOptions = null)
        {
            var bulkCopy = GetBulkCopy(destinationTableName, connection, bulkInsertOptions);

            await bulkCopy.WriteToServer(data.ToDataReader(), destinationTableName, databaseEngine);
        }

        /// <summary>
        /// Bulk insert data reader synchronously. If using sql server, use SqlServer extension methods instead.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="connection"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="databaseEngine"></param>
        /// <param name="bulkInsertOptions"></param>
        public static async Task BulkInsert(
            this IDataReader dataReader, 
            DbConnection connection, 
            string destinationTableName,
            DatabaseEngine databaseEngine,
            GenericBulkCopyOptions bulkInsertOptions = null)
        {
            var bulkCopy = GetBulkCopy(destinationTableName, connection, bulkInsertOptions);
            
            await bulkCopy.WriteToServer(dataReader, destinationTableName, databaseEngine);
        }

        /// <summary>
        /// Data table version.
        /// </summary>
        /// <param name="destinationTableName"></param>
        /// <param name="connection"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        /// <returns></returns>
        private static GenericBulkCopy GetBulkCopy(
            string destinationTableName,
            DbConnection connection,
            GenericBulkCopyOptions sqlServerBulkInsertOptions = null)
        {
            sqlServerBulkInsertOptions = sqlServerBulkInsertOptions ?? new GenericBulkCopyOptions();

            var bulkCopy = new GenericBulkCopy(connection, sqlServerBulkInsertOptions);
            
            return bulkCopy;
        }
    }
}