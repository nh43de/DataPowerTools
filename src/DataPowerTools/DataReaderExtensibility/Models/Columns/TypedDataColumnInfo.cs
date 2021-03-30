using System;

namespace DataPowerTools.DataReaderExtensibility.Columns
{
    public class TypedDataColumnInfo : BasicDataColumnInfo
    {
        public Type DataType { get; set; }

        public override string ToString() => $"{Ordinal}. [{ColumnName}] <{DataType.Name}>";
    }
}