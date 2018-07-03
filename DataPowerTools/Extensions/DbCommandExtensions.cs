/*
    SOME CODE FROM 

    Sequelocity.NET v0.6.0

    Sequelocity.NET is a simple data access library for the Microsoft .NET
    Framework providing lightweight ADO.NET wrapper, object mapper, and helper
    functions. To find out more, visit the project home page at: 
    https://github.com/AmbitEnergyLabs/Sequelocity.NET

    The MIT License (MIT)

    Copyright (c) 2015 Ambit Energy. All rights reserved.

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using DataPowerTools.PowerTools;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// <see cref="DbCommand" /> extensions.
    /// </summary>
    public static class DbCommandExtensions
    {
        /// <summary>Sets the text command to run against the data source.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="commandText">The text command to run against the data source.</param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        public static DbCommand SetCommandText(this DbCommand dbCommand, string commandText)
        {
            dbCommand.CommandText = commandText;

            return dbCommand;
        }

        /// <summary>Appends (+=) to the text command to run against the data source.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="commandText">Text command to append to the text command to run against the data source.</param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        public static DbCommand AppendCommandText(this DbCommand dbCommand, string commandText)
        {
            dbCommand.CommandText += commandText;

            return dbCommand;
        }

        /// <summary>Creates a <see cref="DbParameter" />.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterValue">Parameter value.</param>
        /// <returns><see cref="DbParameter" />.</returns>
        public static DbParameter CreateParameter(this DbCommand dbCommand, string parameterName, object parameterValue)
        {
            var parameter = dbCommand.CreateParameter();

            parameter.ParameterName = parameterName;

            parameter.Value = parameterValue;

            return parameter;
        }

        /// <summary>Creates a <see cref="DbParameter" /> with a given <see cref="DbType" />.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterValue">Parameter value.</param>
        /// <param name="dbType">Parameter type.</param>
        /// <returns><see cref="DbParameter" />.</returns>
        public static DbParameter CreateParameter(this DbCommand dbCommand, string parameterName, object parameterValue,
            DbType dbType)
        {
            var parameter = dbCommand.CreateParameter();

            parameter.ParameterName = parameterName;

            parameter.Value = parameterValue;

            parameter.DbType = dbType;

            return parameter;
        }

        /// <summary>
        /// Creates a <see cref="DbParameter" /> with a given <see cref="DbType" /> and <see cref="ParameterDirection" />.
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterValue">Parameter value.</param>
        /// <param name="dbType">Parameter type.</param>
        /// <param name="parameterDirection">Parameter direction.</param>
        /// <returns><see cref="DbParameter" />.</returns>
        public static DbParameter CreateParameter(this DbCommand dbCommand, string parameterName, object parameterValue,
            DbType dbType, ParameterDirection parameterDirection)
        {
            var parameter = dbCommand.CreateParameter();

            parameter.ParameterName = parameterName;

            parameter.Value = parameterValue;

            parameter.DbType = dbType;

            parameter.Direction = parameterDirection;

            return parameter;
        }

        /// <summary>Adds a <see cref="DbParameter" /> to the <see cref="DbCommand" />.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="dbParameter"><see cref="DbParameter" /> to add.</param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="dbParameter" /> parameter is null.</exception>
        public static DbCommand AddParameter(this DbCommand dbCommand, DbParameter dbParameter)
        {
            if (dbParameter == null)
            {
                throw new ArgumentNullException(nameof(dbParameter));
            }

            dbCommand.Parameters.Add(dbParameter);

            return dbCommand;
        }

        /// <summary>Adds a parameter to the <see cref="DbCommand" />.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterValue">Parameter value.</param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parameterName" /> parameter is null.</exception>
        public static DbCommand AddParameter(this DbCommand dbCommand, string parameterName, object parameterValue)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (parameterValue == null)
            {
                parameterValue = DBNull.Value;
            }

            var parameter = dbCommand.CreateParameter(parameterName, parameterValue);

            dbCommand.Parameters.Add(parameter);

            return dbCommand;
        }

        /// <summary>Adds a parameter to the <see cref="DbCommand" />.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterValue">Parameter value.</param>
        /// <param name="dbType">Parameter type.</param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="parameterName" /> parameter is null.</exception>
        public static DbCommand AddParameter(this DbCommand dbCommand, string parameterName, object parameterValue,
            DbType dbType)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentNullException(nameof(parameterName));
            }

            if (parameterValue == null)
            {
                parameterValue = DBNull.Value;
            }

            var parameter = dbCommand.CreateParameter(parameterName, parameterValue, dbType);

            dbCommand.Parameters.Add(parameter);

            return dbCommand;
        }

        /// <summary>Adds a list of <see cref="DbParameter" />s to the <see cref="DbCommand" />.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="dbParameters">List of database parameters.</param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        public static DbCommand AddParameters(this DbCommand dbCommand, IEnumerable<DbParameter> dbParameters)
        {
            foreach (var dbParameter in dbParameters)
            {
                dbCommand.AddParameter(dbParameter);
            }

            return dbCommand;
        }

        /// <summary>Adds a parameter array of <see cref="DbParameter" />s to the <see cref="DbCommand" />.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="dbParameters">Parameter array of database parameters.</param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        public static DbCommand AddParameters(this DbCommand dbCommand, params DbParameter[] dbParameters)
        {
            foreach (var dbParameter in dbParameters)
            {
                dbCommand.AddParameter(dbParameter);
            }

            return dbCommand;
        }

        /// <summary>Adds a dictionary of parameter names and values to the <see cref="DbCommand" />.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="parameterNameAndValueDictionary">Dictionary of parameter names and values.</param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="parameterNameAndValueDictionary" /> parameter
        /// is null.
        /// </exception>
        public static DbCommand AddParameters(this DbCommand dbCommand,
            IDictionary<string, object> parameterNameAndValueDictionary)
        {
            if (parameterNameAndValueDictionary == null)
            {
                throw new ArgumentNullException(nameof(parameterNameAndValueDictionary));
            }

            foreach (var parameterNameAndValue in parameterNameAndValueDictionary)
            {
                dbCommand.AddParameter(parameterNameAndValue.Key, parameterNameAndValue.Value);
            }

            return dbCommand;
        }
        
        /// <summary>Sets the CommandType.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="commandType">CommandType which specifies how a command string is interpreted.</param>
        public static DbCommand SetCommandType(this DbCommand dbCommand, CommandType commandType)
        {
            dbCommand.CommandType = commandType;

            return dbCommand;
        }

        /// <summary>
        /// Sets the time in seconds to wait for the command to execute before throwing an exception. The default is 30 seconds.
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="commandTimeoutSeconds">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        public static DbCommand SetCommandTimeout(this DbCommand dbCommand, int commandTimeoutSeconds)
        {
            dbCommand.CommandTimeout = commandTimeoutSeconds;

            return dbCommand;
        }

        /// <summary>Sets the transaction associated with the command.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="dbTransaction">The transaction to associate with the command.</param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        public static DbCommand SetTransaction(this DbCommand dbCommand, DbTransaction dbTransaction)
        {
            dbCommand.Transaction = dbTransaction;

            return dbCommand;
        }

        /// <summary>
        /// Starts a database transaction and associates it with the <see cref="DbCommand"/> instance.
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <returns>An object representing the new transaction.</returns>
        public static DbTransaction BeginTransaction(this DbCommand dbCommand)
        {
            dbCommand.OpenConnection();

            var transaction = dbCommand.Connection.BeginTransaction();

            dbCommand.SetTransaction(transaction);

            return transaction;
        }

        /// <summary>
        /// Starts a database transaction with the specified isolation level and associates it with the <see cref="DbCommand"/> instance.
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
        /// <returns>An object representing the new transaction.</returns>
        public static DbTransaction BeginTransaction(this DbCommand dbCommand, IsolationLevel isolationLevel)
        {
            dbCommand.OpenConnection();

            var transaction = dbCommand.Connection.BeginTransaction(isolationLevel);

            dbCommand.SetTransaction(transaction);

            return transaction;
        }

        /// <summary>Opens a database connection.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        public static DbCommand OpenConnection(this DbCommand dbCommand)
        {
            if (dbCommand.Connection.State != ConnectionState.Open)
            {
                dbCommand.Connection.Open();
            }

            return dbCommand;
        }

        /// <summary>Opens a database connection asynchronously.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <returns>A <see cref="Task{TResult}"/> resulting in the given <see cref="DbCommand" /> instance.</returns>
        public static async Task<DbCommand> OpenConnectionAsync(this DbCommand dbCommand)
        {
            if (dbCommand.Connection.State != ConnectionState.Open)
            {
                await dbCommand.Connection.OpenAsync();
            }

            return dbCommand;
        }

        /// <summary>
        /// Closes and disposes the connection and the command itself.
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        public static void CloseAndDispose(this DbCommand dbCommand)
        {
            dbCommand.Connection.Close();

            dbCommand.Connection.Dispose();

            dbCommand.Dispose();
        }

        /// <summary>Gets the command text string with the parameters (?'s) replaced with their values.</summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <returns>Parameter replaced command text.</returns>
        public static string GetParameterReplacedCommandText(this DbCommand dbCommand)
        {
            var commandText = dbCommand.CommandText;

            foreach (DbParameter parameter in dbCommand.Parameters)
            {
                var replacementText = $"'{parameter.Value}' /* {parameter.ParameterName} */";

                commandText = commandText.Replace(parameter.ParameterName, replacementText);
            }

            return commandText;
        }




        /// <summary>
        /// Generates a parameterized MySQL INSERT statement from the given object and adds it to the <see cref="DbCommand" />
        /// .
        /// <para>
        /// Note that the generated query also selects the last inserted id using MySQL's SELECT LAST_INSERT_ID() function.
        /// </para>
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="obj">Object to generate the SQL INSERT statement from.</param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        public static DbCommand AppendInsertForMySql(this DbCommand dbCommand, object obj, string tableName = null)
        {
            return InsertSqlBuilder.AppendInsertForMySql(dbCommand, obj, tableName);
        }

        /// <summary>
        /// Generates a parameterized PostgreSQL INSERT statement from the given object and adds it to the <see cref="DbCommand" />
        /// .
        /// <para>
        /// Note that the generated query also selects the last inserted id using PostgreSQL's LastVal() function.
        /// </para>
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="obj">Object to generate the SQL INSERT statement from.</param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        public static DbCommand AppendInsertForPostgreSql(this DbCommand dbCommand, object obj, string tableName = null)
        {
            return InsertSqlBuilder.AppendInsertForPostgreSql(dbCommand, obj, tableName);
        }

        /// <summary>
        /// Generates a parameterized SQLite INSERT statement from the given object and adds it to the <see cref="DbCommand" />
        /// .
        /// <para>
        /// Note that the generated query also selects the last inserted id using SQLite's SELECT last_insert_rowid() function.
        /// </para>
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="obj">Object to generate the SQL INSERT statement from.</param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        // ReSharper disable once InconsistentNaming
        public static DbCommand AppendInsertForSQLite(this DbCommand dbCommand, object obj, string tableName = null)
        {
            return InsertSqlBuilder.AppendInsertForSQLite(dbCommand, obj, tableName);
        }


        /// <summary>
        /// Generates a parameterized SQL Server INSERT statement from the given object and adds it to the
        /// <see cref="DbCommand" />.
        /// <para>
        /// Note that the generated query also selects the last inserted id using SQL Server's SELECT SCOPE_IDENTITY() function.
        /// </para>
        /// </summary>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="obj">Object to generate the SQL INSERT statement from.</param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        public static DbCommand AppendInsertForSqlServer(this DbCommand dbCommand, object obj, string tableName = null)
        {
            return InsertSqlBuilder.AppendInsertForSqlServer(dbCommand, obj, tableName);
        }
        
        /// <summary>
        /// Generates a list of concatenated parameterized MySQL INSERT statements from the given list of objects and adds it to
        /// the <see cref="DbCommand" />.
        /// <para>
        /// Note that the generated query also selects the last inserted id using MySQL's SELECT LAST_INSERT_ID() function.
        /// </para>
        /// </summary>
        /// <typeparam name="T">Type of the objects in the list.</typeparam>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="objects">List of objects to generate the SQL INSERT statements from.</param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        public static DbCommand AppendInsertsForMySql<T>(this DbCommand dbCommand, IEnumerable<T> objects, string tableName = null)
        {
            foreach (var obj in objects)
            {
                dbCommand.AppendInsertForMySql(obj, tableName);
            }

            return dbCommand;
        }
        
        /// <summary>
        /// Generates a list of concatenated parameterized PostgreSQL INSERT statements from the given list of objects and adds it to
        /// the <see cref="DbCommand" />.
        /// <para>
        /// Note that the generated query also selects the last inserted id using PostgreSQL's LastVal() function.
        /// </para>
        /// </summary>
        /// <typeparam name="T">Type of the objects in the list.</typeparam>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="objects">List of objects to generate the SQL INSERT statements from.</param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        public static DbCommand AppendInsertsForPostgreSql<T>(this DbCommand dbCommand, IEnumerable<T> objects, string tableName = null)
        {
            foreach (var obj in objects)
            {
                dbCommand.AppendInsertForPostgreSql(obj, tableName);
            }

            return dbCommand;
        }
        
        /// <summary>
        /// Generates a list of concatenated parameterized SQLite INSERT statements from the given list of objects and adds it to
        /// the <see cref="DbCommand" />.
        /// <para>
        /// Note that the generated query also selects the last inserted id using SQLite's SELECT last_insert_rowid() function.
        /// </para>
        /// </summary>
        /// <typeparam name="T">Type of the objects in the list.</typeparam>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="objects">List of objects to generate the SQL INSERT statements from.</param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        // ReSharper disable once InconsistentNaming
        public static DbCommand AppendInsertsForSQLite<T>(this DbCommand dbCommand, IEnumerable<T> objects, string tableName = null)
        {
            foreach (var obj in objects)
            {
                dbCommand.AppendInsertForSQLite(obj, tableName);
            }

            return dbCommand;
        }

        /// <summary>
        /// Generates a list of concatenated parameterized SQL Server INSERT statements from the given list of objects and adds it
        /// to the <see cref="DbCommand" />.
        /// <para>
        /// Note that the generated query also selects the last inserted id using SQL Server's SELECT SCOPE_IDENTITY() function.
        /// </para>
        /// </summary>
        /// <typeparam name="T">Type of the objects in the list.</typeparam>
        /// <param name="dbCommand"><see cref="DbCommand" /> instance.</param>
        /// <param name="objects">List of objects to generate the SQL INSERT statements from.</param>
        /// <param name="tableName">
        /// Optional table name to insert into. If none is supplied, it will use the type name. Note that this parameter is
        /// required when passing in an anonymous object or an <see cref="ArgumentNullException" /> will be thrown.
        /// </param>
        /// <returns>The given <see cref="DbCommand" /> instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of 'tableName' cannot be null when the object passed is an anonymous
        /// type.
        /// </exception>
        public static DbCommand AppendInsertsForSqlServer<T>(this DbCommand dbCommand, IEnumerable<T> objects, string tableName = null)
        {
            foreach (var obj in objects)
            {
                dbCommand.AppendInsertForSqlServer(obj, tableName);
            }

            return dbCommand;
        }
    }
}