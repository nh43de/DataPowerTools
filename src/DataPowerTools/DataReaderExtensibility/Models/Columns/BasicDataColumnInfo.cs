using System.Reflection;

namespace DataPowerTools.DataReaderExtensibility.Columns
{
    public class BasicDataColumnInfo : BasicDataFieldInfo
    {
        public int Ordinal { get; set; }

        public override string ToString() => $"{Ordinal}. [{ColumnName}]";

        public PropertyInfo PropertyInfo { get; set; }
    }
}