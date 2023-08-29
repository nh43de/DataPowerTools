using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using DataPowerTools.DataReaderExtensibility.Columns;
using DataPowerTools.Extensions;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    ///     A smart data reader extends an IDataReader. It adapts reader output to source data, transforming it based on
    ///     destination data type. Available fields are ones present in the destination.
    /// </summary>
    public class SmartDataReader<TDataReader> : ExtensibleDataReaderBase<TDataReader>, IDiagnosticDataReader where TDataReader : IDataReader
    {
        public override int FieldCount => ColumnMappingInfo.DestinationColumns.Length;

        public override string GetName(int i)
        {
            var childOrdinal = GetChildOrdinal(i);

            var name = DataReader.GetName(childOrdinal);

            return name;
        }

        public override int GetOrdinal(string name)
        {
            var r = ColumnMappingInfo.SourceColumns.FirstOrDefault(p => p.ColumnName == name)?.Ordinal;

            if (r == null)
                throw new Exception($"Column {name} not available");

            return r.Value;
        }

        public override int GetChildOrdinal(int i)
        {
            var destOrdinal = ColumnMappingInfo.DestinationColumns[i].Ordinal;

            var sourceOrdinal = ColumnMappingInfo.DestinationOrdinalToSourceOrdinal[destOrdinal];

            if (sourceOrdinal == null)
                throw new Exception($"Column {i} not available");

            return sourceOrdinal.Value;
        }

        public object GetValue(string name)
        {
            if (_mappingInfoLazy.Value != null)
            {
                var transformOrdinal = ColumnMappingInfo.SourceColumnNameToDestinationOrdinal[name];

                return transformOrdinal == null
                    ? DataReader[name]
                    : DataTransformsInDestinationOrder[transformOrdinal.Value].Transform(DataReader[name]);
            }

            return DataReader[name];
        }

        public override object GetValue(int i)
        {
            var childOrdinal = GetChildOrdinal(i);

            var o = DataReader.GetValue(childOrdinal);

            if (ColumnMappingInfo != null)
            {
                var transformIndex = ColumnMappingInfo.SourceOrdinalToDestinationOrdinal[childOrdinal];

                return transformIndex == null
                    ? o
                    : DataTransformsInDestinationOrder[transformIndex.Value].Transform(o);
            }

            return o;
        }

        /// <summary>
        ///     Value: ( Source ordinal ) -> ( TransformOrdinal )
        /// </summary>
        private readonly Lazy<ColumnMappingInfo> _mappingInfoLazy;




        public class ColumnTransform
        {
            public TypedDataColumnInfo DestinationColumn { get; set; }
            public DataTransform Transform { get; set; }
        }

        public readonly ColumnTransform[] DataTransformsInDestinationOrder;

        public SmartDataReader(TDataReader dataReader, TypedDataColumnInfo[] destinationColumns,
            DataTransformGroup dataTransformGroup = null) : base(dataReader)
        {
            dataTransformGroup ??= DataTransformGroups.Default;

            DataTransformsInDestinationOrder =
                destinationColumns
                    .StuffToArray(p => p.Ordinal)
                    .Select(destCol =>
                    {
                        var ii = destCol == null ? null : dataTransformGroup(destCol.DataType);

                        return new ColumnTransform
                        {
                            DestinationColumn = destCol,
                            Transform = ii
                        };
                    })
                    .ToArray();

            _mappingInfoLazy = new Lazy<ColumnMappingInfo>(() => dataReader.GetColumnMappings(destinationColumns));
        }

        /// <summary>
        ///     This is the mapping information between source and target.
        /// </summary>
        public ColumnMappingInfo ColumnMappingInfo => _mappingInfoLazy.Value;

        #region SmartTransform Functionality


        public override object this[int i] => GetValue(i);

        public override object this[string name] => GetValue(name);
        
        public string GetReaderDiagnosticInfo()
        {
            return this.PrintDiagnostics();
        }
        
        public override int GetValues(object[] values)
        {
            var i = 0;

            try
            {
                for (; i < FieldCount; i++)
                {
                    if (values.Length <= i)
                        return i;
                    values[i] = GetValue(i);
                }
                return i;
            }
            catch (Exception e)
            {
                var srcValue = "";

                try
                {
                    srcValue = GetValue(i).ToString();
                }
                catch (Exception e1)
                {
                    throw new Exception($"Error retrieving from underlying data reader, field {i} [{ColumnMappingInfo.SourceColumns[i].ColumnName}]", e1);
                }
                
                var destColIndex = ColumnMappingInfo.SourceOrdinalToDestinationOrdinal[i];

                if (destColIndex.HasValue)
                {
                    var destCol = ColumnMappingInfo.DestinationColumns[destColIndex.Value];

                    if (destCol != null)
                    {
                        throw new Exception($"Error converting value '{srcValue}' at field {i} [{ColumnMappingInfo.SourceColumns[i].ColumnName}] to [{destCol.DataType.Name}] type", e);
                    }
                }

                throw new Exception($"Error getting field values on field {i} [{ColumnMappingInfo.SourceColumns[i].ColumnName}] value '{srcValue}'", e);
            }



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