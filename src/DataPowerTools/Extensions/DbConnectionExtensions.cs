using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using DataPowerTools.PowerTools;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// <see cref="DbConnection"/> extensions.
    /// </summary>
    public static class DbConnectionExtensions
    {
        static readonly PropertyInfo ProviderFactoryPropertyInfo = typeof(DbConnection).GetProperty("DbProviderFactory", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Gets the database provider factory for the given <see cref="DbConnection"/>.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        /// <returns><see cref="DbProviderFactory"/>.</returns>
        public static DbProviderFactory GetDbProviderFactory(this DbConnection dbConnection)
        {
            // Note that in .NET v4.5 we could use this new method instead which would avoid the reflection:
            // DbProviderFactory dbProviderFactory = DbProviderFactories.GetFactory( databaseCommand.DbCommand.dbConnection );

            return (DbProviderFactory)ProviderFactoryPropertyInfo.GetValue(dbConnection, null);
        }

        /// <summary>
        /// Gets a blank table that is configured as the specified table. It uses SqlDataAdapter.FillSchema to accomplish this.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static DataTable GetDataSchema(this DbConnection connection, string tableName)
        {
            return Database.GetDataSchema(tableName, connection);
        }

        /// <summary>
        /// Closes and disposes the connection and the command itself.
        /// </summary>
        public static void CloseAndDispose(this DbConnection dbConnection)
        {
            dbConnection.Close();

            dbConnection.Dispose();
        }

        /// <summary>
        /// Generates a create table statement for the enumerable by fitting the data to a best fit table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="enumerable"></param>
        /// <param name="outputTableName"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <returns></returns>
        public static Task CreateTableFor<T>(this DbConnection connection, IEnumerable<T> enumerable, string outputTableName, bool ignoreNonStringReferenceTypes = true)
        {
            var sql = enumerable.FitToCreateTableStatement(outputTableName, null, ignoreNonStringReferenceTypes);

            var cmd = connection.CreateSqlCommand(sql);

            return cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Generates a create table statement for the enumerable by fitting the data to a best fit table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="reader"></param>
        /// <param name="outputTableName"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <returns></returns>
        public static Task CreateTableFor(this DbConnection connection, IDataReader reader, string outputTableName, bool ignoreNonStringReferenceTypes = true)
        {
            var sql = reader.FitToCreateTableStatement(outputTableName, null);

            var cmd = connection.CreateSqlCommand(sql);

            return cmd.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Inserts records into the database by generating insert statements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="enumerable"></param>
        /// <param name="tableName"></param>
        /// <param name="databaseEngine">The database engine.</param>
        /// <returns></returns>
        public static async Task<int> InsertRecords<T>(this DbConnection connection, IEnumerable<T> enumerable, string tableName, DatabaseEngine databaseEngine)
        {
            var cmd = connection.CreateSqlCommand();

            using (cmd)
            {
                cmd.AppendInserts(enumerable, tableName, databaseEngine);

                return await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}