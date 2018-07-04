
//from http://codegists.com/search/csharp-idisposable/185
//from https://github.com/aspnet/Microsoft.Data.Sqlite/issues/289

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Sqlite.Extensions
{
    public static class SqlLiteHelpers
    {
        /// <summary>
        /// Get column metadata for a table in a Sqlite database
        /// </summary>
        public static Dictionary<string, SqlBasicColumnTypes> GetColumnMetaData(DbConnection dbConnection, string destinationTableName)
        {
            if (string.IsNullOrEmpty(destinationTableName))
                throw new ArgumentNullException(nameof(destinationTableName));

            var ColumnNameToTypeDictionary = new Dictionary<string, SqlBasicColumnTypes>();

            var sql = $"pragma table_info('{destinationTableName}')";

            var cmd = dbConnection.CreateCommand();

            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            using (cmd)
            {
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var key = dr["name"].ToString();

                        // data types found @ http://www.tutorialspoint.com/sqlite/sqlite_data_types.htm
                        var columnType = dr["type"].ToString().ToUpper();

                        var typ = SqlLiteHelpers.MapSqliteColumnType(columnType);

                        ColumnNameToTypeDictionary.Add(key, typ);
                    }
                }
            }

            if (ColumnNameToTypeDictionary == null || ColumnNameToTypeDictionary.Count < 1)
                throw new Exception($"{destinationTableName} could not be found in the database");

            return ColumnNameToTypeDictionary;
        }


        public static SqlBasicColumnTypes MapSqliteColumnType(string sqliteColumnType)
        {
            var typ = SqlBasicColumnTypes.Text;

            switch (sqliteColumnType)
            {
                case "INTEGER":
                case "TINYINT":
                case "SMALLINT":
                case "MEDIUMINT":
                case "BIGINT":
                case "UNSIGNED BIG INT":
                case "INT2":
                case "INT8":
                case "INT":
                    typ = SqlBasicColumnTypes.Integer;
                    break;
                case "CLOB":
                case "TEXT":
                    typ = SqlBasicColumnTypes.Text;
                    break;
                case "BLOB":
                    typ = SqlBasicColumnTypes.Blob;
                    break;
                case "REAL":
                case "DOUBLE":
                case "DOUBLE PRECISION":
                case "FLOAT":
                    typ = SqlBasicColumnTypes.Real;
                    break;
                case "NUMERIC":
                    typ = SqlBasicColumnTypes.Numeric;
                    break;
                case "BOOLEAN":
                    typ = SqlBasicColumnTypes.Boolean;
                    break;
                case "DATE":
                case "DATETIME":
                    typ = SqlBasicColumnTypes.Date;
                    break;
                default: // look for fringe cases that need logic
                    if (sqliteColumnType.StartsWith("CHARACTER"))
                        typ = SqlBasicColumnTypes.Text;
                    if (sqliteColumnType.StartsWith("VARCHAR"))
                        typ = SqlBasicColumnTypes.Text;
                    if (sqliteColumnType.StartsWith("VARYING CHARACTER"))
                        typ = SqlBasicColumnTypes.Text;
                    if (sqliteColumnType.StartsWith("NCHAR"))
                        typ = SqlBasicColumnTypes.Text;
                    if (sqliteColumnType.StartsWith("NATIVE CHARACTER"))
                        typ = SqlBasicColumnTypes.Text;
                    if (sqliteColumnType.StartsWith("NVARCHAR"))
                        typ = SqlBasicColumnTypes.Text;
                    if (sqliteColumnType.StartsWith("NVARCHAR"))
                        typ = SqlBasicColumnTypes.Text;
                    if (sqliteColumnType.StartsWith("DECIMAL"))
                        typ = SqlBasicColumnTypes.Numeric;
                    break;
            }

            return typ;
        }
    }
    
    /// <summary>
    /// Defines the mapping between a column in a SqliteBulkCopy instance's data source and a column in the instance's destination table.
    /// </summary>
    public enum SqlBasicColumnTypes
    {
        //
        // Summary:
        //     A signed integer.
        Integer = 1,
        //
        // Summary:
        //     A floating point value.
        Real = 2,
        //
        // Summary:
        //     A text string.
        Text = 3,
        //
        // Summary:
        //     A blob of data.
        Blob = 4,
        // Summary:
        //     A date column
        Date = 5,
        // Summary:
        //     A numeric column
        Numeric = 6,
        // Summary
        //     A boolean column
        Boolean = 7
    }

    /// <summary>
    /// Lets you efficiently bulk load a Sqlite table with data from another source.
    /// </summary>
    public class GenericBulkCopy : IDisposable
    {
        private readonly DbConnection _connection;
        
        public Dictionary<string, SqlBasicColumnTypes> ColumnNameToTypeDictionary { get; private set; }

        /// <summary>
        /// Name of the destination table in the database.
        /// </summary>
        public string DestinationTableName { get; set; }
        /// <summary>
        /// Allows control of the incoming DataReader by closing and disposing of it by default after all bulk copy 
        /// operations have completed if set to TRUE, if set to FALSE you need to do your own cleanup (this is useful 
        /// when your DataReader returns more than one result set).  By default this is set to TRUE.
        /// </summary>
        public bool CloseAndDisploseDataReader { get; set; } = true;

        /// <summary>
        /// Batch size data is chunked into when inserting.  The default size is 1,000 records per batch.
        /// </summary>
        public int BatchSize { get; set; } = 1000;

        /// <summary>
        /// The time in seconds to wait for a batch to load. The default is 30 seconds.
        /// </summary>
        public int BulkCopyTimeout { get; set; } = 30;
        

        /// <summary>
        /// Initializes a new instance of the SqliteBulkCopy class using the specified open instance of SqliteConnection.
        /// </summary>
        /// <param name="connection"></param>
        public GenericBulkCopy(DbConnection connection)
        {
            _connection = connection;
        }
        
        /// <summary>
        /// Copies all rows in the supplied IDataReader to a destination table specified 
        /// by the destinationTableName property of the SqliteBulkCopy object.
        /// </summary>
        /// <param name="reader"></param>
        public void WriteToServer(IDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            
            if (_connection.State == ConnectionState.Closed)
                _connection.Open();

            // build the insert schema
            var insertClause = new StringBuilder();
            insertClause.Append($"INSERT INTO {DestinationTableName} (");

            { 
                var first = true;
                foreach (var c in ColumnNameToTypeDictionary)
                {
                    if (first)
                    {
                        insertClause.Append(c.Key);
                        first = false;
                    }
                    else
                        insertClause.Append("," + c.Key);
                }

                insertClause.Append(")");
                first = true;
            }

            var valuesClause = new StringBuilder();
            var currentBatch = 0;
            while (reader.Read())
            {
                // generate insert values block statement
                if (currentBatch > 0)
                    valuesClause.Append(",");
                valuesClause.Append("(");
                var colFirst = true;
                foreach (var c in ColumnNameToTypeDictionary)
                {
                    if (!colFirst)
                        valuesClause.Append(",");
                    else
                        colFirst = false;
                    var columnValue = reader[c.Key].ToString();
                    if (string.IsNullOrEmpty(columnValue))
                        valuesClause.Append("NULL");
                    else
                    {
                        switch (c.Value)
                        {
                            case SqlBasicColumnTypes.Date:
                                try
                                {
                                    valuesClause.Append($"'{ DateTime.Parse(columnValue) :yyyy-MM-dd HH:mm:ss}'");
                                }
                                catch (Exception exp)
                                {
                                    throw new Exception($"Invalid Cast when loading date column [{ c.Key }] in table [{ DestinationTableName}] in Sqlite DB with data; value being casted '{ columnValue}', incoming values must be of data format consumable by .NET; error:\n {exp.Message}");
                                }
                                break;
                            case SqlBasicColumnTypes.Integer:
                            case SqlBasicColumnTypes.Numeric:
                            case SqlBasicColumnTypes.Real:
                                valuesClause.Append(columnValue);
                                break;
                            case SqlBasicColumnTypes.Boolean:
                                if (columnValue.ToUpper() == "TRUE")
                                    valuesClause.Append("1");
                                else if (columnValue.ToUpper() == "FALSE")
                                    valuesClause.Append("0");
                                else if (int.TryParse(columnValue, out var outValue))
                                {
                                    if (outValue == 1 || outValue == 0)
                                        valuesClause.Append($"{columnValue}");
                                    else // numeric value out of range, throw exception
                                        throw new Exception($"Invalid Cast when loading boolean column [{ c.Key }] in table [{ DestinationTableName}] in Sqlite DB with data; value being casted '{ columnValue}', incoming values can only be True or False (case does not matter), 1 (true) or 0 (false), or NULL");
                                }
                                else // no valid boolean types throw exception
                                    throw new Exception($"Invalid Cast when loading boolean column [{ c.Key }] in table [{ DestinationTableName}] in Sqlite DB with data; value being casted '{ columnValue}', incoming values can only be True or False (case does not matter), 1 (true) or 0 (false), or NULL");
                                break;
                            case SqlBasicColumnTypes.Text:
                            default:
                                valuesClause.Append($"'{columnValue.Replace("'", "''")}'");
                                break;
                        }
                    }
                }
                valuesClause.Append(")");

                currentBatch++;

                if (currentBatch != BatchSize)
                    continue;
                
                var dml = $"BEGIN;\n{insertClause} VALUES {valuesClause};\nCOMMIT;";
                valuesClause.Clear();


                var cmd = GetDbCommand(dml);

                using (cmd)
                    cmd.ExecuteNonQuery();

                currentBatch = 0;
            }
            if (CloseAndDisploseDataReader)
            {
                reader.Close();
                reader.Dispose();
            }

            // if any records remain after the read loop has completed then write them to the DB
            if (currentBatch > 0)
            {
                var dml = $"BEGIN;\n{insertClause} VALUES {valuesClause};\nCOMMIT;";

                var cmd = GetDbCommand(dml);

                using (cmd)
                    cmd.ExecuteNonQuery();
            }
        }



        /// <summary>
        /// Releases all resources used by the current instance of the SqliteBulkCopy class.
        /// </summary>
        public void Dispose()
        {
            Close();
            _connection.Dispose();
        }


        /// <summary>
        /// Close and database connections.
        /// </summary>
        public void Close()
        {
            if (_connection.State == ConnectionState.Open)
                _connection.Close();
        }

        private DbCommand GetDbCommand(string dml)
        {
            var cmd = _connection.CreateCommand();

            cmd.CommandTimeout = BulkCopyTimeout;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = dml;

            return cmd;
        }

    }
}