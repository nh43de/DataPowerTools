using System;
using System.Data;
using System.Linq;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.Extensions;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    ///     A smart data reader extends an IDataReader. It adapts reader output to source data, transforming it based on
    ///     destination data type.
    /// </summary>
    public class SmartDataReader<TDataReader> : ExtensibleDataReader<TDataReader> where TDataReader : IDataReader
    {
        /// <summary>
        ///     Value: ( Source ordinal ) -> ( TransformOrdinal )
        /// </summary>
        private readonly Lazy<ColumnMappingInfo> _mappingInfoLazy;

        public readonly DataTransform[] DataTransformsInDestinationOrder;


        public SmartDataReader(TDataReader dataReader, TypedDataColumnInfo[] destinationColumns,
            DataTransformGroup dataTransformGroup = null) : base(dataReader)
        {
            dataTransformGroup = dataTransformGroup ?? DataTransformGroups.Default;

            DataTransformsInDestinationOrder =
                destinationColumns.Select(destCol => dataTransformGroup(destCol.DataType)).ToArray();

            _mappingInfoLazy = new Lazy<ColumnMappingInfo>(() => dataReader.GetColumnMappings(destinationColumns));
        }

        /// <summary>
        ///     This is the mapping information between source and target.
        /// </summary>
        public ColumnMappingInfo ColumnMappingInfo => _mappingInfoLazy.Value;

        #region SmartTransform Functionality

        public override object this[int i] => TransformObject(DataReader[i], i);

        public override object this[string name] => TransformObject(DataReader[name], name);

        public override object GetValue(int i)
        {
            return TransformObject(DataReader.GetValue(i), i);
        }


        private object TransformObject(object o, string name)
        {
            if (_mappingInfoLazy.Value != null)
            {
                var transformOrdinal = _mappingInfoLazy.Value.SourceColumnNameToDestinationOrdinal[name];
                return transformOrdinal == null
                    ? o
                    : DataTransformsInDestinationOrder[transformOrdinal.Value](o);
            }

            return o;
        }

        private object TransformObject(object o, int sourceIndex)
        {
            if (_mappingInfoLazy.Value != null)
            {
                var transformIndex = _mappingInfoLazy.Value.SourceOrdinalToDestinationOrdinal[sourceIndex];

                return transformIndex == null
                    ? o
                    : DataTransformsInDestinationOrder[transformIndex.Value](o);
            }

            return o;
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

            //var rtn = DataReader.GetValues(values);

            //if (_mappingInfoLazy.LoadSuccess)
            //{
            //    var mappingsInfo = _mappingInfoLazy.Value;

            //    for (var i = 0; i < mappingsInfo.SourceOrdinalDestinationIsString.Length; i++)
            //    {
            //        if (mappingsInfo.SourceOrdinalDestinationIsString[i]) continue;

            //        var destinationIndex = mappingsInfo.SourceOrdinalToDestinationOrdinal[i];

            //        if (destinationIndex == null)
            //            continue;

            //        //if its a non string destination column do a transform
            //        values[i] = DataTransforms[destinationIndex.Value](values[i]);
            //    }
            //}
            //else
            //{
            //    throw new Exception("Failed to load mappings");
            //}

            //return rtn;
        }

        private bool _foundColumnHeaders = false;

        //public override bool Read()
        //{
        //    if (!_foundColumnHeaders)
        //    {
        //        var allAreCols =
        //            _mappingInfoLazy.Value.SourceColumns
        //                .All(
        //                    column =>
        //                        column.ColumnName.ToUpper().Contains("COLUMN") || column.ColumnName == "." ||
        //                        string.IsNullOrWhiteSpace(column.ColumnName));

        //        _foundColumnHeaders = true;
        //    }

        //    while (base.Read())
        //    {
        //        for (var i = 0; i < base.FieldCount; i++)
        //        {
        //            if (string.IsNullOrWhiteSpace(this[i].ToString()) == false
        //                && this[i] != null)
        //                table.Columns[i].ColumnName = cellsValues[i].ToString();
        //            else
        //                table.Columns[i].ColumnName = string.Concat("Column", i);
        //        }

        //        table.Rows.RemoveAt(0);
        //    }
        //    return false;
        //}

        #endregion
    }
}