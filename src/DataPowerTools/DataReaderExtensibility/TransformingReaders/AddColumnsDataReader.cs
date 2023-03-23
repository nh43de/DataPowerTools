using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    /// Data reader that will add columns to an existing data reader.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TDataReader"></typeparam>
    public class AddColumnsDataReader<T, TDataReader> : ExtensibleDataReaderExplicit<TDataReader> where TDataReader : IDataReader
    {
        private readonly Lazy<Dictionary<string, int>> _addColumnIndexByName;

        private readonly Lazy<Dictionary<string, AddColumn<T>>> _addColumnsByNameLazy;
        private readonly int _baseFieldCount;

        public AddColumnsDataReader(
            TDataReader dataReader,
            AddColumn<T>[] addColumns
        ) : base(dataReader)
        {
            //TODO: validate that add columns do not clash with existing column names (use this.DataReader.GetOrdinal(name) - either wrap in try catch or check index > -1 or both)

            AddColumns = addColumns;
            _baseFieldCount = dataReader.FieldCount;
            _addColumnsByNameLazy = new Lazy<Dictionary<string, AddColumn<T>>>(() =>
            {
                return AddColumns
                    .ToDictionary(a => a.ColumnName, a => a);
            });
            _addColumnIndexByName = new Lazy<Dictionary<string, int>>(() =>
            {
                return AddColumns.Select((col, i) => new {col, i})
                    .ToDictionary(a => a.col.ColumnName, a => a.i);
            });
        }

        private AddColumn<T>[] AddColumns { get; }

        public override int FieldCount => _baseFieldCount + AddColumns.Length;

        public override object this[int i] => i > _baseFieldCount - 1 ? GetAddColumnValue(i) : DataReader[i];

        public override int Depth => DataReader.Depth;

        public override object this[string name]
        {
            get
            {
                if (_addColumnsByNameLazy.Value.ContainsKey(name))
                    return _addColumnsByNameLazy.Value[name].ValueFactory(DataReader);

                return DataReader[name];
            }
        }

        public override bool Read()
        {
            return DataReader.Read();
        }

        //TODO: not supported yet
        public override DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        public override object GetValue(int i)
            => i > _baseFieldCount - 1 ? GetAddColumnValue(i) : DataReader.GetValue(i);

        public override string GetName(int i)
            => i > _baseFieldCount - 1 ? GetAddColumn(i).ColumnName : DataReader.GetName(i);

        public override Type GetFieldType(int i)
            => i > _baseFieldCount - 1 ? GetAddColumn(i).GetColumnType() : DataReader.GetFieldType(i);

        public override bool IsDBNull(int i)
            =>
            i > _baseFieldCount - 1
                ? (GetAddColumnValue(i) == null) || Convert.IsDBNull(GetAddColumnValue(i))
                : DataReader.IsDBNull(i);

        public AddColumn<T> GetAddColumn(int srcIndex)
        {
            return AddColumns[srcIndex - _baseFieldCount];
        }

        public object GetAddColumnValue(int srcIndex)
        {
            return AddColumns[srcIndex - _baseFieldCount].ValueFactory(DataReader);
        }

        public override int GetOrdinal(string name)
        {
            if (_addColumnIndexByName.Value.ContainsKey(name))
                return _addColumnIndexByName.Value[name] + _baseFieldCount;

            return DataReader.GetOrdinal(name);
        }

        public override int GetValues(object[] values)
        {
            var i = 0;
            for (; i < FieldCount; i++)
            {
                if (values.Length <= i)
                    return i;
                values[i] = GetValue(i);
            }
            return i;
        }

        //TODO: remove from this class
        public class AddColumn<TResult>
        {
            public string ColumnName { get; set; }
            public RowProjection<TResult> ValueFactory { get; set; }

            public Type GetColumnType()
            {
                return typeof(TResult);
            }

            public AddColumn()
            {
                
            }

            public AddColumn(string columnName, RowProjection<TResult> valueFactory)
            {
                ColumnName = columnName;
                ValueFactory = valueFactory;
            }
        }
    }
}