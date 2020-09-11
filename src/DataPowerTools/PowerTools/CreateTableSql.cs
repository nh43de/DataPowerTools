using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.DataStructures;
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
        
        private static Dictionary<Type, string> DataMapper => new Dictionary<Type, string>
        {
            {typeof(int), "BIGINT"},
            {typeof(string), "NVARCHAR(500)"},
            {typeof(bool), "BIT"},
            {typeof(DateTime), "DATETIME"},
            {typeof(float), "FLOAT"},
            {typeof(decimal), "DECIMAL"},
            {typeof(Guid), "UNIQUEIDENTIFIER"}
        };

        
        /// <summary>
        /// Generates a CREATE TABLE statement from a type definition, where the 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="outputTableName">Output table name, default will be class name.</param>
        /// <returns></returns>
        public static string GenerateCreateTableScriptFromType(Type t, string outputTableName = null)
        {
            var className = outputTableName ?? t.Name;

            var fields = t.GetPropertyAndFieldInfo();

            return GenerateCreateTableScriptFromType(fields, className);
        }

        /// <summary>
        /// Generates a create table script from basic field information.
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="outputTableName"></param>
        /// <returns></returns>
        public static string GenerateCreateTableScriptFromType(IEnumerable<BasicDataFieldInfo> fields,
            string outputTableName = null)
        {
            return GenerateCreateTableScriptFromType(new BasicTableDefinition
            {
                Fields = fields,
                TableName = outputTableName
            });
        }


        /// <summary>
        /// Creates a create table script from basic table definition.
        /// </summary>
        /// <param name="tableDef"></param>
        /// <returns></returns>
        public static string GenerateCreateTableScriptFromType(BasicTableDefinition tableDef)
        {
            var script = new StringBuilder();

            script.AppendLine("CREATE TABLE " + tableDef.TableName);
            script.AppendLine("(");

            var bodyText = tableDef.Fields
                    .Select(field =>
                        DataMapper.ContainsKey(field.FieldType)
                            ? (field.ColumnName + " " + DataMapper[field.FieldType]).Indent()
                            : (field.ColumnName + " BIGINT").Indent())
                    .JoinStr(",\r\n");

            script.AppendLine(bodyText);

            script.AppendLine(")");

            return script.ToString();
        }
        

        /// <summary>
        /// Creates CREATE TABLE script from an array of types - includes FK's for references.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static string FromTypes(Type[] types)
        {
            var returnLines = new List<string>();

            var tables = new List<BasicTableDefinition>();

            // Get Types in the assembly.
            foreach (var t in types)
            {
                var tc = t.GetPropertyAndFieldInfo().ToArray();

                returnLines.Add(GenerateCreateTableScriptFromType(tc, t.Name));
                returnLines.Add("");

                tables.Add(new BasicTableDefinition
                {
                    Fields = tc,
                    TableName = t.Name
                });
            }

            // Total Hacked way to find FK relationships! Too lazy to fix right now
            foreach (var table in tables)
                foreach (var field in table.Fields)
                    foreach (var t2 in tables)
                        if (field.ColumnName == t2.TableName)
                        {
                            // We have a FK Relationship!
                            returnLines.Add("GO");
                            returnLines.Add("ALTER TABLE " + table.TableName + " WITH NOCHECK");
                            returnLines.Add("ADD CONSTRAINT FK_" + field.ColumnName + " FOREIGN KEY (" + field.ColumnName +
                                            ") REFERENCES " + t2.TableName + "(ID)");
                            returnLines.Add("GO");
                        }

            return string.Join("\r\n", returnLines);
        }

        public static string FromDataReader_Smart(string outputTableName, IEnumerable<Func<DataReaderInfo>> dataReaders,
            int? numberOfRowsToExamine = null)
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
                    while (dataReader.Read() && ((numRows < numberOfRowsToExamine) || (numberOfRowsToExamine == null)))
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
        public static string FromDataReader_Smart(string outputTableName, IDataReader dataReader, int? numberOfRowsToExamine = null)
        {
            var drFac =
                new Func<DataReaderInfo>(() => new DataReaderInfo {DataReader = dataReader});

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
                    sqlCol.DataType = SqlGetType(column);

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
                sql += "[" + column.ColumnName + "] " + SqlGetType(column) + ",\n";
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
        public static string SqlGetType(object type, int columnSize, int numericPrecision, int numericScale)
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
        private static string SqlGetType(DataRow schemaRow)
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

            return SqlGetType(schemaRow["DataType"],
                int.Parse(colSize),
                int.Parse(numericPrecision),
                int.Parse(numericScale));
        }

        // Overload based on DataColumn from DataTable type
        private static string SqlGetType(DataColumn column)
        {
            return SqlGetType(column.DataType, column.MaxLength, 10, 2);
        }

    }
}