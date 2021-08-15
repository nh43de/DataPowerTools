﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.Extensions.Objects;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// Extensions for interacting with SQL databases.
    /// </summary>
    public static class SqlPowerExtensions
    {
        #region sql connection 

        /// <summary>
        /// Creates sql connection from string and opens.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static SqlConnection CreateSqlConnection(this string connectionString)
        {
            var c = new SqlConnection(connectionString);
            c.Open();

            return c;
        }
        
        #endregion
        
        #region sql command

        /// <summary>
        /// Creates sql command from sql connection.
        /// </summary>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static SqlCommand CreateSqlCommand(this SqlConnection sqlc, string sql = null, SqlTransaction transaction = null)
        {
            return new SqlCommand(sql, sqlc, transaction);
        }

        /// <summary>
        /// Creates sql command from sql connection.
        /// </summary>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static TCommand CreateSqlCommand<TCommand, TConnection>(this TConnection sqlc, string sql = null, DbTransaction transaction = null)
        where TCommand : DbCommand
        where TConnection : DbConnection
        {
            var c = sqlc.CreateCommand();

            if(sql != null)
                c.CommandText = sql;

            if(transaction != null)
                c.Transaction = transaction;

            c.CommandType = CommandType.Text;

            return (TCommand)c;
        }

        /// <summary>
        /// Creates sql command from sql connection.
        /// </summary>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static DbCommand CreateSqlCommand(this DbConnection sqlc, string sql = null, DbTransaction transaction = null)
        {
            return sqlc.CreateSqlCommand<DbCommand, DbConnection>(sql, transaction);
        }


        #endregion

        #region reader

        /// <summary>
        /// Executes a data reader which will dispose the underlying command when the reader is disposed.
        /// </summary>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="commandBuilder">To pass in options to the command (e.g. set command timeouts, etc.)</param>
        /// <returns></returns>
        public static IDataReader ExecuteReader(this DbConnection sqlc, string sql, Action<DbCommand> commandBuilder = null)
        {
            var cmd = sqlc.CreateSqlCommand(sql);

            commandBuilder?.Invoke(cmd);

            return new DisposingDataReader<IDataReader>(cmd.ExecuteReader(), cmd);
        }

        #endregion

        #region list

        /// <summary>
        /// Executes to list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="maxRows"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        /// <returns></returns>
        public static List<T> ExecuteList<T>(this DbConnection sqlc, string sql, int? maxRows = null, Action<DbCommand> commandBuilder = null) where T : class
        {
            using (var cmd = sqlc.CreateSqlCommand(sql))
            {
                commandBuilder?.Invoke(cmd);
                return cmd.ExecuteList<T>(maxRows);
            }
        }

        /// <summary>
        /// Executes to list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static List<T> ExecuteList<T>(this DbCommand cmd, int? maxRows = null) where T : class
        {
            return cmd.ExecuteToEnumerable<T>(maxRows).ToList();
        }

        #endregion

        #region array

        /// <summary>
        /// Executes to array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="maxRows"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        /// <returns></returns>
        public static T[] ExecuteArray<T>(this DbConnection sqlc, string sql, int? maxRows = null, Action<DbCommand> commandBuilder = null) where T : class
        {
            using (var cmd = sqlc.CreateSqlCommand(sql))
            {
                commandBuilder?.Invoke(cmd);
                return cmd.ExecuteArray<T>(maxRows);
            }
        }

        /// <summary>
        /// Executes to array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static T[] ExecuteArray<T>(this DbCommand cmd, int? maxRows = null) where T : class
        {
            return cmd.ExecuteToEnumerable<T>(maxRows).ToArray();
        }

        #endregion

        #region dictionary 

        /// <summary>
        /// Executes to dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="sqlc"></param>
        /// <param name="keySelector"></param>
        /// <param name="elementSelector"></param>
        /// <param name="maxRows"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TElement> ExecuteDictionary<T, TKey, TElement>(this DbConnection sqlc, string sql, Func<T, TKey> keySelector, Func<T, TElement> elementSelector, int? maxRows = null, Action<DbCommand> commandBuilder = null) where T : class
        {
            using (var cmd = sqlc.CreateSqlCommand(sql))
            {
                commandBuilder?.Invoke(cmd);
                return cmd.ExecuteDictionary(keySelector, elementSelector, maxRows);
            }
        }
        
        /// <summary>
        /// Executes to dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="keySelector"></param>
        /// <param name="elementSelector"></param>
        /// <param name="maxRows"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TElement> ExecuteDictionary<T, TKey, TElement>(this DbCommand cmd, Func<T, TKey> keySelector, Func<T, TElement> elementSelector, int? maxRows = null) where T : class
        {
            return cmd.ExecuteToEnumerable<T>(maxRows).ToDictionary(keySelector, elementSelector);
        }

        #endregion

        #region  datatable

        /// <summary>
        /// Executes to data table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="maxRows"></param>
        /// <param name="name"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        /// <returns></returns>
        public static DataTable ExecuteToDataTable(this DbConnection sqlc, string sql, int? maxRows = null, string name = null, Action<DbCommand> commandBuilder = null)
        {
            using (var cmd = sqlc.CreateSqlCommand(sql))
            {
                commandBuilder?.Invoke(cmd);
                return cmd.ExecuteToDataTable(maxRows, name);
            }
        }

        /// <summary>
        /// Executes to data table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="maxRows"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DataTable ExecuteToDataTable(this DbCommand cmd, int? maxRows = null, string name = null)
        {
            using (var r = cmd.ExecuteReader())
            {
                return r.ToDataTable(maxRows, name);
            }
        }

        #endregion

        #region dataset

        /// <summary>
        /// Executes to data set
        /// </summary>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="dataSetName"></param>
        /// <param name="dataTableNames">The names to assign to the datatables.</param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        /// <returns></returns>
        public static DataSet ExecuteToDataSet(this DbConnection sqlc, string sql, string dataSetName = null, string[] dataTableNames = null, Action<DbCommand> commandBuilder = null)
        {
            using (var cmd = sqlc.CreateSqlCommand(sql))
            {
                commandBuilder?.Invoke(cmd);
                return cmd.ExecuteToDataSet(dataSetName, dataTableNames);
            }
        }

        /// <summary>
        /// Executes to data set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="dataSetName"></param>
        /// <param name="dataTableNames">The names to assign to the datatables.</param>
        /// <returns></returns>
        public static DataSet ExecuteToDataSet(this DbCommand cmd, string dataSetName = null, string[] dataTableNames = null)
        {
            using (var r = cmd.ExecuteReader())
            {
                return r.ToDataSet(dataSetName, dataTableNames);
            }
        }
        
        #endregion
        
        #region enumerable

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="maxRows">Number of rows to limit to.</param>
        /// <returns></returns>
        public static IDisposableEnumerable<T> ExecuteToEnumerable<T>(this DbCommand command, int? maxRows = null) where T: class
        {
            //TODO: need to dispose the enumerable structure and underlying reader when finished reading, otherwise will end up with multiple open result sets after reader finished reading

            IDataReader r = command.ExecuteReader();

            if (maxRows != null)
            {
                r = r.LimitRows(maxRows.Value);
            }

            return new DataReaderEnumerable<T>(r, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<IDisposableEnumerable<T>> ExecuteToEnumerableAsync<T>(this DbCommand command, CancellationToken? token = null) where T: class
        {
            var r = await command.ExecuteReaderAsync(token ?? CancellationToken.None);

            return new DataReaderEnumerable<T>(r, true);
        }


        /// <summary>
        /// This is done using FastMember, and is roughly 20% faster. However, this is much more strict with type casting.
        /// Ie. the C# types should match the SQL type equivalents exactly. 
        /// Even int/byte are not compatable with each other.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        public static IDisposableEnumerable<T> ExecuteToEnumerableFast<T>(this DbCommand command) where T : class
        {
            var r = command.ExecuteReader();

            return new DataReaderEnumerable<T>(r, true);
        }

        /// <summary>
        /// This is done using FastMember, and is roughly 20% faster. However, this is much more strict with type casting.
        /// Ie. the C# types should match the SQL type equivalents exactly. 
        /// Even int/byte are not compatable with each other.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<IDisposableEnumerable<T>> ExecuteToEnumerableAsyncFast<T>(this DbCommand command, CancellationToken? token = null) where T : class
        {
            var r = await command.ExecuteReaderAsync(token ?? CancellationToken.None);

            return new DataReaderEnumerable<T>(r, true);
        }

        #endregion

        #region Scalar

        /// <summary>
        /// Executes SQL (non-query) against a connection string (MSSQL).
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sql"></param>
        /// <param name="token"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        /// <returns></returns>
        public static async Task<T> ExecuteScalarAsync<T>(this string connectionString, string sql, Action<DbCommand> commandBuilder = null, CancellationToken? token = null)
        {
            using (var sqlc = new SqlConnection(connectionString))
            {
                sqlc.Open();
                return await sqlc.ExecuteScalarAsync<T>(sql, commandBuilder, token);
            }
        }

        /// <summary>
        /// Executes SQL (non-query) against a connection string (MSSQL).
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sql"></param>
        public static T ExecuteScalar<T>(this string connectionString, string sql)
        {
            using (var sqlc = new SqlConnection(connectionString))
            {
                sqlc.Open();
                return sqlc.ExecuteScalar<T>(sql);
            }
        }

        /// <summary>
        /// Executes SQL (non-query) against a connection.
        /// </summary>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="token"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        /// <returns></returns>
        public static async Task<T> ExecuteScalarAsync<T>(this DbConnection sqlc, string sql, Action<DbCommand> commandBuilder = null, CancellationToken? token = null)
        {
            using (var cmd = sqlc.CreateSqlCommand(sql))
            {
                commandBuilder?.Invoke(cmd);
                return await cmd.ExecuteScalarAsync<T>(token ?? CancellationToken.None);
            }
        }

        /// <summary>
        /// Executes SQL (non-query) against a connection.
        /// </summary>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        public static T ExecuteScalar<T>(this DbConnection sqlc, string sql, Action<DbCommand> commandBuilder = null)
        {
            using (var cmd = sqlc.CreateSqlCommand(sql))
            {
                commandBuilder?.Invoke(cmd);
                return cmd.ExecuteScalar<T>();
            }
        }
        
        /// <summary>
        /// Executes SQL (non-query) against a connection.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<T> ExecuteScalarAsync<T>(this DbCommand cmd, CancellationToken? token = null)
        {
            return (await cmd.ExecuteScalarAsync(token ?? CancellationToken.None)).ConvertTo<T>();
        }

        /// <summary>
        /// Executes SQL (non-query) against a connection.
        /// </summary>
        /// <param name="cmd"></param>
        public static T ExecuteScalar<T>(this DbCommand cmd)
        {
            return cmd.ExecuteScalar().ConvertTo<T>();
        }

        #endregion

        #region object 

        /// <summary>
        /// Executes SQL (non-query) against a connection string (MSSQL).
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sql"></param>
        /// <param name="token"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        /// <returns></returns>
        public static async Task<T> ExecuteObjectAsync<T>(this string connectionString, string sql, Action<DbCommand> commandBuilder = null, CancellationToken? token = null) where T : class
        {
            using (var sqlc = new SqlConnection(connectionString))
            {
                sqlc.Open();
                return await sqlc.ExecuteObjectAsync<T>(sql, commandBuilder, token);
            }
        }

        /// <summary>
        /// Executes SQL (non-query) against a connection string (MSSQL).
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sql"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        public static T ExecuteObject<T>(this string connectionString, string sql, Action<DbCommand> commandBuilder = null) where T : class
        {
            using (var sqlc = new SqlConnection(connectionString))
            {
                sqlc.Open();
                return sqlc.ExecuteObject<T>(sql, commandBuilder);
            }
        }

        /// <summary>
        /// Executes SQL (non-query) against a connection.
        /// </summary>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="token"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        /// <returns></returns>
        public static async Task<T> ExecuteObjectAsync<T>(this DbConnection sqlc, string sql, Action<DbCommand> commandBuilder = null, CancellationToken? token = null) where T : class
        {
            using (var cmd = sqlc.CreateSqlCommand(sql))
            {
                commandBuilder?.Invoke(cmd);
                return await cmd.ExecuteObjectAsync<T>(token);
            }
        }

        /// <summary>
        /// Executes SQL (non-query) against a connection.
        /// </summary>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        public static T ExecuteObject<T>(this DbConnection sqlc, string sql, Action<DbCommand> commandBuilder = null) where T : class
        {
            using (var cmd = sqlc.CreateSqlCommand(sql))
            {
                commandBuilder?.Invoke(cmd);
                return cmd.ExecuteObject<T>();
            }
        }
        
        /// <summary>
        /// Executes SQL (non-query) against a connection.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<T> ExecuteObjectAsync<T>(this DbCommand cmd, CancellationToken? token = null) where T: class 
        {
            return (await cmd.ExecuteToEnumerableAsync<T>(token)).FirstOrDefault();
        }

        /// <summary>
        /// Executes SQL (non-query) against a connection.
        /// </summary>
        /// <param name="cmd"></param>
        public static T ExecuteObject<T>(this DbCommand cmd) where T : class
        {
            return cmd.ExecuteToEnumerable<T>().FirstOrDefault();
        }

        #endregion

        #region non-query

        /// <summary>
        /// Executes SQL (non-query) against a connection string (MSSQL).
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sql"></param>
        /// <param name="transaction"></param>
        /// <param name="token"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        /// <returns></returns>
        public static async Task ExecuteSqlAsync(this string connectionString, string sql, DbTransaction transaction = null, Action<DbCommand> commandBuilder = null, CancellationToken? token = null)
        {
            using (var sqlc = new SqlConnection(connectionString))
            {
                sqlc.Open();
                await sqlc.ExecuteSqlAsync(sql, transaction, commandBuilder, token);
            }
        }

        /// <summary>
        /// Executes SQL (non-query) against a connection string (MSSQL).
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="sql"></param>
        /// <param name="transaction"></param>
        public static void ExecuteSql(this string connectionString, string sql, DbTransaction transaction = null)
        {
            using (var sqlc = new SqlConnection(connectionString))
            {
                sqlc.Open();
                sqlc.ExecuteSql(sql, transaction);
            }
        }

        /// <summary>
        /// Executes SQL (non-query) against a connection.
        /// </summary>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="transaction"></param>
        /// <param name="token"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        /// <returns></returns>
        public static async Task ExecuteSqlAsync(this DbConnection sqlc, string sql, DbTransaction transaction = null, Action<DbCommand> commandBuilder = null, CancellationToken? token = null)
        {
            //TODO: convention: open conection if closed, and close again if closed

            using (var cmd = sqlc.CreateSqlCommand(sql))
            {
                cmd.Transaction = transaction;
                commandBuilder?.Invoke(cmd);
                await cmd.ExecuteNonQueryAsync(token ?? CancellationToken.None);
            }
        }

        /// <summary>
        /// Executes SQL (non-query) against a connection.
        /// </summary>
        /// <param name="sqlc"></param>
        /// <param name="sql"></param>
        /// <param name="transaction"></param>
        /// <param name="commandBuilder">Configures the command before executing, e.g. to add command timeout.</param>
        public static void ExecuteSql(this DbConnection sqlc, string sql, DbTransaction transaction = null, Action<DbCommand> commandBuilder = null)
        {
            using (var cmd = sqlc.CreateSqlCommand(sql))
            {
                cmd.Transaction = transaction;
                commandBuilder?.Invoke(cmd);
                cmd.ExecuteNonQuery();
            }
        }

        public static async Task OpenIfClosedAsync(this DbConnection sqlc)
        {
            if (sqlc.State != ConnectionState.Open)
                await sqlc.OpenAsync();
        }

        public static void OpenIfClosed(this DbConnection sqlc)
        {
            if (sqlc.State != ConnectionState.Open)
                sqlc.Open();
        }

        #endregion
    }
}