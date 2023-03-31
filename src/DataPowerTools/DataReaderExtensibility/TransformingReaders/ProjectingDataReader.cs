using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataPowerTools.DataStructures;
using DataPowerTools.Extensions;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    /// This overwrites all columns into a projected sequence. This is basically a .Select or .SelectRows
    /// </summary>
    /// <typeparam name="TDataReader"></typeparam>
    public class ProjectingDataReader<TDataReader> : ExtensibleDataReaderExplicit<TDataReader> where TDataReader : IDataReader
    {
        private readonly Dictionary<string, RowProjection<object>> _columnsByName;
        private readonly RowProjection<object>[] _columns;
        private readonly BidirectionalMap<string, int> _nameToOrdinalMapping;


        private readonly int _fieldCount;

        /// <summary>
        /// Creates a data reader that reads from the underlying stream and returns only the columns specified.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="selectColumns">The columns to select.</param>
        public ProjectingDataReader(TDataReader reader, IEnumerable<string> selectColumns)
            : this(reader, selectColumns
                .ToDictionary(name => name, name => new RowProjection<object>(row => row[name])))
        {

        }

        /// <summary>
        /// Creates a data reader that reads from the underlying stream and returns only the columns specified in the transform.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="resultColumnsByColumnName"></param>
        public ProjectingDataReader(TDataReader reader, Dictionary<string, RowProjection<object>> resultColumnsByColumnName)
            : base(reader)
        {
            _columnsByName = resultColumnsByColumnName;
            //
            _columns = resultColumnsByColumnName.Select(n => n.Value).ToArray();
            _fieldCount = _columns.Length;
            //
            _nameToOrdinalMapping = resultColumnsByColumnName.Select((o, i) => new {o, i}).ToBidirectionalMap(p => p.o.Key, p => p.i);
        }

        public ProjectingDataReader(TDataReader dataReader, RowProjection<object>[] resultColumns) : base(dataReader)
        {
            _columns = resultColumns;
            //
            _columnsByName =
                _columns.Select((i, o) => new {Index = i, Projection = o})
                    .ToDictionary(p => "COLUMN" + p.Index, p => p.Index);
            _fieldCount = _columns.Length;
            //
            _nameToOrdinalMapping = _columnsByName.Select((o, i) => new { o, i }).ToBidirectionalMap(p => p.o.Key, p => p.i);
        }

        public override int FieldCount => _fieldCount;

        public override object this[int i] => _columns[i](DataReader);

        public override int Depth => DataReader.Depth;

        public override object this[string name] => _columnsByName[name](DataReader);

        public override int GetOrdinal(string name) => _nameToOrdinalMapping.GetRight(name);

        public override bool Read()
        {
            return DataReader.Read();
        }

        //TODO: not supported yet
        public override DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        public override object GetValue(int i) => _columns[i](DataReader);

        public override string GetName(int i) => _nameToOrdinalMapping.GetLeft(i);

        public override Type GetFieldType(int i) => typeof(object);

        public override bool IsDBNull(int i) => Convert.IsDBNull(GetValue(i));
        
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
    }
}