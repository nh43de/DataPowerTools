using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.DataStructures;
using DataPowerTools.Extensions;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public class AliasingDataReader<TDataReader> : ExtensibleDataReaderExplicit<TDataReader> where TDataReader : IDataReader
    {
        //these need to be lazy because of the way data readers work (field names may not be available until first read)
        
        private Lazy<BasicDataColumnInfo[]> _fieldInfo;

        private Lazy<BidirectionalMap<string, int>> _nameToOrdinalMapping;
        private Lazy<Dictionary<string, string>> _allColumnAliases;


        public AliasingDataReader(TDataReader reader, Dictionary<string, string> columnAliases)
            : base(reader)
        {
            _fieldInfo = new Lazy<BasicDataColumnInfo[]>(() => DataReader.GetFieldInfo());

            _allColumnAliases = new Lazy<Dictionary<string, string>>(() =>
            {
                var r = _fieldInfo.Value
                    .ToDictionary(fi => columnAliases.ContainsKey(fi.ColumnName) ? columnAliases[fi.ColumnName] : fi.ColumnName, fi => fi.ColumnName);

                return r;
            });

            _nameToOrdinalMapping = new Lazy<BidirectionalMap<string, int>>(() =>
                _fieldInfo.Value.ToBidirectionalMap(p => columnAliases.ContainsKey(p.ColumnName) ? columnAliases[p.ColumnName] : p.ColumnName, p => p.Ordinal)
            );
        }
        
        public override object this[int index] //by field index
            => GetValue(index);

        public override object this[string fieldname] //by field name
            => DataReader[_allColumnAliases.Value[fieldname]];
        
        public override int FieldCount => DataReader.FieldCount;

        public override int Depth => DataReader.Depth;

        public override bool IsClosed => DataReader.IsClosed;

        public override int RecordsAffected => DataReader.RecordsAffected;
        

        public override object GetValue(int i) => DataReader[i];
        
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

        public override void Dispose()
        {
            DataReader.Dispose();
        }

        public override string GetName(int i)
        {
            return _nameToOrdinalMapping.Value.GetLeft(i);
        }

        public override int GetOrdinal(string name)
        {
            return _nameToOrdinalMapping.Value.GetRight(name);
        }

        public override void Close() => DataReader.Close();


        public override bool Read() => DataReader.Read();

        public override bool IsDBNull(int i) =>
            (GetValue(i) == null) || Convert.IsDBNull(GetValue(i));
    }
}
