using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.DataStructures;
using DataPowerTools.Extensions;


//TODO: should output a report - Best Fit Data Type, top 10 distinct values, fill level, # of values, # of distinct values, # missing values, y/n is full, y/n is distinct (for finding pks) - reference http://www.agiledatasoftware.com/products/

namespace DataPowerTools.PowerTools
{
    public static class Constants
    {

    }

    public static class CreateTableSql
    {

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
                new Func<DataReaderInfo>(() => new DataReaderInfo { DataReader = dataReader });

            return FromDataReader_Smart(
                outputTableName, new[] { drFac }, numberOfRowsToExamine);
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

                sqlTable.ColumnDefinitions.Add(CreateTableSqlInternal.GetBestFitSqlColumnType(rowVals, data.Columns[colIndex].ColumnName));
            }

            return CreateTableSqlInternal.FromSqlTableDefinition(sqlTable);
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
                    sqlCol.DataType = CreateTableSqlInternal.SqlGetType(column);

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
                sqlTable.PrimaryKeyColumnNames.AddRange(schema.GetPrimaryKeys());
                hasKeys = sqlTable.PrimaryKeyColumnNames.Count > 0;
            }

            return CreateTableSqlInternal.FromSqlTableDefinition(sqlTable);
        }

        public static string FromDataTable2(string tableName, DataTable table)
        {
            var sql = "CREATE TABLE [" + tableName + "] (\n";
            // columns
            foreach (DataColumn column in table.Columns)
                sql += "[" + column.ColumnName + "] " + CreateTableSqlInternal.SqlGetType(column) + ",\n";
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
            return CreateTableSqlInternal.GenerateCreateTableScriptFromType(new BasicTableDefinition
            {
                Fields = fields,
                TableName = outputTableName
            });
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
            var result = Parallel.ForEach(dataReaders, new ParallelOptions { MaxDegreeOfParallelism = 12 },
                dataReaderInfoFac =>
                {
                    //get column names and setup hash dictionaries
                    var numRows = 0;

                    var drInfo = dataReaderInfoFac();

                    using var dataReader = drInfo.DataReader;

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
                sqlTable.ColumnDefinitions.Add(CreateTableSqlInternal.GetBestFitSqlColumnType(uniqueValList[col].Hashset, col));

            return CreateTableSqlInternal.FromSqlTableDefinition(sqlTable);
        }

    }


}