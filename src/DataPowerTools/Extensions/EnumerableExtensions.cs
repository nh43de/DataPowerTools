using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using DataPowerTools.DataStructures;
using DataPowerTools.FastMember;
using DataPowerTools.PowerTools;

namespace DataPowerTools.Extensions
{
    public static class EnumerableExtensions
    {
        //TODO: implement bulk upload for mysql
        ///// <summary>
        ///// Bulk upload enumerable using a server/database name.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="items"></param>
        ///// <param name="destinationServer"></param>
        ///// <param name="destinationDatabase"></param>
        ///// <param name="destinationTable"></param>
        ///// <param name="bulkInsertOptions"></param>
        ///// <param name="members"></param>
        //public static void BulkUploadMySql<T>(
        //    this IEnumerable<T> items,
        //    string destinationServer,
        //    string destinationDatabase,
        //    string destinationTable,
        //    BulkInsertOptions bulkInsertOptions = null,
        //    IEnumerable<string> members = null)
        //{
        //    var cs = Database.GetConnectionString(destinationDatabase, destinationServer);

        //    BulkUploadSqlServer(items, cs, destinationTable, bulkInsertOptions, members);
        //}


        /// <summary>
        /// Returns the object specified as an enumerable that returns itself.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsSingleEnumerable<T>(this T obj)
        {
            return new[] { obj };
        }

        /// <summary>
        /// Enumerates an IEnumerable a specified number of times.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="repeatCount"></param>
        /// <returns></returns>
        public static IEnumerable<T> Repeat<T>(this IEnumerable<T> items, int repeatCount)
        {
            for (var i = 0; i < repeatCount; i++)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                foreach (var a in items) //we are definitely going to enumerate this several times.
                {
                    yield return a;
                }
            }
        }

        /// <summary>
        /// Yield an enumerable that is the boolean inverse (!) of the source boolean enumerable. E.g. [1011] => [0100]
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static IEnumerable<bool> Invert(this IEnumerable<bool> enumerable)
        {
            using (var e = enumerable.GetEnumerator())
            {
                while (e.MoveNext())
                    yield return !e.Current;
            }
        }

        /// <summary>
        /// Returns a one-to-one dictionary of the specified items.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="items"></param>
        /// <param name="leftKeyFunc"></param>
        /// <param name="rightKeyFunc"></param>
        /// <returns></returns>
        internal static BidirectionalMap<TLeft, TRight> ToBidirectionalMap<TItem, TLeft, TRight>(
            this IEnumerable<TItem> items,
            Func<TItem, TLeft> leftKeyFunc, Func<TItem, TRight> rightKeyFunc)
        {
            var a = items.Select(item => new Tuple<TLeft, TRight>(leftKeyFunc(item), rightKeyFunc(item))).ToArray();

            return new BidirectionalMap<TLeft, TRight>(a);
        }
        
        /// <summary>
        /// Returns a datareader from a 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="fieldNames">Field names to include, in that order. All field included by default.</param>
        /// <returns></returns>
        public static IDataReader ToDataReader<T>(this IEnumerable<T> enumerable, string[] fieldNames)
        {
            return ObjectReader.Create(enumerable, fieldNames);
        }

        /// <summary>
        /// Returns a datareader from a 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <returns></returns>
        public static IDataReader ToDataReader<T>(this IEnumerable<T> enumerable, bool ignoreNonStringReferenceTypes = true)
        {
            return ObjectReader.Create(enumerable, ignoreNonStringReferenceTypes);
        }

        /// <summary>
        /// Returns an empty DataTable that has the columns names corresponding to the properties of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="useColumnAttribute"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <returns></returns>
        public static DataTable ToEmptyDataTable<T>(this IEnumerable<T> enumerable, bool useColumnAttribute = false, bool ignoreNonStringReferenceTypes = true)
        {
            return ToDataTable(enumerable, useColumnAttribute, 0, ignoreNonStringReferenceTypes);
        }
       
        /// <summary>
        /// Returns a DataTable from a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="useColumnAttribute">Use data annotations to name columns.</param>
        /// <param name="maxRows"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> enumerable, bool useColumnAttribute = false, int? maxRows = null,
            bool ignoreNonStringReferenceTypes = true, string tableName = null)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            var objectReader = ObjectReader.Create(enumerable, ignoreNonStringReferenceTypes);
            var reader = (IDataReader) objectReader;

            var dtReturn = string.IsNullOrWhiteSpace(tableName) ? new DataTable() : new DataTable(tableName);

            if (enumerable == null)
                return dtReturn;

            if (maxRows != null)
                reader = reader.LimitRows(maxRows.Value);
            
            var colsInitialized = false;

            void InitializeDtColumns(Type t)
            {
                var props = t.GetColumnInfo(ignoreNonStringReferenceTypes);
                
                foreach (var columnDisplayInformation in props)
                {
                    //If useColumnAttribute is on, then use column attribute as the column name. Otherwise, use property name.
                    //When using column attribute, if the column attribute is not found, still default to property name.
                    dtReturn.Columns.Add(useColumnAttribute
                        ? columnDisplayInformation.DisplayName
                        : columnDisplayInformation.ColumnName, columnDisplayInformation.FieldType.GetNonNullableType()); //non nullable type for data tables
                }

                colsInitialized = true;
            }
            
            while (reader.Read())
            {
                if(colsInitialized == false)
                    InitializeDtColumns(objectReader.SourceType);
                
                var dr = dtReturn.NewRow();

                var rowData = new object[dr.ItemArray.Length];

                reader.GetValues(rowData);

                dr.ItemArray = rowData;

                dtReturn.Rows.Add(dr);
            }
            
            return dtReturn;
        }
        
        [DebuggerHidden]
        internal static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> comparer = null)
        {
            return comparer == null ? new HashSet<T>(enumerable) : new HashSet<T>(enumerable, comparer);
        }

        /// <summary>
        /// Shorthand for string.Join(separator, enumerable)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        internal static string JoinStr<T>(this IEnumerable<T> enumerable, string separator = ", ") //TODO: rename to join string
        {
            return string.Join(separator, enumerable);
        }

        [DebuggerHidden]
        internal static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }
        
        ///// <summary>
        ///// Gets the column names of an enumerable. (Enumerates the property names of T)
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="enumerable"></param>
        ///// <param name="ignoreNonStringReferenceTypes"></param>
        ///// <returns></returns>
        //public static string[] GetColumnNames<T>(this IEnumerable<T> enumerable, bool ignoreNonStringReferenceTypes = true)
        //{
        //    var props = enumerable.GetColumnInfo(ignoreNonStringReferenceTypes).Select(i => i.ColumnName).ToArray();

        //    return props;
        //}

        /// <summary>
        /// Gets the type associated with an enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static Type GetEnumerableType<T>(this IEnumerable<T> enumerable)
        {
            var t = enumerable?.GetType().GetInterface(typeof(IEnumerable<>).Name).GetGenericArguments()[0] ?? typeof(T);

            return t;
        }

        /// <summary>
        /// Excepts an enumerable using params.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="tParams"></param>
        /// <returns></returns>
        public static IEnumerable<T> ExceptParams<T>(this IEnumerable<T> enumerable, params T[] tParams)
        {
            return enumerable.Except(tParams as IEnumerable<T>);
        }
        
        /// <summary>
        /// Unions an enumerable sequence using params.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="tParams"></param>
        /// <returns></returns>
        public static IEnumerable<T> UnionParams<T>(this IEnumerable<T> enumerable, params T[] tParams)
        {
            return enumerable.Union(tParams as IEnumerable<T>);
        }

        //public static IEnumerable<object[]> ToRowObjectArray<T>(this IEnumerable<T> items)
        //{
        //    foreach (var item in items)
        //    {
        //        var oa = ObjectAccessor.Create(item);
        //        yield return
        //    }
        //}

        /// <summary>
        /// Writes an enumerable to CSV.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="outputFile"></param>
        /// <param name="fieldNames">Field names to include, in that order. By default all are included.</param>
        public static void WriteCsv<T>(this IEnumerable<T> enumerable, string outputFile, string[] fieldNames = null)
        {
            enumerable.ToDataReader(fieldNames).WriteCsv(outputFile);
        }

        /// <summary>
        /// Writes an enumerable to CSV.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="fieldNames">Field names to include, in that order. By default all are included.</param>
        public static string ToCsvString<T>(this IEnumerable<T> enumerable, string[] fieldNames = null)
        {
            var dr = enumerable.ToDataReader(fieldNames);

            using (dr)
            {
                return dr.AsCsv();
            }
        }
        
        /// <summary>
        /// Fits the data in the enumerable to a create table statement.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="outputTableName"></param>
        /// <param name="numberOfRowsToExamine"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <returns></returns>
        public static string FitToCreateTableSql<T>(this IEnumerable<T> enumerable, string outputTableName, 
            int? numberOfRowsToExamine = null, 
            bool ignoreNonStringReferenceTypes = true)
        {
            var r = CreateTableSql.FromDataReader_Smart(outputTableName, enumerable.ToDataReader(ignoreNonStringReferenceTypes),
                numberOfRowsToExamine);

            return r;
        }


    }
}