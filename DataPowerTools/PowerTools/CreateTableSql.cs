using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataPowerTools.Extensions;


//TODO: should output a report - Best Fit Data Type, top 10 distinct values, fill level, # of values, # of distinct values, # missing values, y/n is full, y/n is distinct (for finding pks) - reference http://www.agiledatasoftware.com/products/

namespace DataPowerTools.PowerTools
{
    /// <summary>
    /// Contains methods pertaining to the creation of create table SQL based on data inputs. Not throughly documented yet and needs cleanup.
    /// </summary>
    public static class CreateTableSql
    {
        public enum AtomicDataType
        {
            Int,
            Decimal,
            Float,
            String,
            Date,
            DateTime,
            Bool,
            Guid
        }

        public static readonly AtomicDataType[] NumericTypes =
        {
            AtomicDataType.Int,
            AtomicDataType.Decimal,
            AtomicDataType.Float
        };

        public static readonly AtomicDataType[] CharTypes =
        {
            AtomicDataType.String
        };

        public static string FromTypes(Type[] types)
        {
            var returnLines = new List<string>();

            var tables = new List<CreateTableSqlFromTypesClass>();

            // Get Types in the assembly.
            foreach (var t in types)
            {
                var tc = new CreateTableSqlFromTypesClass(t);
                tables.Add(tc);
            }

            // Create SQL for each table
            foreach (var table in tables)
            {
                returnLines.Add(table.CreateTableScript());
                returnLines.Add("");
            }

            // Total Hacked way to find FK relationships! Too lazy to fix right now
            foreach (var table in tables)
                foreach (var field in table.Fields)
                    foreach (var t2 in tables)
                        if (field.Value.Name == t2.ClassName)
                        {
                            // We have a FK Relationship!
                            returnLines.Add("GO");
                            returnLines.Add("ALTER TABLE " + table.ClassName + " WITH NOCHECK");
                            returnLines.Add("ADD CONSTRAINT FK_" + field.Key + " FOREIGN KEY (" + field.Key +
                                            ") REFERENCES " + t2.ClassName + "(ID)");
                            returnLines.Add("GO");
                        }

            return string.Join("\r\n", returnLines);
        }

        public static string FromDataReader_Smart(string outputTableName, IEnumerable<Func<DataReaderInfo>> dataReaders,
            int numberOfRowsToExamine = -1)
        {
            //maps column names to hashset of unique values present for those columns
            var uniqueValList = new ConcurrentDictionary<string, ConcurrentHashSet<string>>();

            //we want the field names because these are our output table sql's column names

            //this is not thread safe
            var masterListOfFieldNames = new ConcurrentHashSet<string>();

            //fit the data types from the data readers' data
            var result = Parallel.ForEach(dataReaders, new ParallelOptions {MaxDegreeOfParallelism = 12},
                dataReaderInfoFac =>
                {
                    //get column names and setup hash dictionaries
                    var numRows = 0;

                    var drInfo = dataReaderInfoFac();

                    var dataReader = drInfo.DataReader;

                    var readerFieldNames = dataReader.GetFieldNames();

                    //if there was a problem we may have to read to read first to initialize in order to get fields 
                    var tryAgain = (readerFieldNames == null) || (readerFieldNames.Length == 0);
                    if (tryAgain)
                    {
                        dataReader.Read();
                        readerFieldNames = dataReader.GetFieldNames();
                        numRows++;
                    }

                    if ((readerFieldNames == null) || (readerFieldNames.Length == 0))
                        throw new Exception($"Could not get field names for file {drInfo.FilePath}");

                    //unfortunately this duplicated code is here for a reason.. we want to read to initialize the reader, but we also want to capture the first row if we already read. Please refactor though
                    foreach (var readerFieldName in readerFieldNames)
                    {
                        if (!uniqueValList.ContainsKey(readerFieldName))
                            uniqueValList.AddOrUpdate(
                                readerFieldName, //our key for the dictionary
                                new ConcurrentHashSet<string>(),
                                //if it doesn't exist create a new concurrent dictionary
                                (field, uniqueVals) => //otherwise we want to return the new updated value
                                        uniqueVals);
                        ;

                        if (masterListOfFieldNames.Contains(readerFieldName) == false)
                            masterListOfFieldNames.Add(readerFieldName);
                    }

                    //we tried again and got the field names so we can go ahead and add the values that are currently on record
                    if (tryAgain)
                        foreach (var col in readerFieldNames)
                        {
                            var val = dataReader[col]?.ToString();
                            var uvl = uniqueValList[col];
                            uvl.Add(val);
                        }

                    //collect unique row values by column
                    while (dataReader.Read() && ((numRows < numberOfRowsToExamine) || (numberOfRowsToExamine == -1)))
                    {
                        foreach (var col in readerFieldNames)
                        {
                            var val = dataReader[col]?.ToString();
                            var uvl = uniqueValList[col];
                            uvl.Add(val);
                        }
                        numRows++;
                    }
                });

            var sqlTable = new SqlTableDefinition
            {
                TableName = outputTableName
            };

            foreach (var col in masterListOfFieldNames.ToArray())
                sqlTable.ColumnDefinitions.Add(GetBestFitSqlColumnType(uniqueValList[col].Hashset, col));

            return FromSqlTableDefinition(sqlTable);
        }


        /// <summary>
        ///     Gets schema from a data table using a datareader, but parses the data to determine what datatype is best fit.
        /// </summary>
        /// <param name="outputTableName">The output table name e.g. "CREATE TABLE [NAME] (..."</param>
        /// <param name="dataReader">Data reader implmementation.</param>
        /// <param name="numberOfRowsToExamine">Examine the first n rows for data type determination.</param>
        /// <returns></returns>
        public static string FromDataReader_Smart(string outputTableName, IDataReader dataReader, string sourceFilePath,
            int numberOfRowsToExamine = -1)
        {
            var drFac =
                new Func<DataReaderInfo>(() => new DataReaderInfo {DataReader = dataReader, FilePath = sourceFilePath});

            return FromDataReader_Smart(
                outputTableName, new[] {drFac}, numberOfRowsToExamine);
        }


        /// <summary>
        ///     Gets schema from a data table, but parses the data to determine what datatype is best fit.
        /// </summary>
        /// <param name="outputTableName">The output table name e.g. "CREATE TABLE [NAME] (..."</param>
        /// <param name="data">Data to analyze</param>
        /// <returns></returns>
        public static string FromDataTable_Smart(string outputTableName, DataTable data)
        {
            var sqlTable = new SqlTableDefinition
            {
                TableName = outputTableName
            };


            //loop through each column
            for (var colIndex = 0; colIndex < data.Columns.Count; colIndex++)
            {
                var rowVals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                //collect row values into hashset
                for (var rowIndex = 0; rowIndex < data.Rows.Count; rowIndex++)
                    rowVals.Add(data.Rows[rowIndex][colIndex].ToString());

                sqlTable.ColumnDefinitions.Add(GetBestFitSqlColumnType(rowVals, data.Columns[colIndex].ColumnName));
            }

            return FromSqlTableDefinition(sqlTable);
        }

        public static SqlColumnDefinition GetBestFitSqlColumnType(HashSet<string> vals, string colName)
        {
            var sqlCol = new SqlColumnDefinition
            {
                IsNullable =
                    vals.Any(v => string.IsNullOrEmpty(v) || v.Equals("NULL", StringComparison.CurrentCultureIgnoreCase)),
                ColumnName = colName
            };

            vals.RemoveWhere(string.IsNullOrWhiteSpace);

            var bestFitType = GetBestFitType(vals, colName);

            var sqlType = "dtype";

            if (NumericTypes.Contains(bestFitType))
            {
                var isMoney = vals
                    .All(v =>
                        {
                            var vt = v.Trim();
                            return vt.StartsWith("$")
                                   || vt.EndsWith("$")
                                   || vt.StartsWith("-$")
                                   || (vt.StartsWith("($") && vt.EndsWith(")"));
                        });      // supports ($1200) and  -$1000

                if (isMoney)
                {
                    sqlType = "MONEY";
                }
                else if (bestFitType == AtomicDataType.Int)
                {
                    sqlType = "INT";
                }
                else
                {
                    var decimalValues = vals
                        .Select(v =>
                        {
                            var pieces = v.Split('.');
                            return new
                            {
                                DecimalPlaces = pieces.Length > 1 ? pieces[1].Trim().Length : 0,
                                IntegerSize = pieces[0]?.Trim().Length ?? 0
                            };
                        });

                    var maxIntPart = decimalValues.Max(d => d.IntegerSize);
                    var maxDecimalPlaces = decimalValues.Max(d => d.DecimalPlaces);

                    sqlType = $"DECIMAL({maxIntPart + maxDecimalPlaces}, {maxDecimalPlaces})";
                }
            }
            else if (CharTypes.Contains(bestFitType))
            {
                var maxLength = vals.Select(v => v.Length).Max();

                sqlType = maxLength > 900 ? "VARCHAR(MAX)" : $"VARCHAR({maxLength})";
            }
            else if (bestFitType == AtomicDataType.Guid)
            {
                sqlType = "UNIQUEIDENTIFIER";
            }
            else if (bestFitType == AtomicDataType.Bool)
            {
                sqlType = "BIT";
            }
            else if (bestFitType == AtomicDataType.DateTime)
            {
                sqlType = "DATETIME";
            }
            else if (bestFitType == AtomicDataType.Date)
            {
                sqlType = "DATE";
            }

            if (sqlType == "dtype")
                sqlType += $" /* First 8 distinct values: '{string.Join(",", vals.Take(8))}' */ ";

            sqlCol.DataType = sqlType;

            return sqlCol;
        }


        public static AtomicDataType GetBestFitType(HashSet<string> vals, string colName = "")
        {
            var nonEmptyVals = new HashSet<string>(
                vals
                    .Where(
                        d =>
                            !string.IsNullOrWhiteSpace(d) &&
                            !d.Equals("NULL", StringComparison.CurrentCultureIgnoreCase))
                    .Select(p => p.Trim()
                    ), StringComparer.CurrentCultureIgnoreCase);

            var typeListPriority = new[]
            {
                AtomicDataType.Bool,
                AtomicDataType.Date,
                AtomicDataType.DateTime,
                AtomicDataType.Int,
                AtomicDataType.Decimal,
                AtomicDataType.Float,
                AtomicDataType.Guid,
                AtomicDataType.String
            };

            //loop through type priorities first
            foreach (var type in typeListPriority)
                if (type == AtomicDataType.Bool)
                {
                    if (nonEmptyVals.IsProperSubsetOf(new[] {"true", "false", "0", "1"}))
                        //, "y", "n", "N", "Y"})) --removing these because bulk upload/inserts are difficult with these
                        return AtomicDataType.Bool;
                }
                else if ((type == AtomicDataType.DateTime) || (type == AtomicDataType.Date))
                {
                    DateTime dt;
                    var dateVals = nonEmptyVals
                        .Select(v => new {ParseResult = DateTime.TryParse(v, out dt), OutDate = dt, OrigValue = v});

                    var allAreDates = dateVals.All(v => v.ParseResult);

                    var nonDates = dateVals.Where(v => v.ParseResult == false).ToArray();

                    if (allAreDates)
                    {
                        if (dateVals.All(v => v.OutDate.TimeOfDay.TotalSeconds == 0.00))
                            return AtomicDataType.Date;
                        return AtomicDataType.DateTime;
                    }
                    if (colName.ToLower().Contains("date"))
                        return AtomicDataType.DateTime;
                }
                else if (type == AtomicDataType.Int)
                {
                    int it;
                    if (nonEmptyVals
                        .Select(v => int.TryParse(v, out it))
                        .All(b => b))
                        return AtomicDataType.Int;
                }
                else if (type == AtomicDataType.Decimal)
                {
                    decimal dt;
                    if (nonEmptyVals
                        .Select(v => decimal.TryParse(v, out dt))
                        .All(b => b))
                        return AtomicDataType.Decimal;

                    if (nonEmptyVals
                        .All(
                            v =>
                                v.StartsWith("$")
                                || v.EndsWith("$")
                                || v.StartsWith("-$")
                                || (v.StartsWith("($") && v.EndsWith(")"))
                        )) // supports ($1200) and  -$1000
                        return AtomicDataType.Decimal;

                    if (nonEmptyVals
                        .All(v => v.StartsWith("%") || v.EndsWith("%")))
                        return AtomicDataType.Decimal;
                }
                else if (type == AtomicDataType.Float)
                {
                    float dt;
                    if (nonEmptyVals
                        .Select(v => float.TryParse(v, out dt))
                        .All(b => b))
                        return AtomicDataType.Float;
                    if (nonEmptyVals
                        .All(v => v.StartsWith("$") || v.EndsWith("$")))
                        return AtomicDataType.Float;
                }
                else if (type == AtomicDataType.Guid)
                {
                    Guid dt;
                    if (nonEmptyVals
                        .Select(v => Guid.TryParse(v, out dt))
                        .All(b => b))
                        return AtomicDataType.Guid;
                }
                else if (type == AtomicDataType.String)
                {
                    return AtomicDataType.String;
                }

            return AtomicDataType.String;
        }


        public static string FromSqlTableDefinition(SqlTableDefinition schema)
        {
            var sql = "CREATE TABLE [" + schema.TableName + "] (\n";

            // columns
            foreach (var column in schema.ColumnDefinitions)
            {
                sql += "\t[" + column.ColumnName + "] " + column.DataType;

                if (column.IsNullable == false)
                    sql += " NOT NULL";

                sql += ",\n";
            }
            sql = sql.TrimEnd(',', '\n') + "\n";

            // primary keys
            var pk = ", CONSTRAINT PK_" + schema.TableName + " PRIMARY KEY CLUSTERED (";
            var hasKeys = (schema.PrimaryKeyColumnNames?.Count ?? 0) > 0;
            if (hasKeys)
                pk = schema.PrimaryKeyColumnNames.Aggregate(pk, (current, key) => current + key + ", ");

            pk = pk.TrimEnd(',', ' ', '\n') + ")\n";
            if (hasKeys) sql += pk;

            sql += ")";

            return sql;
        }


        public static string FromDataTable(string tableName, DataTable schema, int[] primaryKeys = null)
        {
            var sqlTable = new SqlTableDefinition
            {
                TableName = tableName
            };

            // columns
            foreach (DataRow column in schema.Rows)
            {
                var sqlCol = new SqlColumnDefinition();

                if (!(schema.Columns.Contains("IsHidden") && (bool) column["IsHidden"]))
                {
                    sqlCol.ColumnName = column["ColumnName"].ToString();
                    sqlCol.DataType = SQLGetType(column);

                    if (schema.Columns.Contains("AllowDBNull") && ((bool) column["AllowDBNull"] == false))
                        sqlCol.IsNullable = false;
                }

                sqlTable.ColumnDefinitions.Add(sqlCol);
            }

            // primary keys
            var hasKeys = (primaryKeys != null) && (primaryKeys.Length > 0);
            if (hasKeys)
            {
                // user defined keys
                foreach (var key in primaryKeys)
                    sqlTable.PrimaryKeyColumnNames.Add(schema.Rows[key]["ColumnName"].ToString());
            }
            else
            {
                // check schema for keys
                sqlTable.PrimaryKeyColumnNames.AddRange(GetPrimaryKeys(schema));
                hasKeys = sqlTable.PrimaryKeyColumnNames.Count > 0;
            }

            return FromSqlTableDefinition(sqlTable);
        }

        public static string FromDataTable2(string tableName, DataTable table)
        {
            var sql = "CREATE TABLE [" + tableName + "] (\n";
            // columns
            foreach (DataColumn column in table.Columns)
                sql += "[" + column.ColumnName + "] " + SQLGetType(column) + ",\n";
            sql = sql.TrimEnd(',', '\n') + "\n";
            // primary keys
            if (table.PrimaryKey.Length > 0)
            {
                sql += "CONSTRAINT [PK_" + tableName + "] PRIMARY KEY CLUSTERED (";
                foreach (var column in table.PrimaryKey)
                    sql += "[" + column.ColumnName + "],";
                sql = sql.TrimEnd(',') + "))\n";
            }

            return sql;
        }

        private static string[] GetPrimaryKeys(DataTable schema)
        {
            var keys = new List<string>();

            foreach (DataRow column in schema.Rows)
                if (schema.Columns.Contains("IsKey") && (bool) column["IsKey"])
                    keys.Add(column["ColumnName"].ToString());

            return keys.ToArray();
        }

        // Return T-SQL data type definition, based on schema definition for a column
        public static string SQLGetType(object type, int columnSize, int numericPrecision, int numericScale)
        {
            switch (type.ToString())
            {
                case "System.String":
                    if (columnSize.ToString() == int.MaxValue.ToString())
                        return "VARCHAR(MAX)";
                    return "VARCHAR(" + (columnSize == -1 ? 255 : columnSize) + ")";

                case "System.Decimal":
                    if (numericScale > 0)
                        return "REAL";
                    if (numericPrecision > 10)
                        return "BIGINT";
                    return "INT";

                case "System.Double":
                case "System.Single":
                    return "REAL";

                case "System.Int64":
                    return "BIGINT";

                case "System.Int16":
                case "System.Int32":
                    return "INT";

                case "System.DateTime":
                    return "DATETIME";

                case "System.Byte[]":
                    return "VARBINARY(MAX)";

                default:
                    throw new Exception(type + " not implemented.");
            }
        }

        // Overload based on row from schema table
        private static string SQLGetType(DataRow schemaRow)
        {
            var colSize = schemaRow["ColumnSize"].ToString();

            if (string.IsNullOrWhiteSpace(colSize))
                colSize = "4000";

            var numericPrecision = schemaRow["NumericPrecision"].ToString();

            if (string.IsNullOrWhiteSpace(numericPrecision))
                numericPrecision = "8";

            var numericScale = schemaRow["NumericScale"].ToString();

            if (string.IsNullOrWhiteSpace(numericScale))
                numericScale = "16";

            return SQLGetType(schemaRow["DataType"],
                int.Parse(colSize),
                int.Parse(numericPrecision),
                int.Parse(numericScale));
        }

        // Overload based on DataColumn from DataTable type
        private static string SQLGetType(DataColumn column)
        {
            return SQLGetType(column.DataType, column.MaxLength, 10, 2);
        }

        private class CreateTableSqlFromTypesClass
        {
            public CreateTableSqlFromTypesClass(Type t)
            {
                ClassName = t.Name;

                foreach (var p in t.GetProperties())
                {
                    var field = new KeyValuePair<string, Type>(p.Name, p.PropertyType);

                    Fields.Add(field);
                }
            }

            private Dictionary<Type, string> dataMapper
            {
                get
                {
                    // Add the rest of your CLR Types to SQL Types mapping here
                    var dataMapper = new Dictionary<Type, string>
                    {
                        {typeof(int), "BIGINT"},
                        {typeof(string), "NVARCHAR(500)"},
                        {typeof(bool), "BIT"},
                        {typeof(DateTime), "DATETIME"},
                        {typeof(float), "FLOAT"},
                        {typeof(decimal), "DECIMAL(18,0)"},
                        {typeof(Guid), "UNIQUEIDENTIFIER"}
                    };

                    return dataMapper;
                }
            }

            public List<KeyValuePair<string, Type>> Fields { get; } = new List<KeyValuePair<string, Type>>();

            public string ClassName { get; } = string.Empty;

            public string CreateTableScript()
            {
                var script = new StringBuilder();

                script.AppendLine("CREATE TABLE " + ClassName);
                script.AppendLine("(");
                script.AppendLine("\t ID BIGINT,");
                for (var i = 0; i < Fields.Count; i++)
                {
                    var field = Fields[i];

                    if (dataMapper.ContainsKey(field.Value))
                        script.Append("\t " + field.Key + " " + dataMapper[field.Value]);
                    else
                        script.Append("\t " + field.Key + " BIGINT");

                    if (i != Fields.Count - 1)
                        script.Append(",");

                    script.Append(Environment.NewLine);
                }

                script.AppendLine(")");

                return script.ToString();
            }
        }

        public class SqlTableDefinition
        {
            public string TableName { get; set; }

            public List<SqlColumnDefinition> ColumnDefinitions { get; set; }
            = new List<SqlColumnDefinition>();

            public List<string> PrimaryKeyColumnNames { get; set; }
            = new List<string>();
        }

        public class SqlColumnDefinition
        {
            public string ColumnName { get; set; }

            public string DataType { get; set; }

            public bool IsNullable { get; set; } = true;
        }

        public class ConcurrentHashSet<T> : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            public HashSet<T> Hashset { get; } = new HashSet<T>();

            public T[] ToArray() => Hashset.ToArray();
            public IEnumerable<T> AsEnumerable() => (IEnumerable<T>) Hashset;

            #region Implementation of ICollection<T> ...ish

            public bool Add(T item)
            {
                _lock.EnterWriteLock();
                try
                {
                    return Hashset.Add(item);
                }
                finally
                {
                    if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
                }
            }

            public void Clear()
            {
                _lock.EnterWriteLock();
                try
                {
                    Hashset.Clear();
                }
                finally
                {
                    if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
                }
            }

            public bool Contains(T item)
            {
                _lock.EnterReadLock();
                try
                {
                    return Hashset.Contains(item);
                }
                finally
                {
                    if (_lock.IsReadLockHeld) _lock.ExitReadLock();
                }
            }

            public bool Remove(T item)
            {
                _lock.EnterWriteLock();
                try
                {
                    return Hashset.Remove(item);
                }
                finally
                {
                    if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
                }
            }

            public int Count
            {
                get
                {
                    _lock.EnterReadLock();
                    try
                    {
                        return Hashset.Count;
                    }
                    finally
                    {
                        if (_lock.IsReadLockHeld) _lock.ExitReadLock();
                    }
                }
            }

            #endregion

            #region Dispose

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                    if (_lock != null)
                        _lock.Dispose();
            }

            ~ConcurrentHashSet()
            {
                Dispose(false);
            }

            #endregion
        }

        public class DataReaderInfo
        {
            public IDataReader DataReader { get; set; }
            public string FilePath { get; set; }
        }
    }
}