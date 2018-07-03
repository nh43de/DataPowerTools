
//from http://codegists.com/search/csharp-idisposable/185
//from https://github.com/aspnet/Microsoft.Data.Sqlite/issues/289

//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Text;
//using System.Threading.Tasks;

//namespace Sqlite.Extensions
//{
//    /// <summary>
//    /// Lets you efficiently bulk load a Sqlite table with data from another source.
//    /// </summary>
//    public class SqliteBulkCopy : IDisposable
//    {
//        #region "Field(s)"
//        private bool _defaultCloseAndDisploseDataReader = true;
//        private int _defaultBatchSize = 1000;
//        private int _defaultCopyTimeout = 30; // seconds
//        private SqliteConnection _cn;
//        private string _tableNm;
//        private Dictionary<string, SqliteColumnTypesExtended> _columns = null;

//        public Dictionary<string, SqliteColumnTypesExtended> ColumnMappings
//        {
//            get
//            {
//                return _columns;
//            }
//        }

//        #endregion

//        #region "Properties"
//        /// <summary>
//        /// Name of the destination table in the database.
//        /// </summary>
//        public string DestinationTableName
//        {
//            get
//            {
//                return _tableNm;
//            }
//            set
//            {
//                _tableNm = value;
//                if (!string.IsNullOrEmpty(value))
//                    SetColumnMetaData();
//            }
//        }
//        /// <summary>
//        /// Allows control of the incoming DataReader by closing and disposing of it by default after all bulk copy 
//        /// operations have completed if set to TRUE, if set to FALSE you need to do your own cleanup (this is useful 
//        /// when your DataReader returns more than one result set).  By default this is set to TRUE.
//        /// </summary>
//        public bool CloseAndDisploseDataReader { get; set; }
//        /// <summary>
//        /// Batch size data is chunked into when inserting.  The default size is 1,000 records per batch.
//        /// </summary>
//        public int BatchSize { get; set; }
//        /// <summary>
//        /// The time in seconds to wait for a batch to load. The default is 30 seconds.
//        /// </summary>
//        public int BulkCopyTimeout { get; set; }
//        #endregion

//        #region "Constructors"
//        /// <summary>
//        /// Initializes a new instance of the SqliteBulkCopy class using the specified open instance of SqliteConnection.
//        /// </summary>
//        /// <param name="connection"></param>
//        public SqliteBulkCopy(SqliteConnection connection)
//        {
//            this.CloseAndDisploseDataReader = _defaultCloseAndDisploseDataReader;
//            this.BatchSize = _defaultBatchSize;
//            this.BulkCopyTimeout = _defaultCopyTimeout;
//            var copy = new System.Data.SqlClient.SqlBulkCopy("");
//            _cn = connection;
//            if (_cn.State == ConnectionState.Closed)
//                _cn.Open();
//        }

//        /// <summary>
//        /// Initializes and opens a new instance of SqliteBulkCopy based on the supplied connectionString. 
//        /// The constructor uses the SqliteConnection to initialize a new instance of the SqliteBulkCopy class.
//        /// </summary>
//        /// <param name="connectionString"></param>
//        public SqliteBulkCopy(string connectionString)
//        {
//            this.CloseAndDisploseDataReader = _defaultCloseAndDisploseDataReader;
//            this.BatchSize = _defaultBatchSize;
//            this.BulkCopyTimeout = _defaultCopyTimeout;
//            _cn = new SqliteConnection(connectionString);
//            if (_cn.State == ConnectionState.Closed)
//                _cn.Open();
//        }
//        #endregion

//        #region "Method(s) and Enum(s)"
//        /// <summary>
//        /// Defines the mapping between a column in a SqliteBulkCopy instance's data source and a column in the instance's destination table.
//        /// </summary>
//        public enum SqliteColumnTypesExtended
//        {
//            //
//            // Summary:
//            //     A signed integer.
//            Integer = 1,
//            //
//            // Summary:
//            //     A floating point value.
//            Real = 2,
//            //
//            // Summary:
//            //     A text string.
//            Text = 3,
//            //
//            // Summary:
//            //     A blob of data.
//            Blob = 4,
//            // Summary:
//            //     A date column
//            Date = 5,
//            // Summary:
//            //     A numeric column
//            Numeric = 6,
//            // Summary
//            //     A boolean column
//            Boolean = 7
//        }

//        /// <summary>
//        /// Get column metadata for a table in a Sqlite database
//        /// </summary>
//        private void SetColumnMetaData()
//        {
//            if (!string.IsNullOrEmpty(this.DestinationTableName))
//            {
//                _columns = new Dictionary<string, SqliteColumnTypesExtended>();
//                var sql = $"pragma table_info('{this.DestinationTableName}')";
//                using (var cmd = new SqliteCommand(sql, _cn) { CommandType = CommandType.Text })
//                {
//                    using (var dr = cmd.ExecuteReader())
//                    {
//                        while (dr.Read())
//                        {
//                            var key = dr["name"].ToString();
//                            var typ = new SqliteColumnTypesExtended();
//                            // data types found @ http://www.tutorialspoint.com/sqlite/sqlite_data_types.htm
//                            var columnType = dr["type"].ToString().ToUpper();
//                            switch (columnType)
//                            {
//                                case "INTEGER":
//                                case "TINYINT":
//                                case "SMALLINT":
//                                case "MEDIUMINT":
//                                case "BIGINT":
//                                case "UNSIGNED BIG INT":
//                                case "INT2":
//                                case "INT8":
//                                case "INT":
//                                    typ = SqliteColumnTypesExtended.Integer;
//                                    break;
//                                case "CLOB":
//                                case "TEXT":
//                                    typ = SqliteColumnTypesExtended.Text;
//                                    break;
//                                case "BLOB":
//                                    typ = SqliteColumnTypesExtended.Blob;
//                                    break;
//                                case "REAL":
//                                case "DOUBLE":
//                                case "DOUBLE PRECISION":
//                                case "FLOAT":
//                                    typ = SqliteColumnTypesExtended.Real;
//                                    break;
//                                case "NUMERIC":
//                                    typ = SqliteColumnTypesExtended.Numeric;
//                                    break;
//                                case "BOOLEAN":
//                                    typ = SqliteColumnTypesExtended.Boolean;
//                                    break;
//                                case "DATE":
//                                case "DATETIME":
//                                    typ = SqliteColumnTypesExtended.Date;
//                                    break;
//                                default: // look for fringe cases that need logic
//                                    if (columnType.StartsWith("CHARACTER"))
//                                        typ = SqliteColumnTypesExtended.Text;
//                                    if (columnType.StartsWith("VARCHAR"))
//                                        typ = SqliteColumnTypesExtended.Text;
//                                    if (columnType.StartsWith("VARYING CHARACTER"))
//                                        typ = SqliteColumnTypesExtended.Text;
//                                    if (columnType.StartsWith("NCHAR"))
//                                        typ = SqliteColumnTypesExtended.Text;
//                                    if (columnType.StartsWith("NATIVE CHARACTER"))
//                                        typ = SqliteColumnTypesExtended.Text;
//                                    if (columnType.StartsWith("NVARCHAR"))
//                                        typ = SqliteColumnTypesExtended.Text;
//                                    if (columnType.StartsWith("NVARCHAR"))
//                                        typ = SqliteColumnTypesExtended.Text;
//                                    if (columnType.StartsWith("DECIMAL"))
//                                        typ = SqliteColumnTypesExtended.Numeric;
//                                    break;
//                            }
//                            _columns.Add(key, typ);
//                        }
//                    }
//                }
//                if (_columns == null || _columns.Count < 1)
//                    throw new Exception($"{this.DestinationTableName} could not be found in the database");
//            }
//        }

//        /// <summary>
//        /// Close and database connections.
//        /// </summary>
//        public void Close()
//        {
//            if (_cn.State == ConnectionState.Open)
//                _cn.Close();
//        }

//        /// <summary>
//        /// Copies all rows in the supplied IDataReader to a destination table specified 
//        /// by the DestinationTableName property of the SqliteBulkCopy object.
//        /// </summary>
//        /// <param name="reader"></param>
//        public void WriteToServer(IDataReader reader)
//        {
//            if (reader == null)
//                throw new ArgumentNullException("reader");

//            // build the insert schema
//            var insertClause = new StringBuilder();
//            insertClause.Append($"INSERT INTO {this.DestinationTableName} (");
//            var first = true;
//            foreach (var c in this.ColumnMappings)
//            {
//                if (first)
//                {
//                    insertClause.Append(c.Key);
//                    first = false;
//                }
//                else
//                    insertClause.Append("," + c.Key);
//            }
//            insertClause.Append(")");

//            first = true;
//            var valuesClause = new StringBuilder();
//            var currentBatch = 0;
//            while (reader.Read())
//            {
//                // generate insert values block statement
//                if (currentBatch > 0)
//                    valuesClause.Append(",");
//                valuesClause.Append("(");
//                var colFirst = true;
//                foreach (var c in this.ColumnMappings)
//                {
//                    if (!colFirst)
//                        valuesClause.Append(",");
//                    else
//                        colFirst = false;
//                    var columnValue = reader[c.Key].ToString();
//                    if (string.IsNullOrEmpty(columnValue))
//                        valuesClause.Append("NULL");
//                    else
//                    {
//                        switch (c.Value)
//                        {
//                            case SqliteColumnTypesExtended.Date:
//                                try
//                                {
//                                    valuesClause.Append($"'{ DateTime.Parse(columnValue).ToString("yyyy-MM-dd HH:mm:ss") }'");
//                                }
//                                catch (Exception exp)
//                                {
//                                    throw new Exception($"Invalid Cast when loading date column [{ c.Key }] in table [{ this.DestinationTableName}] in Sqlite DB with data; value being casted '{ columnValue}', incoming values must be of data format consumable by .NET; error:\n {exp.Message}");
//                                }
//                                break;
//                            case SqliteColumnTypesExtended.Integer:
//                            case SqliteColumnTypesExtended.Numeric:
//                            case SqliteColumnTypesExtended.Real:
//                                valuesClause.Append(columnValue);
//                                break;
//                            case SqliteColumnTypesExtended.Boolean:
//                                var out_value = -1;
//                                if (columnValue.ToUpper() == "TRUE")
//                                    valuesClause.Append("1");
//                                else if (columnValue.ToUpper() == "FALSE")
//                                    valuesClause.Append("0");
//                                else if (int.TryParse(columnValue, out out_value))
//                                {
//                                    if (out_value == 1 || out_value == 0)
//                                        valuesClause.Append($"{columnValue}");
//                                    else // numeric value out of range, throw exception
//                                        throw new Exception($"Invalid Cast when loading boolean column [{ c.Key }] in table [{ this.DestinationTableName}] in Sqlite DB with data; value being casted '{ columnValue}', incoming values can only be True or False (case does not matter), 1 (true) or 0 (false), or NULL");
//                                }
//                                else // no valid boolean types throw exception
//                                    throw new Exception($"Invalid Cast when loading boolean column [{ c.Key }] in table [{ this.DestinationTableName}] in Sqlite DB with data; value being casted '{ columnValue}', incoming values can only be True or False (case does not matter), 1 (true) or 0 (false), or NULL");
//                                break;
//                            case SqliteColumnTypesExtended.Text:
//                            default:
//                                valuesClause.Append($"'{columnValue.Replace("'", "''")}'");
//                                break;
//                        }
//                    }
//                }
//                valuesClause.Append(")");

//                currentBatch++;
//                if (currentBatch == this.BatchSize)
//                {
//                    var dml = $"BEGIN;\n{insertClause.ToString()} VALUES {valuesClause.ToString()};\nCOMMIT;";
//                    valuesClause.Clear();
//                    using (var cmd = new SqliteCommand(dml, _cn) { CommandType = CommandType.Text, CommandTimeout = this.BulkCopyTimeout })
//                        cmd.ExecuteNonQuery();
//                    currentBatch = 0;
//                }
//            }
//            if (this.CloseAndDisploseDataReader)
//            {
//                reader.Close();
//                reader.Dispose();
//            }
//            // if any records remain after the read loop has completed then write them to the DB
//            if (currentBatch > 0)
//            {
//                var dml = $"BEGIN;\n{insertClause.ToString()} VALUES {valuesClause.ToString()};\nCOMMIT;";
//                using (var cmd = new SqliteCommand(dml, _cn) { CommandType = CommandType.Text, CommandTimeout = this.BulkCopyTimeout })
//                    cmd.ExecuteNonQuery();
//            }
//        }

//#pragma warning disable 1998
//        /// <summary>
//        /// The asynchronous version of WriteToServer, which copies all rows in the supplied IDataReader to a 
//        /// destination table specified by the DestinationTableName property of the SqliteBulkCopy object.
//        /// </summary>
//        /// <param name="reader"></param>
//        /// <returns></returns>
//        private async Task WriteToServerAsyncInternal(IDataReader reader)
//        {
//            WriteToServer(reader);
//        }
//#pragma warning restore 1998

//        /// <summary>
//        /// Releases all resources used by the current instance of the SqliteBulkCopy class.
//        /// </summary>
//        public void Dispose()
//        {
//            this.Close();
//            _cn.Dispose();
//        }

//        #endregion
//    }
//}