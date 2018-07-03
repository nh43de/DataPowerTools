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
    /// Methods that assist in bulk uploading to the databases. These need to be cleaned up and unified as there is a lot of duplicated code.
    /// </summary>
    public static class SqlBulkUploadExtensions
    {
        /// <summary>
        /// Bulk insert data table using connection string.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="connectionString"></param>
        /// <param name="bulkInsertOptions"></param>
        public static void BulkInsertSqlServer(
            this DataTable data,
            string connectionString,
            string destinationTableName,
            BulkInsertOptions bulkInsertOptions = null)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                BulkInsertSqlServer(data, connection, destinationTableName, bulkInsertOptions);
            }
        }

        /// <summary>
        /// Bulk insert data table.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="connection"></param>
        /// <param name="bulkInsertOptions"></param>
        public static void BulkInsertSqlServer(
            this DataTable data,
            SqlConnection connection,
            string destinationTableName,
            BulkInsertOptions bulkInsertOptions = null)
        {
            var bulkCopy = GetSqlBulkCopy(data, destinationTableName, connection, bulkInsertOptions);
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
        /// <param name="bulkInsertOptions"></param>
        /// <returns></returns>
        public static async Task BulkInsertSqlServerAsync(
            this IDataReader dataReader,
            string connectionString,
            string destinationTableName,
            AsyncBulkInsertOptions bulkInsertOptions)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                await BulkInsertSqlServerAsync(dataReader, connection, destinationTableName, bulkInsertOptions);
            }
        }
        
        /// <summary>
        /// Bulk insert data reader async.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="connection"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="bulkInsertOptions"></param>
        /// <returns></returns>
        public static async Task BulkInsertSqlServerAsync(this IDataReader dataReader, SqlConnection connection, string destinationTableName, AsyncBulkInsertOptions bulkInsertOptions = null)
        {
            bulkInsertOptions = bulkInsertOptions ?? new AsyncBulkInsertOptions();
            var bulkCopy = GetSqlBulkCopy(dataReader, destinationTableName, connection, bulkInsertOptions);
            
            using (bulkCopy)
            {
                try
                {
                    await bulkCopy.WriteToServerAsync(dataReader, bulkInsertOptions.CancellationToken);
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
        /// <param name="bulkInsertOptions"></param>
        public static void BulkInsertSqlServer(this IDataReader dataReader, string connectionString, string destinationTableName, BulkInsertOptions bulkInsertOptions = null)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                dataReader.BulkInsertSqlServer(connection, destinationTableName, bulkInsertOptions);
            }
        }


        /// <summary>
        /// Bulk insert data reader synchronously.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="connection"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="bulkInsertOptions"></param>
        public static void BulkInsertSqlServer(this IDataReader dataReader, SqlConnection connection, string destinationTableName, BulkInsertOptions bulkInsertOptions = null)
        {
            bulkInsertOptions = bulkInsertOptions ?? new AsyncBulkInsertOptions();
            var bulkCopy = GetSqlBulkCopy(dataReader, destinationTableName, connection, bulkInsertOptions);
            
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
        /// <param name="bulkInsertOptions"></param>
        /// <returns></returns>
        public static SqlBulkCopy GetSqlBulkCopy(
            DataTable data,
            string destinationTableName,
            SqlConnection connection,
            BulkInsertOptions bulkInsertOptions = null)
        {
            bulkInsertOptions = bulkInsertOptions ?? new BulkInsertOptions();

            var bulkCopy = new SqlBulkCopy(connection, bulkInsertOptions.SqlBulkCopyOptions, bulkInsertOptions.SqlTransaction)
            {
                BulkCopyTimeout = bulkInsertOptions.BulkCopyTimeout,
                BatchSize = bulkInsertOptions.BatchSize,
                DestinationTableName = destinationTableName,
                NotifyAfter = bulkInsertOptions.BatchSize
            };
            
            PopulateBulkCopyMappings(bulkCopy, data, destinationTableName, connection, bulkInsertOptions.UseOrdinals);
            
            return bulkCopy;
        }
        
        /// <summary>
        /// Data reader version.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="connection"></param>
        /// <param name="bulkInsertOptions"></param>
        /// <returns></returns>
        public static SqlBulkCopy GetSqlBulkCopy(
            this IDataReader dataReader,
            string destinationTableName,
            SqlConnection connection,
            BulkInsertOptions bulkInsertOptions = null)
        {
            //if (Database.TableExists(connection, destinationTableName) == false)
            //    throw new Exception(
            //        $"Table {destinationTableName} does not exist using connection {connection.ConnectionString}");

            bulkInsertOptions = bulkInsertOptions ?? new BulkInsertOptions();

            var bulkCopy = new SqlBulkCopy(connection, bulkInsertOptions.SqlBulkCopyOptions, bulkInsertOptions.SqlTransaction)
            {
                BulkCopyTimeout = bulkInsertOptions.BulkCopyTimeout,
                BatchSize = bulkInsertOptions.BatchSize,
                DestinationTableName = destinationTableName,
                NotifyAfter = bulkInsertOptions.BatchSize,
                EnableStreaming = true
            };

            if (bulkInsertOptions.RowsCopiedEventHandler != null)
                bulkCopy.SqlRowsCopied += (e, i) => bulkInsertOptions.RowsCopiedEventHandler(i.RowsCopied);
            
            PopulateBulkCopyMappings(bulkCopy, dataReader, destinationTableName, connection, bulkInsertOptions.UseOrdinals);
            
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