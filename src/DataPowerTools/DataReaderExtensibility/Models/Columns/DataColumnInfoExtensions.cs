using System;
using System.Data;
using System.Linq;
using DataPowerTools.Extensions;

namespace DataPowerTools.DataReaderExtensibility.Columns
{
    public static class DataColumnInfoExtensions
    {
        public static TypedDataColumnInfo ToTypedDataColumnInfo(this DataColumn dataColumn)
        {
            return new TypedDataColumnInfo
            {
                Ordinal = dataColumn.Ordinal,
                ColumnName = dataColumn.ColumnName,
                DataType = dataColumn.DataType
            };
        }
        
        public static TypedDataColumnInfo[] GetTypedDataColumnInfo(this Type dataType)
        {
            var r = dataType
                .GetColumnInfo()
                .Select(col => new TypedDataColumnInfo
                {
                    Ordinal = col.Ordinal,
                    ColumnName = col.ColumnName, //from schema annotations
                    DataType = col.FieldType
                }).ToArray();
            
            return r;
        }
    }
}