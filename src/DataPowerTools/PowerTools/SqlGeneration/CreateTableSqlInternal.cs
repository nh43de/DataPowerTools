using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.Extensions;

namespace DataPowerTools.PowerTools
{
    /// <summary>
    /// Contains methods pertaining to the creation of create table SQL based on data inputs. Not throughly documented yet and needs cleanup.
    /// </summary>
    public static class CreateTableSqlInternal
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
            /*
            from debaser

                {typeof(bool), new ColumnInfo(SqlDbType.Bit)},
                {typeof(byte), new ColumnInfo(SqlDbType.TinyInt)},
                {typeof(short), new ColumnInfo(SqlDbType.SmallInt)},
                {typeof(int), new ColumnInfo(SqlDbType.Int)},
                {typeof(long), new ColumnInfo(SqlDbType.BigInt)},

                {typeof(decimal), new ColumnInfo(SqlDbType.Decimal)},
                {typeof(double), new ColumnInfo(SqlDbType.Float)},
                {typeof(float), new ColumnInfo(SqlDbType.Real)},

                {typeof(string), new ColumnInfo(SqlDbType.NVarChar)},

                {typeof(DateTime), new ColumnInfo(SqlDbType.DateTime2)},
                {typeof(DateTimeOffset), new ColumnInfo(SqlDbType.DateTimeOffset)},

                {typeof(Guid), new ColumnInfo(SqlDbType.UniqueIdentifier)},

             */
            {typeof(int), "BIGINT"},
            {typeof(string), "NVARCHAR(500)"},
            {typeof(bool), "BIT"},
            {typeof(DateTime), "DATETIME"},
            {typeof(float), "FLOAT"},
            {typeof(decimal), "DECIMAL"},
            {typeof(Guid), "UNIQUEIDENTIFIER"}
        };

        public static Dictionary<AtomicDataType, string> AtomicTypeToCsharpMapper => new Dictionary<AtomicDataType, string>
        {
            /*
            from debaser

                {typeof(bool), new ColumnInfo(SqlDbType.Bit)},
                {typeof(byte), new ColumnInfo(SqlDbType.TinyInt)},
                {typeof(short), new ColumnInfo(SqlDbType.SmallInt)},
                {typeof(int), new ColumnInfo(SqlDbType.Int)},
                {typeof(long), new ColumnInfo(SqlDbType.BigInt)},

                {typeof(decimal), new ColumnInfo(SqlDbType.Decimal)},
                {typeof(double), new ColumnInfo(SqlDbType.Float)},
                {typeof(float), new ColumnInfo(SqlDbType.Real)},

                {typeof(string), new ColumnInfo(SqlDbType.NVarChar)},

                {typeof(DateTime), new ColumnInfo(SqlDbType.DateTime2)},
                {typeof(DateTimeOffset), new ColumnInfo(SqlDbType.DateTimeOffset)},

                {typeof(Guid), new ColumnInfo(SqlDbType.UniqueIdentifier)},

             */
            { AtomicDataType.Int, "int" },
            { AtomicDataType.Decimal, "decimal" },
            { AtomicDataType.Float, "double" },
            { AtomicDataType.String, "string" },
            { AtomicDataType.Date, "DateTime" },
            { AtomicDataType.DateTime, "DateTime" },
            { AtomicDataType.Bool, "bool" },
            { AtomicDataType.Guid, "Guid" },
        };

        public static string GetCsharpType(AtomicDataType? atomicDataType)
        {
            if(atomicDataType == null)
                return "UNKNOWN_TYPE";
            
            if (AtomicTypeToCsharpMapper.TryGetValue(atomicDataType.Value, out string value))
            {
                return value;
            }

            return $"UNKNOWN_TYPE_{atomicDataType.Value.ToString()}";
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

            sqlCol.BestFitDataType = bestFitType;

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
        public static string SqlGetType(DataRow schemaRow)
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
        public static string SqlGetType(DataColumn column)
        {
            return SqlGetType(column.DataType, column.MaxLength, 10, 2);
        }

    }
}