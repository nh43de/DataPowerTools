namespace DataPowerTools.DataReaderExtensibility.Columns
{
    public class FieldValueInfo
    {
        public string ColumnName { get; set; }
        public int ColumnIndex { get; set; }
        public string Value { get; set; }

        public override string ToString() => $"{ColumnIndex}. [{ColumnName}]: '{Value}'";
    }
}