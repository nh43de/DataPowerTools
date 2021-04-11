using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using DataPowerTools.Connectivity.Helpers;
using DataPowerTools.DataConnectivity;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.DataReaderExtensibility.TransformingReaders;
using DataPowerTools.FastMember;
using DataPowerTools.PowerTools;
using DataPowerTools.Strings;

namespace DataPowerTools.Extensions
{
    public static class DataReaderExtensions
    {
        /// <summary>
        /// Writes datareader to CSV.
        /// </summary>
        public static void WriteCsv(this IDataReader reader, string outputFile)
        {
            Csv.Write(reader, outputFile);
        }

        /// <summary>
        /// Writes datareader to CSV.
        /// </summary>
        public static string AsCsv(this IDataReader reader, bool writeHeaders = true)
        {
            return Csv.WriteString(reader);
        }

        /// <summary>
        /// returns a datareader that will read the object as a single row.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IDataReader AsSingleRowDataReader<T>(this T obj, bool ignoreNonStringReferenceTypes = true)
        {
            return obj.AsSingleEnumerable().ToDataReader(ignoreNonStringReferenceTypes);
        }

        /// <summary>
        /// returns a datareader that will read the object as a single row.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IDataReader AsSingleRowDataReader<T>(this T obj, string[] fieldNames)
        {
            return obj.AsSingleEnumerable().ToDataReader(fieldNames);
        }
        
        /// <summary>
        /// Analyzes the rows in a DataReader and generates a CREATE TABLE statement from the data it finds.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="outputTableName"></param>
        /// <param name="numberOfRowsToExamine"></param>
        /// <returns></returns>
        public static string FitToCreateTableSql(this IDataReader reader, string outputTableName, int? numberOfRowsToExamine)
        {
            return CreateTableSql.FromDataReader_Smart(outputTableName, reader, numberOfRowsToExamine);
        }

        #region Data reader operations

        /// <summary>
        /// Finds headers in the datareader and applies them.
        /// </summary>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader">Source data reader</param>
        /// <param name="headerConfig">Configuration for header finding.</param>
        /// <returns></returns>
        public static IDataReader ApplyHeaders<TDataReader>(
            this TDataReader dataReader,
            HeaderReaderConfiguration headerConfig = null) where TDataReader : IDataReader
        {
            var d = new HeaderDataReader(dataReader, headerConfig ?? new HeaderReaderConfiguration());

            return d;
        }

        /// <summary>
        /// Finds headers in the datareader and applies them.
        /// </summary>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader">Source data reader</param>
        /// <param name="headerRow">The row on which headers are.</param>
        /// <returns></returns>
        public static IDataReader ApplyHeaders<TDataReader>(
            this TDataReader dataReader,
            int headerRow) where TDataReader : IDataReader
        {
            if (headerRow == 0)
                headerRow = 1;

            var d = new HeaderDataReader(dataReader, new HeaderReaderConfiguration
            {
                UseHeaderRow = true,
                HeaderRow = headerRow
            });

            return d;
        }


        /// <summary>
        /// Renames columns.
        /// </summary>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="oldNames"></param>
        /// <param name="newNames"></param>
        /// <returns></returns>
        public static IDataReader ApplyColumnsAliases<TDataReader>(
            this TDataReader dataReader,
            string[] oldNames, string[] newNames) where TDataReader : IDataReader
        {
            var dictionary = oldNames.Zip(newNames, (a, b) => new {Old = a, New = b})
                .ToDictionary(x => x.Old, x => x.New);

            return dataReader.ApplyColumnsAliases(dictionary);
        }

        /// <summary>
        /// Renames columns present in the dictionary.
        /// </summary>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="columnRenames"></param>
        /// <returns></returns>
        public static IDataReader ApplyColumnsAliases<TDataReader>(
            this TDataReader dataReader, 
            Dictionary<string, string> columnRenames) where TDataReader : IDataReader
        {
            var dr = new AliasingDataReader<TDataReader>(dataReader, columnRenames);

            return dr;
        }

        /// <summary>
        /// Renames columns from based on the column data annotations for the specified type. Will rename (type property name) -> (data annotation column name).
        /// </summary>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IDataReader ApplyColumnsAliases<TDataReader>(
            this TDataReader dataReader,
            Type type) where TDataReader : IDataReader
        {
            var ti = type
                .GetColumnInfo()
                .ToDictionary(
                    ci => ci.ColumnName,
                    ci => StringUtils.IsNullOrWhiteSpaceThen(ci.DisplayName, ci.ColumnName));

            var dr = new AliasingDataReader<TDataReader>(dataReader, ti);

            return dr;
        }

        /// <summary>
        /// Renames a column.
        /// </summary>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public static IDataReader ApplyColumnAlias<TDataReader>(
            this TDataReader dataReader, string oldName, string newName) where TDataReader : IDataReader
        {
            return dataReader.ApplyColumnsAliases(new Dictionary<string, string> { { oldName, newName} });
        }

        /// <summary>
        /// Renames columns based on the column data annotations for the specified type. Will rename (type property name) -> (data annotation column name).
        /// </summary>
        /// <typeparam name="T">The type to look for data annotations on.</typeparam>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static IDataReader ApplyColumnsAliases<T, TDataReader>(
            this TDataReader dataReader) where TDataReader : IDataReader
        {
            var type = typeof(T);

            return dataReader.ApplyColumnsAliases(type);
        }

        /// <summary>
        /// Renames columns from based on the column data annotations for the specified type. Will rename (type property name) -> (data annotation column name).
        /// </summary>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IDataReader ApplyColumnsAliasesReverse<TDataReader>(
            this TDataReader dataReader,
            Type type) where TDataReader : IDataReader
        {
            var ti = type
                .GetColumnInfo()
                .ToDictionary(
                    ci => StringUtils.IsNullOrWhiteSpaceThen(ci.DisplayName, ci.ColumnName),
                    ci => ci.ColumnName);

            var dr = new AliasingDataReader<TDataReader>(dataReader, ti);

            return dr;
        }

        /// <summary>
        /// Renames columns based on the column data annotations for the specified type. Will rename (type property name) -> (data annotation column name).
        /// </summary>
        /// <typeparam name="T">The type to look for data annotations on.</typeparam>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static IDataReader ApplyColumnsAliasesReverse<T, TDataReader>(
            this TDataReader dataReader) where TDataReader : IDataReader
        {
            var type = typeof(T);

            return dataReader.ApplyColumnsAliasesReverse(type);
        }

        /// <summary>
        /// Adds a column that is a row projection of the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="columnName"></param>
        /// <param name="rowProjection"></param>
        /// <returns></returns>
        public static IDataReader AddColumn<T, TDataReader>(this TDataReader dataReader, string columnName,
            RowProjection<T> rowProjection) where TDataReader: IDataReader
            //from extended data reader
        {
            var a = new AddColumnsDataReader<T, TDataReader>.AddColumn<T>
            {
                ColumnName = columnName,
                ValueFactory = rowProjection
            };

            return new AddColumnsDataReader<T, TDataReader>(dataReader, new[] {a});
        }

        /// <summary>
        /// Adds a column that is a row projection of the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="secondDataReader"></param>
        /// <returns></returns>
        public static IDataReader Union<TDataReader>(this TDataReader dataReader,
            TDataReader secondDataReader) where TDataReader : IDataReader
        //from extended data reader
        {
            var a = new UnionDataReader<TDataReader>(dataReader, secondDataReader);

            return a;
        }

        /// <summary>
        /// Logs errors that occur during data reader operations. Not tested.
        /// </summary>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="logEvents"></param>
        /// <param name="maxErrorsBeforeThrowing"></param>
        /// <returns></returns>
        public static IDataReader LogErrors<TDataReader>(this TDataReader dataReader, out IEnumerable<DataReaderLogEvent> logEvents,
            long? maxErrorsBeforeThrowing = null) where TDataReader : IDataReader
        {
            var l = new LoggingDataReader<TDataReader>(dataReader, maxErrorsBeforeThrowing);

            logEvents = l.LogEvents;

            return l;


        }

        /// <summary>
        /// Limits rows returned by the data reader.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static IDataReader LimitRows<TDataReader>(this TDataReader dataReader, int maxRows) where TDataReader : IDataReader
        {
            return new RowLimitingDataReader<TDataReader>(dataReader, maxRows);
        }

        /// <summary>
        /// Adds columns to a data reader.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="addColumns"></param>
        /// <returns></returns>
        public static IDataReader AddColumns<T, TDataReader>(this TDataReader dataReader,
            params AddColumnsDataReader<T, TDataReader>.AddColumn<T>[] addColumns) where TDataReader : IDataReader
        {
            return new AddColumnsDataReader<T, TDataReader>(dataReader, addColumns);
        }

        ///// <summary>
        ///// Aliases columns that are present in the dictionary.
        ///// </summary>
        ///// <param name="dataReader"></param>
        ///// <param name="sourceToAliasMapping"></param>
        ///// <returns></returns>
        //public static IDataReader ApplyAlias(this IDataReader dataReader,
        //    Dictionary<string, string> sourceToAliasMapping)
        //{
        //    return new AliasingDataReader(dataReader, sourceToAliasMapping);
        //}


        /// <summary>
        /// Projects all reader columns into a new set of columns.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="projectedColumns"></param>
        /// <returns></returns>
        public static IDataReader ApplyProjection<TDataReader>(this TDataReader dataReader,
            params RowProjection<object>[] projectedColumns) where TDataReader : IDataReader
        {
            return new ProjectingDataReader<TDataReader>(dataReader, projectedColumns);
        }

        /// <summary>
        /// Projects reader into a new set of columns.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="projectedColumns"></param>
        /// <returns></returns>
        public static IDataReader ApplyProjection<TDataReader>(this TDataReader dataReader,
            Dictionary<string, RowProjection<object>> projectedColumns) where TDataReader: IDataReader
        {
            return new ProjectingDataReader<TDataReader>(dataReader, projectedColumns);
        }

        /// <summary>
        /// Applies transformations to columns by column name. Transformation definitions are defined as a dictionary that maps column name to a DataTransform (obj) -> (obj).
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="transforms"></param>
        /// <returns></returns>
        public static IDataReader ApplyTransformation<TDataReader>(this TDataReader dataReader,
            Dictionary<string, DataTransform> transforms) where TDataReader : IDataReader
        {
            var rowProjections = transforms.ToDictionary(t => t.Key,
                t => new RowProjection<object>(row => t.Value?.Invoke(row[t.Key])));
            //this is dense. What we are doing here is turning a DataTransform: (obj) -> (obj) into a RowProjection: (row[]) -> (<TResult>)
            return new ColumnTransformingDataReader<object, TDataReader>(dataReader, rowProjections);
        }

        /// <summary>
        /// Applies transformations to columns by column names. Transformation definitions are defined as a dictionary that maps column name to a RowProjection (row[]) -> (TResult).
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="transforms"></param>
        /// <returns></returns>
        public static IDataReader ApplyTransformation<TResult, TDataReader>(this TDataReader dataReader,
            Dictionary<string, RowProjection<TResult>> transforms) where TDataReader : IDataReader
        {
            return new ColumnTransformingDataReader<TResult, TDataReader>(dataReader, transforms);
        }

        /// <summary>
        /// Applies transformations to column, by column name. A source column name is specified along with a RowProjection which is a (row[]) -> (TResult).
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="columnName"></param>
        /// <param name="rowProjection"></param>
        /// <returns></returns>
        public static IDataReader ApplyTransformation<TResult, TDataReader>(this TDataReader dataReader, string columnName,
            RowProjection<TResult> rowProjection) where TDataReader : IDataReader
            //from extended data reader
        {
            var a = new Dictionary<string, RowProjection<TResult>>
            {
                {columnName, rowProjection}
            };

            return new ColumnTransformingDataReader<TResult, TDataReader>(dataReader, a);
        }

        /// <summary>
        /// Applies transformations to column, by column index. A source column name is specified along with a RowProjection which is a (row[]) -> (TResult).
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="columnIndex"></param>
        /// <param name="rowProjection"></param>
        /// <returns></returns>
        public static IDataReader ApplyTransformation<TResult, TDataReader>(this TDataReader dataReader, int columnIndex,
            RowProjection<TResult> rowProjection) where TDataReader : IDataReader //from extended data reader
        {
            var a = new Dictionary<int, RowProjection<TResult>>
            {
                {columnIndex, rowProjection}
            };

            return new ColumnTransformingDataReader<TResult, TDataReader>(dataReader, a);
        }

        /// <summary>
        /// Applies transformations to columns. Transformation definitions are defined as a array that maps column index to a RowProjection (row[]) -> (TResult).
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="transforms"></param>
        /// <returns></returns>
        public static IDataReader ApplyTransformation<TResult, TDataReader>(this TDataReader dataReader,
            Dictionary<int, RowProjection<TResult>> transforms) where TDataReader : IDataReader
        {
            return new ColumnTransformingDataReader<TResult, TDataReader>(dataReader, transforms);
        }

        /// <summary>
        /// Returns mappings to destination columns given a data reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="destinationColumns"></param>
        /// <returns></returns>
        public static ColumnMappingInfo GetColumnMappings(this IDataReader reader,
            TypedDataColumnInfo[] destinationColumns)
        {
            var readerFieldNames = reader.GetFieldInfo();

            if (readerFieldNames.Length == 0)
                return null;

            return new ColumnMappingInfo(destinationColumns, readerFieldNames);
        }

        /// <summary>
        ///     Tries to get column names from IDataReader. If it is unsuccessful it will default to generic column names
        ///     "Column1", "Column2", ... https://github.com/haf/System.Data.SQLite/blob/master/System.Data.SQLite/SQLiteDataReader.cs
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static BasicDataColumnInfo[] GetFieldInfo(this IDataReader reader)
        {
            var fields = new List<BasicDataColumnInfo>();

            //first try getting using GetSchemaTable method
            try
            {
                var schemaTable = reader.GetSchemaTable();

                if ((schemaTable != null) && (schemaTable.Rows.Count != 0))
                {
                    foreach (DataRow schemaTableRow in schemaTable.Rows)
                    {
                        var colInfo = new BasicDataColumnInfo
                        {
                            ColumnName = schemaTableRow[SchemaTableColumn.ColumnName].ToString(),
                            Ordinal = int.Parse(schemaTableRow[SchemaTableColumn.ColumnOrdinal].ToString())
                        };

                        fields.Add(colInfo);
                    }
                    
                    foreach (var basicDataColumnInfo in fields)
                    {
                        try
                        {
                            basicDataColumnInfo.FieldType = reader.GetFieldType(basicDataColumnInfo.Ordinal);
                        }
                        catch (Exception e)
                        {
                            basicDataColumnInfo.FieldType = typeof(object);
                            //get field type failed
                        }
                    }
                    
                    return fields.ToArray();
                }
            }
            catch (Exception)
            {
                // GetSchemaTable approach failed
            }

            //previous approach failed
            fields = new List<BasicDataColumnInfo>();

            try
            {
                if (reader.FieldCount != 0)
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var colInfo = new BasicDataColumnInfo
                        {
                            Ordinal = i
                        };

                        try
                        {
                            colInfo.ColumnName = reader.GetName(i);
                        }
                        catch (Exception ex)
                        {
                            colInfo.ColumnName = "Column" + i;
                            //throw new Exception("Could not get field information for index: " + i, ex);
                        }

                        fields.Add(colInfo);
                    }
                    
                    foreach (var basicDataColumnInfo in fields)
                    {
                        try
                        {
                            basicDataColumnInfo.FieldType = reader.GetFieldType(basicDataColumnInfo.Ordinal);
                        }
                        catch (Exception e)
                        {
                            basicDataColumnInfo.FieldType = typeof(object);
                            //get field type failed
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get reader field information. ", ex);
            }

            return fields.ToArray();
        }


        /*
         	schema.Columns.Add(SchemaTableColumn.AllowDBNull, typeof(bool)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.BaseColumnName, typeof(string)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.BaseSchemaName, typeof(string)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.BaseTableName, typeof(string)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.ColumnName, typeof(string)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.ColumnSize, typeof(int)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.DataType, typeof(object)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.IsAliased, typeof(bool)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.IsExpression, typeof(bool)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.IsKey, typeof(bool)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.IsLong, typeof(bool)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.IsUnique, typeof(bool)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.NumericPrecision, typeof(short)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.NumericScale, typeof(short)).ReadOnly = true;
			schema.Columns.Add(SchemaTableColumn.ProviderType, typeof(int)).ReadOnly = true;



			schema.Columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(string)).ReadOnly = true;
			schema.Columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string)).ReadOnly = true;
			schema.Columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool)).ReadOnly = true;
			schema.Columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(bool)).ReadOnly = true;
			schema.Columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(bool)).ReadOnly = true;
			schema.Columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(bool)).ReadOnly = true;
        */

        /// <summary>
        /// Applies a mapping and tranformation based on a SQL destination. DataTranformationGroups determine how
        /// tranformations are mapped to destination types.
        /// Column renaming/reordering/aliasing is also setup automatically.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="destinationTable"></param>
        /// <param name="destinationConnectionString"></param>
        /// <param name="transformGroup"></param>
        /// <returns></returns>
        public static SmartDataReader<TDataReader> MapToSqlDestination<TDataReader>(this TDataReader dataReader, string destinationTable,
            string destinationConnectionString, DataTransformGroup transformGroup = null) where TDataReader : IDataReader
        {
            using (var a = new SqlConnection(destinationConnectionString))
            {
                a.Open();

                return MapToSqlDestination<TDataReader>(dataReader, destinationTable, a, transformGroup);
            }
        }
        
        /// <summary>
        /// Gets a Smart data reader based on a destination type and applies transformation group.
        /// </summary>
        /// <typeparam name="TDataReader"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="destinationType"></param>
        /// <param name="transformGroup">Specify the transform group available in the DataTransformGroups static class.</param>
        /// <returns></returns>
        public static SmartDataReader<IDataReader> MapToType<TDataReader>(this TDataReader dataReader, Type destinationType, DataTransformGroup transformGroup = null) where TDataReader : IDataReader
        {
            //var dr = dataReader.ApplyColumnsAliases(destinationType);

            var aliases = destinationType
                .GetColumnInfo()
                .ToDictionary(p => p.DisplayName ?? p.ColumnName,
                    p => p.ColumnName);

            var dr = dataReader.ApplyColumnsAliases(aliases);

            var destinationColumns = destinationType.GetTypedDataColumnInfo();

            return new SmartDataReader<IDataReader>(dr, destinationColumns, transformGroup ?? DataTransformGroups.None);
        }
        
        /// <summary>
        /// Applies a mapping and tranformation based on a SQL destination. DataTranformationGroups determine how
        /// tranformations are mapped to destination types.
        /// Column renaming/reordering/aliasing is also setup automatically.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="destinationTable"></param>
        /// <param name="destinationConnection"></param>
        /// <param name="transformGroup"></param>
        /// <returns></returns>
        public static SmartDataReader<TDataReader> MapToSqlDestination<TDataReader>(this TDataReader dataReader, string destinationTable,
            DbConnection destinationConnection, DataTransformGroup transformGroup = null) where TDataReader : IDataReader
        {
            var destinationColumns = Database.GetDataSchemaColumns(destinationTable, destinationConnection)
                    .Select(c => new TypedDataColumnInfo
                    {
                        ColumnName = c.ColumnName,
                        DataType = c.DataType,
                        Ordinal = c.Ordinal
                    }).ToArray();

            return new SmartDataReader<TDataReader>(dataReader, destinationColumns, transformGroup ?? DataTransformGroups.None);
        }

        /// <summary>
        /// Map and transform by specifying destination column metadata manually.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="destinationColumnInfo">Destination column metadata.</param>
        /// <param name="transformGroup"></param>
        /// <returns></returns>
        public static SmartDataReader<TDataReader> MapToSqlDestination<TDataReader>(this TDataReader dataReader,
            TypedDataColumnInfo[] destinationColumnInfo, DataTransformGroup transformGroup = null) where TDataReader : IDataReader
        {
            return new SmartDataReader<TDataReader>(dataReader, destinationColumnInfo, transformGroup ?? DataTransformGroups.None);
        }
        
        /// <summary>
        /// Calls a progress callback on each row or modulus of row count. Will also notify on last record.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="progress">Progress to notify that a row was read.</param>
        /// <param name="count">Modulus of the row count to notify on. E.g. for every count records will notify. If null will notify on every row. Will also notify on last record.</param>
        /// <returns></returns>
        public static IDataReader NotifyOn<TDataReader>(this TDataReader reader, IProgress<int> progress, int? count = null) where TDataReader : IDataReader
        {
            return new NotifyingDataReader<TDataReader>(reader, progress.Report, count);
        }

        /// <summary>
        /// Calls a progress callback on each row or modulus of row count. Will also notify on last record.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="callback">Callback to notify that a row was read.</param>
        /// <param name="count">Modulus of the row count to notify on. E.g. for every count records will notify. If null will notify on every row. Will also notify on last record.</param>
        /// <returns></returns>
        public static IDataReader NotifyOn<TDataReader>(this TDataReader reader, Action<int> callback, int? count = null) where TDataReader : IDataReader
        {
            return new NotifyingDataReader<TDataReader>(reader, callback, count);
        }

        /// <summary>
        /// Prints the current values of an IDataReader.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static string PrintValues(this IDataReader dataReader)
        {
            var o = new object[dataReader.FieldCount];
            dataReader.GetValues(o);

            var fields = dataReader.GetFieldNames().Zip(o, (s, o1) => new
            {
                Nm = s,
                Val = o1?.ToString()
            });

            return string.Join(", ", fields.Select(p => $"{{'{p.Nm}': '{p.Val}'}}"));
        }


        public static void ReadToEnd(this IDataReader dr)
        {
            while (dr.Read()) { }
        }

        /// <summary>
        /// Fast-forwards to the end of the data reader by calling .REad
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="count"></param>
        public static void Read(this IDataReader dr, int count)
        {
            var i = 0;

            while (i < count && dr.Read())
            {
                i++;
            }
        }

        /// <summary>
        /// Gets rows as enumerable of object[].  This is slower.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static IEnumerable<object[]> GetRows(this IDataReader dr)
        {
            while (dr.Read())
            {
                var fieldCount = dr.FieldCount;
                var objects = new object[fieldCount];
                yield return objects;
            }
        }

        /// <summary>
        /// Yields an IDataReader as an enumerable. Property names must match column names exactly. 
        /// This is slower than using FastMember but implicit casts are made.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="propNames">Prop names to only include.</param>
        /// <returns></returns>
        public static IEnumerable<T> SelectNonStrict<T>(this IDataReader dr, string[] propNames)
        {
            return dr.SelectNonStrict(typeof(T), () => Activator.CreateInstance<T>(), propNames).OfType<T>();
        }

        /// <summary>
        /// Yields an IDataReader as an enumerable. Property names must match column names exactly. 
        /// This is slower than using FastMember but implicit casts are made.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <returns></returns>
        public static IEnumerable<T> SelectNonStrict<T>(this IDataReader dr, bool ignoreNonStringReferenceTypes = true)
        {
            return dr.SelectNonStrict(typeof(T), ignoreNonStringReferenceTypes).OfType<T>();
        }

        /// <summary>
        /// Yields an IDataReader as an enumerable. Property names must match column names exactly. 
        /// This is slower than using FastMember but implicit casts are made.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="type"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <returns></returns>
        public static IEnumerable SelectNonStrict(this IDataReader dr, Type type, bool ignoreNonStringReferenceTypes = true)
        {
            var props = type.GetColumnMemberNames(ignoreNonStringReferenceTypes);
            
            return dr.SelectNonStrict(type, () => Activator.CreateInstance(type), props);
        }

        /// <summary>
        /// Yields an IDataReader as an enumerable. Property names must match column names exactly. 
        /// This is slower than using FastMember but implicit casts are made.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="type"></param>
        /// <param name="propNames">Prop names to only include.</param>
        /// <returns></returns>
        public static IEnumerable SelectNonStrict(this IDataReader dr, Type type, string[] propNames)
        {
            return SelectNonStrict(dr, type, () => Activator.CreateInstance(type), propNames);
        }


        /// <summary>
        /// Yields an IDataReader as an enumerable. Property names must match column names exactly. 
        /// This is slower than using FastMember but implicit casts are made.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="type"></param>
        /// <param name="newObjectFactory"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <returns></returns>
        public static IEnumerable SelectNonStrict(this IDataReader dr, Type type, Func<object> newObjectFactory, bool ignoreNonStringReferenceTypes = true)
        {
            var props = type.GetColumnMemberNames(ignoreNonStringReferenceTypes);

            return SelectNonStrict(dr, type, newObjectFactory, props);
        }


        /// <summary>
        /// Yields an IDataReader as an enumerable. Property names must match column names exactly. 
        /// This is slower than using FastMember but implicit casts are made, and you can specify a type factory.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="type"></param>
        /// <param name="typeFactory">How to make an object of the specified type.</param>
        /// <param name="propNames">Prop names to only include.</param>
        /// <returns></returns>
        public static IEnumerable SelectNonStrict(this IDataReader dr, Type type, Func<object> typeFactory, string[] propNames)
        {
            var props = type.GetColumnInfo(propNames);

            if (type.IsSimpleType())
            {
                while (dr.Read())
                {
                    yield return dr[0].ConvertTo(type);
                }
            }
            else
            {
                try
                {
                    typeFactory();
                }
                catch (Exception e)
                {
                    throw new Exception("Unable to create new instance of desired type.");
                }

                while (dr.Read())
                {
                    var obj = typeFactory();

                    foreach (var prop in props)
                    {
                        var propName = prop.ColumnName;
                        object val;

                        try
                        {
                            val = dr[propName];
                        }
                        catch (Exception e)
                        {
                            throw new Exception($"Source does not contain a value for property '{propName}'");
                        }

                        try
                        {
                            if (val != null && Equals(val, DBNull.Value) == false &&
                                !(val is string && string.IsNullOrEmpty(val as string)))
                                prop.PropertyInfo.SetValue(obj, val.ConvertTo(prop.FieldType), null);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                $"Error fitting {val} into field {propName} ({prop.GetType().Name})");
                        }
                    }

                    yield return obj;
                }
            }
        }


        /// <summary>
        /// Yields an IDataReader as an enumerable, manually specifing how to return a new T from a DataReader. Property names must match column names exactly.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="projection"></param>
        /// <returns></returns>
        public static IEnumerable<T> Select<T>(this IDataReader reader,
            Func<IDataReader, T> projection)
        {
            while (reader.Read())
                yield return projection(reader);
        }
        
        /// <summary>
        /// Select function for data readers. Will apply default convert operations to fit it to a type.
        /// </summary>
        /// <typeparam name="T">Data type.</typeparam>
        /// <param name="dr">Source data reader.</param>
        /// <param name="typeFactory">The default constructor for the type.</param>
        /// <param name="transformGroup">The transform group to apply.</param>
        /// <returns></returns>
        public static IEnumerable<T> Select<T>(this IDataReader dr, Func<T> typeFactory = null, DataTransformGroup transformGroup = null) where T : class
        {
            var d = dr
                .MapToType(typeof(T), transformGroup ?? DataTransformGroups.Default);

            return d.SelectStrict<T>(typeFactory);
        }
        
        //TODO: warning: some duplicated code to above.
        /// <summary>
        /// Strict select function. Yields an IDataReader as an enumerable. Property names must match column names exactly.
        /// This is done using FastMember, and is roughly 20% faster. However, this is much more strict with type casting.
        /// Ie. the C# types should match the SQL type equivalents exactly. 
        /// Even int/byte are not compatable with each other.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="typeFactory">Default factory for new instances of the type.</param>
        /// <returns></returns>
        public static IEnumerable<T> SelectStrict<T>(this IDataReader dr, Func<T> typeFactory = null) where T : class
        {
            var t = typeof(T);

            if (t.IsSimpleType())
            {
                while (dr.Read())
                {
                    var obj = Convert.ChangeType(dr[0], t);
                    yield return (T) obj;
                }
            }
            else
            {
                var accessor = TypeAccessor.Create(t);
                var props = accessor.GetMembers().Select(p => p.Name).ToArray();

                typeFactory ??= () => accessor.CreateNew() as T;

                while (dr.Read())
                {
                    var obj = typeFactory();

                    foreach (var prop in props)
                        if (Equals(dr[prop], DBNull.Value) == false)
                            accessor[obj, prop] = dr[prop];

                    yield return obj;
                }
            }
        }
        
        /// <summary>
        /// Routes the current row count to the Depth property. Good for using on sql queries where Depth does not signify row count.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static IDataReader CountRows<TDataReader>(this TDataReader reader) where TDataReader : IDataReader
        {
            return new RowCountingDataReader<TDataReader>(reader);
        }

        /// <summary>
        /// Executes an action on the datareader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="action"></param>
        public static void Each(this IDataReader reader,
            Action<IDataReader> action)
        {
            while (reader.Read())
                action(reader);
        }
        
        /// <summary>
        /// Executes reader to DataSet.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="dataSetName"></param>
        /// <param name="dataTableNames"></param>
        /// <returns></returns>
        public static DataSet ToDataSet(this IDataReader dr, string dataSetName = "", string[] dataTableNames = null)
        {
            var ds = string.IsNullOrWhiteSpace(dataSetName) ? new DataSet() : new DataSet(dataSetName);

            string GetTableName(int n)
            {
                if ((dataTableNames != null) && (n < dataTableNames.Length)) return dataTableNames[n];
                return null;
            }

            var i = 0;
            ds.Tables.Add(dr.ToDataTable(null, GetTableName(i)));

            while (dr.NextResult())
            {
                i++;
                ds.Tables.Add(dr.ToDataTable(null, GetTableName(i)));
            }
            return ds;
        }

        /// <summary>
        /// Executes reader to DataTable.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="maxRows"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this IDataReader dr, int? maxRows = null, string name = null)
        {
            var hascolumns = false;

            var dt = string.IsNullOrWhiteSpace(name) ? new DataTable() : new DataTable(name);

            if (maxRows.HasValue)
                dr = dr.LimitRows(maxRows.Value);

            void InitializeColumns()
            {
                var fieldInfo = dr.GetFieldInfo();

                var i = 0;
                foreach (var basicDataColumnInfo in fieldInfo)
                {
                    dt.Columns.Add(new DataColumn
                    {
                        ColumnName = string.IsNullOrWhiteSpace(basicDataColumnInfo.ColumnName)
                            ? "Column" + i
                            : basicDataColumnInfo.ColumnName,
                        DataType = (basicDataColumnInfo.FieldType ?? typeof(object)).GetNonNullableType() //get non nullable type
                    });
                    i++;
                }

                hascolumns = true;
            }

            
            while (dr.Read())
            {
                if (hascolumns == false)
                    InitializeColumns();

                var row = dt.NewRow();
                var rowData = new object[row.ItemArray.Length];
                dr.GetValues(rowData);
                row.ItemArray = rowData;
                dt.Rows.Add(row);
            }

            if (hascolumns == false)
                InitializeColumns();

            dr.Dispose();

            return dt;
        }


        /// <summary>
        /// Projects a datareader into a dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TVal"></typeparam>
        /// <param name="dr"></param>
        /// <param name="keySelector"></param>
        /// <param name="valueSelector"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TVal> ToDictionary<TKey, TVal>(this IDataReader dr, Func<IDataReader, TKey> keySelector, Func<IDataReader, TVal> valueSelector)
        {
            var d = new List<Tuple<TKey, TVal>>();

            var key = default(TKey);
            var val = default(TVal);
            try
            {
                while (dr.Read())
                {
                    key = keySelector(dr);
                    val = valueSelector(dr);
                    d.Add(new Tuple<TKey, TVal>(key, val));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error projecting data reader to dictionary.");
            }

            var rtn = new Dictionary<TKey, TVal>(d.Count);
            try
            {
                foreach (var tuple in d) //should be faster than .ToDictionary
                {
                    rtn.Add(tuple.Item1, tuple.Item2);
                }
            }
            catch (Exception)
            {
                throw new Exception($"Error creating lookup table. Items are not one-to-one: [{key},{val}]");
            }
            
            return rtn;
        }

        /// <summary>
        ///     This uses fastmember.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static List<T> ToList<T>(this IDataReader dr) where T : class
        {
            return dr.SelectStrict<T>().ToList();
        }

        /// <summary>
        ///     This uses fastmember.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static T[] ToArray<T>(this IDataReader dr) where T : class
        {
            return dr.SelectStrict<T>().ToArray();
        }

        /// <summary>
        /// Filters an IDataReader.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="rowInclusionCondition"></param>
        /// <returns></returns>
        public static IDataReader Where<TDataReader>(this TDataReader dataReader, RowInclusionCondition rowInclusionCondition) where TDataReader : IDataReader
            //from extended data reader
        {
            return new FilteringDataReader<TDataReader>(dataReader, new[] {rowInclusionCondition});
        }

        /// <summary>
        /// Filters an IDataReader based on multiple conditions (use this instead of chaining multiple where's for better performance).
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="rowInclusionConditions"></param>
        /// <returns></returns>
        public static IDataReader Where<TDataReader>(this TDataReader dataReader,
            params RowInclusionCondition[] rowInclusionConditions) where TDataReader : IDataReader //from extended data reader
        {
            return new FilteringDataReader<TDataReader>(dataReader, rowInclusionConditions);
        }
    
        /// <summary>
        /// Returns an object array of current IDataReader values.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <returns></returns>
        public static object[] GetValues(this IDataReader dataReader)
        {
            var o = new object[dataReader.FieldCount];
            dataReader.GetValues(o);
            return o;
        }

        public static string[] GetFieldNames(this IDataReader reader)
        {
            return GetFieldInfo(reader).Select(f => f.ColumnName).ToArray();
        }

        public static FieldValueInfo[] GetFieldValues<TDataReader>(this SmartDataReader<TDataReader> reader, IEnumerable<int> ordinals) where TDataReader : IDataReader
        {
            return ordinals.Select(i => new FieldValueInfo
            {
                ColumnName = reader.GetName(i),
                ColumnIndex = i,
                Value = reader[i]?.ToString() ?? ""
            }).ToArray();
        }

        /// <summary>
        /// Gets field values as well as information about them.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static FieldValueInfo[] GetFieldValues(this IDataReader dr)
        {
            var fieldInfos = new FieldValueInfo[dr.FieldCount];

            for (var i = 0; i < dr.FieldCount; i++)
                fieldInfos[i] = new FieldValueInfo
                {
                    ColumnName = dr.GetName(i),
                    ColumnIndex = i,
                    Value = dr[i].ToString()
                };

            return fieldInfos;
        }

        #endregion

        /// <summary>
        /// Get .ToString()'ed array of current IDataReader values.
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static string[] GetFieldValueStrings(this IDataReader dr)
        {
            return GetFieldValues(dr).Select(f => f.ToString()).ToArray();
        }
        
        //not really needed
        #region IDataReader read extensions

        ///// <summary>
        /////     Reads a boolean (MS SQL "Bit") value from the specified IDataReader
        ///// </summary>
        ///// <param name="rdr">IDataReader containing the field to read</param>
        ///// <param name="field">Name of the field to read</param>
        ///// <returns>Value of the referenced field or a default of false if the field is null</returns>
        //public static bool ReadBool(this IDataReader rdr, string field)
        //{
        //    var idx = rdr.GetOrdinal(field);
        //    if (Database.IsDbNull(rdr[idx]))
        //        return false;
        //    return (bool)rdr[idx];
        //}

        ///// <summary>
        /////     Reads a DateTime value from the specified IDataReader
        ///// </summary>
        ///// <param name="rdr">IDataReader containing the field to read</param>
        ///// <param name="field">Name of the field to read</param>
        ///// <returns></returns>
        //public static DateTime ReadDateTime(this IDataReader rdr, string field)
        //{
        //    var idx = rdr.GetOrdinal(field);
        //    if (Database.IsDbNull(rdr[idx]))
        //        return DateTime.MinValue;
        //    return (DateTime)rdr[idx];
        //}

        ///// <summary>
        /////     Reads a single precision, floating point value from an IDataReader
        ///// </summary>
        ///// <param name="rdr">IDataReader containing the field to read</param>
        ///// <param name="field">Name of the field to read</param>
        ///// <returns>Value of the referenced field or 0 if the field is null</returns>
        //public static float ReadFloat(this IDataReader rdr, string field)
        //{
        //    var idx = rdr.GetOrdinal(field);
        //    if (Database.IsDbNull(rdr[idx]))
        //        return 0.0f;
        //    return Convert.ToSingle(rdr[idx]);
        //}

        ///// <summary>
        /////     Reads a GUID (UniqueIdentifier MS SQL type) from an IDataReader
        ///// </summary>
        ///// <param name="rdr">IDataReader containing the field to read</param>
        ///// <param name="field">Name of the field to read</param>
        ///// <returns>Value of the referenced field or Guid.Empty if the field is null</returns>
        //public static Guid ReadGuid(this IDataReader rdr, string field)
        //{
        //    var idx = rdr.GetOrdinal(field);
        //    if (Database.IsDbNull(rdr[idx]))
        //        return Guid.Empty;
        //    return (Guid)rdr[idx];
        //}


        ///// <summary>
        /////     Reads an Int32 value from the specified IDataReader. If the value is null, the
        /////     defaultValue is returned
        ///// </summary>
        ///// <param name="rdr">IDataReader containing an integer field</param>
        ///// <param name="field">The name of the field to read</param>
        ///// <param name="defaultValue">Value to return in the case of a NULL value from the IDataReader</param>
        ///// <returns>Int32 value contained in the specified field or defaultValue if the field is null</returns>
        //public static int ReadInt(this IDataReader rdr, string field, int defaultValue)
        //{
        //    var idx = rdr.GetOrdinal(field);
        //    if (Database.IsDbNull(rdr[idx]))
        //        return defaultValue;
        //    return rdr.GetInt32(idx);
        //}

        ///// <summary>
        /////     Reads an int value from the specified IDataReader
        ///// </summary>
        ///// <param name="rdr">IDataReader containing the field to read</param>
        ///// <param name="field">Name of the field to read</param>
        ///// <returns>
        /////     The int value contained in the field. Note: This method will cause an exception if the
        /////     field contains a null value.
        ///// </returns>
        //public static int ReadInt(this IDataReader rdr, string field)
        //{
        //    var idx = rdr.GetOrdinal(field);
        //    return rdr.GetInt32(idx);
        //}

        ///// <summary>
        /////     Reads a long int (MS SQL "BigInt") from the specified data reader
        ///// </summary>
        ///// <param name="rdr">IDataReader containing the field to read</param>
        ///// <param name="field">Name of the field to read</param>
        ///// <returns>Value of the referenced field</returns>
        //public static long ReadInt64(this IDataReader rdr, string field)
        //{
        //    var idx = rdr.GetOrdinal(field);
        //    return rdr.GetInt64(idx);
        //}

        ///// <summary>
        /////     Reads a long int (MS SQL "BigInt") from the specified data reader or returns defaultValue if the
        /////     field is null
        ///// </summary>
        ///// <param name="rdr">IDataReader containing the field to read</param>
        ///// <param name="field">Name of the field to read</param>
        ///// <param name="defaultValue"></param>
        ///// <returns>Int64 value contained in the specified field or defaultValue if the field is null</returns>
        //public static long ReadInt64(this IDataReader rdr, string field, long defaultValue)
        //{
        //    var idx = rdr.GetOrdinal(field);
        //    if (Database.IsDbNull(rdr[idx]))
        //        return defaultValue;
        //    return rdr.GetInt64(idx);
        //}

        ///// <summary>
        /////     Reads a string from the specified IDataReaer
        ///// </summary>
        ///// <param name="rdr">IDataReader containing the field to read</param>
        ///// <param name="field">Name of the field to read</param>
        ///// <returns>Value of the referenced field</returns>
        //public static string ReadString(this IDataReader rdr, string field)
        //{
        //    var idx = rdr.GetOrdinal(field);
        //    return rdr[idx].ToString();
        //}

        #endregion
    }
}