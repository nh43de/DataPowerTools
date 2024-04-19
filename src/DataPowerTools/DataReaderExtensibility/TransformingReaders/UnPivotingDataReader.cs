using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.DataStructures;
using DataPowerTools.Extensions;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    /// This data reader applies a new set of names (Aliases) to the IDataReader[string] accessor.
    /// </summary>
    /// <typeparam name="TDataReader"></typeparam>
    public class UnPivotingDataReader<TDataReader> : ExtensibleDataReaderExplicit<TDataReader> where TDataReader : IDataReader
    {
        private readonly int _leftDimensionColumns;
        //these need to be lazy because of the way data readers work (field names may not be available until first read)

        private Lazy<BasicDataColumnInfo[]> _fieldInfo;

        private int _index = 0; //the current pivot value index (starts at 1, range between 1 and field count - 1). Starts at zero and if zero then will do the initial read.

        private readonly int _fieldCount;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="leftDimensionColumns">Number of left dimensions</param>
        public UnPivotingDataReader(TDataReader reader, int leftDimensionColumns = 1) : base(reader)
        {
            _leftDimensionColumns = leftDimensionColumns;
            _fieldInfo = new Lazy<BasicDataColumnInfo[]>(() => DataReader.GetFieldInfo());
            _fieldCount = 2 + _leftDimensionColumns;
        }

        public override object this[int index] //by field index
            => GetValue(index);

        public override object this[string fieldname] //by field name
            => GetValue(GetOrdinal(fieldname));

        public override int FieldCount => _fieldCount;

        public override int Depth => DataReader.Depth * _fieldCount;

        public override bool IsClosed => DataReader.IsClosed;

        public override int RecordsAffected => DataReader.RecordsAffected;


        public override object GetValue(int i)
        {
            if (i < _leftDimensionColumns)
            {
                return DataReader.GetValue(i);
            }
            
            return (i - _leftDimensionColumns + 1) switch
            {
                //dimension B - from header
                1 => _fieldInfo.Value[_index].ColumnName,
                //use _index
                2 => DataReader.GetValue(_index),
                _ => throw new ArgumentOutOfRangeException(nameof(i), "UnPivoting data reader only has 3 columns")
            };
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
            if (i < _leftDimensionColumns + 1)
            {
                return $"Dimension{i + 1}";
            }

            return (i - _leftDimensionColumns + 1) switch
            {
                //use _index
                2 => "Value",
                _ => throw new ArgumentOutOfRangeException(nameof(i), "UnPivoting data reader only has 3 columns")
            };
        }

        public override int GetOrdinal(string name)
        {
            throw new NotImplementedException("Unpivoting data reader doesn't support getting by col name.");

            //return name switch
            //{
            //    //dimension A - from data reader first col
            //    "DimensionA" => 0,
            //    //dimension B - from header
            //    "DimensionB" => 1,
            //    //use _index
            //    "Value" => 2,
            //    _ => throw new ArgumentOutOfRangeException(nameof(name), "UnPivoting data reader only has 3 columns")
            //};
        }

        public override void Close() => DataReader.Close();

        public override bool Read()
        {
            if (_index == 0)
            {
                _index = _leftDimensionColumns;
                return DataReader.Read();
            }

            var fieldCount = _fieldInfo.Value.Length - 1;

            var nextIndex = _index + 1;

            if (nextIndex <= fieldCount)
            {
                _index = nextIndex;
                return true;
            }

            var r = DataReader.Read();
            _index = _leftDimensionColumns;

            return r;
        }

        public override bool IsDBNull(int i) =>
            (GetValue(i) == null) || Convert.IsDBNull(GetValue(i));
    }
}
