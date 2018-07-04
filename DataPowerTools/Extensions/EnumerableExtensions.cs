using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using DataPowerTools.DataConnectivity.Sql;
using DataPowerTools.DataStructures;
using DataPowerTools.FastMember;
using DataPowerTools.PowerTools;
using Sqlite.Extensions;

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
        /// Bulk upload enumerable using a server/database name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="destinationServer"></param>
        /// <param name="destinationDatabase"></param>
        /// <param name="destinationTable"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        /// <param name="members"></param>
        public static void BulkUploadSqlServer<T>(
            this IEnumerable<T> items,
            string destinationServer,
            string destinationDatabase,
            string destinationTable,
            SqlServerBulkInsertOptions sqlServerBulkInsertOptions = null,
            IEnumerable<string> members = null)
        {
            var cs = Database.GetConnectionString(destinationDatabase, destinationServer);

            BulkUploadSqlServer(items, cs, destinationTable, sqlServerBulkInsertOptions, members);
        }
        
        /// <summary>
        /// Bulk upload enumerable using a connection string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="connectionString"></param>
        /// <param name="destinationTable"></param>
        /// <param name="sqlServerBulkInsertOptions"></param>
        /// <param name="members"></param>
        public static void BulkUploadSqlServer<T>(
            this IEnumerable<T> items,
            string connectionString,
            string destinationTable,
            SqlServerBulkInsertOptions sqlServerBulkInsertOptions = null,
            IEnumerable<string> members = null
        )
        {
            members = members ?? 
                typeof(T).GetProperties().Where(p =>
                        (p.GetGetMethod().IsVirtual == false) && p.GetGetMethod().IsPublic && //TODO: this will have to change if we use dynamic proxies (we use this to skip navigation properties)
                        (p.GetIndexParameters().Length == 0)).Select(p => p.Name).ToArray();
            
            var uploadReader = items.ToDataReader(members?.ToArray());

            uploadReader.BulkInsertSqlServer(connectionString, destinationTable, sqlServerBulkInsertOptions ?? new SqlServerBulkInsertOptions());
        }
        
        /// <summary>
        /// Returns a SortedDictionary of the source dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="existing"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(
            this Dictionary<TKey, TValue> existing)
        {
            return new SortedDictionary<TKey, TValue>(existing);
        }

        /// <summary>
        /// Zips three enumerables together.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="zip1"></param>
        /// <param name="zip2"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> Zip<T, TResult>(this IEnumerable<T> source, IEnumerable<T> zip1, IEnumerable<T> zip2, Func<T, T, T, TResult> func)
        {
            return source.Zip(zip1, (a, b) => new { a, b }).Zip(zip2, (z, z1) => func(z.a, z.b, z1));
        }

        /// <summary>
        /// Adds range to an IList (using foreach).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll"></param>
        /// <param name="enumerable"></param>
        public static void AddRange<T>(this IList<T> coll, IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
                coll.Add(item);
        }

        /// <summary>
        /// Adds range to an observable collection (using foreach).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll"></param>
        /// <param name="enumerable"></param>
        public static void AddRange<T>(this ObservableCollection<T> coll, IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
                coll.Add(item);
        }

        /// <summary>
        /// Adds a range to a HashSet (using foreach).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashSet"></param>
        /// <param name="enumerable"></param>
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
                hashSet.Add(item);
        }

        /// <summary>
        /// Yield an enumerable that is the boolean inverse (!) of the source boolean enumerable.
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
        /// Enumerates an enumerable, returning the current item and an enumerable of the remaining items in the sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Tuple<T, IEnumerable<T>> EnumerateHeadAndTail<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            var en = source.GetEnumerator();
            en.MoveNext();
            return Tuple.Create(en.Current, EnumerateTail(en));
        }

        /// <summary>
        /// Enumerates an enumerator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="en"></param>
        /// <returns></returns>
        public static IEnumerable<T> EnumerateTail<T>(this IEnumerator<T> en)
        {
            while (en.MoveNext()) yield return en.Current;
        }

        /// <summary>
        /// Tries to get list value or returns default(T).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T GetValueOrDefault<T>(this IList<T> list, int index)
        {
            return (index >= 0) && (index < list.Count) ? list[index] : default(T);
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
        public static BidirectionalMap<TLeft, TRight> ToBidirectionalMap<TItem, TLeft, TRight>(
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
        /// Starting at 0, counts up to but not including the specified number. Does not work with negative integers.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static IEnumerable<int> Enumerate(this int val)
        {
            return Enumerable.Range(0, val);
        }

        /// <summary>
        /// Starting at 0, counts up to but not including the specified number. Does not work with negative integers.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="selector">Selector function.</param>
        /// <returns></returns>
        public static IEnumerable<TResult> Enumerate<TResult>(this int val, Func<int, TResult> selector)
        {
            return Enumerable.Range(0, val).Select(selector);
        }

        public static IEnumerable<T> Distinct<T>(
            this IEnumerable<T> source, Func<T, T, bool> comparer)
            where T : class
            => source.Distinct(new DynamicEqualityComparer<T>(comparer));

        private sealed class DynamicEqualityComparer<T> : IEqualityComparer<T>
            where T : class
        {
            private readonly Func<T, T, bool> _func;

            public DynamicEqualityComparer(Func<T, T, bool> func)
            {
                _func = func;
            }

            public bool Equals(T x, T y) => _func(x, y);

            public int GetHashCode(T obj) => 0; // force Equals
        }

        private sealed class DynamicEqualityComparer<TLeft, TRight> : IEqualityComparer<object>
            where TLeft : class where TRight : class
        {
            private readonly Func<TLeft, TRight, bool> _func;

            public DynamicEqualityComparer(Func<TLeft, TRight, bool> func)
            {
                _func = func;
            }
            
            public new bool Equals(object x, object y)
            {
                return _func(x as TLeft, y as TRight);
            }

            public int GetHashCode(object obj) => 0; // force Equals
        }


        /// <summary>
        /// Starting at 0, counts up to but not including the specified number. Does not work with negative integers.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static IEnumerable<short> Count(this short val)
        {
            for (short i = 0; i < val; i++)
            {
                yield return i;
            }
        }

        /// <summary>
        /// Calculates the standard deviation of a set of doubles.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double StdDev(this IEnumerable<double> values)
        {
            // ref: http://warrenseen.com/blog/2006/03/13/how-to-calculate-standard-deviation/
            var mean = 0.0;
            var sum = 0.0;
            var stdDev = 0.0;
            var n = 0;
            foreach (var val in values)
            {
                n++;
                var delta = val - mean;
                mean += delta / n;
                sum += delta * (val - mean);
            }
            if (1 < n)
                stdDev = Math.Sqrt(sum / (n - 1));

            return stdDev;
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
        
        /// <summary>
        /// EXPERIMENTAL. Does a merge action. Use with care and test thoroughly.
        /// </summary>
        /// <typeparam name="TLeft"></typeparam>
        /// <typeparam name="TRight"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="matchPredicate"></param>
        /// <param name="matchedBySourceAction"></param>
        /// <param name="notMatchedByTargetAction"></param>
        /// <param name="notMatchedBySourceAction"></param>
        public static void Merge<TLeft, TRight>(this IEnumerable<TLeft> source, IEnumerable<TRight> target,
            Func<TLeft, TRight, bool> matchPredicate, Action<TLeft, TRight> matchedBySourceAction,
            Action<TLeft> notMatchedByTargetAction, Action<TRight> notMatchedBySourceAction = null) where TLeft : class where TRight : class
        { 
            if(target == null)
                throw new Exception("Error on merge operation - right side cannot be null.");

            var rightSet = target as TRight[] ?? target.ToArray();
            var leftSet = source as TLeft[] ?? source.ToArray();

            var equalityComparer = new DynamicEqualityComparer<TLeft, TRight>(matchPredicate);

            if (matchedBySourceAction != null)
            {
                var joinedSet = leftSet.Join(rightSet, l => l, r => r, (l, r) => new {l, r}, equalityComparer);
                foreach (var x in joinedSet)
                {
                    matchedBySourceAction(x.l, x.r);
                }
            }

            if (notMatchedByTargetAction != null)
            {
                var r1 = leftSet.Except(rightSet, equalityComparer);

                foreach (TLeft r in r1)
                {
                    notMatchedByTargetAction(r);
                }
            }

            if (notMatchedBySourceAction != null)
            {
                var r2 = rightSet.Except(leftSet, equalityComparer);

                foreach (TRight r in r2)
                {
                    notMatchedBySourceAction(r);
                }
            }
        }
        
        /// <summary>
        /// Returns an enumerable that is a subset of an enumerable using the specified start and end index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static IEnumerable<T> GetRange<T>(this IEnumerable<T> source, int start, int end)
        {
            using (var e = source.GetEnumerator())
            {
                var i = 0;
                while ((i < start) && e.MoveNext()) i++;
                while ((i < end) && e.MoveNext())
                {
                    yield return e.Current;
                    i++;
                }
            }
        }

        [DebuggerHidden]
        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            var list = enumerable as IList<T>;
            return list != null ? new ReadOnlyCollection<T>(list) : new ReadOnlyCollection<T>(enumerable.ToArray());
        }

        [DebuggerHidden]
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> comparer = null)
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
        public static string JoinStr<T>(this IEnumerable<T> enumerable, string separator = ", ") //TODO: rename to join string
        {
            return string.Join(separator, enumerable);
        }

        [DebuggerHidden]
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }

        /// <summary>
        ///     For iterator extension method that also includes a bool with the 'isLastItem' value.
        /// </summary>
        /// <example>
        ///     new[] { 1, 2, 3 }.For((item, i, isLastItem) => { });
        /// </example>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">Items to iterate</param>
        /// <param name="action">generic action with T1=Item, T2=i</param>
        [DebuggerHidden]
        public static void For<T>(this IEnumerable<T> items, Action<T, int, bool> action)
        {
            if (items == null) return;

            var enumerator = items.GetEnumerator();

            using (enumerator)
            {
                var i = 0;
                var hasMore = enumerator.MoveNext();

                if (hasMore == false)
                    return;
                
                while (true)
                {
                    var t = enumerator.Current;

                    var hasNext = enumerator.MoveNext();

                    action(t, i, !hasNext);

                    if (hasNext == false)
                        return;

                    i++;
                }
            }
        }

        /// <summary>
        ///     For iterator extension method that also includes a bool with the 'isLastItem' value.
        /// </summary>
        /// <example>
        ///     new[] { 1, 2, 3 }.For((item, i, isLastItem) => { });
        /// </example>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">Items to iterate</param>
        /// <param name="action">generic action with T1=Item, T2=i</param>
        [DebuggerHidden]
        public static void For<T>(this IEnumerable<T> items, Action<T, int> action)
        {
            if (items == null) return;

            var i = 0;
            foreach (var item in items)
            {
                action(item, i);
                i++;
            }
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
        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, params T[] tParams)
        {
            return enumerable.Except(tParams as IEnumerable<T>);
        }


        /// <summary>
        /// Unions an enumerable using params.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="tParams"></param>
        /// <returns></returns>
        public static IEnumerable<T> Union<T>(this IEnumerable<T> enumerable, params T[] tParams)
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
        
        [DebuggerHidden]
        public static void Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items != null)
                foreach (var item in items)
                    action(item);
        }


        /// <summary>
        /// Fits the data in the enumerable to a create table statement.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="outputTableName"></param>
        /// <param name="numberOfRowsToExamine"></param>
        /// <param name="ignoreNonStringReferenceTypes"></param>
        /// <returns></returns>
        public static string FitToCreateTableStatement<T>(this IEnumerable<T> enumerable, string outputTableName, 
            int? numberOfRowsToExamine = null, 
            bool ignoreNonStringReferenceTypes = true)
        {
            var r = CreateTableSql.FromDataReader_Smart(outputTableName, enumerable.ToDataReader(ignoreNonStringReferenceTypes),
                numberOfRowsToExamine);

            return r;
        }




    }
}