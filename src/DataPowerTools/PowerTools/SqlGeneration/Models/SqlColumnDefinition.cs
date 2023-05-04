namespace DataPowerTools.PowerTools
{
    public class SqlColumnDefinition
    {
        public string ColumnName { get; set; }

        public string DataType { get; set; }

        public bool IsNullable { get; set; } = true;

        public CreateTableSqlInternal.AtomicDataType? BestFitDataType { get; set; }
    }
}