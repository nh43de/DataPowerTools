using System;
using System.Reflection;

namespace DataPowerTools.DataReaderExtensibility.Columns
{
    public class BasicDataColumnInfo 
    {
        public int Ordinal { get; set; }
        public string ColumnName { get; set; }
        public Type FieldType { get; set; }

        public override string ToString() => $"{Ordinal}. [{ColumnName}]";

        public PropertyInfo PropertyInfo { get; set; }
    }
 
}