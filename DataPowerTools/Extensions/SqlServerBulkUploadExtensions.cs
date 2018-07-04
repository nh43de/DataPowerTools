using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataPowerTools.DataConnectivity.Sql;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.PowerTools;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// These are specific to SQL SERVER, and uses bcp (bulkcopy) to load records into db very quickly.
    /// Methods that assist in bulk uploading to the databases. These need to be cleaned up and unified as there is a lot of duplicated code.
    /// </summary>
    public static class SqlServerBulkUploadExtensions
    {

        /// <summary>
        /// Bulk upload enumerable using a server/database name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="destinationServer"></param>
        /// <param name="destinationDatabase"></param>
        /// <param name="destinationTable"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        /// <param name="members"></param>
        public static void BulkUploadSqlServer<T>(
            this IEnumerable<T> items,
            string destinationServer,
            string destinationDatabase,
            string destinationTable,
            SqlServerBulkInsertOptions sqlServerBulkInsertOptions = null,
            IEnumerable<string> members = null)
        {
            var cs = Database.GetConnectionString(destinationDatabase, destinationServer);

            BulkUploadSqlServer(items, cs, destinationTable, sqlServerBulkInsertOptions, members);
        }

        /// <summary>
        /// Bulk upload enumerable using a connection string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="connectionString"></param>
        /// <param name="destinationTable"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        /// <param name="members"></param>
        public static void BulkUploadSqlServer<T>(
            this IEnumerable<T> items,
            string connectionString,
            string destinationTable,
            SqlServerBulkInsertOptions sqlServerBulkInsertOptions = null,
            IEnumerable<string> members = null
        )
        {
            members = members ??
                typeof(T).GetProperties().Where(p =>
                        (p.GetGetMethod().IsVirtual == false) && p.GetGetMethod().IsPublic && //TODO: this will have to change if we use dynamic proxies (we use this to skip navigation properties)
                        (p.GetIndexParameters().Length == 0)).Select(p => p.Name).ToArray();

            var uploadReader = items.ToDataReader(members?.ToArray());

            uploadReader.BulkInsertSqlServer(connectionString, destinationTable, sqlServerBulkInsertOptions ?? new SqlServerBulkInsertOptions());
        }

        /// <summary>
        /// Bulk insert data table using connection string.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="connectionString"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        public static void BulkInsertSqlServer(
            this DataTable data,
            string connectionString,
            string destinationTableName,
            SqlServerBulkInsertOptions sqlServerBulkInsertOptions = null)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                BulkInsertSqlServer(data, connection, destinationTableName, sqlServerBulkInsertOptions);
            }
        }

        /// <summary>
        /// Bulk insert data table.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="connection"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        public static void BulkInsertSqlServer(
            this DataTable data,
            SqlConnection connection,
            string destinationTableName,
            SqlServerBulkInsertOptions sqlServerBulkInsertOptions = null)
        {
            var bulkCopy = GetSqlBulkCopy(data, destinationTableName, connection, sqlServerBulkInsertOptions);
            using (bulkCopy)
            {
                bulkCopy.WriteToServer(data);
            }
        }

        /// <summary>
        /// Bulk insert data reader async using connection string.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="connectionString"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        /// <returns></returns>
        public static async Task BulkInsertSqlServerAsync(
            this IDataReader dataReader,
            string connectionString,
            string destinationTableName,
            AsyncSqlServerBulkInsertOptions sqlServerBulkInsertOptions)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                await BulkInsertSqlServerAsync(dataReader, connection, destinationTableName, sqlServerBulkInsertOptions);
            }
        }
        
        /// <summary>
        /// Bulk insert data reader async.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="connection"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        /// <returns></returns>
        public static async Task BulkInsertSqlServerAsync(this IDataReader dataReader, SqlConnection connection, string destinationTableName, AsyncSqlServerBulkInsertOptions sqlServerBulkInsertOptions = null)
        {
            sqlServerBulkInsertOptions = sqlServerBulkInsertOptions ?? new AsyncSqlServerBulkInsertOptions();
            var bulkCopy = GetSqlBulkCopy(dataReader, destinationTableName, connection, sqlServerBulkInsertOptions);
            
            using (bulkCopy)
            {
                try
                {
                    await bulkCopy.WriteToServerAsync(dataReader, sqlServerBulkInsertOptions.CancellationToken);
                }
                catch (SqlException ex)
                {
                    var errmsgbcp = "";
                    if (ex.Message.Contains("Received an invalid column length from the bcp client for colid"))
                    {
                        errmsgbcp += GetBulkCopyDetailedExceptionMessage(ex, bulkCopy);
                        throw new Exception(errmsgbcp, ex);
                    }
                    throw;
                }
            }
        }
        
        /// <summary>
        /// Bulk insert data reader using connection string.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="connectionString"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        public static void BulkInsertSqlServer(this IDataReader dataReader, string connectionString, string destinationTableName, SqlServerBulkInsertOptions sqlServerBulkInsertOptions = null)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                dataReader.BulkInsertSqlServer(connection, destinationTableName, sqlServerBulkInsertOptions);
            }
        }


        /// <summary>
        /// Bulk insert data reader synchronously.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="connection"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        public static void BulkInsertSqlServer(this IDataReader dataReader, SqlConnection connection, string destinationTableName, SqlServerBulkInsertOptions sqlServerBulkInsertOptions = null)
        {
            sqlServerBulkInsertOptions = sqlServerBulkInsertOptions ?? new AsyncSqlServerBulkInsertOptions();
            var bulkCopy = GetSqlBulkCopy(dataReader, destinationTableName, connection, sqlServerBulkInsertOptions);
            
            using (bulkCopy)
            {
                try
                {
                    bulkCopy.WriteToServer(dataReader);
                }
                catch (SqlException ex)
                {
                    var errmsgbcp = "";
                    if (ex.Message.Contains("Received an invalid column length from the bcp client for colid"))
                    {
                        errmsgbcp += GetBulkCopyDetailedExceptionMessage(ex, bulkCopy);
                        throw new Exception(errmsgbcp, ex);
                    }
                    throw;               
                }
            }
        }

        /// <summary>
        /// TODO: this needs to be implemented (for MP, needs to be MapAllSourceColumns).
        /// </summary>
        public enum BulkMappingOptions
        {
            /// <summary>
            /// Maps source columns to destination columns, if columns on either side are left unmapped, no error is thrown.
            /// </summary>
            Default,
            /// <summary>
            /// All source columns must be mapped to a destination column.
            /// </summary>
            MapAllSourceColumns,
            /// <summary>
            /// All destination columns must be mapped.
            /// </summary>
            MapAllDestinationColumns,
            /// <summary>
            /// All source and destination columns must be mapped.
            /// </summary>
            Strict
        }

        /// <summary>
        /// Data table version.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="connection"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        /// <returns></returns>
        public static SqlBulkCopy GetSqlBulkCopy(
            DataTable data,
            string destinationTableName,
            SqlConnection connection,
            SqlServerBulkInsertOptions sqlServerBulkInsertOptions = null)
        {
            sqlServerBulkInsertOptions = sqlServerBulkInsertOptions ?? new SqlServerBulkInsertOptions();

            var bulkCopy = new SqlBulkCopy(connection, sqlServerBulkInsertOptions.SqlBulkCopyOptions, sqlServerBulkInsertOptions.SqlTransaction)
            {
                BulkCopyTimeout = sqlServerBulkInsertOptions.BulkCopyTimeout,
                BatchSize = sqlServerBulkInsertOptions.BatchSize,
                DestinationTableName = destinationTableName,
                NotifyAfter = sqlServerBulkInsertOptions.BatchSize
            };
            
            PopulateBulkCopyMappings(bulkCopy, data, destinationTableName, connection, sqlServerBulkInsertOptions.UseOrdinals);
            
            return bulkCopy;
        }
        
        /// <summary>
        /// Data reader version.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="connection"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        /// <returns></returns>
        public static SqlBulkCopy GetSqlBulkCopy(
            this IDataReader dataReader,
            string destinationTableName,
            SqlConnection connection,
            SqlServerBulkInsertOptions sqlServerBulkInsertOptions = null)
        {
            //if (Database.TableExists(connection, destinationTableName) == false)
            //    throw new Exception(
            //        $"Table {destinationTableName} does not exist using connection {connection.ConnectionString}");

            sqlServerBulkInsertOptions = sqlServerBulkInsertOptions ?? new SqlServerBulkInsertOptions();

            var bulkCopy = new SqlBulkCopy(connection, sqlServerBulkInsertOptions.SqlBulkCopyOptions, sqlServerBulkInsertOptions.SqlTransaction)
            {
                BulkCopyTimeout = sqlServerBulkInsertOptions.BulkCopyTimeout,
                BatchSize = sqlServerBulkInsertOptions.BatchSize,
                DestinationTableName = destinationTableName,
                NotifyAfter = sqlServerBulkInsertOptions.BatchSize,
                EnableStreaming = true
            };

            if (sqlServerBulkInsertOptions.RowsCopiedEventHandler != null)
                bulkCopy.SqlRowsCopied += (e, i) => sqlServerBulkInsertOptions.RowsCopiedEventHandler(i.RowsCopied);
            
            PopulateBulkCopyMappings(bulkCopy, dataReader, destinationTableName, connection, sqlServerBulkInsertOptions.UseOrdinals);
            
            return bulkCopy;
        }



        private static void PopulateBulkCopyMappings(SqlBulkCopy bulkCopy, DataTable dataTable, string destinationTable, SqlConnection destinationConnection, bool useOrdinals = false)
        {
            var sourceColumnNames = dataTable.Columns.OfType<DataColumn>().Select(col => col.ColumnName).ToArray();

            try
            {
                var destinationNonComputedColumnNames = Database.GetNonComputedColumns(destinationTable, destinationConnection);
                PopulateBulkCopyMappings(bulkCopy, sourceColumnNames, destinationNonComputedColumnNames, useOrdinals);
            }
            catch (Exception)
            {
                PopulateBulkCopyMappings(bulkCopy, sourceColumnNames);
            }
        }

        private static void PopulateBulkCopyMappings(SqlBulkCopy bulkCopy, IDataReader reader, string destinationTable, SqlConnection destinationConnection, bool useOrdinals = false)
        {
            var sourceColumnNames = reader.GetFieldNames();
            try
            {
                var destinationNonComputedColumnNames = Database.GetNonComputedColumns(destinationTable, destinationConnection);
                PopulateBulkCopyMappings(bulkCopy, sourceColumnNames, destinationNonComputedColumnNames, useOrdinals);
            }
            catch (Exception)
            {
                PopulateBulkCopyMappings(bulkCopy, sourceColumnNames);
            }
        }

        //private static void PopulateBulkCopyMappings(SqlBulkCopy bulkCopy, PrimitiveColumnMapping[] mappings)
        //{
        //    foreach (var colItem in mappings)
        //    {
        //        bulkCopy.ColumnNameToTypeDictionary.Add(new SqlBulkCopyColumnMapping(colItem.SourceName, colItem.DestinationName));
        //    }
        //}

        private static void PopulateBulkCopyMappings(SqlBulkCopy bulkCopy, IEnumerable<string> sourceColumnNames, IEnumerable<string> destinationColumnNames, bool useOrdinals = false)
        {
            if (!useOrdinals)
            {
                var matchedColumns = sourceColumnNames.GroupJoin(destinationColumnNames, s => s.ToLower(), s => s.ToLower(), //matches irrespective of casing
                (sourceCol, destCols) =>
                new {
                    SourceColumn = sourceCol,
                    DestinationColumns = destCols.ToArray()
                }).ToArray();
                
                //try using explicit naming
                foreach (var colItem in matchedColumns)
                {
                    if (colItem.DestinationColumns.Length > 1)
                    {
                        throw new Exception("Found multiple matching destination columns: " + colItem.DestinationColumns.JoinStr(", "));
                    }

                    var destCol = colItem.DestinationColumns.FirstOrDefault();

                    if (destCol != null)
                    {
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(colItem.SourceColumn, destCol));
                    }
                }
            }
            else //use ordinals
            {
                var columnNames = sourceColumnNames.ToArray();

                //clear and try using ordinals
                bulkCopy.ColumnMappings.Clear();

                var i = 0;
                foreach (var colItem in columnNames)
                {
                    bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, i));
                    i++;
                }
            }
        }

        private static void PopulateBulkCopyMappings(SqlBulkCopy bulkCopy, IEnumerable<string> columnNames, bool useOrdinals = false)
        {
            if (!useOrdinals)
            {
                //try using explicit naming
                foreach (var colItem in columnNames)
                    bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(colItem, colItem));
            }
            else
            {
                //clear and try using ordinals
                bulkCopy.ColumnMappings.Clear();

                var i = 0;
                foreach (var colItem in columnNames)
                {
                    bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(i, i));
                    i++;
                }
            }
        }

        public static string GetBulkCopyDetailedExceptionMessage(SqlException ex, SqlBulkCopy bulkCopy)
        {
            //TODO: this exception is good stuff
            var pattern = @"\d+";
            var match = Regex.Match(ex.Message, pattern);
            var index = Convert.ToInt32(match.Value) - 1;

            var fi = typeof(SqlBulkCopy).GetField("_sortedColumnMappings",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var sortedColumns = fi.GetValue(bulkCopy);
            var items =
                (object[])
                sortedColumns.GetType()
                    .GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(sortedColumns);

            var itemdata = items[index].GetType()
                .GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance);
            var metadata = itemdata.GetValue(items[index]);

            var column =
                metadata.GetType()
                    .GetField("column", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(metadata);
            var length =
                metadata.GetType()
                    .GetField("length", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(metadata);
            return $"Column: {column} contains data with a length greater than: {length}";
        }
    }
}