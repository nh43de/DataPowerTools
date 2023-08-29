using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.Extensions;

namespace DataPowerTools.PowerTools
{
    /// <summary>
    /// Helper methods for dealing with MSSQL databases. I think connection string-based methods should be deprecated in favor of DbConnection based static methods.
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Creates a simple connection string.
        /// </summary>
        /// <param name="initialCatalog"></param>
        /// <param name="dataSource"></param>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="integratedSecuity"></param>
        /// <returns></returns>
        public static string GetConnectionString( //mssql
            string initialCatalog = "",
            string dataSource = "",
            string userId = "",
            string password = "",
            bool integratedSecuity = true)
        {
            var sqlCnxStringBuilder = new SqlConnectionStringBuilder();

            if (!string.IsNullOrEmpty(initialCatalog))
                sqlCnxStringBuilder.InitialCatalog = initialCatalog;

            if (!string.IsNullOrEmpty(dataSource))
                sqlCnxStringBuilder.DataSource = dataSource;

            if (!string.IsNullOrEmpty(userId))
                sqlCnxStringBuilder.UserID = userId;

            if (!string.IsNullOrEmpty(password))
                sqlCnxStringBuilder.Password = password;

            // set the integrated security status
            sqlCnxStringBuilder.IntegratedSecurity = integratedSecuity;

            // now flip the properties that were changed
            return sqlCnxStringBuilder.ConnectionString;
        }

        /// <summary>
        /// Table exists?
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static async Task<bool> TableExistsAsync(this DbConnection connection, string tableName)
        {
            return await Task.Run(() =>
            {
                var schema = connection.GetSchema("Tables", new[] {null, null, tableName});
                return schema.Rows.Count > 0;
            });
        }

        /// <summary>
        /// Disables all triggers on a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connection"></param>
        public static void DisableTriggers(string tableName, SqlConnection connection)
        {
            connection.Open();
            tableName = GetTableName(tableName, connection);
            using (var s = new SqlCommand("DISABLE TRIGGER ALL ON " + tableName + ";", connection))
            {
                s.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Disables all triggers on a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionString"></param>
        public static void DisableTriggers(string tableName, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                DisableTriggers(tableName, connection);
            }
        }

        /// <summary>
        /// Enables all triggers on a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionString"></param>
        public static void EnableTriggers(string tableName, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                EnableTriggers(tableName, connection);
            }

        }        
        
        /// <summary>
        /// Enables all triggers on a table.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connection"></param>
        public static void EnableTriggers(string tableName, SqlConnection connection)
        {
            tableName = GetTableName(tableName, connection);
            using (var s = new SqlCommand("ENABLE TRIGGER ALL ON " + tableName + ";", connection))
            {
                s.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the properly formatted table name: [schema].[tablename]
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static string GetTableName(string tableName, string connectionString)
        {
            using (var sqlc = new SqlConnection(connectionString))
            {
                sqlc.Open();
                return GetTableName(tableName, sqlc);
            }
        }

        /// <summary>
        /// Gets the properly formatted table name: [schema].[tablename]
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static string GetTableName(string tableName, SqlConnection connection)
        {
            using (
                var cmd = new SqlCommand(@"
SELECT  '[' + [s].[name] + '].[' + [t].[name] + ']' tableName
FROM    [sys].[tables] [t]
        LEFT JOIN [sys].[schemas] AS [s]
            ON [s].[schema_id] = [t].[schema_id]
WHERE [t].[object_id] = OBJECT_ID(@table_name);", connection))
            {
                cmd.Parameters.Add(new SqlParameter("@table_name", tableName));

                return (string)cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Gets a list of all tables and their FK-tier heirarchy.
        /// This is for making deletion and insertion tasks easier when dealing with FK graphs.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static List<TableHierarchy> 
            GetTableHierarchy(string connectionString)
        {
            using (var sqlc = new SqlConnection(connectionString))
            {
                sqlc.Open();

                return GetTableHierarchy(sqlc);
            }
        }

        /// <summary>
        /// Gets a list of all tables and their FK-tier heirarchy.
        /// This is for making deletion and insertion tasks easier when dealing with FK graphs.
        /// The higher the level, the fewer dependencies it has. For deletion, order by Level 
        /// descending (6, 5, 4, ..) , for insertion, insert by Level Ascending (0, 1, 2, ..).
        /// </summary>
        /// <param name="sqlc"></param>
        /// <returns></returns>
        public static List<TableHierarchy> GetTableHierarchy(SqlConnection sqlc) //mssql
        {
            //TODO: modify so that it shows which tables are truncate-able (no FK references whatsoever)
            const string sql = @"DECLARE @RITable TABLE
    (
      [object_id] INT PRIMARY KEY ,
      [SchemaName] sysname NOT NULL ,
      [TableName] sysname NOT NULL ,
      [RILevel] TINYINT DEFAULT 0 ,
      [IsSelfReferencing] TINYINT DEFAULT 0 ,
      [HasExcludedRelationship] TINYINT DEFAULT 0 ,
      [UpdateCount] TINYINT DEFAULT 0
    );

INSERT  @RITable
        ( [object_id] ,
          [SchemaName] ,
          [TableName] ,
          [RILevel] ,
          [IsSelfReferencing] ,
          [HasExcludedRelationship] ,
          [UpdateCount]
        )
SELECT  [tables].[object_id] ,
        [schemas].[name] ,
        [tables].[name] ,
        0 ,
        SUM(CASE WHEN [foreign_keys].[parent_object_id] IS NULL THEN 0
                 ELSE 1
            END) ,
        SUM(CASE WHEN [foreign_keys02].[referenced_object_id] IS NULL THEN 0
                 ELSE 1
            END) ,
        0
FROM    [sys].[tables] [tables]
        JOIN [sys].[schemas] [schemas]
            ON [tables].[schema_id] = [schemas].[schema_id]
        LEFT JOIN [sys].[foreign_keys] [foreign_keys]
            ON [tables].[object_id] = [foreign_keys].[parent_object_id]
               AND [tables].[object_id] = [foreign_keys].[referenced_object_id]
        LEFT JOIN [sys].[foreign_keys] [foreign_keys01]
            ON [tables].[object_id] = [foreign_keys01].[parent_object_id]
        LEFT JOIN [sys].[foreign_keys] [foreign_keys02]
            ON [foreign_keys01].[parent_object_id] = [foreign_keys02].[referenced_object_id]
               AND [foreign_keys01].[referenced_object_id] = [foreign_keys02].[parent_object_id]
               AND [foreign_keys01].[parent_object_id] <> [foreign_keys01].[referenced_object_id]
WHERE   [tables].[name] NOT IN ( 'sysdiagrams', 'dtproperties' )
GROUP BY [tables].[object_id] ,
        [schemas].[name] ,
        [tables].[name];

DECLARE @LookLevel INT;

DECLARE @MyRowcount INT;

SELECT  @LookLevel = 0 ,
        @MyRowcount = -1;  

WHILE ( @MyRowcount <> 0 )
BEGIN
    UPDATE  [ChildTable]
    SET     [ChildTable].[RILevel] = @LookLevel + 1 ,
            [ChildTable].[UpdateCount] = [ChildTable].[UpdateCount] + 1
    FROM    @RITable [ChildTable]
            JOIN [sys].[foreign_keys] [foreign_keys]
                ON [ChildTable].[object_id] = [foreign_keys].[parent_object_id]
            JOIN @RITable [ParentTable]
                ON [foreign_keys].[referenced_object_id] = [ParentTable].[object_id]
                   AND [ParentTable].[RILevel] = @LookLevel
            LEFT JOIN [sys].[foreign_keys] [foreign_keysEX]
                ON [foreign_keys].[parent_object_id] = [foreign_keysEX].[referenced_object_id]
                   AND [foreign_keys].[referenced_object_id] = [foreign_keysEX].[parent_object_id]
                   AND [foreign_keys].[parent_object_id] <> [foreign_keys].[referenced_object_id]
    WHERE   [ChildTable].[object_id] <> [ParentTable].[object_id]
            AND [foreign_keysEX].[referenced_object_id] IS NULL;

    SELECT  @MyRowcount = @@ROWCOUNT;

    SELECT  @LookLevel = @LookLevel + 1;

END;  

SELECT  [RITable].[SchemaName] [SchemaName] ,
        [RITable].[TableName] [TableName] ,
        [RITable].[RILevel] [Level] ,
        CASE WHEN [RITable].[IsSelfReferencing] > 0 THEN CAST(1 AS BIT)
             ELSE CAST(0 AS BIT)
        END [IsSelfReferencing] ,
        CASE WHEN [RITable].[HasExcludedRelationship] > 0 THEN CAST(1 AS BIT)
             ELSE CAST(0 AS BIT)
        END [HasExcludedRelationship]
FROM    @RITable [RITable]
ORDER BY [RITable].[RILevel] ASC ,
        [RITable].[TableName] ASC;

-- Excluded relationships
/*
SELECT  [foreign_keys01].[name] [ForeignKeyName] ,
        [ParentSchema].[name] [ParentSchema] ,
        [ParentObject].[name] [ParentTable] ,
        [ChildSchema].[name] [ChildSchema] ,
        [ChildObject].[name] [ChildTable]
FROM    [sys].[foreign_keys] [foreign_keys01]
        JOIN [sys].[foreign_keys] [foreign_keys02]
            ON [foreign_keys01].[parent_object_id] = [foreign_keys02].[referenced_object_id]
               AND [foreign_keys01].[referenced_object_id] = [foreign_keys02].[parent_object_id]
               AND [foreign_keys01].[parent_object_id] <> [foreign_keys01].[referenced_object_id]
        JOIN [sys].[objects] [ParentObject]
            ON [foreign_keys01].[parent_object_id] = [ParentObject].[object_id]
        JOIN [sys].[schemas] [ParentSchema]
            ON [ParentObject].[schema_id] = [ParentSchema].[schema_id]
        JOIN [sys].[objects] [ChildObject]
            ON [foreign_keys01].[referenced_object_id] = [ChildObject].[object_id]
        JOIN [sys].[schemas] [ChildSchema]
            ON [ChildObject].[schema_id] = [ChildSchema].[schema_id]
*/";

            using (var cmd = new SqlCommand(sql, sqlc))
            using (var a = cmd.ExecuteReader())
            {
                try
                {
                    var b = a.SelectStrict<TableHierarchy>().ToList();
                    return b;
                }
                catch (Exception e)
                {
                    throw new Exception("Get table heirarchy failed.", e);
                }
                finally
                {
                    cmd.Dispose();
                    a.Dispose();
                }
            }
        }
        
        /// <summary>
        /// Validates that a connection string can connect and execute "SELECT 1" on the target.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static bool ValidateConnection(string connectionString)
        {
            try
            {
                connectionString.ExecuteSql("SELECT 1");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Executes SQL to a DataTable. DUPLICATED CODE.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static DataTable ExecuteToDataTable(string sql, SqlConnection connection)
        {
            try
            {
                var dataSet = new DataSet();

                using (var dataAdapter = new SqlDataAdapter(sql, connection))
                {
                    dataAdapter.Fill(dataSet);
                }

                return dataSet.Tables[0];
            }
            catch (Exception exception)
            {
                throw new Exception("Error executing to dataset: " + exception.Message);
            }
        }

        /// <summary>
        /// Gets a list of tables in the [schema].[table] format. //TODO: make this return a struct { tablename, schemaname }
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static List<string> GetTableList(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                return GetTableList(connection);
            }
        }

        /// <summary>
        /// Gets a list of tables in the [schema].[table] format. //TODO: make this return a struct { tablename, schemaname }
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static List<string> GetTableList(SqlConnection connection) //mssql
        {
            var tables = new List<string>();

            using (var cmd = new SqlCommand(@"
SELECT  '[' + [s].[name] + '].[' + [t].[name] + ']' tableName
FROM    [sys].[tables] [t]
        LEFT JOIN [sys].[schemas] AS [s]
            ON [s].[schema_id] = [t].[schema_id]", connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        tables.Add(reader.GetString(0));
                }
            }
            return tables;
        }
        
        public static void TruncateTable(string tableName, SqlConnection sqlc)
        {
            tableName = GetTableName(tableName, sqlc);
            using (var s = new SqlCommand($"TRUNCATE TABLE {tableName};", sqlc))
            {
                s.ExecuteNonQuery();
            }
        }

        public static void TruncateTable(string tableName, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                TruncateTable(tableName, connection);
            }
        }

        public static async Task DeleteFromTableAsync(string tableName, SqlConnection sqlc, CancellationToken token = new CancellationToken())
        {
            tableName = GetTableName(tableName, sqlc);
            using (var s = new SqlCommand($"DELETE FROM {tableName};", sqlc))
            {
                await s.ExecuteNonQueryAsync(token);
            }
        }
        
        public static void DeleteFromTable(string tableName, SqlConnection sqlc)
        {
            tableName = GetTableName(tableName, sqlc);
            using (var s = new SqlCommand($"DELETE FROM {tableName};", sqlc))
            {
                s.ExecuteNonQuery();
            }
        }

        public static void DeleteFromTable(string tableName, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                DeleteFromTable(tableName, connection);
            }
        }
        
        public static bool TableExists(string connectionString, string tableName)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return TableExists(connection, tableName);
            }
        }
        
        public static void DropTable(SqlConnection connection, string tableName)
        {
            tableName = GetTableName(tableName, connection);
            using (var cmdDrop = new SqlCommand("DROP TABLE " + tableName, connection))
            {
                cmdDrop.ExecuteNonQuery();
            }
        }

        public static void DropTable(string connectionString, string tableName)
        {
            using (var sqlc = new SqlConnection(connectionString))
            {
                DropTable(sqlc, tableName);
            }
        }

        public static async Task<string[]> GetDropSchemaObjectsSql(SqlConnection connection, string schemaName, bool dropSchema = true)
        {
            var sql = @"/*-----------------------------------------------------------------------------------------
  NH - based on the following, but modified.

  Original Author : Ranjith Kumar S
  Date:    31/01/10
 
  Description: It drop all the objects in a schema and then the schema itself
 
  Limitations:
 
    1. If a table has a PK with XML or a Spatial Index then it wont work
       (workaround: drop that table manually and re run it)
    2. If the schema is referred by a XML Schema collection then it wont work
 
If it is helpful, Please send your comments ranjith_842@hotmail.com or visit http://www.ranjithk.com

-------------------------------------------------------------------------------------------*/

SET NOCOUNT ON;
 
DECLARE @SQL VARCHAR(5000);

IF OBJECT_ID('tempdb..#dropcode') IS NOT NULL
    DROP TABLE [#dropcode];

CREATE TABLE [#dropcode]
    (
      [ID] INT IDENTITY(1, 1) ,
      [SQLstatement] VARCHAR(5000)
    );
 
-- removes all the foreign keys that reference a PK in the target schema
SET @SQL = 'SELECT  ''ALTER TABLE '' + SCHEMA_NAME([fk].[schema_id]) + ''.''
        + OBJECT_NAME([fk].[parent_object_id]) + '' DROP CONSTRAINT '' + [fk].[name]
FROM    [sys].[foreign_keys] [fk]
        JOIN [sys].[tables] [t]
            ON [t].[object_id] = [fk].[referenced_object_id]
WHERE   [t].[schema_id] = SCHEMA_ID(''' + @SchemaName + ''')
        AND [fk].[schema_id] <> [t].[schema_id]
ORDER BY [fk].[name] DESC;';
 
INSERT  INTO [#dropcode]
        EXEC ( @SQL
            );
 
 -- drop all default constraints, check constraints and Foreign Keys
SET @SQL = 'SELECT  ''ALTER TABLE '' + SCHEMA_NAME([t].[schema_id]) + ''.''
        + OBJECT_NAME([fk].[parent_object_id]) + '' DROP CONSTRAINT '' + [fk].[name]
FROM    [sys].[objects] [fk]
        JOIN [sys].[tables] [t]
            ON [t].[object_id] = [fk].[parent_object_id]
WHERE   [t].[schema_id] = SCHEMA_ID(''' + @SchemaName + ''')
        AND [fk].[type] IN ( ''D'', ''C'', ''F'' );';

INSERT  INTO [#dropcode]
        EXEC ( @SQL
            );
 
 -- drop all other objects in order  
SELECT  @SQL = 'SELECT CASE WHEN [SO].[type] = ''PK''
             THEN ''ALTER TABLE ['' + SCHEMA_NAME([SO].[schema_id]) + ''].[''
                  + OBJECT_NAME([SO].[parent_object_id]) + ''] DROP CONSTRAINT [''
                  + [SO].[name] + '']''
             WHEN [SO].[type] = ''U''
             THEN ''DROP TABLE ['' + SCHEMA_NAME([SO].[schema_id]) + ''].['' + [SO].[name] + '']''
             WHEN [SO].[type] = ''V''
             THEN ''DROP VIEW ['' + SCHEMA_NAME([SO].[schema_id]) + ''].['' + [SO].[name] + '']''
             WHEN [SO].[type] = ''P''
             THEN ''DROP PROCEDURE ['' + SCHEMA_NAME([SO].[schema_id]) + ''].[''
                  + [SO].[name] + '']''
             WHEN [SO].[type] = ''TR''
             THEN ''DROP TRIGGER ['' + SCHEMA_NAME([SO].[schema_id]) + ''].[''
                  + [SO].[name] + '']''
             WHEN [SO].[type] = ''SN''
             THEN ''DROP SYNONYM ['' + SCHEMA_NAME([SO].[schema_id]) + ''].[''
                  + [SO].[name] + '']''
             WHEN [SO].[type] = ''SO''
             THEN ''DROP SEQUENCE ['' + SCHEMA_NAME([SO].[schema_id]) + ''].[''
                  + [SO].[name] + '']''
             WHEN [SO].[type] IN ( ''FN'', ''TF'', ''IF'', ''FS'', ''FT'' )
             THEN ''DROP FUNCTION ['' + SCHEMA_NAME([SO].[schema_id]) + ''].[''
                  + [SO].[name] + '']''
        END
FROM    [sys].[objects] [SO]
WHERE   [SO].[schema_id] = SCHEMA_ID(''' + @SchemaName
        + ''')
        AND [SO].[type] IN ( ''PK'', ''FN'', ''TF'', ''TR'', ''V'', ''U'', ''P'', ''SN'', ''IF'',
                         ''SO'' )
ORDER BY CASE WHEN [SO].[type] = ''PK'' THEN 1
              WHEN [SO].[type] IN ( ''FN'', ''TF'', ''P'', ''IF'', ''FS'', ''FT'' ) THEN 2
              WHEN [SO].[type] = ''TR'' THEN 3
              WHEN [SO].[type] = ''V'' THEN 4
              WHEN [SO].[type] = ''U'' THEN 5
              ELSE 6
         END;';

INSERT  INTO [#dropcode]
        EXEC ( @SQL
            );

IF @DropSchema = 1 AND EXISTS ( SELECT * FROM sys.[schemas] AS [s] WHERE [s].[name] = '' + @SchemaName + '' )
BEGIN
    INSERT  INTO [#dropcode]
            ( [SQLstatement]
            )
    VALUES  ( 'DROP SCHEMA [' + @SchemaName + ']'
            );
END;

SELECT  [SQLstatement]
FROM    [#dropcode] AS [d];";

            string[] rtn;

            using (var cmd = connection.CreateSqlCommand(sql))
            {
                cmd.Parameters.Add(new SqlParameter("@SchemaName", schemaName));
                cmd.Parameters.Add(new SqlParameter("@DropSchema", dropSchema));

                using (var cmdR = await cmd.ExecuteReaderAsync())
                {
                    rtn = cmdR.ToArray<string>();
                }
            }

            return rtn;
        }

        /// <summary>
        /// Returns true if table exists, false if otherwise.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool TableExists(SqlConnection connection, string tableName)
        {
            //SqlCommand cmd = new SqlCommand("SELECT * FROM sys.Tables where name = @table_name", connection);
            //cmd.Parameters.Add(new SqlParameter("@table_name", destinationTableName));

            DataTable a;

            try
            {
                using (
                    var cmd = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@table_name) IS NULL THEN 0 ELSE 1 END col1",
                        connection))
                {
                    cmd.Parameters.Add(new SqlParameter("@table_name", tableName));

                    using (var b = cmd.ExecuteReader())
                    {
                        a = b.ToDataTable(1);
                    }
                }
            }
            catch
            {
                return false;
            }
            return a.Rows[0]["col1"].ToString() == "1";
        }

        /// <summary>
        /// Determines if the specified object is null or is a DBNull value. Note that there is a difference between null and DbNull by default.
        /// </summary>
        /// <param name="rec">Object to test</param>
        /// <returns>True if the object is NULL or is a DBNull value</returns>
        public static bool IsDbNull(object rec)
        {
            return (rec == null) || (rec == DBNull.Value) || Convert.IsDBNull(rec);
        }


        /// <summary>
        /// Gets a blank table that is configured as the specified table. It uses SqlDataAdapter.FillSchema to accomplish this.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static DataTable GetDataSchema(string tableName, DbConnection connection)
        {
            //TODO: this is duplicated in MsSqlDatabaseConnection and PowerTools.database
            var noncomputedColumns = (connection is SqlConnection)
                ? GetNonComputedColumns(tableName, (SqlConnection) connection)
                : GetTableColumns(tableName, connection);

            var cols = "[" + string.Join("],[", noncomputedColumns) + "]";

            try
            {
                using (var sc = connection.CreateSqlCommand(@"SELECT " + cols + " from " + tableName))
                {
                    //var sa = new SqlDataAdapter(sc);
                    //var dt = new DataTable();
                    
                    //sa.FillSchema(dt, SchemaType.Source);

                    var r = sc.ExecuteToDataTable(0);
                    
                    return r;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Could not get data schema for table [{tableName}]. Column names: '{cols}': {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a blank table that is configured as the specified table. It uses SqlDataAdapter.FillSchema to accomplish this.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static DataTable GetDataSchema(string tableName, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                return GetDataSchema(tableName, connection);
            }
        }

        /// <summary>
        /// Gets the columns of a table that are not computed columns.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static string[] GetNonComputedColumns(string tableName, string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                return GetNonComputedColumns(tableName, connection);
            }
        }

        static bool IsTempTableName(string tableName)
        { 
            // Regex pattern to match temporary table names starting with '#'
            var pattern = @"^\[?#(\w+)";

            // Create a regex object and match the input tableName
            var regex = new Regex(pattern);
            var match = regex.Match(tableName);

            // Return true if the input matches the pattern, indicating it's a temp table name
            return match.Success;
        }

        /// <summary>
        /// Gets the columns of a table that are not computed columns.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static string[] GetNonComputedColumns(string tableName, SqlConnection connection)
        {
            //TODO: this is duplicated in MsSqlDatabaseConnection and sqlocity.database
            var dt = new DataTable();

            var isTempTable = IsTempTableName(tableName);

            var table = isTempTable ? "tempdb.." + tableName : tableName;

            var tempDbQualifier = isTempTable ? "tempdb." : "";

            try
            {
                using (var sc = new SqlCommand(@$"select name, column_id from {tempDbQualifier}sys.all_columns a
                                                    where a.object_id = OBJECT_ID(@table_name)
                                                    except
                                                    select name, column_id from {tempDbQualifier}sys.computed_columns a
                                                    where a.object_id = OBJECT_ID(@table_name)
                                                    order by column_id", connection))
                {
                    sc.Parameters.Add(new SqlParameter("@table_name", table));

                    using (var sa = new SqlDataAdapter(sc))
                    {
                        sa.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Could not get list of computed columns for table [{tableName}]: {ex.Message}");
            }

            return dt.Rows.Cast<DataRow>()
                .Select(row => row["name"].ToString())
                .Where(colname => !string.IsNullOrWhiteSpace(colname)).ToArray();
        }

        /// <summary>
        /// Gets the columns of a table that are not computed columns.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static string[] GetTableColumns(string tableName, DbConnection connection)
        {
            if (connection.GetType().Name.ToLower().Contains("sqlite")) //TODO: crude, but will need some significant changes to be better at handling different connection types
            {
                try
                {
                    using (var cmd = connection.CreateSqlCommand(@"select * from pragma_table_info(@table_name)"))
                    {
                        var p = cmd.CreateParameter();

                        p.ParameterName = "@table_name";
                        p.Value = tableName;

                        cmd.Parameters.Add(p);
                        
                        var r = cmd.ExecuteReader().SelectRows(dr => dr["name"]?.ToString()).ToArray();
                        
                        return r;
                    }
                }
                catch (Exception e)
                {
                    //didn't work. that's fine
                }
            }

            try
            {
                var s = connection.GetSchema("Tables", new[] { tableName });
                
                var r = s
                    .Rows.OfType<DataRow>().Select(row => row["COLUMN_NAME"].ToString()).ToArray();

                return r;
            }
            catch (Exception e)
            {
                //didn't work. that's fine
            }

            //can also materialize schema by filling select top 0 to a datatable

            try
            {
                using (var sc = connection.CreateSqlCommand(@"
                    SELECT  [c].[COLUMN_NAME]
                    FROM    [INFORMATION_SCHEMA].[COLUMNS] AS [c]
                    WHERE   [c].[TABLE_NAME] = @table_name"))
                {
                    var p = sc.CreateParameter();

                    p.ParameterName = "@table_name";
                    p.Value = tableName;
                        
                    sc.Parameters.Add(p);
                    
                    return sc.ExecuteArray<string>()
                        .Where(colname => !string.IsNullOrWhiteSpace(colname)).ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Could not get list of computed columns for table [{tableName}]: {ex.Message}");
            }
        }


        /// <summary>
        /// Gets the data schema columns of the specified database table.
        /// </summary>
        /// <param name="destinationTableName"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static IEnumerable<TypedDataColumnInfo> GetDataSchemaTypedColumnInfo(string destinationTableName, DbConnection connection)
        {
            var destinationColumnsQ = GetDataSchemaColumns(destinationTableName, connection).ToArray();
            
            var destinationColumns = destinationColumnsQ
                .Select(c => new TypedDataColumnInfo
                {
                    ColumnName = c.ColumnName,
                    DataType = c.DataType,
                    Ordinal = c.Ordinal
                }).ToArray();

            return destinationColumns;
        }
        
        /// <summary>
        /// Gets the data schema columns of the specified database table.
        /// </summary>
        /// <param name="destinationTableName"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static IEnumerable<DataColumn> GetDataSchemaColumns(string destinationTableName, DbConnection connection)
        {
            return GetDataSchema(destinationTableName, connection).GetDataColumns();
        }

        /// <summary>
        /// Gets the data schema columns of the specified database table.
        /// </summary>
        /// <param name="destinationTableName"></param>
        /// <param name="connString"></param>
        /// <returns></returns>
        public static IEnumerable<DataColumn> GetDataSchemaColumns(string destinationTableName, string connString)
        {
            return GetDataSchema(destinationTableName, connString).GetDataColumns();
        }

        //todo port to .net stardard
//        /// <summary>
//        /// Gets schema field definitions containing additional data. May be obsolete, in favor of GetDataSchema() methods.
//        /// </summary>
//        /// <param name="connection"></param>
//        /// <param name="tableName"></param>
//        /// <param name="ignoreFields"></param>
//        /// <returns></returns>
//        public static async Task<List<DbColumnInfo>> GetSchemaFieldDefs(DbConnection connection,
//            string tableName, params string[] ignoreFields)
//        {
//            DataColumn d;
//            
//            return await Task.Run(() =>
//            {
//                var schema = connection.GetSchema("Columns", new[] {null, null, tableName});
//                return (from row in schema.AsEnumerable()
//                    let fieldName = row.Field<string>("COLUMN_NAME")
//                    let size = row.Field<int?>("CHARACTER_MAXIMUM_LENGTH")
//                    let dataType = DbColumnInfo.StringToDataType(row.Field<string>("DATA_TYPE"))
//                    where !ignoreFields.Contains(fieldName)
//                    select new DbColumnInfo {FieldName = fieldName, Size = size ?? 0, DataType = dataType}).ToList();
//            });
//        }
    }
}