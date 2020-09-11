using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataPowerTools.DataStructures;
using DataPowerTools.Extensions;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public class ColumnTransformingDataReader<TResult, TDataReader> : ExtensibleDataReaderExplicit<TDataReader> where TDataReader : IDataReader
    {
        //these need to be lazy because of the way data readers work (field names may not be available until first read)
        private readonly Lazy<Dictionary<string, RowProjection<TResult>>> _columnActions;
        private readonly Lazy<Dictionary<int, RowProjection<TResult>>> _columnActionsByOrdinal;
        private Lazy<BidirectionalMap<string, int>> _nameToOrdinalMapping;
        
        public ColumnTransformingDataReader(TDataReader reader, Dictionary<string, RowProjection<TResult>> columnActions = null)
            : base(reader)
        {
            //by string name
            _columnActions = new Lazy<Dictionary<string, RowProjection<TResult>>>(() => columnActions ?? new Dictionary<string, RowProjection<TResult>>()); 

            GetMappings();

            _columnActionsByOrdinal = new Lazy<Dictionary<int, RowProjection<TResult>>>(() =>
            {
                return _columnActions.Value.ToDictionary(action => _nameToOrdinalMapping.Value[action.Key], pair => pair.Value);
            }); 
        }

        public ColumnTransformingDataReader(TDataReader reader,
            Dictionary<int, RowProjection<TResult>> columnActionsByOrdinal = null) : base(reader)
        {
            //by ordinals
            _columnActionsByOrdinal = new Lazy<Dictionary<int, RowProjection<TResult>>>(() => columnActionsByOrdinal ?? new Dictionary<int, RowProjection<TResult>>());  

            GetMappings();

            _columnActions = new Lazy<Dictionary<string, RowProjection<TResult>>>(() =>
            {
                return _columnActionsByOrdinal.Value.ToDictionary(action => _nameToOrdinalMapping.Value[action.Key], pair => pair.Value);
            }); 
        }

        public override object this[int index] //by field index
            => GetValue(index);

        public override object this[string fieldname] //by field name
            => _columnActions.Value.ContainsKey(fieldname)
                ? _columnActions.Value[fieldname](DataReader) //transform
                : DataReader[fieldname];

        public override int FieldCount => DataReader.FieldCount;

        public override int Depth => DataReader.Depth;
        public override bool IsClosed => DataReader.IsClosed;
        public override int RecordsAffected => DataReader.RecordsAffected;

        private void GetMappings()
        {
            _nameToOrdinalMapping = new Lazy<BidirectionalMap<string, int>>(() =>
            {
                return DataReader.GetFieldNames()
                    .Select((colName, i) => new
                    {
                        ColName = colName,
                        Index = i
                    })
                    .ToBidirectionalMap(p => p.ColName, p => p.Index);
            }); 
        }

        public override object GetValue(int i) //by index
        {
            return _columnActionsByOrdinal.Value.ContainsKey(i)
                ? _columnActionsByOrdinal.Value[i](DataReader) //transform
                : DataReader[i];
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

        public override void Dispose()
        {
            DataReader.Dispose();
        }

        public override string GetName(int i)
        {
            return DataReader.GetName(i);
        }

        public override int GetOrdinal(string name)
        {
            return DataReader.GetOrdinal(name);
        }

        public override void Close() => DataReader.Close();


        public override bool Read() => DataReader.Read();

        public override bool IsDBNull(int i) =>
            (GetValue(i) == null) || Convert.IsDBNull(GetValue(i));
    }
}