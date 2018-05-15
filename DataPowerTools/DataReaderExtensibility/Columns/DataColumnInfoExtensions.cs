using System;
using System.Data;

namespace DataPowerTools.DataReaderExtensibility.Columns
{
    [Obsolete]
    public static class DataColumnInfoExtensions
    {
        [Obsolete]
        public static TypedDataColumnInfo FromDataColumn(this DataColumn dataColumn)
        {
            return new TypedDataColumnInfo
            {
                Ordinal = dataColumn.Ordinal,
                ColumnName = dataColumn.ColumnName,
                DataType = dataColumn.DataType
            };
        }
    }
}